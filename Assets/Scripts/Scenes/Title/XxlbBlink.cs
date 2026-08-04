using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay.Scenes.Title
{
    public class XxlbBlink : MonoBehaviour
    {
        const float BLINK_CHECK_INTERVAL_SEC = 0.1f;

        public Sprite XxlbDefault;
        public Sprite XxlbBlinksHalf;
        public Sprite XxlbBlinks;
        public Sprite XxlbWink;
        public Image Image;

        void Start()
        {
            var rect = GetComponent<RectTransform>();
            WinkLoopAsync(destroyCancellationToken).Forget();
            // 起始位置
            float startY = rect.anchoredPosition.y;
            float endY = startY - 20f; // 浮动高度（可调）

            // 创建往返浮动动画
            LMotion.Create(startY, endY, 2.5f)      // 2.5 秒完成一次下降
                .WithEase(Ease.InOutSine)       // 平滑的上下浮动
                .WithLoops(-1, LoopType.Yoyo)       // 无限循环 + 往返
                .BindToAnchoredPositionY(rect)    // 绑定到物体的 Y 轴
                .AddTo(this);
        }

        async UniTask WinkLoopAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                var ran = Random.Range(0, 1000);
                if (ran <= 10)
                {
                    Image.sprite = XxlbBlinksHalf;
                    await UniTask.WaitForSeconds(0.1f, cancellationToken: cancellationToken);
                    Image.sprite = XxlbBlinks;
                    await UniTask.WaitForSeconds(0.1f, cancellationToken: cancellationToken);
                    Image.sprite = XxlbDefault;
                    await UniTask.WaitForSeconds(0.2f, cancellationToken: cancellationToken);
                }

                if (ran == 6)
                {
                    Image.sprite = XxlbWink;
                    await UniTask.WaitForSeconds(0.3f, cancellationToken: cancellationToken);
                    Image.sprite = XxlbDefault;
                    await UniTask.WaitForSeconds(0.2f, cancellationToken: cancellationToken);
                }
                await UniTask.WaitForSeconds(BLINK_CHECK_INTERVAL_SEC, cancellationToken: cancellationToken);
            }
        }
    }
}
