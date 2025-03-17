using System;

namespace MajdataPlay
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
