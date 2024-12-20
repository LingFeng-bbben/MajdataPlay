using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    public class FPSDisplayer : MonoBehaviour
    {
        public static Color BgColor { get; set; } = new Color(0, 0, 0);

        float _frameTimer = 1;
        float?[] _data = new float?[150];
        int _index = 0;
        TextMeshPro _textDisplayer;
        GameSetting _setting;

        private void Awake()
        {
            for (var i = 0; i < _data.Length; i++)
                _data[i] = null;
        }
        void Start()
        {
            _textDisplayer = GetComponent<TextMeshPro>();
            DontDestroyOnLoad(this);
            _setting = MajInstances.Setting;
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

                _textDisplayer.text = $"FPS {1 / fpsDelta:F2}";
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