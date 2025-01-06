using MajdataPlay.Types;
using MajdataPlay.Utils;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    public sealed class FastLateDisplayer: MonoBehaviour
    {
        public Vector3 Position
        {
            get => effectObject.transform.position;
            set => effectObject.transform.position = value;
        }
        public Vector3 LocalPosition
        {
            get => effectObject.transform.localPosition;
            set => effectObject.transform.localPosition = value;
        }
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
            var skin = MajInstances.SkinManager.GetJudgeTextSkin();
            fastSprite = skin.Fast;
            lateSprite = skin.Late;
        }
        public void Reset()
        {
            effectObject.SetActive(false);
        }
        public void Play(in JudgeResult judgeResult)
        {
            if (judgeResult.IsMissOrTooFast || judgeResult.Diff == 0)
            {
                Reset();
                return;
            }
            effectObject.SetActive(true);
            if (judgeResult.IsFast)
                textRenderer.sprite = fastSprite;
            else
                textRenderer.sprite = lateSprite;
            if (judgeResult.IsBreak)
                animator.SetTrigger("break");
            else
                animator.SetTrigger("perfect");
        }
    }
}
