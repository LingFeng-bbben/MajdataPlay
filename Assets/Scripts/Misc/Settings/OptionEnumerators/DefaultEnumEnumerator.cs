using MajdataPlay.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Settings.OptionEnumerators;
public class DefaultEnumEnumerator : OptionEnumeratorBase, IOptionEnumerator
{
    protected override void InitInternal()
    {
        if (!Type.IsEnum)
        {
            throw new InvalidOperationException("Type provided must be an Enum");
        }
        var values = Enum.GetValues(Type);
        OptionValues = new object[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            OptionValues[i] = values.GetValue(i);
        }
        var value = Value;
        ValueIndex = OptionValues.FindIndex(x => (int)x == (int)value);
    }
}
