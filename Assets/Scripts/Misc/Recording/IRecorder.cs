using System;
using System.Threading.Tasks;

namespace MajdataPlay.Recording
{
    internal interface IRecorder : IDisposable
    {
        bool IsRecording { get; }
        bool IsConnected { get; }
        void StartRecord();
        Task StartRecordAsync();
        void StopRecord();
        Task StopRecordAsync();
        void OnLateUpdate();
    }
}
