using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace MajdataPlay.Buffers
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class PlayerLoopFunctionAttribute : PreserveAttribute
    {
        public LoopTiming Timing
        {
            get => _timing;
            init => _timing = value;
        }

        LoopTiming _timing = LoopTiming.Update;
    }
}
