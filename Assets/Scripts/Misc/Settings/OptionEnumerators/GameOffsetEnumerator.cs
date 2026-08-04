using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MajdataPlay.Settings.OptionEnumerators;
public sealed class GameOffsetEnumerator : DefaultNumberEnumerator, IOptionEnumerator, IDisposable
{
    OffsetUnitOption _lastOffsetUnit;
    public override void OnUpdate()
    {
        CheckOffsetUnit();
    }
    public void Dispose()
    {
        CheckOffsetUnit();
    }
    protected override void InitInternal()
    {
        base.InitInternal();
        _lastOffsetUnit = MajEnv.Settings.Debug.OffsetUnit;
        UpdateOptionStep();
    }
    void CheckOffsetUnit()
    {
        var currentOffsetUnit = MajEnv.Settings.Debug.OffsetUnit;
        if (currentOffsetUnit == _lastOffsetUnit)
        {
            return;
        }
        else if (currentOffsetUnit == OffsetUnitOption.Second)
        {
            CurrentValue = Math.Round((decimal)MajEnv.FRAME_LENGTH_SEC * CurrentValue, 3);
            var valueToSet = (object)CurrentValue;
            OptionValues[0] = valueToSet;
            Value = Convert.ChangeType(valueToSet, Type);
            ChartSettingStorage.ConvertUnitToSecond();
        }
        else
        {
            CurrentValue = Math.Round(CurrentValue / (decimal)MajEnv.FRAME_LENGTH_SEC, 1);
            var valueToSet = (object)CurrentValue;
            OptionValues[0] = valueToSet;
            Value = Convert.ChangeType(valueToSet, Type);
            ChartSettingStorage.ConvertUnitToFrame();
        }
        UpdateOptionStep();
        OptionValues[0] = Value;
        UpdateValueText();
        _lastOffsetUnit = currentOffsetUnit;
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
