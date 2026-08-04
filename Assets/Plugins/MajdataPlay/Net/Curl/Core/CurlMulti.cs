using MajdataPlay.Net.Curl.PInvoke;
using MajdataPlay.Net.Curl.Utils;
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
#nullable enable
namespace MajdataPlay.Net.Curl.Core
{
    internal class CurlMulti : CurlHandle, IAsyncDisposable
    {
        class CurlTask
        {
            public CurlRequestState State
            {
                get
                {
                    return (CurlRequestState)Volatile.Read(ref _state);
                }
            }
            public CurlRequest Request { get; }
            public CurlResponse Response { get; }
            public Task<CurlResponse> Task
            {
                get => _taskSource.Task;
            }
            public CancellationToken CancellationToken { get; }

            int _state = (int)CurlRequestState.Created;

            readonly CurlReadOrWriteCallback _onHeaderReceivedCallback;
            readonly TaskCompletionSource<CurlResponse> _taskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

            public CurlTask(CurlRequest request, CancellationToken token = default)
            {
                Request = request;
                Response = new CurlResponse(request);
                CancellationToken = token;
                _onHeaderReceivedCallback = OnHeaderReceived;
                Request.SetOption(CurlOption.HeaderFunction, _onHeaderReceivedCallback);
            }

            public bool TryEnterSubmittedState()
            {
                var lastState = Interlocked.CompareExchange(ref _state, (int)CurlRequestState.Submitted, (int)CurlRequestState.Created);

                return lastState == (int)CurlRequestState.Created;
            }
            public bool TryEnterHeaderReadState()
            {
                var lastState = Interlocked.CompareExchange(ref _state, (int)CurlRequestState.HeaderRead, (int)CurlRequestState.Submitted);

                if (lastState == (int)CurlRequestState.Submitted)
                {
                    _taskSource.TrySetResult(Response);
                    return true;
                }
                return false;
            }
            public bool TryEnterCompletedState(CurlCode result)
            {
                var lastState = Interlocked.CompareExchange(ref _state, (int)CurlRequestState.Completed, (int)CurlRequestState.HeaderRead);

                if (lastState == (int)CurlRequestState.HeaderRead)
                {
                    Response.ResultCode = result;
                    Response.Complete();
                    _taskSource.TrySetResult(Response);
                    return true;
                }

                return false;
            }
            public bool TryFail(Exception abortException)
            {
                var state = Volatile.Read(ref _state);
                switch ((CurlRequestState)state)
                {
                    case CurlRequestState.Created:
                    case CurlRequestState.Submitted:
                    case CurlRequestState.HeaderRead:
                        break;
                    default:
                        return false;
                }
                var oldState = Interlocked.CompareExchange(ref _state, (int)CurlRequestState.Faulted, state);
                if (oldState == state)
                {
                    Response.Abort(abortException);
                    _taskSource.TrySetException(abortException);
                    return true;
                }
                return false;
            }
            public bool TryEnterCancelledState()
            {
                var state = Volatile.Read(ref _state);

                switch ((CurlRequestState)state)
                {
                    case CurlRequestState.Created:
                    case CurlRequestState.Submitted:
                    case CurlRequestState.HeaderRead:
                        break;

                    default:
                        return false;
                }

                var oldState = Interlocked.CompareExchange(ref _state, (int)CurlRequestState.Cancelled, state);

                if (oldState == state)
                {
                    Response.Abort();
                    _taskSource.TrySetCanceled(CancellationToken);
                    return true;
                }

                return false;
            }

