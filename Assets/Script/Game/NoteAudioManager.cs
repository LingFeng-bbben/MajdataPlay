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
                if(isBreak)
                    audioManager.PlaySFX(SFXSampleType.BREAK);
                else
                    audioManager.PlaySFX(SFXSampleType.JUDGE);
                break;
            default:
                
                break;
        }
        if (judge != JudgeType.Miss && isBreak)
        {
            audioManager.PlaySFX(SFXSampleType.JUDGE_BREAK);
        }
        if (judge != JudgeType.Miss && isEx)
        {
            audioManager.PlaySFX(SFXSampleType.JUDGE_EX);
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
