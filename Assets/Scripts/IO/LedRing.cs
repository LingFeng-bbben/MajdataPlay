using MajdataPlay.Utils;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
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
                return _ledColors.Span;
            }
        }
        
        readonly static Memory<Color> _ledColors = new Color[8];
        readonly static ReadOnlyMemory<Led> _ledDevices = Array.Empty<Led>();
        readonly static bool _isEnabled = true;

        static LedRing()
        {
            _isEnabled = MajInstances.Settings.IO.OutputDevice.Led.Enable;
            var ledDevices = new Led[8];
            for (var i = 0; i < 8; i++)
            {
                ledDevices[i] = new()
                {
                    Index = i,
                };
            }
            if (!_isEnabled)
            {
                for (var i = 0; i < 8; i++)
                {
                    ledDevices[i].SetColor(Color.black);
                }
            }

            _ledDevices = ledDevices;
        }

        internal static void OnPreUpdate()
        {
            var ledDevices = _ledDevices.Span;
            var ledColors = _ledColors.Span;
            for (var i = 0; i < 8; i++)
            {
                ledColors[i] = ledDevices[i].Color;
            }
        }
        public static void SetAllLight(Color lightColor)
        {
            if (!_isEnabled)
                return;
            foreach (var device in _ledDevices.Span)
            {
                device!.SetColor(lightColor);
            }
        }
        public static void SetButtonLight(Color lightColor, int button)
        {
            if (!_isEnabled)
                return;
            _ledDevices.Span[button].SetColor(lightColor);
        }
        public static void SetButtonLightWithTimeout(Color lightColor, int button, long durationMs = 500)
        {
            if (!_isEnabled)
                return;
            _ledDevices.Span[button].SetColor(lightColor, durationMs);
        }
        public static void SetButtonLightWithTimeout(Color lightColor, int button, TimeSpan duration)
        {
            if (!_isEnabled)
                return;
            _ledDevices.Span[button].SetColor(lightColor, duration);
        }
        
        class Led
        {
            public int Index { get; init; } = 0;
            public Color Color
            {
                get
                {
                    if (_expTime is null)
                        return _color;

                    var now = DateTime.Now;
                    var expTime = (DateTime)_expTime;
                    if (now > expTime)
                        return _color;
                    else
                        return _immediateColor;
                }
            }

            DateTime? _expTime = null;
            Color _color = Color.white;
            Color _immediateColor = Color.white;

            public void SetColor(Color newColor)
            {
                _color = newColor;
                _expTime = null;
            }
            public void SetColor(Color newColor, long durationMs)
            {
                SetColor(newColor, TimeSpan.FromMilliseconds(durationMs));
            }
            public void SetColor(Color newColor, TimeSpan duration)
            {
                var now = DateTime.Now;
                var exp = now + duration;
                _immediateColor = newColor;
                _expTime = exp;
            }
        }
    }
}