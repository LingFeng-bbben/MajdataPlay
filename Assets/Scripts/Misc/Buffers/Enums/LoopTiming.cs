using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Buffers
{
    [Flags]
    internal enum LoopTiming
    {
        PreUpdate = 1,
        Update,
        LateUpdate,
        FixedUpdate
    }
}
