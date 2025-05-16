using System;

namespace MajdataPlay.Buffers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class OnLateUpdateAttribute : PlayerLoopFunctionAttribute
    {
        public OnLateUpdateAttribute() : base()
        {
            Timing = LoopTiming.LateUpdate;
        }
    }
}
