using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Timer
{
    public class WinapiTimeProvider : ITimeProvider
    {
        public TimerType Type { get; } = TimerType.Winapi;
        public long Ticks 
        { 
            get
            {
                GetSystemTimePreciseAsFileTime(out long fileTime);
                var now = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromTicks(fileTime);

                return now.Ticks - _startAt;
            }
        }

        public long _startAt = 0;

        public WinapiTimeProvider()
        {
            GetSystemTimePreciseAsFileTime(out long fileTime);
            var now = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromTicks(fileTime);

            _startAt = now.Ticks;
        }

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern void GetSystemTimePreciseAsFileTime(out long filetime);
    }
}
