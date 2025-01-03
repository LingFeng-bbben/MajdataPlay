using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using ManagedBass;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
#nullable enable
public class PreviewSoundPlayer : MonoBehaviour
{
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

        var selectSound = MajInstances.AudioManager.GetSFX("bgm_select.mp3");
        selectSound.SetVolume(MajInstances.Setting.Audio.Volume.BGM);
        token.ThrowIfCancellationRequested();
        await UniTask.Delay(1000,cancellationToken: token, cancelImmediately:true);
        token.ThrowIfCancellationRequested();
        var trackPath = info.TrackPath ?? string.Empty;
        if (!File.Exists(trackPath) && !info.IsOnline)
            throw new AudioTrackNotFoundException(trackPath);
        using (var previewSample = await MajInstances.AudioManager.LoadMusicAsync(trackPath,speedChange:true))
        {
            if (previewSample is null)
                throw new InvalidAudioTrackException("Failed to decode audio track", trackPath);
            previewSample.SetVolume(MajInstances.Setting.Audio.Volume.BGM);
            //set sample.CurrentSec Not implmented
            previewSample.IsLoop = true;
            previewSample.CurrentSec = 0;
            previewSample.Play();
            token.ThrowIfCancellationRequested();
            await UniTask.Delay(500, cancellationToken: token, cancelImmediately: true);
            token.ThrowIfCancellationRequested();
            for (float i = 1f; i > 0; i = i - 0.2f)
            {
                token.ThrowIfCancellationRequested();
                selectSound.Volume = i * MajInstances.Setting.Audio.Volume.BGM;
                await UniTask.Delay(100, cancellationToken: token, cancelImmediately: true);
            }
            while (true)
            {
                await UniTask.Yield(token, cancelImmediately: true);
            }
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        var selectSound = MajInstances.AudioManager.GetSFX("bgm_select.mp3");
        selectSound.SetVolume(MajInstances.Setting.Audio.Volume.BGM);
    }

}
