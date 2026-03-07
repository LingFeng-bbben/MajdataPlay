using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Settings.OptionEnumerators;
public sealed class DefaultReadOnlyEnumerator : OptionEnumeratorBase, IOptionEnumerator
{
    protected override void InitInternal()
    {
        IsReadOnly = true;
        OptionValues = new object[1]
        {
            Value
        };
        ValueIndex = 0;
    }
}
