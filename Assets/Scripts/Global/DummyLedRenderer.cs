using MajdataPlay.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay
{
    internal sealed class DummyLedRenderer: MajSingleton
    {
        SpriteRenderer[] _dummyLights = Array.Empty<SpriteRenderer>();
        protected override void Awake()
        {
            base.Awake();
            _dummyLights = GameObject.GetComponentsInChildren<SpriteRenderer>();
        }
        internal void OnPreUpdate()
        {
            var ledColors = LightManager.LedColors;
            for (var i = 0; i < ledColors.Length; i++)
            {
                _dummyLights[i].color = ledColors[i];
            }
        }
    }
}
