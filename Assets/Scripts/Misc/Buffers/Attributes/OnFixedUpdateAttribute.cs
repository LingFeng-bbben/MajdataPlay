using System;

namespace MajdataPlay.Buffers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class OnFixedUpdateAttribute : PlayerLoopFunctionAttribute
    {
        public OnFixedUpdateAttribute() : base()
        {
            Timing = LoopTiming.FixedUpdate;
        }
    }
}
