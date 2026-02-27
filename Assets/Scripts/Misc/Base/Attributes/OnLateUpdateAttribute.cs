using MajdataPlay.Buffers;
using System;

namespace MajdataPlay
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class OnLateUpdateAttribute : PlayerLoopCallbackAttribute
    {
        public OnLateUpdateAttribute() : base()
        {
            Timing = LoopTiming.LateUpdate;
        }
    }
}
