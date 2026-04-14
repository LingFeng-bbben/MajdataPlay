using System;

namespace MajdataPlay.Settings.OptionEnumerators;
public class DefaultBooleanEnumerator : OptionEnumeratorBase, IOptionEnumerator
{
    protected override void InitInternal()
    {
        if (Type != typeof(bool) && Type != typeof(bool?))
        {
            MajDebug.LogError($"[SettingUI]Invalid boolean type: {Type}");
            throw new InvalidOperationException("Type provided must be an Boolean");
        }
        if (IsOptional)
        {
            OptionValues = new object[3]
            {
                false, true, null
            };
        }
        else
        {
            OptionValues = new object[2]
            {
                false, true
            };
        }
        InitValueTexts();
        var value = Value;
        if(value is true)
        {
            ValueIndex = 1;
        }
        else if(value is null)
        {
            ValueIndex = 2;
        }
        else
        {
            ValueIndex = 0;
        }
    }
}
