using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MajdataPlay.Settings.OptionEnumerators;
public sealed class GameOffsetEnumerator : DefaultNumberEnumerator, IOptionEnumerator
{
    OffsetUnitOption _lastOffsetUnit;
    public override void OnUpdate()
    {
        var currentOffsetUnit = MajEnv.Settings.Debug.OffsetUnit;
        if (currentOffsetUnit == _lastOffsetUnit)
        {
            return;
        }
        else if (currentOffsetUnit == OffsetUnitOption.Second)
        {
            MajEnv.Settings.Judge.AudioOffset = MathF.Round(MajEnv.FRAME_LENGTH_SEC * MajEnv.Settings.Judge.AudioOffset, 3);
            MajEnv.Settings.Judge.JudgeOffset = MathF.Round(MajEnv.FRAME_LENGTH_SEC * MajEnv.Settings.Judge.JudgeOffset, 3);
            MajEnv.Settings.Judge.TouchPanelOffset = MathF.Round(MajEnv.FRAME_LENGTH_SEC * MajEnv.Settings.Judge.TouchPanelOffset, 3);
            MajEnv.Settings.Judge.AnswerOffset = MathF.Round(MajEnv.FRAME_LENGTH_SEC * MajEnv.Settings.Judge.AnswerOffset, 3);
            MajEnv.Settings.Game.SlideFadeInOffset = MathF.Round(MajEnv.FRAME_LENGTH_SEC * MajEnv.Settings.Game.SlideFadeInOffset, 3);
            MajEnv.Settings.Debug.DisplayOffset = MathF.Round(MajEnv.FRAME_LENGTH_SEC * MajEnv.Settings.Debug.DisplayOffset, 3);
            ChartSettingStorage.ConvertUnitToSecond();
        }
        else
        {
            MajEnv.Settings.Judge.AudioOffset = MathF.Round(MajEnv.Settings.Judge.AudioOffset / MajEnv.FRAME_LENGTH_SEC, 1);
            MajEnv.Settings.Judge.JudgeOffset = MathF.Round(MajEnv.Settings.Judge.JudgeOffset / MajEnv.FRAME_LENGTH_SEC, 1);
            MajEnv.Settings.Judge.TouchPanelOffset = MathF.Round(MajEnv.Settings.Judge.TouchPanelOffset / MajEnv.FRAME_LENGTH_SEC, 1);
            MajEnv.Settings.Judge.AnswerOffset = MathF.Round(MajEnv.Settings.Judge.AnswerOffset / MajEnv.FRAME_LENGTH_SEC, 1);
            MajEnv.Settings.Game.SlideFadeInOffset = MathF.Round(MajEnv.Settings.Game.SlideFadeInOffset / MajEnv.FRAME_LENGTH_SEC, 1);
            MajEnv.Settings.Debug.DisplayOffset = MathF.Round(MajEnv.Settings.Debug.DisplayOffset / MajEnv.FRAME_LENGTH_SEC, 1);
            ChartSettingStorage.ConvertUnitToFrame();
        }
        UpdateOptionStep();
        _lastOffsetUnit = currentOffsetUnit;
    }
    protected override void InitInternal()
    {
        base.InitInternal();
        _lastOffsetUnit = MajEnv.Settings.Debug.OffsetUnit;
        UpdateOptionStep();
    }
    void UpdateOptionStep()
    {
        switch (Name)
        {
            case "AudioOffset":
            case "JudgeOffset":
            case "AnswerOffset":
            case "TouchPanelOffset":
            case "SlideFadeInOffset":
                {
                    if (MajEnv.Settings.Debug.OffsetUnit == OffsetUnitOption.Second)
                    {
                        MaxValue = null;
                        MinValue = null;
                        Step = 0.001m;
                    }
                    else
                    {
                        MaxValue = null;
                        MinValue = null;
                        Step = 0.1m;
                    }
                }
                break;
            case "DisplayOffset":
                {
                    if (MajEnv.Settings.Debug.OffsetUnit == OffsetUnitOption.Second)
                    {
                        MaxValue = null;
                        MinValue = 0;
                        Step = 0.001m;
                    }
                    else
                    {
                        MaxValue = null;
                        MinValue = 0;
                        Step = 0.1m;
                    }
                }
                break;
        }
    }
}
