using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MajdataPlay
{
    public class ScreenPosCameraMover : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            transform.position = new Vector3(0, 1.5f + 2.7f * MajInstances.Settings.Display.MainScreenPosition,-10);
        }
    }
}
