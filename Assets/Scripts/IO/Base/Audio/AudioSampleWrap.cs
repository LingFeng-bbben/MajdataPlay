using System;

namespace MajdataPlay.IO
{
    public abstract class AudioSampleWrap : IDisposable, IPausableSoundProvider
    {
        public readonly static AudioSampleWrap Empty = new EmptyAudioSample();

        public string Name { get; set; }
        public abstract bool IsEmpty { get; }
        public SFXSampleType SampleType { get; set; }
        public abstract bool IsPlaying { get; }
        public abstract float Volume { get; set; }
        public abstract float Speed { get; set; }
        public abstract double CurrentSec { get; set; }
        public abstract TimeSpan Length { get; }
        public abstract bool IsLoop { get; set; }
        public abstract void Play();
        public abstract void Pause();
        public abstract void Stop();
        public abstract void PlayOneShot();
        public abstract void SetVolume(float volume);
        public abstract void Dispose();
        class EmptyAudioSample : AudioSampleWrap
        {
            public override bool IsEmpty => true;

            public override bool IsPlaying => true;

            public override float Volume
            {
                get => 1;
                set
                {

                }
            }
            public override float Speed
            {
                get => 1;
                set
                {

                }
            }
            public override double CurrentSec
            {
                get => 0;
                set
                {

                }
            }

            public override TimeSpan Length => TimeSpan.Zero;

            public override bool IsLoop
            {
                get => false;
                set
                {

                }
            }
            public override void Dispose() { }
            public override void Pause() { }
            public override void Play() { }
            public override void PlayOneShot() { }
            public override void SetVolume(float volume) { }
            public override void Stop() { }
        }
    }
}