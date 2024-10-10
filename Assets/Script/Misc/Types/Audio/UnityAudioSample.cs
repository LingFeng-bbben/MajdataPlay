using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace MajdataPlay.Types
{
    public class UnityAudioSample : AudioSampleWrap
    {
        private AudioClip audioClip;
        private AudioSource audioSource;
        private GameObject gameObject;
        public override bool IsLoop
        {
            get
            {
                return audioSource.loop;
            }
            set { audioSource.loop = value; }
        }
        public override double CurrentSec 
        { 
            get => audioSource.time; 
            set => audioSource.time = (float)value; 
        }
        public override float Volume 
        { 
            get => audioSource.volume; 
            set => audioSource.volume = value.Clamp(0,1); 
        }
        public override TimeSpan Length => TimeSpan.FromSeconds(audioClip.length);
        public override bool IsPlaying => audioSource.isPlaying;
        public UnityAudioSample(AudioClip audioClip, GameObject gameObject)
        {
            this.audioClip = audioClip;
            this.gameObject = gameObject;
            this.audioClip.LoadAudioData();
            audioSource = this.gameObject.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.bypassEffects = true;
        }
        ~UnityAudioSample() => Dispose();

        public override void PlayOneShot()
        {
            audioSource.time = 0;
            audioSource.Play();
        }
        public override void SetVolume(float volume) => Volume = volume;
        public override void Play()
        {
            audioSource.Play();
        }
        public override void Pause()
        {
            audioSource.Pause();
        }
        public override void Stop()
        {
            audioSource.Stop();
            audioSource.time = 0;
        }
        public override void Dispose()
        {
            audioSource.Stop();
            audioClip.UnloadAudioData();
            UnityEngine.Object.Destroy(audioSource);
        }
        public static UnityAudioSample ReadFromFile(string filePath,GameObject gameObject)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.UNKNOWN))
            {
                www.SendWebRequest();
                while (!www.isDone) ;
                var myClip = DownloadHandlerAudioClip.GetContent(www);
                return new UnityAudioSample(myClip, gameObject);
            }
        }
        public static async UniTask<UnityAudioSample> ReadFromFileAsync(string filePath, GameObject gameObject)
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