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
        const int FLAG_NOT_INIT = 0;
        const int FLAG_INITED = 1;

        const float MAIN_DISPLAY_POS_Y = 540;
        const float SUB_COVER_HEIGHT = 390;
        const float SUB_COVER_WIDTH = 1080;
        const float SUB_COVER_POS_Y = 315;

        const float SUB_DISPLAY_ORIGINAL_POS_Y = 735f;
        const float SUB_DISPLAY_HEIGHT = 450f;
        const float MAIN_DISPLAY_HEIGHT = 1080f;
        const float SUB_COVER_BOTTOM_POS_Y = -960;

        int _flag = FLAG_NOT_INIT;

        Transform _transform;
        RectTransform _rt;
        
        // 主屏幕 Y 轴的"零偏移基准位置"（Canvas 坐标系），偏移量以此为基础叠加
        float _basePosY;
        // 预制体自带的原始 Y 位置，关闭 MainScreenTransform 功能时用于完整还原
        float _originalPosY;
        // 父节点的 RectTransform，用于在无 CanvasScaler 时读取父容器高度以计算屏幕中心
        RectTransform? _parentRt;

        // 脏检测（避免每帧重复计算）
        float _lastOffset = float.NaN;       // 上一帧的主屏幕偏移量
        float _lastScale = float.NaN;        // 上一帧的主屏幕缩放
        float _lastSubDisplayOffset = float.NaN; // 上一帧的副屏幕偏移量
        bool _lastTransformDisplay;          // 上一帧 MainScreenTransform 开关的状态

        // 屏幕中心缓存，缩放时需要以屏幕中心为锚点补偿位移
        float _cachedScreenCenterY;
        CanvasScaler? _canvasScaler;
        // 跨场景保存真正的原始分辨率
        static Vector2? _originalReferenceResolution;
        // 当前场景启动时记录的基准分辨率
        Vector2 _baseReferenceResolution;

        // 持久化 Canvas 同步
        // SceneSwitcher 的 DontDestroyOnLoad Canvas 负责场景切换过渡动画，
        // 必须与当前场景的 Canvas 保持相同的缩放和位置，否则过渡时画面会跳变
        CanvasScaler? _persistentCanvasScaler;
        RectTransform? _persistentMainDisplay;

        // 副屏遮罩（Sub_Cover）
        GameObject _subCover;
        Transform _subCoverTransform;
        RectTransform _subCoverRectTransform;
        
        GameObject _subCoverBottom;
        Transform _subCoverBottomTransform;
        RectTransform _subCoverBottomRectTransform;

        //副屏幕（Sub_Display）

        RectTransform _subDisplay;

        DisplayOptions? _displayOptions;


        void Awake()
        {
            _displayOptions = MajInstances.Settings?.Display;
            _rt = GetComponent<RectTransform>();
            _transform = transform;
            _parentRt = _transform.parent as RectTransform;
            // 保存预制体的原始Y位置，用于关闭MainScreenTransform时恢复
            _originalPosY = _rt.anchoredPosition.y;

            _parentRt = _transform.parent as RectTransform;
            // 先查找CanvasScaler，再计算屏幕中心
            var rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
            if (rootCanvas is not null)
            {
                _canvasScaler = rootCanvas.GetComponent<CanvasScaler>();
                if (_canvasScaler is not null)
                {
                    // 只在第一次记录真正的原始分辨率，避免场景切换时读到被修改过的值
                    if (_originalReferenceResolution is null)
                    {
                        _originalReferenceResolution = _canvasScaler.referenceResolution;
                    }
                    _baseReferenceResolution = (Vector2)_originalReferenceResolution;
                }
            }
            // 从Screen尺寸直接计算未缩放Canvas中心（Awake中rect可能尚未初始化）
            if (_canvasScaler is not null)
            {
                var baseScaleFactor = Mathf.Min(
                    Screen.width / _baseReferenceResolution.x,
                    Screen.height / _baseReferenceResolution.y
                );
                _cachedScreenCenterY = Screen.height / baseScaleFactor / 2f;
            }
            else if (_parentRt is not null && _parentRt.rect.height > 0)
            {
                _cachedScreenCenterY = _parentRt.rect.height / 2f;
            }
            else
            {
                _cachedScreenCenterY = _displayOptions?.MainScreenCachedScreenCenterY ?? 960f;
            }
            if (_displayOptions != null)
            {
                _displayOptions.MainScreenCachedScreenCenterY = _cachedScreenCenterY;
            }

            // 查找SceneSwitcher的持久化Canvas，用于同步过渡动画
            foreach (var scaler in FindObjectsOfType<CanvasScaler>(true))
            {
                if (scaler is not null && scaler != _canvasScaler)
                {
                    var canvas = scaler.GetComponent<Canvas>();
                    if (canvas is not null && canvas.isRootCanvas)
                    {
                        _persistentCanvasScaler = scaler;
                        var mainDisplay = scaler.transform.Find("Main_Display");
                        if (mainDisplay is not null)
                        {
                            _persistentMainDisplay = mainDisplay.GetComponent<RectTransform>();
                        }
                        break;
                    }
                }
            }
            _basePosY = 810f;
            _rt.anchoredPosition = new Vector2(0, _basePosY);

            var sub = _transform.parent.Find("Sub_Cover");
            if (sub is not null)
            {
                _subCover = sub.gameObject;
                _subCoverTransform = sub;
                _subCoverRectTransform = sub.GetComponent<RectTransform>();
            }
            var subBottom = _transform.parent.Find("Sub_Cover_Bottom");
            if (subBottom is not null)
            {
                _subCoverBottom = subBottom.gameObject;
                _subCoverBottomTransform = subBottom;
                _subCoverBottomRectTransform = subBottom.GetComponent<RectTransform>();
            }
            var subDisplay = _transform.parent.Find("Sub_Display");
            if (subDisplay is not null)
            {
                _subDisplay = subDisplay.GetComponent<RectTransform>();
            }
        }
        
        void RestoreOriginal()
        {
            // 恢复到预制体的原始状态
            if (_canvasScaler != null)
            {
                _canvasScaler.referenceResolution = _baseReferenceResolution;
            }
            _rt.anchoredPosition = new Vector2(0, _originalPosY);
            _rt.localScale = Vector3.one;
            // 同步恢复持久化Canvas
            if (_persistentCanvasScaler != null)
            {
                _persistentCanvasScaler.referenceResolution = _baseReferenceResolution;
            }
            if (_persistentMainDisplay != null)
            {
                _persistentMainDisplay.anchoredPosition = new Vector2(0, _originalPosY);
            }
            // 恢复Sub_Display位置
            if (_subDisplay != null)
            {
                _subDisplay.anchoredPosition = new Vector2(_subDisplay.anchoredPosition.x, SUB_DISPLAY_ORIGINAL_POS_Y);
            }
            // 恢复Sub_Cover位置和大小
            if (_subCoverRectTransform != null)
            {
                _subCoverRectTransform.anchoredPosition = new Vector2(0, SUB_COVER_POS_Y);
                _subCoverRectTransform.sizeDelta = new Vector2(SUB_COVER_WIDTH, SUB_COVER_HEIGHT);
            }
        }

        void ApplyPosition(float offset, float scale, bool updateCache = false)
        {
            if (updateCache && _canvasScaler == null)
            {
                // localScale路径：parent rect高度不受我们代码影响，可以安全读取
                // CanvasScaler路径：屏幕中心已在Awake中从Screen尺寸计算，运行时不更新
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
                _rt.anchoredPosition = new Vector2(0, posY);
                _rt.localScale = Vector3.one;
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
                _rt.anchoredPosition = new Vector2(0, posYBase + scaleCorrection);
                if (scale > 0)
                    _rt.localScale = new Vector3(scale, scale, 1f);
            }
        }

        void ApplySubDisplayOffset(float offset)
        {
            if (_subDisplay == null)
            {
                return;
            }
            float newY = SUB_DISPLAY_ORIGINAL_POS_Y + offset;
            _subDisplay.anchoredPosition = new Vector2(_subDisplay.anchoredPosition.x, newY);
        }

        void UpdateSubCover()
        {
            // Sub_Display底边 (pivot 0.5,0.5)
            float subDisplayBottom = _subDisplay.anchoredPosition.y - SUB_DISPLAY_HEIGHT / 2f;
            // Main_Display顶边 (pivot 0.5,0.5)
            float mainDisplayTop = _rt.anchoredPosition.y + MAIN_DISPLAY_HEIGHT / 2f;

            float coverHeight = Mathf.Max(0f, subDisplayBottom - mainDisplayTop);
            float coverCenterY = (subDisplayBottom + mainDisplayTop) / 2f;

            _subCoverRectTransform.anchoredPosition = new Vector2(0, coverCenterY);
            _subCoverRectTransform.sizeDelta = new Vector2(SUB_COVER_WIDTH, coverHeight);
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

                var subOffset = _displayOptions.SubDisplayOffset;
                _lastSubDisplayOffset = subOffset;
                ApplySubDisplayOffset(subOffset * 100f);

                UpdateSubCover();
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
                        ApplyTransform();
                    }
                    goto case FLAG_INITED;
                case FLAG_INITED:
                    {
                        var transformDisplay = _displayOptions!.MainScreenTransform;

                        if (!transformDisplay)
                        {
                            if (_lastTransformDisplay)
                            {
                                _lastTransformDisplay = false;
                                _lastOffset = float.NaN;
                                _lastScale = float.NaN;
                                _lastSubDisplayOffset = float.NaN;
                                RestoreOriginal();
                            }
                            return;
                        }
                        _lastTransformDisplay = true;

                        var subDisplayOffset = _displayOptions.SubDisplayOffset;
                        var screenOffset = _displayOptions.MainScreenOffset;
                        var screenScale = _displayOptions.MainScreenScale;

                        bool subChanged = subDisplayOffset != _lastSubDisplayOffset;
                        bool mainChanged = screenOffset != _lastOffset || screenScale != _lastScale;

                        if (!subChanged && !mainChanged)
                        {
                            return;
                        }

                        if (subChanged)
                        {
                            _lastSubDisplayOffset = subDisplayOffset;
                            ApplySubDisplayOffset(subDisplayOffset * 100f);
                        }

                        if (mainChanged)
                        {
                            _lastOffset = screenOffset;
                            _lastScale = screenScale;
                            ApplyPosition(screenOffset, screenScale, updateCache: true);
                        }
                        UpdateSubCover();
                    }
                    return;
            }
        }
    }
}
