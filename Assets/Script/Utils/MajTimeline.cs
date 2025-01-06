using MajdataPlay.Timer;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Utils
{
    public static class MajTimeline
    {
        public static TimeSpan Time
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var time = UnscaledTime;
                return time * TimeScale;
            }
        }
        public static TimeSpan UnscaledTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if(_timeProviders.TryGetValue(Timer,out var timeProvider))
                {
                    return TimeSpan.FromTicks(timeProvider.Ticks);
                }
                else
                {
                    Debug.LogError($"Time provider not found: {Timer}");
                    return TimeSpan.Zero;
                }
            }
        }
        public static double TimeScale { get; set; } = 1;
        public static TimerType Timer { get; set; } = TimerType.Winapi;

        static Dictionary<TimerType, ITimeProvider> _timeProviders = new();
        static MajTimeline()
        {
            _timeProviders = new()
            {
                { TimerType.Unity , new UnityTimeProvider() },
                { TimerType.Winapi, new WinapiTimeProvider() },
                { TimerType.Stopwatch, new StopwatchTimeProvider() },
            };
        }
        public static MajTimer CreateTimer()
        {
            var now = UnscaledTime;
            var offset = now.Ticks;

            return new MajTimer(offset);
        }
    }
}
