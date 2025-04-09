using System;

namespace MajdataPlay.Recording
{
    internal interface IRecorder : IDisposable
    {
        public bool IsRecording { get; set; }
        public bool IsConnected { get; set; }
        public void StartRecord();
        public void StopRecord();
    }
}
