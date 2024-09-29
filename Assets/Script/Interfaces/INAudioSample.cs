using NAudio.Wave;
using System;

namespace MajdataPlay.Interfaces
{
    public interface INAudioSample
    {
        int Length { get; }
        TimeSpan TrackLen { get; }
    }
}
