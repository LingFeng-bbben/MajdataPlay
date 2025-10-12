using MajdataPlay.Utils;
using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    internal static class LedRing
    {
        public static bool IsEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _isEnabled;
            }
        }
        public static bool IsConnected
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            private set;
        }
        public static ReadOnlySpan<Color> LedColors
        {
            get
            {
                return _ledColors;
            }
        }
        
        readonly static Color[] _ledColors = new Color[8];
        readonly static Led[] _ledDevices = new Led[8];
        readonly static LedCommonUpdateFunction[] _ledCommFuncs = new LedCommonUpdateFunction[8];
        readonly static LedLinearUpdateFunction[] _ledLinearFuncs = new LedLinearUpdateFunction[8];
        readonly static LedSineUpdateFunction[] _ledSineFuncs = new LedSineUpdateFunction[8];
        readonly static bool _isEnabled = true;

        static LedRing()
        {
            _isEnabled = MajInstances.Settings.IO.OutputDevice.Led.Enable;
            var ledDevices = _ledDevices;
            var ledCommFuncs = _ledCommFuncs;
            var ledLinearFuncs = _ledLinearFuncs;
            var ledSineFuncs = _ledSineFuncs;

            for (var i = 0; i < 8; i++)
            {
                ledCommFuncs[i] = new();
                ledLinearFuncs[i] = new();
                ledSineFuncs[i] = new();
                ledDevices[i] = new(ledCommFuncs[i])
                {
                    Index = i,
                };
            }
            if (!_isEnabled)
            {
                for (var i = 0; i < 8; i++)
                {
                    ledCommFuncs[i].SetColor(Color.black);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnPreUpdate()
        {
            var ledDevices = _ledDevices;
            var ledColors = _ledColors;
            var deltaMs = MajTimeline.DeltaTime * 1000f;

            for (var i = 0; i < 8; i++)
            {
                var device = ledDevices[i];
                var func = device.UpdateFunction;

                func.Update(deltaMs);
                ledColors[i] = device.Color;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAllLight(Color lightColor)
        {
            if (!_isEnabled)
            {
                return;
            }
            for (var i = 0; i < 8; i++)
            {
                var func = _ledCommFuncs[i];
                func.SetColor(lightColor);
                _ledDevices[i].UpdateFunction = func;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAllLightLinearTo(Color from, Color to, long durationMs)
        {
            if (!_isEnabled)
            {
                return;
            }
            SetAllLightLinearTo(from, to, TimeSpan.FromMilliseconds(durationMs));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAllLightLinearTo(Color from, Color to, TimeSpan duration)
        {
            if (!_isEnabled)
            {
                return;
            }
            for (var i = 0; i < 8; i++)
            {
                var func = _ledLinearFuncs[i];
                func.LinearTo(from, to, duration);
                _ledDevices[i].UpdateFunction = func;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAllLightSineFunc(Color color, long T_Ms, float phi = 0.5f)
        {
            if (!_isEnabled)
            {
                return;
            }
            SetAllLightSineFunc(color, TimeSpan.FromMilliseconds(T_Ms), phi);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAllLightSineFunc(Color color, TimeSpan T, float phi = 0.5f)
        {
            if (!_isEnabled)
            {
                return;
            }
            for (var i = 0; i < 8; i++)
            {
                var func = _ledSineFuncs[i];
                func.SetSineFunc(color, T, phi);
                _ledDevices[i].UpdateFunction = func;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAllLightUpdateFunc(ReadOnlySpan<ILedUpdateFunction> funcs)
        {
            if (!_isEnabled)
            {
                return;
            }
            if(funcs.Length <= 8)
            {
                throw new ArgumentException("funcs length must be 8", nameof(funcs));
            }
            for (var i = 0; i < 8; i++)
            {
                _ledDevices[i].UpdateFunction = funcs[i];
            }
        }

        public static void SetButtonLight(Color lightColor, int button)
        {
            if (!_isEnabled)
            {
                return;
            }
            var func = _ledCommFuncs[button];
            func.SetColor(lightColor);
            _ledDevices[button].UpdateFunction = func;
        }
        public static void SetButtonLightWithTimeout(Color lightColor, int button, long durationMs = 500)
        {
            if (!_isEnabled)
            {
                return;
            }
            var func = _ledCommFuncs[button];
            func.SetColor(lightColor, durationMs);
            _ledDevices[button].UpdateFunction = func;
        }
        public static void SetButtonLightWithTimeout(Color lightColor, int button, TimeSpan duration)
        {
            if (!_isEnabled)
            {
                return;
            }
            var func = _ledCommFuncs[button];
            func.SetColor(lightColor, duration);
            _ledDevices[button].UpdateFunction = func;
        }

        public static void LinearTo(int button, Color from, Color to, long durationMs)
        {
            if (!_isEnabled)
            {
                return;
            }
            LinearTo(button, from, to, TimeSpan.FromMilliseconds(durationMs));
        }
        public static void LinearTo(int button, Color from, Color to, TimeSpan duration)
        {
            if (!_isEnabled)
            {
                return;
            }
            var func = _ledLinearFuncs[button];
            func.LinearTo(from, to, duration);
            _ledDevices[button].UpdateFunction = func;
        }

        public static void SetSineFunc(int button, Color color, long T_Ms, float phi = 0.5f)
        {
            if (!_isEnabled)
            {
                return;
            }
            SetSineFunc(button, color, TimeSpan.FromMilliseconds(T_Ms), phi);
        }
        public static void SetSineFunc(int button, Color color, TimeSpan T, float phi = 0.5f)
        {
            if (!_isEnabled)
            {
                return;
            }
            var func = _ledSineFuncs[button];
            func.SetSineFunc(color, T, phi);
            _ledDevices[button].UpdateFunction = func;
        }

        public static void SetUpdateFunc(int button, ILedUpdateFunction func)
        {
            if (!_isEnabled)
            {
                return;
            }
            if(func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            _ledDevices[button].UpdateFunction = func;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        class Led
        {
            public int Index { get; init; } = 0;
            public Color Color
            {
                get
                {
                    return _updateFunction.Current;
                }
            }
            public ILedUpdateFunction UpdateFunction
            {
                get
                {
                    return _updateFunction;
                }
                set
                {
                    _updateFunction = value;
                }
            }

            ILedUpdateFunction _updateFunction;

            public Led(ILedUpdateFunction updateFunction)
            {
                if(updateFunction is null)
                {
                    throw new ArgumentNullException(nameof(updateFunction));
                }
                _updateFunction = updateFunction;
            }
        }
        class LedCommonUpdateFunction: ILedUpdateFunction
        {
            public Color Current
            {
                get
                {
                    return _currentColor;
                }
            }
            Color _defaultColor = Color.white;
            Color _targetColor = Color.white;

            Color _currentColor = Color.white;

            float _durationMs = 0f;
            float _elapsedMs = 0f;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update(float deltaMs)
            {
                if (_elapsedMs >= _durationMs)
                {
                    _currentColor = _defaultColor;
                    return;
                }
                _elapsedMs += deltaMs;
                _currentColor = _targetColor;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _elapsedMs = 0f;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetColor(Color newColor)
            {
                _defaultColor = newColor;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetColor(Color newColor, long durationMs)
            {
                SetColor(newColor, TimeSpan.FromMilliseconds(durationMs));
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetColor(Color newColor, TimeSpan duration)
            {
                _targetColor = newColor;
                _durationMs = (float)duration.TotalMilliseconds;
            }
        }
        class LedLinearUpdateFunction : ILedUpdateFunction
        {
            public Color Current
            {
                get
                {
                    return _currentColor;
                }
            }
            Color _from = Color.white;
            Color _to = Color.white;

            Color _currentColor = Color.white;

            float _durationMs = 0f;
            float _elapsedMs = 0f;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update(float deltaMs)
            {
                if (_elapsedMs >= _durationMs)
                {
                    _currentColor = _to;
                    return;
                }
                var t = _elapsedMs / _durationMs;
                _currentColor = Color.Lerp(_from, _to, t);
                _elapsedMs += deltaMs;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _elapsedMs = 0f;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void LinearTo(Color from, Color to, long durationMs)
            {
                LinearTo(from, to, TimeSpan.FromMilliseconds(durationMs));
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void LinearTo(Color from, Color to, TimeSpan duration)
            {
                _from = from;
                _to = to;
                _durationMs = (float)duration.TotalMilliseconds;
            }
        }
        class LedSineUpdateFunction : ILedUpdateFunction
        {
            public Color Current
            {
                get
                {
                    return _currentColor;
                }
            }
            Color _defaultColor = Color.white;
            Color _currentColor = Color.white;

            float _T_Ms = 0f;
            float _elapsedMs = 0f;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update(float deltaMs)
            {
                if (_elapsedMs >= _T_Ms)
                {
                    _elapsedMs = _elapsedMs % _T_Ms;
                }
                var a = Mathf.Abs(Mathf.Sin((_elapsedMs / _T_Ms) * Mathf.PI));
                _currentColor = Color.Lerp(Color.black, _defaultColor, a);
                _elapsedMs += deltaMs;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _elapsedMs = 0f;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetSineFunc(Color color, long T_Ms, float phi = 0.5f)
            {
                SetSineFunc(color, TimeSpan.FromMilliseconds(T_Ms), phi);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetSineFunc(Color color, TimeSpan T, float phi = 0.5f)
            {
                if(phi > 1f || phi < 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(phi), "phi must be in [0, 1]");
                }
                if(T.TotalMilliseconds <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(T), "T must be greater than 0");
                }
                _defaultColor = color;
                _T_Ms = (float)T.TotalMilliseconds;
                _elapsedMs = _T_Ms * phi;
            }
        }
    }
}