using System;

namespace MajdataPlay.IO
{
    public interface ISoundProvider
    {
        bool IsPlaying { get; }
        bool IsLoop { get; set; }
        float Volume { get; set; }
        double CurrentSec { get; set; }
        TimeSpan Length { get; }
        void Play();
        void PlayOneShot();
    }
}
