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
        void Start()
        {
            rt = GetComponent<RectTransform>();
            var pos = MajInstances.Settings.Display.MainScreenPosition;
            rt.anchoredPosition = new Vector2(0, 810f - 270f * pos);
            if(pos != 1f)
                transform.parent.Find("Sub_Cover").gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
