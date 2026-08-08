using AOT;
using MajdataPlay.Net.Curl.Core.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net.Curl.Core
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
        public CurlHttpConfig Config { get; }
        public CancellationToken CancellationToken { get; }

        int _state = (int)CurlRequestState.Created;

        
        readonly Action<CurlTask> _onResume;
        readonly TaskCompletionSource<CurlResponse> _taskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public CurlTask(CurlRequest request, CurlHttpConfig config, Action<CurlTask> onResume, CancellationToken token = default)
        {
            Request = request;
            Response = new CurlResponse(request, OnResumeRequest, config);
            Config = config;
            CancellationToken = token;
            _onResume = onResume;

            var taskHandle = GCHandle.Alloc(this, GCHandleType.Weak);
            var handlePtr = GCHandle.ToIntPtr(taskHandle);
            Request.SetOption(CurlOption.Private, handlePtr);
            Request.SetOption(CurlOption.ReadData, handlePtr);
            Request.SetOption(CurlOption.WriteData, handlePtr);
            Request.SetOption(CurlOption.HeaderData, handlePtr);
            Request.SetOption(CurlOption.SeekData, handlePtr);
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

        void OnResumeRequest()
        {
            _onResume(this);
        }
    }
}
