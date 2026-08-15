using MajdataPlay.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay
{
    internal sealed class DummyLedRenderer: MajComponent
    {
        SpriteRenderer[] _dummyLights = Array.Empty<SpriteRenderer>();

        readonly static Color[] _ledRingColors = new Color[8];
        protected override void Awake()
        {
            base.Awake();
            Majdata<DummyLedRenderer>.SetAsSingleton(this);
            _dummyLights = GameObject.GetComponentsInChildren<SpriteRenderer>();
        }
        public static void SetLedRingColorData(ReadOnlySpan<Color> colors)
        {
            colors.CopyTo(_ledRingColors);
        }
        internal void OnLateUpdate()
        {
            var ledColors = _ledRingColors.AsSpan();
            for (var i = 0; i < ledColors.Length; i++)
            {
                _dummyLights[i].color = ledColors[i];
            }
        }
    }
}
