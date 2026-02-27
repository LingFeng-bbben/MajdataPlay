using MajdataPlay.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace MajdataPlay
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class PlayerLoopCallbackAttribute : PreserveAttribute
    {
        public LoopTiming Timing
        {
            get => _timing;
            init => _timing = value;
        }

        LoopTiming _timing = LoopTiming.Update;

        [Flags]
        public enum LoopTiming
        {
            PreUpdate = 1,
            Update,
            LateUpdate,
            FixedUpdate
        }
    }
}
