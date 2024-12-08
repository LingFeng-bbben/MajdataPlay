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


        JudgeTextSkin _skin;

        bool _displayBreakScore = false;
        bool _displayCriticalPerfect = false;
        void Start() 
        {
            _skin = MajInstances.SkinManager.GetJudgeTextSkin();
            _displayBreakScore = MajInstances.Setting.Display.DisplayBreakScore;
            _displayCriticalPerfect = MajInstances.Setting.Display.DisplayCriticalPerfect;
            Sprite breakSprite;

            if (_displayCriticalPerfect)
            {
                if (_displayBreakScore)
                    breakSprite = _skin.Break_2600_Shine;
                else
                    breakSprite = _skin.CP_Shine;
            }
            else
            {
                if (_displayBreakScore)
                    breakSprite = _skin.Break_2600_Shine;
                else
                    breakSprite = _skin.P_Shine;
            }
            breakRenderer.sprite = breakSprite;
        }
        public void Reset()
        {
            effectObject.SetActive(false);
        }
        public void Play(in JudgeResult judgeResult, bool isClassC = false)
        {
            effectObject.SetActive(true);
            var isBreak = judgeResult.IsBreak;
            var result = judgeResult.Result;

            if (isBreak && _displayBreakScore)
                LoadBreakSkin(judgeResult,isClassC);
            else
                LoadTapSkin(judgeResult,isClassC);
            
            if (isBreak && result == JudgeType.Perfect)
                animator.SetTrigger("break");
            else
                animator.SetTrigger("perfect");
        }
        void LoadTapSkin(in JudgeResult judgeResult,bool isClassC = false)
        {
            switch (judgeResult.Result)
            {
                case JudgeType.LateGood:
                    textRenderer.sprite = isClassC ? _skin.Good.Late : _skin.Good.Normal;
                    break;
                case JudgeType.FastGood:
                    textRenderer.sprite = isClassC ? _skin.Good.Fast : _skin.Good.Normal;
                    break;
                case JudgeType.LateGreat:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat2:
                    textRenderer.sprite = isClassC ? _skin.Great.Late : _skin.Great.Normal;
                    break;
                case JudgeType.FastGreat:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat2:
                    textRenderer.sprite = isClassC ? _skin.Great.Fast : _skin.Great.Normal;
                    break;
                case JudgeType.LatePerfect1:
                case JudgeType.LatePerfect2:
                    textRenderer.sprite = isClassC ? _skin.Perfect.Late : _skin.Perfect.Normal;
                    break;
                case JudgeType.FastPerfect1:
                case JudgeType.FastPerfect2:
                    textRenderer.sprite = isClassC ? _skin.Perfect.Fast : _skin.Perfect.Normal;
                    break;
                case JudgeType.Perfect:
                    if (isClassC)
                    {
                        var isJust = judgeResult.Diff == 0;
                        var isFast = judgeResult.IsFast;
                        if(_displayCriticalPerfect)
                        {
                            if (isJust)
                                textRenderer.sprite = _skin.CriticalPerfect.Normal;
                            else if (isFast)
                                textRenderer.sprite = _skin.CriticalPerfect.Fast;
                            else
                                textRenderer.sprite = _skin.CriticalPerfect.Late;
                        }
                        else
                        {
                            if (isJust)
                                textRenderer.sprite = _skin.Perfect.Normal;
                            else if (isFast)
                                textRenderer.sprite = _skin.Perfect.Fast;
                            else
                                textRenderer.sprite = _skin.Perfect.Late;
                        }
                    }
                    else
                    {
                        if(_displayCriticalPerfect)
                            textRenderer.sprite = _skin.CriticalPerfect.Normal;
                        else
                            textRenderer.sprite = _skin.Perfect.Normal;
                    }
                    break;
                default:
                    textRenderer.sprite = _skin.Miss;
                    break;
            }
        }
        void LoadBreakSkin(in JudgeResult judgeResult, bool isClassC = false)
        {
            switch (judgeResult.Result)
            {
                case JudgeType.LateGood:
                    textRenderer.sprite = isClassC ? _skin.Break_1000.Late : _skin.Break_1000.Normal;
                    break;
                case JudgeType.FastGood:
                    textRenderer.sprite = isClassC ? _skin.Break_1000.Fast : _skin.Break_1000.Normal;
                    break;
                case JudgeType.LateGreat2:
                    textRenderer.sprite = isClassC ? _skin.Break_1250.Late : _skin.Break_1250.Normal;
                    break;
                case JudgeType.FastGreat2:
                    textRenderer.sprite = isClassC ? _skin.Break_1250.Fast : _skin.Break_1250.Normal;
                    break;
                case JudgeType.LateGreat1:
                    textRenderer.sprite = isClassC ? _skin.Break_1500.Late : _skin.Break_1500.Normal;
                    break;
                case JudgeType.FastGreat1:
                    textRenderer.sprite = isClassC ? _skin.Break_1500.Fast : _skin.Break_1500.Normal;
                    break;
                case JudgeType.LateGreat:
                    textRenderer.sprite = isClassC ? _skin.Break_2000.Late : _skin.Break_2000.Normal;
                    break;
                case JudgeType.FastGreat:
                    textRenderer.sprite = isClassC ? _skin.Break_2000.Fast : _skin.Break_2000.Normal;
                    break;
                case JudgeType.LatePerfect2:
                    textRenderer.sprite = isClassC ? _skin.Break_2500.Late : _skin.Break_2500.Normal;
                    break;
                case JudgeType.FastPerfect2:
                    textRenderer.sprite = isClassC ? _skin.Break_2500.Fast : _skin.Break_2500.Normal;
                    break;
                case JudgeType.LatePerfect1:
                    textRenderer.sprite = isClassC ? _skin.Break_2550.Late : _skin.Break_2550.Normal;
                    break;
                case JudgeType.FastPerfect1:
                    textRenderer.sprite = isClassC ? _skin.Break_2550.Fast : _skin.Break_2550.Normal;
                    break;
                case JudgeType.Perfect:
                    {
                        if(isClassC)
                        {
                            var isJust = judgeResult.Diff == 0;
                            var isFast = judgeResult.IsFast;
                            if (isJust)
                                textRenderer.sprite = _skin.Break_2600.Normal;
                            else if (isFast)
                                textRenderer.sprite = _skin.Break_2600.Fast;
                            else
                                textRenderer.sprite = _skin.Break_2600.Late;
                        }
                        else
                        {
                            textRenderer.sprite = _skin.Break_2600.Normal;
                        }
                    }
                    break;
                default:
                    textRenderer.sprite = _skin.Break_0;
                    break;
            }
        }
        
    }
}
