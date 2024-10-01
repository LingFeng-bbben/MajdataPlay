using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Types
{
    public class InvalidAudioTrackException: Exception
    {
        public string TrackPath { get; private set; }
        public InvalidAudioTrackException(string msg, string trackPath) : base(msg)
        {
            TrackPath = trackPath;
        }
    }
}
