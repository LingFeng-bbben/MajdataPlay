using UnityEngine;

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

        public override void PlayOneShot()
        {
            audioSource.time = 0;
            audioSource.Play();
        }
        public override bool GetPlayState()
        {
            return audioSource.isPlaying;
        }
        public override void SetVolume(float volume)
        {
            audioSource.volume = volume;
        }
        public override double GetCurrentTime()
        {
            return audioSource.time;
        }
        public override void SetCurrentTime(float time)
        {
            audioSource.time = time;
        }
        public override void Play()
        {
            audioSource.Play();
        }
        public override void Pause()
        {
            audioSource.Pause();
        }
    }
}