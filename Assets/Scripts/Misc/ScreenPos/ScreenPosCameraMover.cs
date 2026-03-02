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
        
        float _baseY;
        float _originalCameraY; // 保存场景的原始相机Y位置
        float _baseOrthographicSize;
        float _lastOffset = float.NaN;
        float _lastScale = float.NaN;
        bool _lastTransformDisplay;
        
        
        void Awake()
        {
            _displayOptions = MajInstances.Settings?.Display;
            _transform = transform;
            _cam = GetComponent<Camera>();
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
            var initTransform = _displayOptions!.MainScreenTransform;
            _lastTransformDisplay = initTransform;
            if (initTransform)
            {
                var offset = _displayOptions!.MainScreenOffset;
                var scale = _displayOptions!.MainScreenScale;
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
                        if (_displayOptions is null)
                        {
                            _displayOptions = MajInstances.Settings?.Display;
                            if (_displayOptions is null)
                            {
                                return;
                            }
                            _flag = FLAG_INITED;
                            _originalCameraY = transform.position.y; // 保存场景原始相机位置
                            _baseY = 1.5f;
                            //获取当前正交视图比例
                            _baseOrthographicSize = _cam.orthographicSize;
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
                            goto case FLAG_INITED;
                        }
                    }
                    return;
                case FLAG_INITED:
                    {
                        var transformDisplay = _displayOptions!.MainScreenTransform;

                        //如果没开调整显示位置，就直接return
                        if (!transformDisplay)
                        {
                            if (_lastTransformDisplay)
                            {
                                _lastTransformDisplay = false;
                                _lastOffset = float.NaN;
                                _lastScale = float.NaN;
                                RestoreOriginal();
                            }
                            return;
                        }

                        //跑到这就是开了，标记开启了调整显示位置
                        _lastTransformDisplay = true;

                        var screenOffset = _displayOptions.MainScreenOffset;
                        var screenScale = _displayOptions.MainScreenScale;

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
