using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    public sealed class TapEffectDisplayer : MonoBehaviour
    {
        public Vector3 Position => effectParent.transform.position;
        public Vector3 LocalPosition
        {
            get => effectParent.transform.localPosition;
            set
            {
                effectParent.transform.localPosition = value;
                //perfectDisplayer.transform.localPosition = value;
                //greatDisplayer.transform.localPosition = value;
                //goodDisplayer.transform.localPosition = value;
                var effectPosition = value;
                var textDistance = 1f * (2 - DistanceRatio);
                var fastLateDistance = textDistance + 0.66f;

                Vector3 textPosition;
                Vector3 fastLatePosition;
                if (effectPosition.magnitude == 0)
                {
                    textPosition = effectPosition.GetPoint(new Vector3(0, -0.01f, 0), -textDistance);
                    fastLatePosition = effectPosition.GetPoint(new Vector3(0, -0.01f, 0), -fastLateDistance);
                }
                else
                {
                    textPosition = effectPosition.GetPoint(-textDistance);
                    fastLatePosition = effectPosition.GetPoint(-fastLateDistance);
                }

                _judgeTextDisplayer.LocalPosition = textPosition;
                _fastLateDisplayerA.LocalPosition = fastLatePosition;
            }
        }
        /// <summary>
        /// 用于调整Text、Fast/Late的显示位置
        /// </summary>
        public float DistanceRatio { get; set; } = 1f;

        [SerializeField]
        GameObject effectParent;
        [SerializeField]
        GameObject perfectDisplayer;
        [SerializeField]
        GameObject greatDisplayer;
        [SerializeField]
        GameObject goodDisplayer;

        [SerializeField]
        Animator perfectAnim;
        [SerializeField]
        Animator greatAnim;
        [SerializeField]
        Animator goodAnim;

        [SerializeField]
        JudgeTextDisplayer _judgeTextDisplayer;
        [SerializeField]
        FastLateDisplayer _fastLateDisplayerA;
        [SerializeField]
        FastLateDisplayer _fastLateDisplayerB;

        bool _isEnabled = false;

        void Start()
        {
            Reset();
            var effectPosition = effectParent.transform.localPosition;
            var textDistance = 1f * (2 - DistanceRatio);
            var fastLateDistance = textDistance + 0.66f;

            Vector3 textPosition;
            Vector3 fastLatePosition;
            if (effectPosition.magnitude == 0)
            {
                textPosition = effectPosition.GetPoint(new Vector3(0, -0.01f, 0), -textDistance);
                fastLatePosition = effectPosition.GetPoint(new Vector3(0, -0.01f, 0), -fastLateDistance);
            }
            else
            {
                textPosition = effectPosition.GetPoint(-textDistance);
                fastLatePosition = effectPosition.GetPoint(-fastLateDistance);
            }
            _judgeTextDisplayer.LocalPosition = textPosition;
            _fastLateDisplayerA.LocalPosition = fastLatePosition;
            _fastLateDisplayerB.LocalPosition = textPosition;
            _isEnabled = MajInstances.Setting.Display.OuterJudgeDistance != 0 && DistanceRatio != 0;
        }
        public void Reset()
        {
            perfectDisplayer.SetActive(false);
            greatDisplayer.SetActive(false);
            goodDisplayer.SetActive(false);
        }
        /// <summary>
        /// 将Effect、Text和FastLate设置为非活动状态
        /// </summary>
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
            var isBreak = judgeResult.IsBreak;
            var result = judgeResult.Grade;
            if (!judgeResult.IsMissOrTooFast)
                Reset();
            else
                return;
            switch (result)
            {
                case JudgeGrade.LateGood:
                case JudgeGrade.FastGood:
                    if (isBreak)
                    {
                        perfectDisplayer.SetActive(true);
                        perfectAnim.speed = 0.9f;
                        perfectAnim.SetTrigger("bGood");
                    }
                    else
                        goodDisplayer.SetActive(true);
                    break;
                case JudgeGrade.LateGreat:
                case JudgeGrade.LateGreat1:
                case JudgeGrade.LateGreat2:
                case JudgeGrade.FastGreat2:
                case JudgeGrade.FastGreat1:
                case JudgeGrade.FastGreat:
                    if (isBreak)
                    {
                        perfectDisplayer.SetActive(true);
                        perfectAnim.speed = 0.9f;
                        perfectAnim.SetTrigger("bGreat");
                    }
                    else
                    {
                        greatDisplayer.SetActive(true);
                        greatAnim.SetTrigger("great");
                    }
                    break;
                case JudgeGrade.LatePerfect2:
                case JudgeGrade.FastPerfect2:
                case JudgeGrade.LatePerfect1:
                case JudgeGrade.FastPerfect1:
                case JudgeGrade.Perfect:
                    perfectDisplayer.SetActive(true);
                    if (isBreak)
                    {
                        perfectAnim.speed = 0.9f;
                        perfectAnim.SetTrigger("break");
                    }
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
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.NoteJudgeType, judgeResult);

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
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.NoteJudgeType, judgeResult);
                canPlay = canPlay && NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.FastLateType, judgeResult);
            }
            if (!canPlay)
                return canPlay;
            var skin = MajInstances.SkinManager.GetJudgeTextSkin();
            var isCritical = MajInstances.Setting.Display.DisplayCriticalPerfect;
            var displayBreakScore = MajInstances.Setting.Display.DisplayBreakScore;
            if (isBreak)
            {
                if(displayBreakScore)
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
                                else if(isFast)
                                {
                                    if(isCritical)
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
