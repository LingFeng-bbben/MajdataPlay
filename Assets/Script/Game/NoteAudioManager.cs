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
                audioManager.PlaySFX("tap_ex.wav");
                return;
            }

            switch (judgeResult.Result)
            {
                case JudgeType.LateGood:
                case JudgeType.FastGood:
                    audioManager.PlaySFX("tap_good.wav");
                    break;
                case JudgeType.LateGreat:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat2:
                case JudgeType.FastGreat2:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat:
                    audioManager.PlaySFX("tap_great.wav");
                    break;
                case JudgeType.LatePerfect2:
                case JudgeType.FastPerfect2:
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                    audioManager.PlaySFX("tap_perfect.wav");
                    break;
                case JudgeType.Perfect:
                    audioManager.PlaySFX("tap_perfect.wav");
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
                    audioManager.PlaySFX("break_tap.wav");
                    break;
                case JudgeType.Perfect:
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