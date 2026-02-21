using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay
{
    public class PauseButtonHider : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
#if !(UNITY_ANDROID || UNITY_IOS)
            GetComponent<Image>().color = new Color(0,0,0,0);
#endif
        }
    }
}
