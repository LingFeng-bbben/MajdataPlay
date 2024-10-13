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
        List<float> _data = new();
        TextMeshPro _textDisplayer;
        GameSetting _setting;
        
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
            _data.Add(delta);
            var count = _data.Count;
            if (count > 150)
                _data = _data.Skip(count - 150).ToList();
            if (_frameTimer <= 0)
            {
                _textDisplayer.enabled = _setting.Debug.DisplayFPS;
                var newColor = new Color(1.0f - BgColor.r, 1.0f - BgColor.g, 1.0f - BgColor.b);
                var fpsDelta = _data.Sum() / count;

                _textDisplayer.text = $"FPS\n{1 / fpsDelta:F2}";
                _textDisplayer.color = newColor;
                _frameTimer = 1;
            }
            else
                _frameTimer -= delta;
        }
    }
}