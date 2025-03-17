using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class PlayerLoopFunctionAttribute : Attribute
    {
        public LoopTiming Timing
        {
            get => _timing;
            init => _timing = value;
        }

        LoopTiming _timing = LoopTiming.Update;
    }
    [Flags]
    internal enum LoopTiming
    {
        PreUpdate,
        Update,
        LateUpdate,
        FixedUpdate
    }
}
