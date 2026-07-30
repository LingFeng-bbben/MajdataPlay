using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;

namespace MajdataPlay.Scenes.Title
{
    public class XxlbBlink : MonoBehaviour
    {
        public Sprite XxlbDefault;
        public Sprite XxlbBlinksHalf;
        public Sprite XxlbBlinks;
        public Sprite XxlbWink;
        public Image Image;

        void Awake()
        {
//#if UNITY_IOS appstore njm
//            gameObject.SetActive(false);
//#endif
        }
        void Start()
        {
            Image = GetComponent<Image>();
            var rect = GetComponent<RectTransform>();
            Winkloop().Forget();
            // 起始位置
            float startY = rect.anchoredPosition.y;
            float endY = startY - 20f; // 浮动高度（可调）

            // 创建往返浮动动画
            LMotion.Create(startY, endY, 2.5f)      // 1.5 秒完成一次上升
                .WithEase(Ease.InOutSine)       // 平滑的上下浮动
                .WithLoops(-1, LoopType.Yoyo)       // 无限循环 + 往返
                .BindToAnchoredPositionY(rect);    // 绑定到物体的 Y 轴
        }

        async UniTaskVoid Winkloop()
        {
            while (true)
            {
                var ran = Random.Range(0, 1000);
                if (ran <= 10)
                {
                    Image.sprite = XxlbBlinksHalf;
                    await UniTask.WaitForSeconds(0.1f);
                    Image.sprite = XxlbBlinks;
                    await UniTask.WaitForSeconds(0.1f);
                    Image.sprite = XxlbDefault;
                    await UniTask.WaitForSeconds(0.2f);
                }

                if (ran == 6)
                {
                    Image.sprite = XxlbWink;
                    await UniTask.WaitForSeconds(0.3f);
                    Image.sprite = XxlbDefault;
                    await UniTask.WaitForSeconds(0.2f);
                }
                await UniTask.Yield();
            }
        }
    }
}