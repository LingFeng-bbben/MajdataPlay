using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class FPSDisplayer : MonoBehaviour
{
    public static Color BgColor { get; set; } = new Color(0,0,0);
    List<float> data = new();
    TextMeshPro textDisplayer;
    long frameCount = 0;
    void Start()
    {
        textDisplayer = GetComponent<TextMeshPro>();
        DontDestroyOnLoad(this);
        if (!GameManager.Instance.Setting.Debug.DisplayFPS)
            gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        var count = data.Count;
        data.Add(Time.deltaTime);

        if (count > 150)
            data = data.Skip(count - 150).ToList();
        if(frameCount % 60 == 0)
        {
            var newColor = new Color(1.0f - BgColor.r, 1.0f - BgColor.g, 1.0f - BgColor.b);
            var delta = data.Sum() / count;
            textDisplayer.text = $"FPS\n{1 / delta:F2}";
            textDisplayer.color = newColor;
        }
        frameCount++;
    }
}
