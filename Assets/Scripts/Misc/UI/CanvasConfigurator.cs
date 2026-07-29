using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.UI
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasConfigurator : MonoBehaviour
    {
        Canvas _canvas;
        SceneSwitcher _sceneSwitcher;

        void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _sceneSwitcher = MajInstances.SceneSwitcher;
            _canvas.worldCamera = _sceneSwitcher.MainCamera;
            SceneSwitcher.OnSceneChanged += OnSceneChanged;
        }

        void OnDestroy()
        {
            SceneSwitcher.OnSceneChanged -= OnSceneChanged;
        }

        void OnSceneChanged(object? sender, (MajScenes NewScene, MajScenes OldScene) args)
        {
            _canvas.worldCamera = _sceneSwitcher.MainCamera;
        }
    }
}
