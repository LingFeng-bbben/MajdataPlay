using System;
#nullable enable
namespace MajdataPlay.Net
{
    public interface IProgressReporter<TProgress,TEventArgs>
    {
        TProgress Progress { get; }
        void OnProgressChanged(TEventArgs args);
    }
}
