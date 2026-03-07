using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MajdataPlay.Settings.OptionEnumerators;
public sealed class EngineEnumSettingEnumerator: DefaultEnumEnumerator, IOptionEnumerator
{
    object _lastValue;
    public override void OnUpdate()
    {
        if (Current == _lastValue)
        {
            return;
        }
        switch (PropertyInfo.Name)
        {
            case "RenderQuality":
                QualitySettings.SetQualityLevel(Convert.ToInt32(Current), true);
                break;
        }
        _lastValue = Current;
    }
    protected override void InitInternal()
    {
        base.InitInternal();
        _lastValue = Current;
    }
}
