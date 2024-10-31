using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
#nullable enable
public class PreviewSoundPlayer : MonoBehaviour
{
    AudioSampleWrap? _previewSample; 
    CancellationTokenSource? _cancellationTokenSource = null;
    public void PlayPreviewSound(SongDetail info)
    {
        if(_cancellationTokenSource is not null)
        {
            if(!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();
        }
        _cancellationTokenSource = new();
        PlayPreviewAsync(info, _cancellationTokenSource.Token).Forget();
    }

    async UniTaskVoid PlayPreviewAsync(SongDetail info,CancellationToken token)
    {
        if(_previewSample is not null)
        {
            _previewSample.Stop();
            _previewSample.Dispose();
            _previewSample = null;
        }
        var selectSound = MajInstances.AudioManager.GetSFX(SFXSampleType.SELECT_BGM);
        selectSound.SetVolume(MajInstances.Setting.Audio.Volume.BGM);
        token.ThrowIfCancellationRequested();
        await UniTask.Delay(1000);
        token.ThrowIfCancellationRequested();
        var trackPath = info.TrackPath ?? string.Empty;
        if (!File.Exists(trackPath) && !info.isOnline)
            throw new AudioTrackNotFoundException(trackPath);
        _previewSample = await MajInstances.AudioManager.LoadMusicAsync(trackPath);
        if (_previewSample is null)
            throw new InvalidAudioTrackException("Failed to decode audio track", trackPath);
        _previewSample.SetVolume(MajInstances.Setting.Audio.Volume.BGM);
        //set sample.CurrentSec Not implmented
        _previewSample.IsLoop = true;
        _previewSample.Play();
        await UniTask.Delay(500);
        token.ThrowIfCancellationRequested();
        for (float i = 1f; i > 0; i = i - 0.2f)
        {
            token.ThrowIfCancellationRequested();
            selectSound.Volume = i * MajInstances.Setting.Audio.Volume.BGM;
            await UniTask.Delay(100);
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        if (_previewSample is not null)
        {
            _previewSample.Stop();
            _previewSample.Dispose();
            _previewSample = null;
        }
        var selectSound = MajInstances.AudioManager.GetSFX(SFXSampleType.SELECT_BGM);
        selectSound.SetVolume(MajInstances.Setting.Audio.Volume.BGM);
    }

}
