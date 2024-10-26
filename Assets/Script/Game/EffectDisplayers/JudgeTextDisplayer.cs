using MajdataPlay.Types;
using MajdataPlay.Utils;
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

        bool _displayBreakScore = false;
        void Start() 
        {
            var skin = MajInstances.SkinManager.GetJudgeTextSkin();
            _displayBreakScore = MajInstances.Setting.Display.DisplayBreakScore;

            if (MajInstances.Setting.Display.DisplayCriticalPerfect)
            {
                //breakSprite = skin.CP_Break;
                if (_displayBreakScore)
                    breakSprite = skin.Break_2600_Shine;
                else
                    breakSprite = skin.CP_Break;
                cPerfectSprite = skin.CriticalPerfect;
            }
            else
            {
                //breakSprite = skin.P_Break;
                breakSprite = skin.Break_2600_Shine;
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

            if (isBreak && _displayBreakScore)
                LoadBreakSkin(judgeResult);
            else
                LoadTapSkin(judgeResult);
            
            if (isBreak && result == JudgeType.Perfect)
                animator.SetTrigger("break");
            else
                animator.SetTrigger("perfect");
        }
        void LoadTapSkin(in JudgeResult judgeResult)
        {
            switch (judgeResult.Result)
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
        }
        void LoadBreakSkin(in JudgeResult judgeResult)
        {
            var skin = MajInstances.SkinManager.GetJudgeTextSkin();
            switch (judgeResult.Result)
            {
                case JudgeType.LateGood:
                case JudgeType.FastGood:
                    textRenderer.sprite = skin.Break_1000;
                    break;
                case JudgeType.LateGreat:
                case JudgeType.FastGreat:
                    textRenderer.sprite = skin.Break_1250;
                    break;
                case JudgeType.LateGreat1:
                case JudgeType.FastGreat1:
                    textRenderer.sprite = skin.Break_1500;
                    break;
                case JudgeType.LateGreat2:
                case JudgeType.FastGreat2:
                    textRenderer.sprite = skin.Break_2000;
                    break;
                case JudgeType.LatePerfect2:
                case JudgeType.FastPerfect2:
                    textRenderer.sprite = skin.Break_2500;
                    break;
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                    textRenderer.sprite = skin.Break_2550;
                    break;
                case JudgeType.Perfect:
                    textRenderer.sprite = skin.Break_2600;
                    break;
                default:
                    textRenderer.sprite = skin.Break_0;
                    break;
            }
        }
    }
}
