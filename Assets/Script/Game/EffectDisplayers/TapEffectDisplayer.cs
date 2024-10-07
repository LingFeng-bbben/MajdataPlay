using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    public sealed class TapEffectDisplayer: MonoBehaviour
    {
        public Vector3 Position => perfectDisplayer.transform.position;
        public Vector3 LocalPosition
        {
            get => perfectDisplayer.transform.localPosition;
            set
            {
                perfectDisplayer.transform.localPosition = value;
                greatDisplayer.transform.localPosition = value;
                goodDisplayer.transform.localPosition = value;
                var effectPosition = value;
                var textDistance = 1f * (2 - DistanceRatio);
                var fastLateDistance = textDistance + 0.66f;

                Vector3 textPosition;
                Vector3 fastLatePosition;
                if (effectPosition.magnitude == 0)
                {
                    textPosition = effectPosition.GetPoint(new Vector3(0, -0.01f, 0), -textDistance);
                    fastLatePosition = effectPosition.GetPoint(new Vector3(0, -0.01f, 0) ,- fastLateDistance);
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
            var effectPosition = perfectDisplayer.transform.localPosition;
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
        public void PlayEffect(in JudgeResult judgeResult)
        {
            var isBreak = judgeResult.IsBreak;
            var result = judgeResult.Result;
            if (!judgeResult.IsMiss)
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
        public void PlayResult(in JudgeResult judgeResult)
        {
            bool canPlay;
            if (judgeResult.IsBreak)
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(GameManager.Instance.Setting.Display.BreakJudgeType, judgeResult);
            else
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(GameManager.Instance.Setting.Display.NoteJudgeType, judgeResult);

            if (!canPlay)
            {
                judgeTextDisplayer.Reset();
                return;
            }
            judgeTextDisplayer.Play(judgeResult);
        }
        public void PlayFastLate(in JudgeResult judgeResult)
        {
            bool canPlay;
            if (judgeResult.IsBreak)
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(GameManager.Instance.Setting.Display.BreakFastLateType, judgeResult);
            else
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(GameManager.Instance.Setting.Display.FastLateType, judgeResult);
            if (!canPlay)
            {
                fastLateDisplayer.Reset();
                return;
            }
            fastLateDisplayer.Play(judgeResult);
        }
    }
}
