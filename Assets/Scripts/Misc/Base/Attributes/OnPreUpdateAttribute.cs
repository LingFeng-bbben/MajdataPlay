using MajdataPlay.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class OnPreUpdateAttribute : PlayerLoopCallbackAttribute
    {
        public OnPreUpdateAttribute() : base()
        {
            Timing = LoopTiming.PreUpdate;
        }
    }
}
