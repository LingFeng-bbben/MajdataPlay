using MajdataPlay.Diagnostics;
using MajdataPlay.Editor;
using MajdataPlay.IO;
using MajdataPlay.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    [DefaultExecutionOrder(175)]
    public class RectCornerPositionUpdater: MonoBehaviour
    {
        const int FRAME_DELAY = 6;
        // 脏检测（避免每帧重复计算）
        [SerializeField]
        [ReadOnlyField]
        float _lastMainDisplayOffset = float.NaN;       // 上一帧的主屏幕偏移量
        [SerializeField]
        [ReadOnlyField]
        float _lastMainDisplayScale = float.NaN;        // 上一帧的主屏幕缩放
        [SerializeField]
        [ReadOnlyField]
        float _lastSubDisplayOffset = float.NaN; // 上一帧的副屏幕偏移量
        [SerializeField]
        [ReadOnlyField]
        float _lastSubDisplayScale = float.NaN; // 上一帧的副屏幕偏移量
        [SerializeField]
        [ReadOnlyField]
        bool _lastTransformDisplay;          // 上一帧 MainScreenTransform 开关的状态
        [SerializeField]
        [ReadOnlyField]
        int _frameCount = 0;

        DisplayOptions? _displayOptions;
        RectTransform _rectTransform;


        [SerializeField]
        [ReadOnlyField]
        readonly Vector3[] _worldCorners = new Vector3[4];

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
        void Update()
        {
            if (_frameCount < FRAME_DELAY)
            {
                _frameCount++;
                return;
            }
            var displayOptions = _displayOptions ??= MajEnv.Settings?.Display;
            if (displayOptions is null)
            {
                return;
            }
            if (_frameCount == FRAME_DELAY)
            {
                _frameCount++;
                RefreshCornersPosition();
                return;
            }
            var transformDisplay = displayOptions.MainScreenTransform;
            var subDisplayOffset = displayOptions.SubDisplayOffset;
            var subDisplayScale = displayOptions.SubDisplayScale;
            var screenOffset = displayOptions.MainScreenOffset;
            var screenScale = displayOptions.MainScreenScale;
            var subChanged = subDisplayOffset != _lastSubDisplayOffset || subDisplayScale != _lastSubDisplayScale;
            var mainChanged = screenOffset != _lastMainDisplayOffset || screenScale != _lastMainDisplayScale;
            var isAnyUpdated = transformDisplay != _lastTransformDisplay || subChanged || mainChanged;

            if(isAnyUpdated)
            {
                _lastSubDisplayOffset = subDisplayOffset;
                _lastSubDisplayScale = subDisplayScale;
                _lastMainDisplayOffset = screenOffset;
                _lastMainDisplayScale = screenScale;
                _lastTransformDisplay = transformDisplay;
                _frameCount = FRAME_DELAY;
            }
        }
        void RefreshCornersPosition()
        {
            _rectTransform.GetWorldCorners(_worldCorners);
            var edge = new Vector4();
            edge.x = _worldCorners[0].x; // left
            edge.y = _worldCorners[1].y; // top
            edge.z = _worldCorners[2].x; // right
            edge.w = _worldCorners[3].y; // bottom
            InputManager.SubScreenEdge = edge;
            MajDebug.LogDebug(edge);
        }
    }
}
