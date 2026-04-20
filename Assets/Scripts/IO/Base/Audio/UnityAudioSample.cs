using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Numerics;
using MajdataPlay.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
#nullable enable
namespace MajdataPlay.IO
{
    public class UnityAudioSample : AudioSampleWrap
    {
        private AudioClip? _audioClip;
        private AudioSource? _audioSource;
        private GameObject _gameObject;
        public override bool IsLoop
        {
            get
            {
                ThrowIfDisposed();
                return _audioSource!.loop;
            }
            set 
            {
                ThrowIfDisposed();
                _audioSource!.loop = value; 
            }
        }
        public override bool IsEmpty
        {
            get
            {
                return false;
            }
        }
        public override double CurrentSec
        {
            get
            {
                ThrowIfDisposed();
                ThrowIfCanSeekNotSupported();
                return _audioSource!.time;
            }
            set
            {
                ThrowIfDisposed();
                _audioSource!.time = (float)value;
            }
        }
        public override float Volume
        {
            get
            {
                ThrowIfDisposed();
                return _audioSource!.volume;
            }
            set
            {
                ThrowIfDisposed();
                var volume = value.Clamp(0, 1) * MajInstances.Settings.Audio.Volume.Global.Clamp(0, 1);
                _audioSource!.volume = volume;
            }
        }
        public override float Speed
        {
            get
            {
                ThrowIfDisposed();
                return _audioSource!.pitch;
            }
            set
            {
                ThrowIfDisposed();
                _audioSource!.pitch = value;
            }
        }// this is not perfect and will change the pitch. FUCK YOU UNITY
        public override TimeSpan Length
        {
            get
            {
                return TimeSpan.FromSeconds(_audioClip!.length);
            }
        }
        public override bool IsPlaying
        {
            get
            {
                ThrowIfDisposed();
                return _audioSource!.isPlaying;
            }
        }
        public UnityAudioSample(AudioClip audioClip, GameObject gameObject)
        {
            if (audioClip is null)
            {
                throw new ArgumentNullException(nameof(audioClip));
            }
            if (gameObject is null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            this._audioClip = audioClip;
            this._gameObject = gameObject;
            this._audioClip.LoadAudioData();
            _audioSource = this._gameObject.AddComponent<AudioSource>();
            _audioSource.clip = audioClip;
            _audioSource.loop = false;
            _audioSource.bypassEffects = true;
        }
        ~UnityAudioSample()
        {
            Dispose();
        }

        public override void PlayOneShot()
        {
            ThrowIfDisposed();
            _audioSource!.time = 0;
            _audioSource.Play();
        }
        public override void SetVolume(float volume)
        {
            ThrowIfDisposed();
            Volume = volume;
        }
        public override void Play()
        {
            ThrowIfDisposed();
            _audioSource!.Play();
        }
        public override void Pause()
        {
            ThrowIfDisposed();
            _audioSource!.Pause();
        }
        public override void Stop()
        {
            ThrowIfDisposed();
            _audioSource!.Stop();
            _audioSource.time = 0;
        }
        public override void Dispose()
        {
            if(_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            UniTask.Post(() =>
            {
                _audioSource!.Stop();
                _audioClip!.UnloadAudioData();
                UnityEngine.Object.Destroy(_audioSource);
                UnityEngine.Object.DestroyImmediate(_audioClip, true);
            });
        }
        public override async ValueTask DisposeAsync()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToMainThread();
                _audioSource!.Stop();
                _audioClip!.UnloadAudioData();
                UnityEngine.Object.Destroy(_audioSource);
                UnityEngine.Object.DestroyImmediate(_audioClip, true);

                _audioClip = null;
                _audioSource = null;
            }
        }
        static string GetAudioSourcePath(string filePath)
        {
            if (Uri.TryCreate(filePath, UriKind.Absolute, out var uri) && uri.IsFile)
            {
                return uri.LocalPath;
            }
            return filePath;
        }
        static AudioType GetAudioType(string filePath)
        {
            var extension = Path.GetExtension(GetAudioSourcePath(filePath)).ToLowerInvariant();
            return extension switch
            {
                ".mp3" => AudioType.MPEG,
                ".ogg" => AudioType.OGGVORBIS,
                ".wav" => AudioType.WAV,
                ".aif" or ".aiff" => AudioType.AIFF,
                _ => AudioType.UNKNOWN
            };
        }
        static void ThrowIfUnsupported(string filePath)
        {
#if UNITY_OPENHARMONY
            var extension = Path.GetExtension(GetAudioSourcePath(filePath));
            if (string.Equals(extension, ".opus", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidAudioTrackException("OpenHarmony not support .opus tracks.", filePath);
            }
#endif
        }
        static UnityWebRequest CreateAudioRequest(string filePath)
        {
            ThrowIfUnsupported(filePath);
            filePath = filePath.Replace('\\', '/');
            string encoded = Uri.EscapeUriString(filePath);
            encoded = encoded.Replace("+", "%2B");
            var www = UnityWebRequestMultimedia.GetAudioClip(encoded, GetAudioType(filePath));
            www.SetRequestHeader("User-Agent", MajEnv.HTTP_USER_AGENT);
            return www;
        }
        public static UnityAudioSample Create(string filePath, GameObject gameObject)
        {
            using (UnityWebRequest www = CreateAudioRequest(filePath))
            {
                www.SendWebRequest();
                while (!www.isDone)
                {
                }
                if (www.result != UnityWebRequest.Result.Success)
                {
                    throw new InvalidAudioTrackException(www.error ?? "Failed to load audio track", filePath);
                }

                var myClip = DownloadHandlerAudioClip.GetContent(www);
                if (myClip is null)
                {
                    throw new InvalidAudioTrackException("Failed to decode audio track", filePath);
                }

                return new UnityAudioSample(myClip, gameObject)
                {
                    CanSeek = filePath.StartsWith("file://"),
                };
            }
        }
        public static async UniTask<UnityAudioSample> CreateAsync(string filePath, GameObject gameObject)
        {
            using (UnityWebRequest www = CreateAudioRequest(filePath))
            {
                await www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    throw new InvalidAudioTrackException(www.error ?? "Failed to load audio track", filePath);
                }

                var myClip = DownloadHandlerAudioClip.GetContent(www);
                if (myClip is null)
                {
                    throw new InvalidAudioTrackException("Failed to decode audio track", filePath);
                }

                return new UnityAudioSample(myClip, gameObject)
                {
                    CanSeek = filePath.StartsWith("file://")
                };
            }
        }
    }
}