using NAudio.Wave;

#nullable enable
namespace MajdataPlay.Interfaces
{
    public interface INAudioSampleProvider : INAudioSample,ISampleProvider
    {
        int Position { get; set; }
        float Volume { get; set; }
        bool IsLoop { get; set; }
        bool IsPlaying { get; set; }
    }
    public interface INAudioSampleProvider<out T> :INAudioSampleProvider where T : INAudioSample
    {
        T? Sample { get; }
    }
}
