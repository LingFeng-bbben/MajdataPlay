using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay.Scenes.List
{
    public class DanmakuDisplayer : MonoBehaviour
    {
        RectTransform rt;
        // Start is called before the first frame update
        void Start()
        {
            rt = GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(770f, -55f);
        }

        // Update is called once per frame
        void Update()
        {
            rt.anchoredPosition -= new Vector2(300f * Time.deltaTime, 0f);
            if (rt.anchoredPosition.x < -1000f) Destroy(gameObject);
        }
    }
}