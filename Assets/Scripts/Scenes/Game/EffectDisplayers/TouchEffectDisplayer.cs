using MajdataPlay.Extensions;
using MajdataPlay.Game.Notes;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using UnityEngine;

namespace MajdataPlay.Game
{
    public sealed class TouchEffectDisplayer: MonoBehaviour
    {
        public float DistanceRatio { get; set; } = 1f;
        public SensorArea SensorPos { get; set; } = SensorArea.C;

        [SerializeField]
        GameObject effectObject;

        [SerializeField]
        Animator effectAnim;

        [SerializeField]
        JudgeTextDisplayer _judgeTextDisplayer;
        [SerializeField]
        FastLateDisplayer _fastLateDisplayerA;
        [SerializeField]
        FastLateDisplayer _fastLateDisplayerB;

        bool _isEnabled = true;
        void Start()
        {
            var distance = TouchBase.GetDistance(SensorPos.GetGroup());
            var rotation = TouchBase.GetRoation(TouchBase.GetAreaPos(SensorPos), SensorPos);
            var textDistance = distance - (0.66f * (2 - DistanceRatio));
            var fastLateDistance = textDistance - 0.56f;
            transform.rotation = rotation;

            var effectPos = effectObject.transform.localPosition;
            effectPos.y += distance;
            effectObject.transform.localPosition = effectPos;

            var textPos = _judgeTextDisplayer.LocalPosition;
            textPos.y = textDistance;
            _judgeTextDisplayer.LocalPosition = textPos;
            _fastLateDisplayerB.LocalPosition = textPos;

            var fastLatePos = _fastLateDisplayerA.LocalPosition;
            fastLatePos.y = fastLateDistance;
            _fastLateDisplayerA.LocalPosition = fastLatePos;

            _isEnabled = MajInstances.Setting.Display.InnerJudgeDistance != 0;
        }
        public void Reset()
        {
            effectObject.SetActive(false);
        }
        public void ResetAll()
        {
            Reset();
            _judgeTextDisplayer.Reset();
            _fastLateDisplayerA.Reset();
            _fastLateDisplayerB.Reset();
        }
        public void Play(in JudgeResult judgeResult)
        {
            _judgeTextDisplayer.Reset();
            _fastLateDisplayerA.Reset();
            _fastLateDisplayerB.Reset();
            PlayEffect(judgeResult);
            if (!_isEnabled)
                return;
            if (IsClassCAvailable(judgeResult))
            {
                _judgeTextDisplayer.Play(judgeResult,true);
            }
            else
            {
                if(PlayResult(judgeResult))
                {
                    PlayFastLate(judgeResult, _fastLateDisplayerA);
                }
                else
                {
                    PlayFastLate(judgeResult, _fastLateDisplayerB);
                }
            }
        }
        void PlayEffect(in JudgeResult judgeResult)
        {
            var result = judgeResult.Grade;
            if (!judgeResult.IsMissOrTooFast)
                Reset();
            else
                return;
            effectObject.SetActive(true);
            switch (result)
            {
                case JudgeGrade.LateGood:
                case JudgeGrade.FastGood:
                    effectAnim.SetTrigger("good");
                    break;
                case JudgeGrade.LateGreat:
                case JudgeGrade.LateGreat1:
                case JudgeGrade.LateGreat2:
                case JudgeGrade.FastGreat2:
                case JudgeGrade.FastGreat1:
                case JudgeGrade.FastGreat:
                    effectAnim.SetTrigger("great");
                    break;
                case JudgeGrade.LatePerfect2:
                case JudgeGrade.FastPerfect2:
                case JudgeGrade.LatePerfect1:
                case JudgeGrade.FastPerfect1:
                case JudgeGrade.Perfect:
                    effectAnim.SetTrigger("perfect");
                    break;
                default:
                    break;
            }
        }
        bool PlayResult(in JudgeResult judgeResult)
        {
            bool canPlay;
            if (judgeResult.IsBreak)
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.BreakJudgeType, judgeResult);
            else
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.TouchJudgeType, judgeResult);
            if (!canPlay)
            {
                return false;
            }
            _judgeTextDisplayer.Play(judgeResult);
            return true;
        }
        bool PlayFastLate(in JudgeResult judgeResult, FastLateDisplayer displayer)
        {
            bool canPlay;
            if (judgeResult.IsBreak)
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.BreakFastLateType, judgeResult);
            else
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.FastLateType, judgeResult);
            if (!canPlay)
            {
                return false;
            }
            displayer.Play(judgeResult);
            return true;
        }
        static bool IsClassCAvailable(in JudgeResult judgeResult)
        {
            if (judgeResult.IsMissOrTooFast)
                return false;

            var isBreak = judgeResult.IsBreak;
            var canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.TouchJudgeType, judgeResult);
            canPlay = canPlay && NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.FastLateType, judgeResult);

            if (!canPlay)
                return canPlay;
            var skin = MajInstances.SkinManager.GetJudgeTextSkin();
            var isCritical = MajInstances.Setting.Display.DisplayCriticalPerfect;
            var displayBreakScore = MajInstances.Setting.Display.DisplayBreakScore;
            if (isBreak)
            {
                if (displayBreakScore)
                {
                    switch (judgeResult.Grade)
                    {
                        case JudgeGrade.LateGood:
                            return canPlay && skin.Break_1000.Late is not null;
                        case JudgeGrade.FastGood:
                            return canPlay && skin.Break_1000.Fast is not null;

                        case JudgeGrade.LateGreat2:
                            return canPlay && skin.Break_1250.Late is not null;
                        case JudgeGrade.FastGreat2:
                            return canPlay && skin.Break_1250.Fast is not null;

                        case JudgeGrade.LateGreat1:
                            return canPlay && skin.Break_1500.Late is not null;
                        case JudgeGrade.FastGreat1:
                            return canPlay && skin.Break_1500.Fast is not null;

                        case JudgeGrade.LateGreat:
                            return canPlay && skin.Break_2000.Late is not null;
                        case JudgeGrade.FastGreat:
                            return canPlay && skin.Break_2000.Fast is not null;

                        case JudgeGrade.LatePerfect2:
                            return canPlay && skin.Break_2500.Late is not null;
                        case JudgeGrade.FastPerfect2:
                            return canPlay && skin.Break_2500.Fast is not null;

                        case JudgeGrade.LatePerfect1:
                            return canPlay && skin.Break_2550.Late is not null;
                        case JudgeGrade.FastPerfect1:
                            return canPlay && skin.Break_2550.Fast is not null;

                        case JudgeGrade.Perfect:
                            return canPlay && skin.Break_2600.Late is not null;
                    }
                }
                else
                {
                    switch (judgeResult.Grade)
                    {
                        case JudgeGrade.LateGood:
                            return canPlay && skin.Good.Late is not null;
                        case JudgeGrade.FastGood:
                            return canPlay && skin.Good.Fast is not null;

                        case JudgeGrade.LateGreat:
                        case JudgeGrade.LateGreat1:
                        case JudgeGrade.LateGreat2:
                            return canPlay && skin.Great.Late is not null;
                        case JudgeGrade.FastGreat:
                        case JudgeGrade.FastGreat1:
                        case JudgeGrade.FastGreat2:
                            return canPlay && skin.Great.Fast is not null;
                        case JudgeGrade.LatePerfect1:
                        case JudgeGrade.LatePerfect2:
                            return canPlay && skin.Perfect.Late is not null;
                        case JudgeGrade.FastPerfect1:
                        case JudgeGrade.FastPerfect2:
                            return canPlay && skin.Perfect.Fast is not null;
                        case JudgeGrade.Perfect:
                            {
                                var isJust = judgeResult.Diff == 0;
                                var isFast = judgeResult.IsFast;
                                if (isJust)
                                    return false;
                                else if (isFast)
                                {
                                    if (isCritical)
                                        return canPlay && skin.CriticalPerfect.Fast is not null;
                                    else
                                        return canPlay && skin.Perfect.Fast is not null;
                                }
                                else
                                {
                                    if (isCritical)
                                        return canPlay && skin.CriticalPerfect.Late is not null;
                                    else
                                        return canPlay && skin.Perfect.Late is not null;
                                }
                            }
                    }
                }
            }
            else
            {
                switch (judgeResult.Grade)
                {
                    case JudgeGrade.LateGood:
                        return canPlay && skin.Good.Late is not null;
                    case JudgeGrade.FastGood:
                        return canPlay && skin.Good.Fast is not null;

                    case JudgeGrade.LateGreat2:
                    case JudgeGrade.LateGreat1:
                    case JudgeGrade.LateGreat:
                        return canPlay && skin.Great.Late is not null;
                    case JudgeGrade.FastGreat2:
                    case JudgeGrade.FastGreat1:
                    case JudgeGrade.FastGreat:
                        return canPlay && skin.Great.Fast is not null;
                    case JudgeGrade.LatePerfect1:
                    case JudgeGrade.LatePerfect2:
                        return canPlay && skin.Perfect.Late is not null;
                    case JudgeGrade.FastPerfect1:
                    case JudgeGrade.FastPerfect2:
                        return canPlay && skin.Perfect.Fast is not null;
                    case JudgeGrade.Perfect:
                        {
                            if (judgeResult.Diff == 0)
                                return false;
                            if (judgeResult.IsFast)
                            {
                                if (isCritical)
                                    return canPlay && skin.CriticalPerfect.Fast is not null;
                                else
                                    return canPlay && skin.Perfect.Fast is not null;
                            }
                            else
                            {
                                if (isCritical)
                                    return canPlay && skin.CriticalPerfect.Late is not null;
                                else
                                    return canPlay && skin.Perfect.Late is not null;
                            }
                        }
                }
            }

            return canPlay;
        }
    }
}