            unsafe UIntPtr OnHeaderReceived(IntPtr ptr, UIntPtr size, UIntPtr nmemb, IntPtr userdata)
            {
                var length = (int)(size.ToUInt32() * nmemb.ToUInt32());
                var buffer = new ReadOnlySpan<byte>((void*)ptr, length);

                if (ParseHttpHeader(buffer, Response.Message))
                {
                    TryEnterHeaderReadState();
                }

                return (UIntPtr)length;
            }
            static bool ParseHttpHeader(ReadOnlySpan<byte> line, HttpResponseMessage response)
            {
                while (line.Length > 0)
                {
                    var last = line[line.Length - 1];
                    if (last == '\r' || last == '\n' || last == ' ')
                    {
                        line = line.Slice(0, line.Length - 1);
                    }
                    else
                    {
                        break;
                    }
                }

                if (line.Length == 0)
                {
                    return true;
                }
                var isHTTPHeader = line.Length >= 5 
                    && line[0] == 'H' 
                    && line[1] == 'T' 
                    && line[2] == 'T' 
                    && line[3] == 'P' 
                    && line[4] == '/';

                // HTTP status code parse
                if (isHTTPHeader)
                {
                    var code = ParseStatusCode(line);
                    if (code > 0)
                    {
                        response.StatusCode = (HttpStatusCode)code;
                        response.Headers.Clear();
                        response.Content.Headers.Clear();
                    }
                    return false;
                }

                // find ':'
                var colonIndex = -1;
                for (var i = 0; i < line.Length; i++)
                {
                    if (line[i] == ':')
                    {
                        colonIndex = i;
                        break;
                    }
                }

                // Colon not found or at the start of the line, invalid header format
                if (colonIndex <= 0)
                {
                    return false;
                }

                // Header key and value extraction
                var keyBytes = TrimSpan(line.Slice(0, colonIndex));
                var valueBytes = TrimSpan(line.Slice(colonIndex + 1));

                // If the key is empty, skip this header
                if (keyBytes.Length == 0)
                {
                    return false;
                }

                var keyStr = GetKeyString(keyBytes);
                var valueStr = Encoding.UTF8.GetString(valueBytes);

                if (!response.Headers.TryAddWithoutValidation(keyStr, valueStr))
                {
                    response.Content.Headers.TryAddWithoutValidation(keyStr, valueStr);
                }
                return false;
            }

            
            public static CurlTask? FromHandle(IntPtr handle)
            {
                var gcHandle = GCHandle.FromIntPtr(handle);
                if (gcHandle.IsAllocated)
                {
                    return gcHandle.Target as CurlTask;
                }
                return null;
            }

            static int ParseStatusCode(ReadOnlySpan<byte> line)
            {
                int firstSpace = -1;
                for (int i = 5; i < line.Length; i++)
                {
                    if (line[i] == ' ')
                    {
                        firstSpace = i;
                        break;
                    }
                }

                if (firstSpace > 0 && firstSpace + 3 < line.Length)
                {
                    var codeStart = firstSpace + 1;
                    while (codeStart < line.Length && line[codeStart] == 32)
                    {
                        codeStart++;
                    }

                    if (codeStart + 2 < line.Length)
                    {
                        var d1 = line[codeStart];
                        var d2 = line[codeStart + 1];
                        var d3 = line[codeStart + 2];

                        if (d1 >= '0' && d1 <= '9' &&
                            d2 >= '0' && d2 <= '9' &&
                            d3 >= '0' && d3 <= '9')
                        {
                            return ((d1 - '0') * 100) + ((d2 - '0') * 10) + (d3 - '0');
                        }
                    }
                }
                return -1;
            }
            static string GetKeyString(ReadOnlySpan<byte> keyBytes)
            {
                if (IsMatch(keyBytes, "Content-Type"))
                {
                    return "Content-Type";
                }
                if (IsMatch(keyBytes, "Content-Length"))
                {
                    return "Content-Length";
                }
                if (IsMatch(keyBytes, "Server"))
                {
                    return "Server";
                }
                if (IsMatch(keyBytes, "Date"))
                {
                    return "Date";
                }
                if (IsMatch(keyBytes, "Set-Cookie"))
                {
                    return "Set-Cookie";
                }
                if (IsMatch(keyBytes, "Cache-Control"))
                {
                    return "Cache-Control";
                }
                if (IsMatch(keyBytes, "Connection"))
                {
                    return "Connection";
                }
                if (IsMatch(keyBytes, "Location"))
                {
                    return "Location";
                }
                if (IsMatch(keyBytes, "Transfer-Encoding"))
                {
                    return "Transfer-Encoding";
                }

                return Encoding.UTF8.GetString(keyBytes);
            }
            static bool IsMatch(ReadOnlySpan<byte> span, string knownKey)
            {
                if (span.Length != knownKey.Length)
                {
                    return false;
                }

                for (var i = 0; i < span.Length; i++)
                {
                    var b = span[i];
                    var c = knownKey[i];

                    if (b == (byte)c)
                    {
                        continue;
                    }
                    if (b >= 'A' && b <= 'Z' && (b + 32) == c)
                    {
                        continue;
                    }
                    if (b >= 'a' && b <= 'z' && (b - 32) == c)
                    {
                        continue;
                    }

                    return false;
                }
                return true;
            }
            static ReadOnlySpan<byte> TrimSpan(ReadOnlySpan<byte> span)
            {
                var start = 0;
                while (start < span.Length && span[start] == 32)
                {
                    start++;
                }

                var end = span.Length - 1;
                while (end >= start && span[end] == 32)
                {
                    end--;
                }

                var length = end - start + 1;
                if (length <= 0)
                {
                    return ReadOnlySpan<byte>.Empty;
                }
                return span.Slice(start, length);
            }
        }

