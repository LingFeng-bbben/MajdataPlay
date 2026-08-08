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
        public CurlTaskState State
        {
            get
            {
                return (CurlTaskState)Volatile.Read(ref _state);
            }
        }
        public CurlRequest Request { get; }
        public CurlResponse Response { get; }
        public Task<CurlResponse> Task
        {
            get => _taskSource.Task;
        }
        public CurlHttpConfig Config { get; }
        internal CurlEasy Easy { get; }
        internal CancellationToken CancellationToken { get; }


        int _state = (int)CurlTaskState.Created;

        readonly Action<CurlTask> _onResume;
        readonly Action<CurlTask> _onDispose;
        readonly GCHandle _taskHandle;
        readonly TaskCompletionSource<CurlResponse> _taskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public CurlTask(CurlEasy curlEasy, 
            CurlRequest request, 
            CurlHttpConfig config, 
            Action<CurlTask> onResume, 
            Action<CurlTask> onDispose, 
            CancellationToken token = default)
        {
            Easy = curlEasy;
            Request = request;
            // transfer ownership of CurlEasy to CurlResponse
            Response = new CurlResponse(curlEasy, request, OnResumeRequested, OnDisposeRequested, config);
            Config = config;
            CancellationToken = token;
            _onResume = onResume;
            _onDispose = onDispose;

            _taskHandle = GCHandle.Alloc(this, GCHandleType.Weak);
            var handlePtr = GCHandle.ToIntPtr(_taskHandle);
            Easy.SetOption(CurlOption.Private, handlePtr);
            Easy.SetOption(CurlOption.WriteData, handlePtr);
            Easy.SetOption(CurlOption.HeaderData, handlePtr);
        }

        ~CurlTask()
        {
            if (_taskHandle.IsAllocated)
            {
                _taskHandle.Free();
            }
        }

        public bool TryEnterSubmittedState()
        {
            var lastState = Interlocked.CompareExchange(ref _state, (int)CurlTaskState.Submitted, (int)CurlTaskState.Created);

            return lastState == (int)CurlTaskState.Created;
        }
        public bool TryEnterHeaderReadState()
        {
            var lastState = Interlocked.CompareExchange(ref _state, (int)CurlTaskState.HeaderRead, (int)CurlTaskState.Submitted);

            if (lastState == (int)CurlTaskState.Submitted)
            {
                _taskSource.TrySetResult(Response);
                return true;
            }
            return false;
        }
        public bool TryEnterCompletedState(CurlCode result)
        {
            var lastState = Interlocked.CompareExchange(ref _state, (int)CurlTaskState.Completed, (int)CurlTaskState.HeaderRead);

            if (lastState == (int)CurlTaskState.HeaderRead)
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
            switch ((CurlTaskState)state)
            {
                case CurlTaskState.Created:
                case CurlTaskState.Submitted:
                case CurlTaskState.HeaderRead:
                    break;
                default:
                    return false;
            }
            var oldState = Interlocked.CompareExchange(ref _state, (int)CurlTaskState.Faulted, state);
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

            switch ((CurlTaskState)state)
            {
                case CurlTaskState.Created:
                case CurlTaskState.Submitted:
                case CurlTaskState.HeaderRead:
                    break;

                default:
                    return false;
            }

            var oldState = Interlocked.CompareExchange(ref _state, (int)CurlTaskState.Cancelled, state);

            if (oldState == state)
            {
                Response.Abort();
                _taskSource.TrySetCanceled(CancellationToken);
                return true;
            }

            return false;
        }

        void OnResumeRequested()
        {
            _onResume(this);
        }
        void OnDisposeRequested()
        {
            _onDispose(this);
        }
    }
}
