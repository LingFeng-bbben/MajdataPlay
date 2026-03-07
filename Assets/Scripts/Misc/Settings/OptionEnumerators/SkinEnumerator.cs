using MajdataPlay.Collections;
using System.Linq;


namespace MajdataPlay.Settings.OptionEnumerators;
public sealed class SkinEnumerator : OptionEnumeratorBase, IOptionEnumerator
{
    int _lastValueIndex = 0;
    SkinManager _skinManager;
    public override bool MoveNext()
    {
        if (IsReadOnly)
        {
            return false;
        }
        base.MoveNext();
        SetSkin();
        _lastValueIndex = ValueIndex;
        return true;
    }
    public override bool MovePrevious()
    {
        if (IsReadOnly)
        {
            return false;
        }
        base.MovePrevious();
        SetSkin();
        _lastValueIndex = ValueIndex;
        return true;
    }
    void SetSkin()
    {
        if(_lastValueIndex == ValueIndex)
        {
            return;
        }
        var skins = _skinManager.LoadedSkins;
        var newSkin = skins.Find(x => x.Name == OptionValues[ValueIndex].ToString());
        _skinManager.SelectedSkin = newSkin;
    }
    protected override void InitInternal()
    {
        _skinManager = MajInstances.SkinManager;
        var skinNames = _skinManager.LoadedSkins.Select(x => x.Name)
                                                .ToArray();
        var currentSkin = _skinManager.SelectedSkin;
        OptionValues = skinNames;
        var currentIndex = skinNames.FindIndex(x => x == currentSkin.Name);
        if(currentIndex == -1)
        {
            currentIndex = 0;
        }
        ValueIndex = currentIndex;
        _lastValueIndex = currentIndex;
    }
}
