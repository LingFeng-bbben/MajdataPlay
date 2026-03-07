using System;

namespace MajdataPlay.Settings.OptionEnumerators;
public class DefaultBooleanEnumerator : OptionEnumeratorBase, IOptionEnumerator
{
    protected override void InitInternal()
    {
        if (Type != typeof(bool))
        {
            throw new InvalidOperationException("Type provided must be an Boolean");
        }
        OptionValues = new object[2]
        {
            false, true
        };
        var value = Value;
        if(value is true)
        {
            ValueIndex = 1;
        }
        else
        {
            ValueIndex = 0;
        }
    }
}
