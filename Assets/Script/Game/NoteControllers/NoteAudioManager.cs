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

            if (judgeResult.IsMissOrTooFast)
                return;
            else if (isBreak)
            {
                PlayBreakTapSound(judgeResult);
                return;
            }
            else if (isEx)
            {
                audioManager.PlaySFX("tap_ex.wav");
                return;
            }

            switch (judgeResult.Grade)
            {
                case JudgeGrade.LateGood:
                case JudgeGrade.FastGood:
                    audioManager.PlaySFX("tap_good.wav");
                    break;
                case JudgeGrade.LateGreat:
                case JudgeGrade.LateGreat1:
                case JudgeGrade.LateGreat2:
                case JudgeGrade.FastGreat2:
                case JudgeGrade.FastGreat1:
                case JudgeGrade.FastGreat:
                    audioManager.PlaySFX("tap_great.wav");
                    break;
                case JudgeGrade.LatePerfect2:
                case JudgeGrade.FastPerfect2:
                case JudgeGrade.LatePerfect1:
                case JudgeGrade.FastPerfect1:
                    audioManager.PlaySFX("tap_perfect.wav");
                    break;
                case JudgeGrade.Perfect:
                    audioManager.PlaySFX("tap_perfect.wav");
                    break;
            }
        }
        void PlayBreakTapSound(in JudgeResult judgeResult)
        {
            switch (judgeResult.Grade)
            {
                case JudgeGrade.LateGood:
                case JudgeGrade.FastGood:
                case JudgeGrade.LateGreat:
                case JudgeGrade.LateGreat1:
                case JudgeGrade.LateGreat2:
                case JudgeGrade.FastGreat2:
                case JudgeGrade.FastGreat1:
                case JudgeGrade.FastGreat:
                case JudgeGrade.LatePerfect2:
                case JudgeGrade.FastPerfect2:
                case JudgeGrade.LatePerfect1:
                case JudgeGrade.FastPerfect1:
                    audioManager.PlaySFX("break_tap.wav");
                    break;
                case JudgeGrade.Perfect:
                    audioManager.PlaySFX("break.wav");
                    audioManager.PlaySFX("break_tap.wav");
                    break;
            }
        }

        public void PlayTouchSound()
        {
            audioManager.PlaySFX("touch.wav");
        }

        public void PlayHanabiSound()
        {
            audioManager.PlaySFX("touch_hanabi.wav");
        }
        public void PlayTouchHoldSound()
        {
            var riser = audioManager.GetSFX("touch_Hold_riser.wav");
            if(!riser.IsPlaying)
                audioManager.PlaySFX("touch_Hold_riser.wav");
        }
        public void StopTouchHoldSound()
        {
            audioManager.StopSFX("touch_Hold_riser.wav");
        }

        public void PlaySlideSound(bool isBreak)
        {
            if (isBreak)
            {
                audioManager.PlaySFX("slide_break_start.wav");
            }
            else
            {
                audioManager.PlaySFX("slide.wav");
            }
        }

        public void PlayBreakSlideEndSound()
        {
            audioManager.PlaySFX("slide_break_slide.wav");
            audioManager.PlaySFX("break_slide.wav");
        }
    }
}