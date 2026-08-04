using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Settings.OptionEnumerators;
public sealed class EngineBooleanSettingEnumerator : DefaultBooleanEnumerator, IOptionEnumerator
{
    bool _lastValue = false;

    public override void OnUpdate()
    {
        var currentValue = (bool)Value;
        if (_lastValue == currentValue)
        {
            return;
        }
        switch (Name)
        {
            case "VSync":
                QualitySettings.vSyncCount = currentValue ? 1 : 0;
                break;
        }
        _lastValue = currentValue;
    }
    protected override void InitInternal()
    {
        base.InitInternal();
        _lastValue = (bool)Value;
    }
}
