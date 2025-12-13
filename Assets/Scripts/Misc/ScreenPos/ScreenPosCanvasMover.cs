using MajdataPlay.Numerics;
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

        // Start is called before the first frame update
        void Awake()
        {
            rt = GetComponent<RectTransform>();
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
            Refresh();
        }
        internal void Refresh()
        {
            var pos = MajInstances.Settings?.Display.MainScreenPosition ?? 1;
            var posY = 810f - 270f * pos;
            rt.anchoredPosition = new Vector2(0, posY);
            if (pos != 1f)
            {
                if(_subCover is not null)
                {
                    var diffY = MAIN_DISPLAY_POS_Y - posY;
                    var newSubCoverHeight = (SUB_COVER_HEIGHT + diffY).Clamp(0,float.MaxValue);
                    var newSubCoverPosY = (SUB_COVER_POS_Y - diffY / 2f).Clamp(0, float.MaxValue);

                    var originPos = _subCoverRectTransform!.anchoredPosition;
                    _subCoverRectTransform!.anchoredPosition = new Vector2(originPos.x, newSubCoverPosY);
                    _subCoverRectTransform!.sizeDelta = new Vector2(SUB_COVER_WIDTH, newSubCoverHeight);
                }
                if(_subCoverBottom is not null)
                {
                    var diffY = MAIN_DISPLAY_POS_Y - posY;
                    var newSubCoverBottomHeight = (SUB_COVER_BOTTOM_HEIGHT - diffY).Clamp(0, float.MaxValue);
                    var newSubCoverBottomPosY = (SUB_COVER_BOTTOM_POS_Y - diffY / 2f).Clamp(float.MinValue, 0);
                    var originPos = _subCoverBottomRectTransform!.anchoredPosition;
                    _subCoverBottomRectTransform!.anchoredPosition = new Vector2(originPos.x, newSubCoverBottomPosY);
                    _subCoverBottomRectTransform!.sizeDelta = new Vector2(SUB_COVER_BOTTOM_WIDTH, newSubCoverBottomHeight);
                }
            }
        }
    }
}
