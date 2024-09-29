using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Interfaces
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
