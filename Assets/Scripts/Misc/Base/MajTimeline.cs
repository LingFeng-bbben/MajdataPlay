using MajdataPlay.Timer;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    public static class MajTimeline
    {
        public static ulong FrameCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _frameCount;
            }
        }
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
                return TimeSpan.FromTicks(_currentTimer.Ticks);
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
        public static ITimeProvider TimeProvider
        {
            get => _currentTimer;
            set
            {
                _currentTimer = value;
            }
        }
        public static ReadOnlyMemory<ITimeProvider> BuiltInTimeProviders
        {
            get => _builtInTimeProviders;
        }

        static ulong _frameCount = 0;
        static ITimeProvider _currentTimer;
        static TimeSpan _lastUpdateTime = TimeSpan.Zero;
        readonly static ReadOnlyMemory<ITimeProvider> _builtInTimeProviders = new ITimeProvider[3]
        {
            new UnityTimeProvider(),
            new WinapiTimeProvider(),
            new StopwatchTimeProvider()
        };
        static MajTimeline()
        {
            _currentTimer = _builtInTimeProviders.Span[0];
        }
        public static MajTimer CreateTimer()
        {
            var now = UnscaledTime;
            var offset = now.Ticks;

            return new MajTimer(offset);
        }
        internal static void OnPreUpdate()
        {
            var current = UnscaledTime;
            var deltaTime = current - _lastUpdateTime;
            UnscaledDeltaTime = (float)deltaTime.TotalSeconds;
            _lastUpdateTime = current;
            _frameCount++;
        }
    }
}
