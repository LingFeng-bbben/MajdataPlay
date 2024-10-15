using System;

namespace MajdataPlay.IO
{
    public interface INAudioSample
    {
        int Length { get; }
        TimeSpan TrackLen { get; }
    }
}
