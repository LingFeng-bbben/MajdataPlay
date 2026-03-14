using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using ManagedBass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Scenes.List
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
                {
                    _cancellationTokenSource.Cancel();
                }
            }
            _cancellationTokenSource = new();
            ListManager.AllBackgroundTasks.Add(PlayPreviewAsync(info, _cancellationTokenSource.Token));
        }
        async Task PlayPreviewAsync(ISongDetail info, CancellationToken token)
        {

            var selectSound = MajInstances.AudioManager.GetSFX("bgm_select.mp3");
            selectSound.SetVolume(MajInstances.Settings.Audio.Volume.BGM);
            token.ThrowIfCancellationRequested();
            await UniTask.Delay(1000, cancellationToken: token, cancelImmediately: true);
            token.ThrowIfCancellationRequested();

            var simaiChart = await info.GetMaidataAsync(token: token);
            var previewSample = await info.GetPreviewAudioTrackAsync(token: token);

            try
            {
                var previewOffsetSec = -1f;
                var previewLengthSec = -1f;
                if (previewSample is null || previewSample.IsEmpty)
                {
                    throw new InvalidAudioTrackException("Failed to decode audio track", string.Empty);
                }
                for (var i = 0; i < simaiChart.Commands.Count; i++)
                {
                    var command = simaiChart.Commands[i];
                    switch(command.Prefix)
                    {
                        case "demo_seek":
                            if (float.TryParse(command.Value, out var offsetSec))
                            {
                                if(previewOffsetSec != -1)
                                {
                                    MajDebug.LogWarning($"Multiple \"&demo_seek\" commands found. Previous value: {previewOffsetSec}, new value: {offsetSec}. Ignored.");
                                }
                                else if(offsetSec < 0)
                                {
                                    MajDebug.LogWarning($"Invalid \"&demo_seek\" value: {offsetSec}. Value must be non-negative. Ignored.");
                                }
                                else
                                {
                                    previewOffsetSec = offsetSec;
                                }
                            }
                            else
                            {
                                MajDebug.LogWarning($"Failed to parse \"&demo_seek\" value: {command.Value}");
                            }
                            break;
                        case "demo_len":
                            if (float.TryParse(command.Value, out var lenSec))
                            {
                                if (previewLengthSec != -1)
                                {
                                    MajDebug.LogWarning($"Multiple \"&demo_len\" commands found. Previous value: {previewOffsetSec}, new value: {lenSec}, ignored");
                                }
                                else if (lenSec <= 0)
                                {
                                    MajDebug.LogWarning($"Invalid \"&demo_len\" value: {lenSec}. Value must be positive. Ignored.");
                                }
                                else
                                {
                                    previewLengthSec = lenSec;
                                }
                            }
                            else
                            {
                                MajDebug.LogWarning($"Failed to parse \"&demo_len\" value: {command.Value}");
                            }
                            break;
                    }
                }
                if(previewOffsetSec == -1)
                {
                    previewOffsetSec = 0;
                }
                else
                {
                    if(previewOffsetSec >= (float)previewSample.Length.TotalSeconds)
                    {
                        previewOffsetSec = 0;
                    }    
                }
                if (previewLengthSec == -1)
                {
                    previewLengthSec = (float)previewSample.Length.TotalSeconds;
                }
                else
                {
                    previewLengthSec = Math.Min(previewLengthSec, (float)previewSample.Length.TotalSeconds - previewOffsetSec);
                    previewLengthSec = Math.Max(0, previewLengthSec);
                }
                MajDebug.LogDebug($"Playing preview song\nOffset: {previewOffsetSec}s\nLength: {previewLengthSec}s");
                previewSample.SetVolume(MajInstances.Settings.Audio.Volume.BGM);
                //set sample.CurrentSec Not implmented
                previewSample.IsLoop = true;
                if(previewSample.CanSeek)
                {
                    previewSample.CurrentSec = previewOffsetSec;
                }
                else
                {
                    previewSample.Stop();
                }
                previewSample.Speed = 1.0f;
                previewSample.Play();
                token.ThrowIfCancellationRequested();
                await UniTask.Delay(500, cancellationToken: token, cancelImmediately: true);
                token.ThrowIfCancellationRequested();
                for (var i = 1f; i > 0; i = i - 0.2f)
                {
                    token.ThrowIfCancellationRequested();
                    selectSound.Volume = i * GetVolume();
                    await UniTask.Delay(100, cancellationToken: token, cancelImmediately: true);
                }
                while (true)
                {
                    if(previewSample.CanSeek)
                    {
                        var currentSec = previewSample.CurrentSec;
                        if (previewLengthSec != 0)
                        {
                            if (currentSec - (previewOffsetSec + previewLengthSec) > -0.5f)
                            {
                                for (var i = 1f; i > 0; i = i - 0.2f)
                                {
                                    token.ThrowIfCancellationRequested();
                                    previewSample.Volume = i * GetVolume();
                                    await UniTask.Delay(100, cancellationToken: token, cancelImmediately: true);
                                }
                                previewSample.Pause();
                                await UniTask.Delay(1000, cancellationToken: token, cancelImmediately: true);
                                previewSample.Volume = GetVolume();
                                previewSample.CurrentSec = previewOffsetSec;
                                previewSample.Play();
                            }
                        }
                    }
                    previewSample.Volume = GetVolume();
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
                {
                    previewSample.Pause();
                    previewSample.IsLoop = false;
                }
            }
        }
        float GetVolume()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (GameManager.IsAppOnFocus)
            {
                return MajInstances.Settings.Audio.Volume.BGM;
            }
            else
            {
                return 0;
            }
#else
            return MajInstances.Settings.Audio.Volume.BGM;
#endif
        }
        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            var selectSound = MajInstances.AudioManager.GetSFX("bgm_select.mp3");
            selectSound.SetVolume(MajInstances.Settings.Audio.Volume.BGM);
        }
    }
}