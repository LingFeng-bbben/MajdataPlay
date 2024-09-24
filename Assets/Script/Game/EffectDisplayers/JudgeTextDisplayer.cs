using MajdataPlay.Types;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    public sealed class JudgeTextDisplayer: MonoBehaviour
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
        [SerializeField]
        SpriteRenderer breakRenderer;

        Sprite breakSprite;
        Sprite cPerfectSprite;
        Sprite perfectSprite;
        Sprite greatSprite;
        Sprite goodSprite;
        Sprite missSprite;
        void Start() 
        {
            var skin = SkinManager.Instance.GetJudgeTextSkin();

            if(GameManager.Instance.Setting.Display.DisplayCriticalPerfect)
            {
                breakSprite = skin.CP_Break;
                cPerfectSprite = skin.CriticalPerfect;
            }
            else
            {
                breakSprite = skin.P_Break;
                cPerfectSprite = skin.Perfect;
            }
            breakRenderer.sprite = breakSprite;
            perfectSprite = skin.Perfect;
            greatSprite = skin.Great;
            goodSprite = skin.Good;
            missSprite = skin.Miss;
        }
        public void Reset()
        {
            effectObject.SetActive(false);
        }
        public void Play(in JudgeResult judgeResult)
        {
            effectObject.SetActive(true);
            var isBreak = judgeResult.IsBreak;
            var result = judgeResult.Result;

            switch (result)
            {
                case JudgeType.LateGood:
                case JudgeType.FastGood:
                    textRenderer.sprite = goodSprite;
                    break;
                case JudgeType.LateGreat:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat2:
                case JudgeType.FastGreat2:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat:
                    textRenderer.sprite = greatSprite;
                    break;
                case JudgeType.LatePerfect2:
                case JudgeType.FastPerfect2:
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                    textRenderer.sprite = perfectSprite;
                    break;
                case JudgeType.Perfect:
                    textRenderer.sprite = cPerfectSprite;
                    break;
                default:
                    textRenderer.sprite = missSprite;
                    break;
            }
            if (isBreak && result == JudgeType.Perfect)
                animator.SetTrigger("break");
            else
                animator.SetTrigger("perfect");
        }
    }
}
