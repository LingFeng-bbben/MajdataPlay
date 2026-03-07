using MajdataPlay.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Settings.OptionEnumerators;
public sealed class LanguageEnumerator : OptionEnumeratorBase, IOptionEnumerator
{
    public override bool MoveNext()
    {
        if (IsReadOnly)
        {
            return false;
        }
        base.MoveNext();
        Localization.SetLang((string)Current);
        return true;
    }
    public override bool MovePrevious()
    {
        if (IsReadOnly)
        {
            return false;
        }
        base.MovePrevious();
        Localization.SetLang((string)Current);
        return true;
    }
    protected override void InitInternal()
    {
        var availableLangs = Localization.Available;
        if (availableLangs.Length == 0)
        {
            ValueIndex = 0;
            OptionValues = new object[] { "Unavailable" };
            IsReadOnly = true;
            Value = "Unavailable";
            return;
        }
        var langNames = availableLangs.Select(x => x.ToString())
                                      .ToArray();
        var currentLang = Localization.Current;
        OptionValues = langNames;
        var currentIndex = availableLangs.FindIndex(x => x == currentLang);
        if(currentIndex != -1)
        {
            currentIndex = 0;
        }
        ValueIndex = currentIndex;
    }
}
