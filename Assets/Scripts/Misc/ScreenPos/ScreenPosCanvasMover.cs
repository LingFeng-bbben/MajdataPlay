using MajdataPlay.Numerics;
using MajdataPlay.Settings;
using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay
{
    public class ScreenPosCanvasMover : MonoBehaviour
    {
        RectTransform rt;


        float _basePosY;
        float _originalPosY; // 保存预制体的原始Y位置
        RectTransform? _parentRt;
        float _lastOffset = float.NaN;
        float _lastScale = float.NaN;
        float _cachedScreenCenterY;
        bool _lastTransformDisplay;

        CanvasScaler? _canvasScaler;
        static Vector2? _originalReferenceResolution;
        Vector2 _baseReferenceResolution;

        // SceneSwitcher的持久化Canvas，过渡动画需要同步缩放和位置
        CanvasScaler? _persistentCanvasScaler;
        RectTransform? _persistentMainDisplay;
        
        
        GameObject? _subCover;
        Transform? _subCoverTransform;
        RectTransform? _subCoverRectTransform;

        GameObject? _subCoverBottom;
        Transform? _subCoverBottomTransform;
        RectTransform? _subCoverBottomRectTransform;

        const float MAIN_DISPLAY_POS_Y = 540;
        const float SUB_COVER_HEIGHT = 390;
        const float SUB_COVER_WIDTH = 1080;
        const float SUB_COVER_POS_Y = 315;

        const float SUB_COVER_BOTTOM_HEIGHT = 0;
        const float SUB_COVER_BOTTOM_WIDTH = 1080;
        const float SUB_COVER_BOTTOM_POS_Y = -960;

        //const int FLAG_NOT_INIT = 0;
        //const int FLAG_INITED = 1;

        //int _flag = FLAG_NOT_INIT;
        //float _lastMainScreenPos = 1f;
        DisplayOptions? _displayOptions;

        // Start is called before the first frame update
        void Awake()
        {
            rt = GetComponent<RectTransform>();

            // 保存预制体的原始Y位置，用于关闭MainScreenTransform时恢复
            _originalPosY = rt.anchoredPosition.y;

            _displayOptions = MajInstances.Settings?.Display;


            _parentRt = transform.parent as RectTransform;
            // 先查找CanvasScaler，再计算屏幕中心
            var rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
            if (rootCanvas != null)
            {
                _canvasScaler = rootCanvas.GetComponent<CanvasScaler>();
                if (_canvasScaler != null)
                {
                    // 只在第一次记录真正的原始分辨率，避免场景切换时读到被修改过的值
                    if (_originalReferenceResolution == null)
                        _originalReferenceResolution = _canvasScaler.referenceResolution;
                    _baseReferenceResolution = _originalReferenceResolution.Value;
                }
            }

            // 从Screen尺寸直接计算未缩放Canvas中心（Awake中rect可能尚未初始化）
            // Expand模式: scaleFactor = Min(screenW/refW, screenH/refH)
            if (_canvasScaler != null)
            {
                float baseScaleFactor = Mathf.Min(
                    Screen.width / _baseReferenceResolution.x,
                    Screen.height / _baseReferenceResolution.y
                );
                _cachedScreenCenterY = Screen.height / baseScaleFactor / 2f;
            }
            else if (_parentRt != null && _parentRt.rect.height > 0)
            {
                _cachedScreenCenterY = _parentRt.rect.height / 2f;
            }
            else
            {
                _cachedScreenCenterY = _displayOptions!.MainScreenCachedScreenCenterY;
            }
            _displayOptions!.MainScreenCachedScreenCenterY = _cachedScreenCenterY;

            // 查找SceneSwitcher的持久化Canvas（DontDestroyOnLoad），用于同步过渡动画
            foreach (var scaler in FindObjectsOfType<CanvasScaler>(true))
            {
                if (scaler != _canvasScaler && scaler != null)
                {
                    var canvas = scaler.GetComponent<Canvas>();
                    if (canvas != null && canvas.isRootCanvas)
                    {
                        _persistentCanvasScaler = scaler;
                        var mainDisplay = scaler.transform.Find("Main_Display");
                        if (mainDisplay != null)
                            _persistentMainDisplay = mainDisplay.GetComponent<RectTransform>();
                        break;
                    }
                }
            }

            _basePosY = 810f;
            rt.anchoredPosition = new Vector2(0, _basePosY);

            
            ApplyTransform();
            
            
            var sub = transform.parent.Find("Sub_Cover");
            if (sub != null)
            {
                _subCover = sub.gameObject;
                _subCoverTransform = sub;
                _subCoverRectTransform = sub.GetComponent<RectTransform>();
            }
            var subBottom = transform.parent.Find("Sub_Cover_Bottom");
            if (subBottom != null)
            {
                _subCoverBottom = subBottom.gameObject;
                _subCoverBottomTransform = subBottom;
                _subCoverBottomRectTransform = subBottom.GetComponent<RectTransform>();
            }
        }
        
        void RestoreOriginal()
        {
            // 恢复到预制体的原始状态
            if (_canvasScaler != null)
                _canvasScaler.referenceResolution = _baseReferenceResolution;
            rt.anchoredPosition = new Vector2(0, _originalPosY);
            rt.localScale = Vector3.one;
            // 同步恢复持久化Canvas
            if (_persistentCanvasScaler != null)
                _persistentCanvasScaler.referenceResolution = _baseReferenceResolution;
            if (_persistentMainDisplay != null)
                _persistentMainDisplay.anchoredPosition = new Vector2(0, _originalPosY);
        }

        void ApplyPosition(float offset, float scale, bool updateCache = false)
        {
            if (updateCache && _canvasScaler == null)
            {
                // localScale路径：parent rect高度不受我们代码影响，可以安全读取
                // CanvasScaler路径：屏幕中心已在Awake中从Screen尺寸计算，运行时不更新
                // （因为referenceResolution的修改时机与rect.height读取不同步，会算出错误值）
                float parentHeight = _parentRt != null ? _parentRt.rect.height : 0f;
                if (parentHeight > 0)
                {
                    float realCenter = parentHeight / 2f;
                    if (realCenter != _cachedScreenCenterY)
                    {
                        _cachedScreenCenterY = realCenter;
                        _displayOptions!.MainScreenCachedScreenCenterY = realCenter;
                    }
                }
            }

            if (_canvasScaler != null && scale > 0)
            {
                // 通过缩小CanvasScaler的referenceResolution来实现放大效果
                var newRef = new Vector2(
                    _baseReferenceResolution.x / scale,
                    _baseReferenceResolution.y / scale
                );
                _canvasScaler.referenceResolution = newRef;
                float posY = (_basePosY - offset * 270f) + _cachedScreenCenterY * (1f / scale - 1f);
                rt.anchoredPosition = new Vector2(0, posY);
                rt.localScale = Vector3.one;
                // 同步持久化Canvas（SceneSwitcher过渡动画）
                if (_persistentCanvasScaler != null)
                    _persistentCanvasScaler.referenceResolution = newRef;
                if (_persistentMainDisplay != null)
                    _persistentMainDisplay.anchoredPosition = new Vector2(0, posY);
            }
            else
            {
                // 回退到localScale方式（无CanvasScaler时）
                float posYBase = _basePosY - offset * 270f;
                float scaleCorrection = (_cachedScreenCenterY - posYBase) * (1f - scale);
                rt.anchoredPosition = new Vector2(0, posYBase + scaleCorrection);
                if (scale > 0)
                    rt.localScale = new Vector3(scale, scale, 1f);
            }
        }

        void ApplyTransform()
        {
            var initTransform = _displayOptions!.MainScreenTransform;
            _lastTransformDisplay = initTransform;
            if (initTransform)
            {
                var offset = _displayOptions!.MainScreenPosition;
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
            var transformDisplay = _displayOptions!.MainScreenTransform;

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
            _lastTransformDisplay = true;

            var screenOffset = _displayOptions!.MainScreenPosition;
            var screenScale = _displayOptions!.MainScreenScale;


            bool mainChanged = screenOffset != _lastOffset || screenScale != _lastScale;
            
            if (!mainChanged) return;

            if (mainChanged)
            {
                _lastOffset = screenOffset;
                _lastScale = screenScale;
                ApplyPosition(screenOffset, screenScale, updateCache: true);
            }
            
            /*
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
                    var posY = 810f - 270f * pos;
                    rt.anchoredPosition = new Vector2(0, posY);
                    if (pos != 1f)
                    {
                        if (_subCover is not null)
                        {
                            var diffY = MAIN_DISPLAY_POS_Y - posY;
                            var newSubCoverHeight = (SUB_COVER_HEIGHT + diffY).Clamp(0, float.MaxValue);
                            var newSubCoverPosY = (SUB_COVER_POS_Y - diffY / 2f).Clamp(0, float.MaxValue);

                            var originPos = _subCoverRectTransform!.anchoredPosition;
                            _subCoverRectTransform!.anchoredPosition = new Vector2(originPos.x, newSubCoverPosY);
                            _subCoverRectTransform!.sizeDelta = new Vector2(SUB_COVER_WIDTH, newSubCoverHeight);
                        }
                        if (_subCoverBottom is not null)
                        {
                            var diffY = MAIN_DISPLAY_POS_Y - posY;
                            var newSubCoverBottomHeight = (SUB_COVER_BOTTOM_HEIGHT - diffY).Clamp(0, float.MaxValue);
                            var newSubCoverBottomPosY = (SUB_COVER_BOTTOM_POS_Y - diffY / 2f).Clamp(float.MinValue, 0);
                            var originPos = _subCoverBottomRectTransform!.anchoredPosition;
                            _subCoverBottomRectTransform!.anchoredPosition = new Vector2(originPos.x, newSubCoverBottomPosY);
                            _subCoverBottomRectTransform!.sizeDelta = new Vector2(SUB_COVER_BOTTOM_WIDTH, newSubCoverBottomHeight);
                        }
                    }
                    return;
            }
            */
        }
        
        void OnDestroy()
        {
            // 不再还原CanvasScaler：
            // 1. 每个场景有自己的CanvasTemplate实例，销毁后CanvasScaler跟着消失
            // 2. 还原会导致过渡动画期间画面闪回默认位置/缩放
            // 3. _originalReferenceResolution是static的，新场景始终能读到正确的原始值
        }
        
    }
}