        readonly Task _workerThread;
        readonly CancellationTokenSource _cts = new();
        readonly HashSet<CurlTask> _activeTasks = new();
        readonly ConcurrentQueue<CurlTask> _pendingTasks = new();
        readonly ConcurrentQueue<CurlTask> _pendingCancels = new();

        public CurlMulti() 
        {
            ThisHandle = LibCurl.curl_multi_init();
            _workerThread = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        public Task<CurlResponse> AddToQueue(CurlRequest request, CancellationToken token = default)
        {
            ThrowIfDisposed();
            var curlTask = new CurlTask(request, token);
            var taskHandle = GCHandle.Alloc(curlTask, GCHandleType.Weak);
            request.SetOption(CurlOption.Private, GCHandle.ToIntPtr(taskHandle));
            _pendingTasks.Enqueue(curlTask);
            token.Register(() =>
            {
                _pendingCancels.Enqueue(curlTask);
            });

            return curlTask.Task;
        }

        void Run()
        {
            var thisToken = _cts.Token;
            while(!thisToken.IsCancellationRequested)
            {
                while(_pendingTasks.TryDequeue(out var pendingTask))
                {
                    if(pendingTask.CancellationToken.IsCancellationRequested)
                    {
                        continue;
                    }
                    if(pendingTask.TryEnterSubmittedState())
                    {
                        _activeTasks.Add(pendingTask);
                        LibCurl.curl_multi_add_handle(ThisHandle, pendingTask.Request.Handle);
                    }                    
                }

                while(_pendingCancels.TryDequeue(out var cancelTask))
                {
                    if(cancelTask.TryEnterCancelledState())
                    {
                        if (_activeTasks.Remove(cancelTask))
                        {
                            LibCurl.curl_multi_remove_handle(ThisHandle, cancelTask.Request.Handle);
                        }
                    }
                }

                if(thisToken.IsCancellationRequested)
                {
                    return;
                }

                LibCurl.curl_multi_perform(ThisHandle, out var runningHandles);

                if (thisToken.IsCancellationRequested)
                {
                    return;
                }
                var msgHandle = IntPtr.Zero;
                while ((msgHandle = LibCurl.curl_multi_info_read(ThisHandle, out var remaining)) != IntPtr.Zero)
                {
                    var multiMsg = Marshal.PtrToStructure<CurlMsg>(msgHandle);
                    if(multiMsg.Code == CurlMsgCode.Done)
                    {
                        CompleteTask(multiMsg);
                    }
                }

                LibCurl.curl_multi_poll(ThisHandle, IntPtr.Zero, 0, 1000, out _);
            }
        }

        void CompleteTask(CurlMsg multiMsg)
        {
            var easyHandle = multiMsg.EasyHandle;
            LibCurl.curl_easy_getinfo(easyHandle, CurlInfo.Private, out IntPtr privatePtr);
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
                LibCurl.curl_easy_setopt(easyHandle, CurlOption.Private, IntPtr.Zero);
            }
            LibCurl.curl_multi_remove_handle(ThisHandle, easyHandle);
        }

        public override void Dispose()
        {
            var handle = Interlocked.Exchange(ref ThisHandle, IntPtr.Zero);
            if (handle != IntPtr.Zero)
            {
                _cts.Cancel();
                _cts.Dispose();
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
                LibCurl.curl_multi_remove_handle(handle, easyHandle);
                task.Request.Dispose();
            }
            
            while (_pendingTasks.TryDequeue(out var task))
            {
                DisposeCurlTask(task);
            }
            while (_pendingCancels.TryDequeue(out var task))
            {
                DisposeCurlTask(task);
            }
            foreach(var task in _activeTasks)
            {
                DisposeCurlTask(task);
            }
            _activeTasks.Clear();

            LibCurl.curl_multi_cleanup(handle);
        }
    }    
}
