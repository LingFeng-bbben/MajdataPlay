using MajdataPlay.Types;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    public sealed class FastLateDisplayer: MonoBehaviour
    {
        [SerializeField]
        GameObject effectObject;
        [SerializeField]
        Animator animator;

        [SerializeField]
        SpriteRenderer textRenderer;

        Sprite fastSprite;
        Sprite lateSprite;
        void Start()
        {
            var skin = SkinManager.Instance.GetJudgeTextSkin();
            fastSprite = skin.Fast;
            lateSprite = skin.Late;
        }
        public void Reset()
        {
            effectObject.SetActive(false);
        }
        public void Play(in JudgeResult judgeResult)
        {
            effectObject.SetActive(true);
            if (judgeResult.IsFast)
                textRenderer.sprite = fastSprite;
            else
                textRenderer.sprite = lateSprite;
        }
    }
}
