using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Timer
{
    public struct MajTimer
    {
        public TimeSpan Time
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return UnscaledTime * TimeScale;
            }
        }
        public TimeSpan UnscaledTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var now = MajTimeline.UnscaledTime;
                var offset = TimeSpan.FromTicks(_offset);
                return now - offset;
            }
        }
        public double TotalSeconds => Time.TotalSeconds;
        public float TotalSecondsAsFloat => (float)TotalSeconds;
        public double TotalMilliseconds => Time.TotalMilliseconds;
        public float TotalMillisecondsAsFloat => (float)TotalMilliseconds;
        public double TimeScale { get; set; }
        public DateTime StartAt { get; init; }

        /// <summary>
        /// Ticks
        /// </summary>
        readonly long _offset;
        public MajTimer(long offset)
        {
            TimeScale = 1;
            StartAt = DateTime.Now;
            _offset = offset;
        }
    }
}
