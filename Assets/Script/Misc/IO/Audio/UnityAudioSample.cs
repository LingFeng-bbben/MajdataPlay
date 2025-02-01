using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using System;
using UnityEngine;
using UnityEngine.Networking;
#nullable enable
namespace MajdataPlay.IO
{
    public class UnityAudioSample : AudioSampleWrap
    {
        private AudioClip _audioClip;
        private AudioSource _audioSource;
        private GameObject _gameObject;
        public override bool IsLoop
        {
            get
            {
                return _audioSource.loop;
            }
            set { _audioSource.loop = value; }
        }
        public override bool IsEmpty => false;
        public override double CurrentSec
        {
            get => _audioSource.time;
            set => _audioSource.time = (float)value;
        }
        public override float Volume
        {
            get => _audioSource.volume;
            set
            {
                var volume = value.Clamp(0, 1) * MajInstances.Setting.Audio.Volume.Global.Clamp(0, 1);
                _audioSource.volume = volume;
            }
        }
        public override float Speed
        {
            get => _audioSource.pitch;
            set => _audioSource.pitch = value;
        }// this is not perfect and will change the pitch. FUCK YOU UNITY
        public override TimeSpan Length => TimeSpan.FromSeconds(_audioClip.length);
        public override bool IsPlaying => _audioSource.isPlaying;
        public UnityAudioSample(AudioClip audioClip, GameObject gameObject)
        {
            this._audioClip = audioClip;
            this._gameObject = gameObject;
            this._audioClip.LoadAudioData();
            _audioSource = this._gameObject.AddComponent<AudioSource>();
            _audioSource.clip = audioClip;
            _audioSource.loop = false;
            _audioSource.bypassEffects = true;
        }
        ~UnityAudioSample() => Dispose();

        public override void PlayOneShot()
        {
            _audioSource.time = 0;
            _audioSource.Play();
        }
        public override void SetVolume(float volume) => Volume = volume;
        public override void Play()
        {
            _audioSource.Play();
        }
        public override void Pause()
        {
            _audioSource.Pause();
        }
        public override void Stop()
        {
            _audioSource.Stop();
            _audioSource.time = 0;
        }
        public override void Dispose()
        {
            _audioSource.Stop();
            _audioClip.UnloadAudioData();
            UnityEngine.Object.Destroy(_audioSource);
        }
        public static UnityAudioSample Create(string filePath, GameObject gameObject)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.UNKNOWN))
            {
                www.SendWebRequest();
                while (!www.isDone) ;
                var myClip = DownloadHandlerAudioClip.GetContent(www);
                return new UnityAudioSample(myClip, gameObject);
            }
        }
        public static async UniTask<UnityAudioSample> CreateAsync(string filePath, GameObject gameObject)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.UNKNOWN))
            {
                await www.SendWebRequest();
                var myClip = DownloadHandlerAudioClip.GetContent(www);
                return new UnityAudioSample(myClip, gameObject);
            }
        }
    }
}