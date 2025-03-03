using System;

namespace MajdataPlay.View.Types
{
    internal readonly struct MajWsRequestBase
    {
        public MajWsRequestType requestType { get; init; }
        public object requestData { get; init; }
    }
    public enum MajWsRequestType 
    {
        Reset,
        Load,
        Play,
        Pause,
        Resume,
        Stop,
        Status
    }
}