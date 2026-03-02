using MajdataPlay.Editor;
using MajdataPlay.Settings;
using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    public class ScreenPosCameraMover : MonoBehaviour
    {
        const int FLAG_NOT_INIT = 0;
        const int FLAG_INITED = 1;

        Camera _cam;
        int _flag = FLAG_NOT_INIT;
        //float _lastMainScreenPos = 1f;

        Transform _transform;
        DisplayOptions? _displayOptions;

        [SerializeField]
        [ReadOnlyField]
        float _baseY;
        [SerializeField]
        [ReadOnlyField]
        float _originalCameraY; // 保存场景的原始相机Y位置
        [SerializeField]
        [ReadOnlyField]
        float _baseOrthographicSize;
        [SerializeField]
        [ReadOnlyField]
        float _lastOffset = float.NaN;
        [SerializeField]
        [ReadOnlyField]
        float _lastScale = float.NaN;

        [SerializeField]
        [ReadOnlyField]
        float _lastMainScreenOffset;
        [SerializeField]
        [ReadOnlyField]
        float _lastMainScreenScale;
        [SerializeField]
        [ReadOnlyField]
        bool _lastTransformDisplay;
        
        
        void Awake()
        {
            _displayOptions = MajInstances.Settings?.Display;
            _transform = transform;
            _cam = GetComponent<Camera>();
            _originalCameraY = _transform.position.y; // 保存场景原始相机位置
            _baseY = 1.5f;
            //获取当前正交视图比例
            _baseOrthographicSize = _cam.orthographicSize;
        }
        void RestoreOriginal()
        {
            _transform.position = new Vector3(0, _originalCameraY, -10);
            if (_cam.orthographic)
            {
                _cam.orthographicSize = _baseOrthographicSize;
            }
        }

        void ApplyPosition(float offset, float scale)
        {
            _transform.position = new Vector3(0, _baseY + offset * 2.7f, -10);
            if (scale > 0 && _cam.orthographic)
            {
                _cam.orthographicSize = _baseOrthographicSize / scale;
            }
        }

        void ApplyTransform()
        {
            if (_lastTransformDisplay)
            {
                var offset = _lastMainScreenOffset;
                var scale = _lastMainScreenScale;
                _lastOffset = offset;
                _lastScale = scale;
                ApplyPosition(offset, scale);
            }
            else
            {
                RestoreOriginal();
            }
        }

        void Update()
        {
            switch (_flag)
            {
                case FLAG_NOT_INIT:
                    {
                        _displayOptions = MajInstances.Settings?.Display;
                        if (_displayOptions is null)
                        {
                            return;
                        }
                        _flag = FLAG_INITED;
                        
                        _lastTransformDisplay = _displayOptions.MainScreenTransform;
                        _lastMainScreenOffset = _displayOptions.MainScreenOffset;
                        _lastMainScreenScale = _displayOptions.MainScreenScale;
                        ApplyTransform();
                        //transform.position = new Vector3(0, 1.5f + 2.7f * (MajInstances.Settings?.Display.MainScreenPosition ?? 1f), -10); //Original


                        var aspectratio = (float)Screen.width / (float)Screen.height;


                        if (aspectratio < (9f / 18f))
                        {
                            _cam.rect = new Rect(0, 0.22f, 1, 1);
                        }
                        else if (aspectratio < (9f / 16f))
                        {
                            _cam.rect = new Rect(0, 0.12f, 1, 1);
                        }
                        else
                        {
                            _cam.rect = new Rect(0, 0, 1, 1);
                        }
                    }
                    goto case FLAG_INITED;
                case FLAG_INITED:
                    {
                        var transformDisplay = _lastTransformDisplay;
                        _lastTransformDisplay = _displayOptions!.MainScreenTransform;
                        _lastMainScreenOffset = _displayOptions!.MainScreenOffset;
                        _lastMainScreenScale = _displayOptions!.MainScreenScale;
                        //如果没开调整显示位置，就直接return
                        if (!_lastTransformDisplay)
                        {
                            if(transformDisplay)
                            {
                                _lastTransformDisplay = false;
                                _lastOffset = float.NaN;
                                _lastScale = float.NaN;
                                RestoreOriginal();
                            }
                            return;
                        }

                        //跑到这就是开了，标记开启了调整显示位置
                        //_lastTransformDisplay = true;

                        var screenOffset = _lastMainScreenOffset;
                        var screenScale = _lastMainScreenScale;

                        if (screenOffset == _lastOffset && screenScale == _lastScale)
                        {
                            return;
                        }
                        _lastOffset = screenOffset;
                        _lastScale = screenScale;

                        //变换位置
                        ApplyPosition(screenOffset, screenScale);
                    }
                    return;
            }
        }
    }
}
