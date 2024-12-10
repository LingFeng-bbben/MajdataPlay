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

                judgeTextDisplayer.LocalPosition = textPosition;
                fastLateDisplayer.LocalPosition = fastLatePosition;
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
        JudgeTextDisplayer judgeTextDisplayer;
        [SerializeField]
        FastLateDisplayer fastLateDisplayer;


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
            judgeTextDisplayer.LocalPosition = textPosition;
            fastLateDisplayer.LocalPosition = fastLatePosition;
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
            judgeTextDisplayer.Reset();
            fastLateDisplayer.Reset();
        }
        public void Play(in JudgeResult judgeResult)
        {
            PlayEffect(judgeResult);
            if(IsClassCAvailable(judgeResult))
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
            var isBreak = judgeResult.IsBreak;
            var result = judgeResult.Result;
            if (!judgeResult.IsMissOrTooFast)
                Reset();
            else
                return;
            switch (result)
            {
                case JudgeType.LateGood:
                case JudgeType.FastGood:
                    if (isBreak)
                    {
                        perfectDisplayer.SetActive(true);
                        perfectAnim.speed = 0.9f;
                        perfectAnim.SetTrigger("bGood");
                    }
                    else
                        goodDisplayer.SetActive(true);
                    break;
                case JudgeType.LateGreat:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat2:
                case JudgeType.FastGreat2:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat:
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
                case JudgeType.LatePerfect2:
                case JudgeType.FastPerfect2:
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                case JudgeType.Perfect:
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
        void PlayResult(in JudgeResult judgeResult)
        {
            bool canPlay;
            if (judgeResult.IsBreak)
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.BreakJudgeType, judgeResult);
            else
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.NoteJudgeType, judgeResult);

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
