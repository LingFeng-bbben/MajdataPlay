using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay
{
    public class ScreenPosCanvasMover : MonoBehaviour
    {
        RectTransform rt;
        // Start is called before the first frame update
        void Awake()
        {
            rt = GetComponent<RectTransform>();
            Refresh();
        }
        internal void Refresh()
        {
            var pos = MajInstances.Settings.Display.MainScreenPosition;
            rt.anchoredPosition = new Vector2(0, 810f - 270f * pos);
            if (pos != 1f)
            {
                var sub = transform.parent.Find("Sub_Cover");
                if (sub != null)
                {
                    sub.gameObject.SetActive(false);
                }
            }
        }
    }
}
