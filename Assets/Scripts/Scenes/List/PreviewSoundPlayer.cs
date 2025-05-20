using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using ManagedBass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace MajdataPlay.List
{
#nullable enable
    public class PreviewSoundPlayer : MonoBehaviour
    {
        CancellationTokenSource? _cancellationTokenSource = null;
        public void PlayPreviewSound(ISongDetail info)
        {
            if (_cancellationTokenSource is not null)
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                    _cancellationTokenSource.Cancel();
            }
            _cancellationTokenSource = new();
            PlayPreviewAsync(info, _cancellationTokenSource.Token).Forget();
        }

        async UniTaskVoid PlayPreviewAsync(ISongDetail info, CancellationToken token)
        {

            var selectSound = MajInstances.AudioManager.GetSFX("bgm_select.mp3");
            selectSound.SetVolume(MajInstances.Settings.Audio.Volume.BGM);
            token.ThrowIfCancellationRequested();
            await UniTask.Delay(1000, cancellationToken: token, cancelImmediately: true);
            token.ThrowIfCancellationRequested();

            var previewSample = await info.GetPreviewAudioTrackAsync(token);

            try
            {
                if (previewSample is null || previewSample.IsEmpty)
                    throw new InvalidAudioTrackException("Failed to decode audio track", string.Empty);
                previewSample.SetVolume(MajInstances.Settings.Audio.Volume.BGM);
                //set sample.CurrentSec Not implmented
                previewSample.IsLoop = true;
                previewSample.CurrentSec = 0;
                previewSample.Play();
                token.ThrowIfCancellationRequested();
                await UniTask.Delay(500, cancellationToken: token, cancelImmediately: true);
                token.ThrowIfCancellationRequested();
                for (var i = 1f; i > 0; i = i - 0.2f)
                {
                    token.ThrowIfCancellationRequested();
                    selectSound.Volume = i * MajInstances.Settings.Audio.Volume.BGM;
                    await UniTask.Delay(100, cancellationToken: token, cancelImmediately: true);
                }
                while (true)
                {
                    await UniTask.Yield(token, cancelImmediately: true);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (previewSample is not null && !previewSample.IsEmpty)
                    previewSample.Pause();
            }
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            var selectSound = MajInstances.AudioManager.GetSFX("bgm_select.mp3");
            selectSound.SetVolume(MajInstances.Settings.Audio.Volume.BGM);
        }

    }
}