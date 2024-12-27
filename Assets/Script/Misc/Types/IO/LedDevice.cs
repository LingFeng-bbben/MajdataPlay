using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    internal class LedDevice
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
        public void SetColor(Color newColor,long durationMs)
        {
            SetColor(newColor,TimeSpan.FromMilliseconds(durationMs));
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
