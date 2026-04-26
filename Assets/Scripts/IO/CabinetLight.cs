using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable enable
// ReSharper disable MemberCanBePrivate.Global
namespace MajdataPlay.IO
{
    internal static class CabinetLight
    {
        public static bool IsEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isEnabled && _isSupported;
        }

        public static bool IsSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isSupported;
        }

        public static float Brightness
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _updateFunction.Current;
        }

        internal static byte ReportBrightness
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)Mathf.RoundToInt(Mathf.Clamp01(Brightness) * 255f);
        }

        static TimeSpan _lastUpdateTime;
        static readonly CabinetLightCommonUpdateFunction _commonFunc = new();
        static readonly CabinetLightLinearUpdateFunction _linearFunc = new();
        static readonly CabinetLightSineUpdateFunction _sineFunc = new();
        static bool _isSupported;
        static bool _isEnabled;

        static ICabinetLightUpdateFunction _updateFunction = _commonFunc;

        [Conditional("UNITY_STANDALONE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetSupported(bool isSupported, bool isEnabled)
        {
            _isSupported = isSupported;
            _isEnabled = isEnabled;
        }

        [Conditional("UNITY_STANDALONE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LedFuncUpdate()
        {
            if (!IsEnabled)
            {
                return;
            }
            var currentTime = MajTimeline.UnscaledTime;
            var deltaMs = (float)(currentTime - _lastUpdateTime).TotalMilliseconds;
            _lastUpdateTime = currentTime;
            _updateFunction.Update(deltaMs);
        }

        [Conditional("UNITY_STANDALONE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLight(byte brightness)
        {
            SetLight(brightness / 255f);
        }

        [Conditional("UNITY_STANDALONE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLight(float brightness)
        {
            if (!IsEnabled)
            {
                return;
            }
            _commonFunc.SetBrightness(Mathf.Clamp01(brightness));
            _updateFunction = _commonFunc;
        }

        [Conditional("UNITY_STANDALONE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLightWithTimeout(float brightness, long durationMs = 500)
        {
            SetLightWithTimeout(brightness, TimeSpan.FromMilliseconds(durationMs));
        }

        [Conditional("UNITY_STANDALONE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLightWithTimeout(float brightness, TimeSpan duration)
        {
            if (!IsEnabled)
            {
                return;
            }
            _commonFunc.SetBrightness(Mathf.Clamp01(brightness), duration);
            _updateFunction = _commonFunc;
        }

        [Conditional("UNITY_STANDALONE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLightLinearTo(float from, float to, long durationMs)
        {
            SetLightLinearTo(from, to, TimeSpan.FromMilliseconds(durationMs));
        }

        [Conditional("UNITY_STANDALONE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLightLinearTo(float from, float to, TimeSpan duration)
        {
            if (!IsEnabled)
            {
                return;
            }
            _linearFunc.LinearTo(Mathf.Clamp01(from), Mathf.Clamp01(to), duration);
            _updateFunction = _linearFunc;
        }

        [Conditional("UNITY_STANDALONE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLightSineFunc(float brightness, long tMs, float phi = 0.5f)
        {
            SetLightSineFunc(brightness, TimeSpan.FromMilliseconds(tMs), phi);
        }

        [Conditional("UNITY_STANDALONE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLightSineFunc(float brightness, TimeSpan t, float phi = 0.5f)
        {
            if (!IsEnabled)
            {
                return;
            }
            _sineFunc.SetSineFunc(Mathf.Clamp01(brightness), t, phi);
            _updateFunction = _sineFunc;
        }

        interface ICabinetLightUpdateFunction
        {
            float Current { get; }

            void Update(float deltaMs);
            void Reset();
        }

        class CabinetLightCommonUpdateFunction : ICabinetLightUpdateFunction
        {
            public float Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _currentBrightness;
            }

            float _defaultBrightness;
            float _targetBrightness;
            float _currentBrightness;
            float _durationMs;
            float _elapsedMs;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update(float deltaMs)
            {
                if (_elapsedMs >= _durationMs)
                {
                    _currentBrightness = _defaultBrightness;
                    return;
                }
                _elapsedMs += deltaMs;
                _currentBrightness = _targetBrightness;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _elapsedMs = 0f;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetBrightness(float brightness)
            {
                _defaultBrightness = brightness;
                _currentBrightness = brightness;
                _durationMs = 0f;
                _elapsedMs = 0f;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetBrightness(float brightness, TimeSpan duration)
            {
                _targetBrightness = brightness;
                _durationMs = (float)duration.TotalMilliseconds;
                _elapsedMs = 0f;
            }
        }

        class CabinetLightLinearUpdateFunction : ICabinetLightUpdateFunction
        {
            public float Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _currentBrightness;
            }

            float _from;
            float _to;
            float _currentBrightness;
            float _durationMs;
            float _elapsedMs;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update(float deltaMs)
            {
                if (_elapsedMs >= _durationMs)
                {
                    _currentBrightness = _to;
                    return;
                }
                var t = _durationMs <= 0f ? 1f : _elapsedMs / _durationMs;
                _currentBrightness = Mathf.Lerp(_from, _to, t);
                _elapsedMs += deltaMs;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _elapsedMs = 0f;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void LinearTo(float from, float to, TimeSpan duration)
            {
                _from = from;
                _to = to;
                _durationMs = (float)duration.TotalMilliseconds;
                _elapsedMs = 0f;
            }
        }

        class CabinetLightSineUpdateFunction : ICabinetLightUpdateFunction
        {
            public float Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _currentBrightness;
            }

            float _defaultBrightness;
            float _currentBrightness;
            float _tMs;
            float _elapsedMs;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update(float deltaMs)
            {
                if (_elapsedMs >= _tMs)
                {
                    _elapsedMs %= _tMs;
                }
                var t = _elapsedMs / _tMs;
                var a = t < 0.5f ? t * 2f : (1f - t) * 2f;
                _currentBrightness = Mathf.Lerp(_defaultBrightness * 0.25f, _defaultBrightness, a);
                _elapsedMs += deltaMs;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _elapsedMs = 0f;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetSineFunc(float brightness, TimeSpan t, float phi = 0.5f)
            {
                if (phi > 1f || phi < 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(phi), "phi must be in [0, 1]");
                }
                if (t.TotalMilliseconds <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(t), "t must be greater than 0");
                }
                _defaultBrightness = brightness;
                _tMs = (float)t.TotalMilliseconds;
                _elapsedMs = _tMs * phi;
                var a = phi < 0.5f ? phi * 2f : (1f - phi) * 2f;
                _currentBrightness = Mathf.Lerp(_defaultBrightness * 0.25f, _defaultBrightness, a);
            }
        }
    }
}
