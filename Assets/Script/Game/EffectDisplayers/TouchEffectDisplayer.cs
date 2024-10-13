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
        public void PlayEffect(in JudgeResult judgeResult)
        {
            var result = judgeResult.Result;
            if (!judgeResult.IsMiss)
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
        public void PlayResult(in JudgeResult judgeResult)
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
        public void PlayFastLate(in JudgeResult judgeResult)
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
    }
}
