using System.Diagnostics;

namespace MajdataPlay.Timer
{
    internal sealed class StopwatchTimeProvider : ITimeProvider
    {
        public BuiltInTimeProvider Type { get; } = BuiltInTimeProvider.Stopwatch;
        public long Ticks
        {
            get
            {
                return _ticks;
            }
        }

        long _ticks = 0;

        Stopwatch _stopwatch = new();
        public StopwatchTimeProvider() 
        {
            _stopwatch.Start();
        }
        ~StopwatchTimeProvider() 
        {
            _stopwatch.Stop();
        }
        public void OnPreUpdate()
        {
            _ticks = _stopwatch.ElapsedTicks;
        }
    }
}
