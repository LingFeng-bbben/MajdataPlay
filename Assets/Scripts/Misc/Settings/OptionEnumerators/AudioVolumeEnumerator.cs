using MajdataPlay.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Settings.OptionEnumerators;
public sealed class AudioVolumeEnumerator : DefaultNumberEnumerator, IOptionEnumerator
{
    decimal _lastValue = 0;
    AudioManager _audioManager;
    public override void OnUpdate()
    {
        if(_lastValue != CurrentValue)
        {
            UpdateVolume();
            _lastValue = CurrentValue;
        }        
    }
    protected override void InitInternal()
    {
        base.InitInternal();
        _lastValue = CurrentValue;
        _audioManager = MajInstances.AudioManager;
    }
    void UpdateVolume()
    {
        _audioManager.ReadVolumeFromSettings();
    }
}
