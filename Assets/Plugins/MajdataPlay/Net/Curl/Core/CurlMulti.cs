using MajdataPlay.Diagnostics;
using MajdataPlay.Net.Curl.Core.PInvoke;
using MajdataPlay.Net.Curl.Lifecycle;
using MajdataPlay.Net.Curl.Utils;
using MajdataPlay.UnsafeKit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        readonly ConcurrentQueue<CurlTask> _pendingToDisposeTasks = new();

        readonly Action<CurlTask> _onResumeRequested;
        readonly Action<CurlTask> _onDisposeRequested;

        internal CurlMulti() 
        {
            LibCurlLifecycle.Retain();
            ThisHandle = LibCurl.Multi.Init();
            _onResumeRequested = OnTaskResume;
            _onDisposeRequested = OnTaskDispose;
            _workerThread = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        public Task<CurlResponse> AddToQueue(HttpRequestMessage request, Stream? contentStream, CurlHttpConfig config, CancellationToken token = default)
        {
            ThrowIfDisposed();
            var curlEasy = new CurlEasy();
            var curlRequest = new CurlRequest(request, contentStream);
            var curlTask = new CurlTask(curlEasy, curlRequest, config, _onResumeRequested, _onDisposeRequested, token);

            curlRequest.ApplyTo(curlEasy);
            config.ApplyToEasy(curlEasy, curlRequest.RequestUri);
            config.ApplyToMulti(this);

            _pendingToSubmitTasks.Enqueue(curlTask);
            
            token.Register(() =>
            {
                _pendingToCancelTasks.Enqueue(curlTask);
            });

            WakeUp();

            return curlTask.Task;
        }

        void OnTaskResume(CurlTask task)
        {
            _pendingToResumeTasks.Enqueue(task);
            WakeUp();
        }
        void OnTaskDispose(CurlTask task)
        {
            _pendingToDisposeTasks.Enqueue(task);
            WakeUp();
        }
        void Run()
        {
            var thisToken = _cts.Token;
            Thread.CurrentThread.Name = "Curl multi worker";
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;
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
                            multiReturnCode = LibCurl.Multi.AddEasyHandle(ThisHandle, pendingTask.Easy.Handle);
                        }
                    }

                    while (_pendingToCancelTasks.TryDequeue(out var cancelTask))
                    {
                        if (cancelTask.TryEnterCancelledState())
                        {
                            if (_activeTasks.Remove(cancelTask))
                            {
                                multiReturnCode = LibCurl.Multi.RemoveEasyHandle(ThisHandle, cancelTask.Easy.Handle);
                            }
                        }
                    }

                    while (_pendingToResumeTasks.TryDequeue(out var pausedTask))
                    {
                        returnCode = pausedTask.Easy.Pause(CurlPauseAction.None);
                    }

                    while (_pendingToDisposeTasks.TryDequeue(out var disposeTask))
                    {
                        disposeTask.TryFail(new ObjectDisposedException(nameof(CurlEasy)));
                        disposeTask.Response.CleanUp();
                        _activeTasks.Remove(disposeTask);
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
            if(UnsafeHelper.TryGetInstanceFromGCHandle<CurlTask>(privatePtr, out var curlTask))
            {
                _activeTasks.Remove(curlTask);
                var curlErr = CurlUtility.GetEasyException(easyHandle, multiMsg.Data.Result);
                if (curlErr is not null)
                {
                    curlTask.TryFail(curlErr);
                }
                else
                {
                    curlTask.TryEnterHeaderReadState();
                    curlTask.TryEnterCompletedState(multiMsg.Data.Result);
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
            var disposedEx = new ObjectDisposedException(nameof(CurlEasy));
            void DisposeCurlTask(CurlTask task)
            {
                task.TryFail(disposedEx);
                var easyHandle = task.Easy.Handle;
                LibCurl.Multi.RemoveEasyHandle(handle, easyHandle);
                task.Response.Dispose();
            }
            foreach (var task in _activeTasks)
            {
                DisposeCurlTask(task);
            }
            _activeTasks.Clear();
            while (_pendingToSubmitTasks.TryDequeue(out var task))
            {
                DisposeCurlTask(task);
            }
            while (_pendingToDisposeTasks.TryDequeue(out var task))
            {
                task.Response.CleanUp();
            }         

            LibCurl.Multi.CleanUp(handle);
            LibCurlLifecycle.Release();
        }
    }    
}
