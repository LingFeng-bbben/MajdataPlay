using MajdataPlay.Buffers;
using System;

namespace MajdataPlay
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class OnFixedUpdateAttribute : PlayerLoopCallbackAttribute
    {
        public OnFixedUpdateAttribute() : base()
        {
            Timing = LoopTiming.FixedUpdate;
        }
    }
}
