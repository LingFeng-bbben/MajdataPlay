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
            get => _gameObject.transform.position;
            set => _gameObject.transform.position = value;
        }
        public Vector3 LocalPosition
        {
            get => _gameObject.transform.localPosition;
            set => _gameObject.transform.localPosition = value;
        }

        GameObject _gameObject;
        Animator _animator;

        [SerializeField]
        SpriteRenderer textRenderer;
        [SerializeField]
        SpriteRenderer breakRenderer;


        JudgeTextSkin _skin;

        bool _displayBreakScore = false;
        bool _displayCriticalPerfect = false;
        void Awake()
        {
            _gameObject = gameObject;
            _animator = GetComponent<Animator>();
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
            _gameObject.SetActive(false);
        }
        public void Play(in JudgeResult judgeResult, bool isClassC = false)
        {
            _gameObject.SetActive(true);
            var isBreak = judgeResult.IsBreak;
            var result = judgeResult.Grade;

            if (isBreak && _displayBreakScore)
                LoadBreakSkin(judgeResult,isClassC);
            else
                LoadTapSkin(judgeResult,isClassC);
            
            if (isBreak && result == JudgeGrade.Perfect)
                _animator.SetTrigger("break");
            else
                _animator.SetTrigger("perfect");
        }
        void LoadTapSkin(in JudgeResult judgeResult,bool isClassC = false)
        {
            switch (judgeResult.Grade)
            {
                case JudgeGrade.LateGood:
                    textRenderer.sprite = isClassC ? _skin.Good.Late : _skin.Good.Normal;
                    break;
                case JudgeGrade.FastGood:
                    textRenderer.sprite = isClassC ? _skin.Good.Fast : _skin.Good.Normal;
                    break;
                case JudgeGrade.LateGreat:
                case JudgeGrade.LateGreat1:
                case JudgeGrade.LateGreat2:
                    textRenderer.sprite = isClassC ? _skin.Great.Late : _skin.Great.Normal;
                    break;
                case JudgeGrade.FastGreat:
                case JudgeGrade.FastGreat1:
                case JudgeGrade.FastGreat2:
                    textRenderer.sprite = isClassC ? _skin.Great.Fast : _skin.Great.Normal;
                    break;
                case JudgeGrade.LatePerfect1:
                case JudgeGrade.LatePerfect2:
                    textRenderer.sprite = isClassC ? _skin.Perfect.Late : _skin.Perfect.Normal;
                    break;
                case JudgeGrade.FastPerfect1:
                case JudgeGrade.FastPerfect2:
                    textRenderer.sprite = isClassC ? _skin.Perfect.Fast : _skin.Perfect.Normal;
                    break;
                case JudgeGrade.Perfect:
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
            switch (judgeResult.Grade)
            {
                case JudgeGrade.LateGood:
                    textRenderer.sprite = isClassC ? _skin.Break_1000.Late : _skin.Break_1000.Normal;
                    break;
                case JudgeGrade.FastGood:
                    textRenderer.sprite = isClassC ? _skin.Break_1000.Fast : _skin.Break_1000.Normal;
                    break;
                case JudgeGrade.LateGreat2:
                    textRenderer.sprite = isClassC ? _skin.Break_1250.Late : _skin.Break_1250.Normal;
                    break;
                case JudgeGrade.FastGreat2:
                    textRenderer.sprite = isClassC ? _skin.Break_1250.Fast : _skin.Break_1250.Normal;
                    break;
                case JudgeGrade.LateGreat1:
                    textRenderer.sprite = isClassC ? _skin.Break_1500.Late : _skin.Break_1500.Normal;
                    break;
                case JudgeGrade.FastGreat1:
                    textRenderer.sprite = isClassC ? _skin.Break_1500.Fast : _skin.Break_1500.Normal;
                    break;
                case JudgeGrade.LateGreat:
                    textRenderer.sprite = isClassC ? _skin.Break_2000.Late : _skin.Break_2000.Normal;
                    break;
                case JudgeGrade.FastGreat:
                    textRenderer.sprite = isClassC ? _skin.Break_2000.Fast : _skin.Break_2000.Normal;
                    break;
                case JudgeGrade.LatePerfect2:
                    textRenderer.sprite = isClassC ? _skin.Break_2500.Late : _skin.Break_2500.Normal;
                    break;
                case JudgeGrade.FastPerfect2:
                    textRenderer.sprite = isClassC ? _skin.Break_2500.Fast : _skin.Break_2500.Normal;
                    break;
                case JudgeGrade.LatePerfect1:
                    textRenderer.sprite = isClassC ? _skin.Break_2550.Late : _skin.Break_2550.Normal;
                    break;
                case JudgeGrade.FastPerfect1:
                    textRenderer.sprite = isClassC ? _skin.Break_2550.Fast : _skin.Break_2550.Normal;
                    break;
                case JudgeGrade.Perfect:
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
