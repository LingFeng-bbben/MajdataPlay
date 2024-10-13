using MajdataPlay.Types;
using UnityEngine;
using MajdataPlay.IO;
using MajdataPlay.Utils;
#nullable enable
namespace MajdataPlay.Game
{
    public class NoteAudioManager : MonoBehaviour
    {
        AudioManager audioManager => MajInstances.AudioManager;
        void Awake()
        {
            MajInstanceHelper<NoteAudioManager>.Instance = this;
        }
        void OnDestroy()
        {
            MajInstanceHelper<NoteAudioManager>.Free();
        }
        public void PlayTapSound(in JudgeResult judgeResult)
        {
            var isBreak = judgeResult.IsBreak;
            var isEx = judgeResult.IsEX;

            if (judgeResult.IsMiss)
                return;
            else if (isBreak)
            {
                PlayBreakTapSound(judgeResult);
                return;
            }
            else if (isEx)
            {
                audioManager.PlaySFX(SFXSampleType.JUDGE_EX);
                return;
            }

            switch (judgeResult.Result)
            {
                case JudgeType.LateGood:
                case JudgeType.FastGood:
                    audioManager.PlaySFX(SFXSampleType.GOOD);
                    break;
                case JudgeType.LateGreat:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat2:
                case JudgeType.FastGreat2:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat:
                    audioManager.PlaySFX(SFXSampleType.GREAT);
                    break;
                case JudgeType.LatePerfect2:
                case JudgeType.FastPerfect2:
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                    audioManager.PlaySFX(SFXSampleType.JUDGE);
                    break;
                case JudgeType.Perfect:
                    if (isBreak)
                        audioManager.PlaySFX(SFXSampleType.BREAK);
                    else
                        audioManager.PlaySFX(SFXSampleType.JUDGE);
                    break;
            }
        }
        void PlayBreakTapSound(in JudgeResult judgeResult)
        {
            switch (judgeResult.Result)
            {
                case JudgeType.LateGood:
                case JudgeType.FastGood:
                case JudgeType.LateGreat:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat2:
                case JudgeType.FastGreat2:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat:
                case JudgeType.LatePerfect2:
                case JudgeType.FastPerfect2:
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                    audioManager.PlaySFX(SFXSampleType.JUDGE_BREAK);
                    break;
                case JudgeType.Perfect:
                    audioManager.PlaySFX(SFXSampleType.BREAK);
                    audioManager.PlaySFX(SFXSampleType.JUDGE_BREAK);
                    break;
            }
        }

        public void PlayTouchSound()
        {
            audioManager.PlaySFX(SFXSampleType.TOUCH);
        }

        public void PlayHanabiSound()
        {
            audioManager.PlaySFX(SFXSampleType.HANABI);
        }
        public void PlayTouchHoldSound()
        {
            audioManager.PlaySFX(SFXSampleType.TOUCH_HOLD_RISER);
        }
        public void StopTouchHoldSound()
        {
            audioManager.StopSFX(SFXSampleType.TOUCH_HOLD_RISER);
        }

        public void PlaySlideSound(bool isBreak)
        {
            if (isBreak)
            {
                audioManager.PlaySFX(SFXSampleType.BREAK_SLIDE_START);
            }
            else
            {
                audioManager.PlaySFX(SFXSampleType.SLIDE);
            }
        }

        public void PlayBreakSlideEndSound()
        {
            audioManager.PlaySFX(SFXSampleType.BREAK_SLIDE);
        }
    }
}