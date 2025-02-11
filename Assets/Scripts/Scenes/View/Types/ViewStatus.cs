using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.View.Types
{
    internal enum ViewStatus
    {
        Idle,
        Loaded,
        Ready,
        Error,
        Playing,
        Paused,
        Busy
    }
}
