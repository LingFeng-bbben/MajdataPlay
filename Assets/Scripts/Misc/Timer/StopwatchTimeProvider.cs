using System.Diagnostics;

namespace MajdataPlay.Timer
{
    internal class StopwatchTimeProvider : ITimeProvider
    {
        public BuiltInTimeProvider Type { get; } = BuiltInTimeProvider.Stopwatch;
        public long Ticks => _stopwatch.ElapsedTicks;

        Stopwatch _stopwatch = new();
        public StopwatchTimeProvider() 
        {
            _stopwatch.Start();
        }
        ~StopwatchTimeProvider() 
        {
            _stopwatch.Stop();
        }
    }
}
