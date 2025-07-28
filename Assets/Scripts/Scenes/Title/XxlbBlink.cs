using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay.Scenes.Title
{
    public class XxlbBlink : MonoBehaviour
    {
        public Sprite XxlbDefault;
        public Sprite XxlbBlinks;
        public Image image;
        // Start is called before the first frame update
        void Start()
        {
            image = GetComponent<Image>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            var ran = Random.Range(0, 80);
            if (ran == 1)
            {
                image.sprite = XxlbBlinks;
            }
            else
            {
                image.sprite = XxlbDefault;
            }
        }
    }
}