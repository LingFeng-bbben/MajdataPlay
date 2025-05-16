using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.IO
{
    public class AudioTrackNotFoundException : InvalidAudioTrackException
    {
        public AudioTrackNotFoundException(string trackPath) : base("Audio track not found", trackPath) { }
    }
}
