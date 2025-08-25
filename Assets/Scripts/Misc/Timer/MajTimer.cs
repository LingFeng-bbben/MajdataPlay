using System;
using System.Runtime.CompilerServices;
#nullable enable
namespace MajdataPlay.Timer
{
    public struct MajTimer
    {
        public TimeSpan ElapsedTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return UnscaledElapsedTime * TimeScale;
            }
        }
        public TimeSpan UnscaledElapsedTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var now = MajTimeline.UnscaledTime;
                var offset = TimeSpan.FromTicks(_offset);
                return now - offset;
            }
        }
        public double ElapsedSeconds => ElapsedTime.TotalSeconds;
        public float ElapsedSecondsAsFloat => (float)ElapsedSeconds;
        public double ElapsedMilliseconds => ElapsedTime.TotalMilliseconds;
        public float ElapsedMillisecondsAsFloat => (float)ElapsedMilliseconds;
        public long ElapsedTicks => ElapsedTime.Ticks;
        public double UnscaledElapsedSeconds => UnscaledElapsedTime.TotalSeconds;
        public float UnscaledElapsedSecondsAsFloat => (float)UnscaledElapsedSeconds;
        public double UnscaledElapsedMilliseconds => UnscaledElapsedTime.TotalMilliseconds;
        public float UnscaledElapsedMillisecondsAsFloat => (float)UnscaledElapsedMilliseconds;
        public long UnscaledElapsedTicks => UnscaledElapsedTime.Ticks;
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
