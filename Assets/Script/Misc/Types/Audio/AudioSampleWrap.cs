namespace MajdataPlay.Types
{
    public abstract class AudioSampleWrap
    {
        public abstract bool IsLoop { get; set; }
        public abstract bool GetPlayState();
        public abstract void Play();
        public abstract void Pause();
        public abstract void PlayOneShot();
        public abstract double GetCurrentTime();
        public abstract void SetCurrentTime(float time);
        public abstract void SetVolume(float volume);
    }
}