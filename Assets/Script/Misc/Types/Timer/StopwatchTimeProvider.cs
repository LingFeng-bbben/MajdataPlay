using MajdataPlay.Types;
using System.Diagnostics;

namespace MajdataPlay.Timer
{
    internal class StopwatchTimeProvider : ITimeProvider
    {
        public TimerType Type { get; } = TimerType.Stopwatch;
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
