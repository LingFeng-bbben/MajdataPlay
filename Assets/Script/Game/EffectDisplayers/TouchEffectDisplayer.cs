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
        public SensorType SensorPos { get; set; } = SensorType.C;

        [SerializeField]
        GameObject effectObject;
        [SerializeField]
        GameObject textObject;
        [SerializeField]
        GameObject fastLateObject;

        [SerializeField]
        Animator effectAnim;

        [SerializeField]
        JudgeTextDisplayer judgeTextDisplayer;
        [SerializeField]
        FastLateDisplayer fastLateDisplayer;
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

            var textPos = textObject.transform.localPosition;
            textPos.y = textDistance;
            textObject.transform.localPosition = textPos;

            var fastLatePos = fastLateObject.transform.localPosition;
            fastLatePos.y = fastLateDistance;
            fastLateObject.transform.localPosition = fastLatePos;
        }
        public void Reset()
        {
            effectObject.SetActive(false);
        }
        public void ResetAll()
        {
            Reset();
            judgeTextDisplayer.Reset();
            fastLateDisplayer.Reset();
        }
        public void Play(in JudgeResult judgeResult)
        {
            PlayEffect(judgeResult);
            if (IsClassCAvailable(judgeResult))
            {
                judgeTextDisplayer.Play(judgeResult,true);
            }
            else
            {
                PlayResult(judgeResult);
                PlayFastLate(judgeResult);
            }
        }
        void PlayEffect(in JudgeResult judgeResult)
        {
            var result = judgeResult.Result;
            if (!judgeResult.IsMissOrTooFast)
                Reset();
            else
                return;
            effectObject.SetActive(true);
            switch (result)
            {
                case JudgeType.LateGood:
                case JudgeType.FastGood:
                    effectAnim.SetTrigger("good");
                    break;
                case JudgeType.LateGreat:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat2:
                case JudgeType.FastGreat2:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat:
                    effectAnim.SetTrigger("great");
                    break;
                case JudgeType.LatePerfect2:
                case JudgeType.FastPerfect2:
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                case JudgeType.Perfect:
                    effectAnim.SetTrigger("perfect");
                    break;
                default:
                    break;
            }
        }
        void PlayResult(in JudgeResult judgeResult)
        {
            bool canPlay;
            if (judgeResult.IsBreak)
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.BreakJudgeType, judgeResult);
            else
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.TouchJudgeType, judgeResult);
            if (!canPlay)
            {
                judgeTextDisplayer.Reset();
                return;
            }
            judgeTextDisplayer.Play(judgeResult);
        }
        void PlayFastLate(in JudgeResult judgeResult)
        {
            bool canPlay;
            if (judgeResult.IsBreak)
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.BreakFastLateType, judgeResult);
            else
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.FastLateType, judgeResult);
            if (!canPlay)
            {
                fastLateDisplayer.Reset();
                return;
            }
            fastLateDisplayer.Play(judgeResult);
        }
        static bool IsClassCAvailable(in JudgeResult judgeResult)
        {
            bool canPlay;
            var isBreak = judgeResult.IsBreak;

            if (judgeResult.IsMissOrTooFast)
                return false;
            if (isBreak)
            {
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.BreakJudgeType, judgeResult);
                canPlay = canPlay && NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.BreakFastLateType, judgeResult);
            }
            else
            {
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.TouchJudgeType, judgeResult);
                canPlay = canPlay && NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.FastLateType, judgeResult);
            }
            if (!canPlay)
                return canPlay;
            var skin = MajInstances.SkinManager.GetJudgeTextSkin();
            var isCritical = MajInstances.Setting.Display.DisplayCriticalPerfect;
            var displayBreakScore = MajInstances.Setting.Display.DisplayBreakScore;
            if (isBreak)
            {
                if (displayBreakScore)
                {
                    switch (judgeResult.Result)
                    {
                        case JudgeType.LateGood:
                            return canPlay && skin.Break_1000.Late is not null;
                        case JudgeType.FastGood:
                            return canPlay && skin.Break_1000.Fast is not null;

                        case JudgeType.LateGreat2:
                            return canPlay && skin.Break_1250.Late is not null;
                        case JudgeType.FastGreat2:
                            return canPlay && skin.Break_1250.Fast is not null;

                        case JudgeType.LateGreat1:
                            return canPlay && skin.Break_1500.Late is not null;
                        case JudgeType.FastGreat1:
                            return canPlay && skin.Break_1500.Fast is not null;

                        case JudgeType.LateGreat:
                            return canPlay && skin.Break_2000.Late is not null;
                        case JudgeType.FastGreat:
                            return canPlay && skin.Break_2000.Fast is not null;

                        case JudgeType.LatePerfect2:
                            return canPlay && skin.Break_2500.Late is not null;
                        case JudgeType.FastPerfect2:
                            return canPlay && skin.Break_2500.Fast is not null;

                        case JudgeType.LatePerfect1:
                            return canPlay && skin.Break_2550.Late is not null;
                        case JudgeType.FastPerfect1:
                            return canPlay && skin.Break_2550.Fast is not null;

                        case JudgeType.Perfect:
                            return canPlay && skin.Break_2600.Late is not null;
                    }
                }
                else
                {
                    switch (judgeResult.Result)
                    {
                        case JudgeType.LateGood:
                            return canPlay && skin.Good.Late is not null;
                        case JudgeType.FastGood:
                            return canPlay && skin.Good.Fast is not null;

                        case JudgeType.LateGreat:
                        case JudgeType.LateGreat1:
                        case JudgeType.LateGreat2:
                            return canPlay && skin.Great.Late is not null;
                        case JudgeType.FastGreat:
                        case JudgeType.FastGreat1:
                        case JudgeType.FastGreat2:
                            return canPlay && skin.Great.Fast is not null;
                        case JudgeType.LatePerfect1:
                        case JudgeType.LatePerfect2:
                            return canPlay && skin.Perfect.Late is not null;
                        case JudgeType.FastPerfect1:
                        case JudgeType.FastPerfect2:
                            return canPlay && skin.Perfect.Fast is not null;
                        case JudgeType.Perfect:
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
                switch (judgeResult.Result)
                {
                    case JudgeType.LateGood:
                        return canPlay && skin.Good.Late is not null;
                    case JudgeType.FastGood:
                        return canPlay && skin.Good.Fast is not null;

                    case JudgeType.LateGreat2:
                    case JudgeType.LateGreat1:
                    case JudgeType.LateGreat:
                        return canPlay && skin.Great.Late is not null;
                    case JudgeType.FastGreat2:
                    case JudgeType.FastGreat1:
                    case JudgeType.FastGreat:
                        return canPlay && skin.Great.Fast is not null;
                    case JudgeType.LatePerfect1:
                    case JudgeType.LatePerfect2:
                        return canPlay && skin.Perfect.Late is not null;
                    case JudgeType.FastPerfect1:
                    case JudgeType.FastPerfect2:
                        return canPlay && skin.Perfect.Fast is not null;
                    case JudgeType.Perfect:
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
