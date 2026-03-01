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

        Camera cam;
        int _flag = FLAG_NOT_INIT;
        float _lastMainScreenPos = 1f;

        Transform _transform;
        DisplayOptions? _displayOptions;

        void Awake()
        {
            _transform = transform;
        }
        void Start()
        {
#if UNITY_ANDROID || UNITY_IOS
            cam = GetComponent<Camera>();
            var aspectratio = (float)Screen.width / (float)Screen.height;


            if (aspectratio < (9f / 18f))
            {
                cam.rect = new Rect(0, 0.22f, 1, 1);
            }
            else if (aspectratio < (9f / 16f))
            {
                cam.rect = new Rect(0, 0.12f, 1, 1);
            }
            else
            {
                cam.rect = new Rect(0, 0, 1, 1);
            }
#endif
        }
        void Update()
        {
            switch (_flag)
            {
                case FLAG_NOT_INIT:
                    if (_displayOptions is null)
                    {
                        _displayOptions = MajInstances.Settings?.Display;
                        if (_displayOptions is null)
                        {
                            return;
                        }
                        _flag = FLAG_INITED;
                        goto case FLAG_INITED;
                    }
                    return;
                case FLAG_INITED:
                    var pos = _displayOptions!.MainScreenPosition;
                    if (_lastMainScreenPos == pos)
                    {
                        return;
                    }
                    _lastMainScreenPos = pos;
                    _transform.position = new Vector3(0, 1.5f + 2.7f * pos, -10);
                    return;
            }
        }
    }
}
