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

        float frameTimer = 1;
        List<float> data = new();
        TextMeshPro textDisplayer;
        GameSetting _setting = MajInstanceHelper<GameSetting>.Instance!;
        
        void Start()
        {
            textDisplayer = GetComponent<TextMeshPro>();
            DontDestroyOnLoad(this);
            if (!_setting.Debug.DisplayFPS)
                gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            var delta = Time.deltaTime;
            data.Add(delta);
            var count = data.Count;
            if (count > 150)
                data = data.Skip(count - 150).ToList();
            if (frameTimer <= 0)
            {
                var newColor = new Color(1.0f - BgColor.r, 1.0f - BgColor.g, 1.0f - BgColor.b);
                var fpsDelta = data.Sum() / count;

                textDisplayer.text = $"FPS\n{1 / fpsDelta:F2}";
                textDisplayer.color = newColor;
                frameTimer = 1;
            }
            else
                frameTimer -= delta;
        }
    }
}