using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PreviewSoundPlayer : MonoBehaviour
{
    AudioSampleWrap sample; 
    public void PlayPreviewSound(SongDetail info)
    {
        StopAllCoroutines();
        StartCoroutine(DelayPlayPreview(info));
    }

    IEnumerator DelayPlayPreview(SongDetail info)
    {
        if(sample != null)
        {
            sample.Stop();
            sample.Dispose();
            sample = null;
        }
        var selectSound = MajInstances.AudioManager.GetSFX(SFXSampleType.SELECT_BGM);
        selectSound.SetVolume(MajInstances.Setting.Audio.Volume.BGM);

        //if (info.isOnline) yield break;

        yield return new WaitForSeconds(1f);

        var trackPath = info.TrackPath ?? string.Empty;
        if (!File.Exists(trackPath) && !info.isOnline)
            throw new AudioTrackNotFoundException(trackPath);
        sample =  MajInstances.AudioManager.LoadMusic(trackPath);
        if (sample is null)
            throw new InvalidAudioTrackException("Failed to decode audio track", trackPath);
        sample.SetVolume(MajInstances.Setting.Audio.Volume.BGM);
        //set sample.CurrentSec Not implmented
        sample.IsLoop = true;
        sample.Play();
        yield return new WaitForSeconds(0.5f);
        for (float i = 1f; i > 0; i = i - 0.2f)
        {
            selectSound.Volume = i * MajInstances.Setting.Audio.Volume.BGM;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnDestroy()
    {
        sample.Stop();
        sample.Dispose();
        sample = null;
        var selectSound = MajInstances.AudioManager.GetSFX(SFXSampleType.SELECT_BGM);
        selectSound.SetVolume(MajInstances.Setting.Audio.Volume.BGM);

    }

}
