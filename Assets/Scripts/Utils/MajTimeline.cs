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
        /// <summary>
        /// The time in seconds since the start of the game.
        /// </summary>
        public static TimeSpan Time
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var time = UnscaledTime;
                return time * TimeScale;
            }
        }
        /// <summary>
        /// The time in seconds since the start of the game.
        /// </summary>
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
                    MajDebug.LogError($"Time provider not found: {Timer}");
                    return TimeSpan.Zero;
                }
            }
        }
        /// <summary>
        /// The interval in seconds from the last frame to the current one.
        /// </summary>
        public static float DeltaTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return UnscaledDeltaTime / TimeScale;
            }
        }
        /// <summary>
        /// The timeScale-independent interval in seconds from the last frame to the current one.
        /// </summary>
        public static float UnscaledDeltaTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }
        public static float FixedDeltaTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnityEngine.Time.fixedDeltaTime;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => UnityEngine.Time.fixedDeltaTime = value;
        }
        public static float FixedUnscaledDeltaTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnityEngine.Time.fixedUnscaledDeltaTime;
        }
        public static float TimeScale { get; set; } = 1;
        public static TimerType Timer { get; set; } = TimerType.Winapi;

        static TimeSpan _lastUpdateTime = TimeSpan.Zero;
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
        internal static void OnPreUpdate()
        {
            var deltaTime = UnscaledTime - _lastUpdateTime;
            UnscaledDeltaTime = (float)deltaTime.TotalMilliseconds;
        }
    }
}
