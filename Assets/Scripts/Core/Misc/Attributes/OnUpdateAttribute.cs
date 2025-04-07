﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class OnUpdateAttribute : PlayerLoopFunctionAttribute
    {
        public OnUpdateAttribute() : base()
        {
            Timing = LoopTiming.Update;
        }
    }
}
