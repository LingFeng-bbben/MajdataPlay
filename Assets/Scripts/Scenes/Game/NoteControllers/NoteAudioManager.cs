using MajdataPlay.Types;
using UnityEngine;
using MajdataPlay.IO;
using MajdataPlay.Utils;
#nullable enable
namespace MajdataPlay.Game
{
    public class NoteAudioManager : MonoBehaviour
    {
        readonly AudioManager _audioManager = MajInstances.AudioManager;

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
                _audioManager.PlaySFX("tap_ex.wav");
                return;
            }

            switch (judgeResult.Grade)
            {
                case JudgeGrade.LateGood:
                case JudgeGrade.FastGood:
                    _audioManager.PlaySFX("tap_good.wav");
                    break;
                case JudgeGrade.LateGreat:
                case JudgeGrade.LateGreat2nd:
                case JudgeGrade.LateGreat3rd:
                case JudgeGrade.FastGreat3rd:
                case JudgeGrade.FastGreat2nd:
                case JudgeGrade.FastGreat:
                    _audioManager.PlaySFX("tap_great.wav");
                    break;
                case JudgeGrade.LatePerfect3rd:
                case JudgeGrade.FastPerfect3rd:
                case JudgeGrade.LatePerfect2nd:
                case JudgeGrade.FastPerfect2nd:
                    _audioManager.PlaySFX("tap_perfect.wav");
                    break;
                case JudgeGrade.Perfect:
                    _audioManager.PlaySFX("tap_perfect.wav");
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
                case JudgeGrade.LateGreat2nd:
                case JudgeGrade.LateGreat3rd:
                case JudgeGrade.FastGreat3rd:
                case JudgeGrade.FastGreat2nd:
                case JudgeGrade.FastGreat:
                case JudgeGrade.LatePerfect3rd:
                case JudgeGrade.FastPerfect3rd:
                case JudgeGrade.LatePerfect2nd:
                case JudgeGrade.FastPerfect2nd:
                    _audioManager.PlaySFX("break_tap.wav");
                    break;
                case JudgeGrade.Perfect:
                    _audioManager.PlaySFX("break.wav");
                    _audioManager.PlaySFX("break_tap.wav");
                    break;
            }
        }

        public void PlayTouchSound()
        {
            _audioManager.PlaySFX("touch.wav");
        }

        public void PlayHanabiSound()
        {
            _audioManager.PlaySFX("touch_hanabi.wav");
        }
        public void PlayTouchHoldSound()
        {
            var riser = _audioManager.GetSFX("touch_Hold_riser.wav");
            if(!riser.IsPlaying)
                _audioManager.PlaySFX("touch_Hold_riser.wav");
        }
        public void StopTouchHoldSound()
        {
            _audioManager.StopSFX("touch_Hold_riser.wav");
        }

        public void PlaySlideSound(bool isBreak)
        {
            if (isBreak)
            {
                _audioManager.PlaySFX("slide_break_start.wav");
            }
            else
            {
                _audioManager.PlaySFX("slide.wav");
            }
        }

        public void PlayBreakSlideEndSound()
        {
            _audioManager.PlaySFX("slide_break_slide.wav");
            _audioManager.PlaySFX("break_slide.wav");
        }
    }
}