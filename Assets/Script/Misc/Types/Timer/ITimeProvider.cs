using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Timer
{
    public interface ITimeProvider
    {
        public TimerType Type { get; }
        public long Ticks { get; }
    }
}
