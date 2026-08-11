using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Timer
{
    public sealed class WinapiTimeProvider : ITimeProvider
    {
        public BuiltInTimeProvider Type { get; } = BuiltInTimeProvider.Winapi;
        public long Ticks 
        { 
            get
            {
                return _ticks;
            }
        }

        long _startAt = 0;
        long _ticks = 0;

        public WinapiTimeProvider()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX
            GetSystemTimePreciseAsFileTime(out long fileTime);
            //var now = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromTicks(fileTime);

            //_startAt = now.Ticks;
            _startAt = fileTime;
#endif
        }
        public void OnPreUpdate()
        {
#if UNITY_STANDALONE_WIN
            GetSystemTimePreciseAsFileTime(out long fileTime);
            
            _ticks = fileTime - _startAt;
#endif
        }

#if UNITY_STANDALONE_WIN
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern void GetSystemTimePreciseAsFileTime(out long filetime);
#endif
    }
}
