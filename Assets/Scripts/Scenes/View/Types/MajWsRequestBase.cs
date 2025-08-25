using System;
#nullable enable
namespace MajdataPlay.Scenes.View.Types
{
    internal readonly struct MajWsRequestBase
    {
        public MajWsRequestType requestType { get; init; }
        public object? requestData { get; init; }
    }
    public enum MajWsRequestType 
    {
        Reset,
        Load,
        Parse,
        Play,
        Pause,
        Resume,
        Stop,
        State
    }
}