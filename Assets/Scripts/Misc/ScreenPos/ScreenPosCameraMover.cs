using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MajdataPlay
{
    public class ScreenPosCameraMover : MonoBehaviour
    {
        // Start is called before the first frame update
        Camera cam;
        void Start()
        {
            transform.position = new Vector3(0, 1.5f + 2.7f * (MajInstances.Settings?.Display.MainScreenPosition ?? 1f), -10);
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


    }
}
