using MajdataPlay.Net.Curl.Core.PInvoke;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MajdataPlay.Net.Curl.Utils;
using MajdataPlay.Net.Curl.Lifecycle;
using System.IO;
using MajdataPlay.Diagnostics;
#nullable enable
namespace MajdataPlay.Net.Curl.Core
{
    public class CurlMulti : CurlHandle, IAsyncDisposable
    {
        readonly Task _workerThread;
        readonly CancellationTokenSource _cts = new();
        readonly HashSet<CurlTask> _activeTasks = new();
        readonly ConcurrentQueue<CurlTask> _pendingToSubmitTasks = new();
        readonly ConcurrentQueue<CurlTask> _pendingToCancelTasks = new();
        readonly ConcurrentQueue<CurlTask> _pendingToResumeTasks = new();

        readonly Action<CurlTask> _onRequestResume;

        internal CurlMulti() 
        {
            LibCurlLifecycle.Retain();
            ThisHandle = LibCurl.Multi.Init();
            _onRequestResume = OnTaskResume;
            _workerThread = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        public Task<CurlResponse> AddToQueue(HttpRequestMessage request, Stream? contentStream, CurlHttpConfig config, CancellationToken token = default)
        {
            ThrowIfDisposed();
            var curlRequest = new CurlRequest(request, contentStream);
            var curlTask = new CurlTask(curlRequest, config, _onRequestResume, token);
            
            config.ApplyToRequest(curlRequest);
            config.ApplyToMulti(this);

            _pendingToSubmitTasks.Enqueue(curlTask);
            
            token.Register(() =>
            {
                _pendingToCancelTasks.Enqueue(curlTask);
            });

            WakeUp();

            return curlTask.Task;
        }

        void OnTaskResume(CurlTask request)
        {
            _pendingToResumeTasks.Enqueue(request);
            WakeUp();
        }
        void Run()
        {
            var thisToken = _cts.Token;
            Thread.CurrentThread.Name = "Curl multi worker";
            while (!thisToken.IsCancellationRequested)
            {
                try
                {
                    var returnCode = default(CurlCode?);
                    var multiReturnCode = default(CurlMCode?);
                    while (_pendingToSubmitTasks.TryDequeue(out var pendingTask))
                    {
                        if (pendingTask.CancellationToken.IsCancellationRequested)
                        {
                            continue;
                        }
                        if (pendingTask.TryEnterSubmittedState())
                        {
                            _activeTasks.Add(pendingTask);
                            multiReturnCode = LibCurl.Multi.AddEasyHandle(ThisHandle, pendingTask.Request.Handle);
                        }
                    }

                    while (_pendingToCancelTasks.TryDequeue(out var cancelTask))
                    {
                        if (cancelTask.TryEnterCancelledState())
                        {
                            if (_activeTasks.Remove(cancelTask))
                            {
                                multiReturnCode = LibCurl.Multi.RemoveEasyHandle(ThisHandle, cancelTask.Request.Handle);
                            }
                        }
                    }

                    while (_pendingToResumeTasks.TryDequeue(out var pausedTask))
                    {
                        returnCode = LibCurl.Easy.Pause(pausedTask.Request.Handle, LibCurl.CURLPAUSE_RECV_CONT);
                    }

                    if (thisToken.IsCancellationRequested)
                    {
                        return;
                    }

                    multiReturnCode = LibCurl.Multi.Perform(ThisHandle, out var runningHandles);

                    if (thisToken.IsCancellationRequested)
                    {
                        return;
                    }
                    while (LibCurl.Multi.GetMessage(ThisHandle, out var remaining) is CurlMsg multiMsg)
                    {
                        if (multiMsg.Code == CurlMsgCode.Done)
                        {
                            CompleteTask(multiMsg);
                        }
                    }

                    multiReturnCode = LibCurl.Multi.Poll(ThisHandle, IntPtr.Zero, 0, 1000, out _);
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"[libcurl][Multi worker]{e}");
                    Thread.Sleep(2500);
                }
            }            
        }
        void WakeUp()
        {
            LibCurl.Multi.Wakeup(ThisHandle);
        }

        void CompleteTask(CurlMsg multiMsg)
        {
            var easyHandle = multiMsg.EasyHandle;
            LibCurl.Easy.GetInfo(easyHandle, CurlInfo.Private, out IntPtr privatePtr);
            if (privatePtr != IntPtr.Zero)
            {
                var taskHandle = GCHandle.FromIntPtr(privatePtr);
                if (taskHandle.IsAllocated)
                {
                    var curlTask = (CurlTask)taskHandle.Target;
                    _activeTasks.Remove(curlTask);
                    var curlErr = CurlUtility.GetEasyException(easyHandle, multiMsg.Data.Result);
                    if (curlErr is not null)
                    {
                        curlTask.TryFail(curlErr);
                    }
                    else
                    {
                        curlTask.TryEnterCompletedState(multiMsg.Data.Result);
                    }
                    taskHandle.Free();
                }
                LibCurl.Easy.SetOption(easyHandle, CurlOption.Private, IntPtr.Zero);
            }
            LibCurl.Multi.RemoveEasyHandle(ThisHandle, easyHandle);
        }

        public override void Dispose()
        {
            var handle = Interlocked.Exchange(ref ThisHandle, IntPtr.Zero);
            if (handle != IntPtr.Zero)
            {
                _cts.Cancel();
                _cts.Dispose();
                WakeUp();
                _workerThread.Wait();
                CleanUp(handle);
                GC.SuppressFinalize(this);
            }
        }
        public async ValueTask DisposeAsync()
        {
            var handle = Interlocked.Exchange(ref ThisHandle, IntPtr.Zero);
            if (handle != IntPtr.Zero)
            {
                _cts.Cancel();
                _cts.Dispose();
                WakeUp();
                await _workerThread;
                CleanUp(handle);
                GC.SuppressFinalize(this);
            }
        }
        void CleanUp(IntPtr handle)
        {
            var disposedEx = new ObjectDisposedException(nameof(CurlRequest));
            void DisposeCurlTask(CurlTask task)
            {
                task.TryFail(disposedEx);
                var easyHandle = task.Request.Handle;
                LibCurl.Multi.RemoveEasyHandle(handle, easyHandle);
                task.Request.Dispose();
            }
            
            while (_pendingToSubmitTasks.TryDequeue(out var task))
            {
                DisposeCurlTask(task);
            }
            while (_pendingToCancelTasks.TryDequeue(out var task))
            {
                DisposeCurlTask(task);
            }
            foreach(var task in _activeTasks)
            {
                DisposeCurlTask(task);
            }
            _activeTasks.Clear();

            LibCurl.Multi.CleanUp(handle);
            LibCurlLifecycle.Release();
        }
    }    
}
