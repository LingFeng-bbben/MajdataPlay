using MajdataPlay.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MajdataPlay.IO;

public class NoteAudioManager : MonoBehaviour
{
    AudioManager audioManager =>AudioManager.Instance;
    public void PlayTapSound(bool isBreak,bool isEx, JudgeType judge)
    {
        switch (judge)
        {
            case JudgeType.LateGood:
            case JudgeType.FastGood:
                audioManager.PlaySFX("good.wav");
                break;
            case JudgeType.LateGreat:
            case JudgeType.LateGreat1:
            case JudgeType.LateGreat2:
            case JudgeType.FastGreat2:
            case JudgeType.FastGreat1:
            case JudgeType.FastGreat:
                audioManager.PlaySFX("great.wav");
                break;
            case JudgeType.LatePerfect2:
            case JudgeType.FastPerfect2:
            case JudgeType.LatePerfect1:
            case JudgeType.FastPerfect1:
                audioManager.PlaySFX("judge.wav");
                break;
            case JudgeType.Perfect:
                if(isBreak)
                    audioManager.PlaySFX("break.wav");
                else
                    audioManager.PlaySFX("judge.wav");
                break;
            default:
                
                break;
        }
        if (judge != JudgeType.Miss && isBreak)
        {
            audioManager.PlaySFX("judge_break.wav");
        }
        if (judge != JudgeType.Miss && isEx)
        {
            audioManager.PlaySFX("judge_ex.wav");
        }
    }

    public void PlayTouchSound()
    {
        audioManager.PlaySFX("touch.wav");
    }

    public void PlayHanabiSound()
    {
        audioManager.PlaySFX("hanabi.wav");
    }
    public void PlayTouchHoldSound()
    {
        audioManager.PlaySFX("touchHold_riser.wav");
    }
    public void StopTouchHoldSound()
    {
        audioManager.StopSFX("touchHold_riser.wav");
    }

    public void PlaySlideSound(bool isBreak)
    {
        if (isBreak)
        {
            audioManager.PlaySFX("break_slide_start.wav");
        }
        else
        {
            audioManager.PlaySFX("slide.wav");
        }
    }

    public void PlayBreakSlideEndSound()
    {
        audioManager.PlaySFX("break_slide.wav");
    }
}
