using MajdataPlay.Interfaces;
using System;

namespace MajdataPlay.IO
{
    public abstract class AudioSampleWrap : IDisposable, IPausableSoundProvider
    {
        public abstract bool IsPlaying { get; }
        public abstract float Volume { get; set; }
        public abstract double CurrentSec { get; set; }
        public abstract TimeSpan Length { get; }
        public abstract bool IsLoop { get; set; }
        public abstract void Play();
        public abstract void Pause();
        public abstract void Stop();
        public abstract void PlayOneShot();
        public abstract void SetVolume(float volume);
        public abstract void Dispose();
    }
}