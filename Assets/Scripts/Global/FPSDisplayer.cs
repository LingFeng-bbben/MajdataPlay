using Cysharp.Text;
using MajdataPlay.Utils;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    internal sealed class FPSDisplayer : MajSingleton
    {
        public static Color BgColor { get; set; } = new Color(0, 0, 0);

        float _frameTimer = 1;
        float?[] _data = new float?[150];
        int _index = 0;
        TextMeshPro _textDisplayer;
        GameSetting _setting;

        protected override void Awake()
        {
            base.Awake();
            for (var i = 0; i < _data.Length; i++)
                _data[i] = null;

            _textDisplayer = GetComponent<TextMeshPro>();
            _setting = MajInstances.Settings;
            _textDisplayer.enabled = _setting.Debug.DisplayFPS;
        }

        void LateUpdate()
        {
            var delta = Time.deltaTime;
            AddSample(delta);
            var count = Count();
            if (_frameTimer <= 0)
            {
                _textDisplayer.enabled = _setting.Debug.DisplayFPS;
                //var newColor = new Color(1.0f - BgColor.r, 1.0f - BgColor.g, 1.0f - BgColor.b);
                var fpsDelta = Sum() / count;

                _textDisplayer.text = ZString.Format("FPS {0:F2}", 1 / fpsDelta);
                //_textDisplayer.color = newColor;
                _frameTimer = 1;
            }
            else
                _frameTimer -= delta;
        }
        void AddSample(float data)
        {
            if (_index >= 150)
                _index = 0;
            _data[_index++] = data;
        }
        int Count()
        {
            var count = 0;
            for(var i = 0;i < _data.Length; i++)
            {

                if (_data[i] is not null)
                    count++;
            }
            return count;
        }
        float Sum()
        {
            var total = 0f;
            for (var i = 0; i < _data.Length; i++)
            {
                var sample = _data[i];
                if (sample is not null)
                    total += (float)sample;
            }
            return total;
        }
    }
}