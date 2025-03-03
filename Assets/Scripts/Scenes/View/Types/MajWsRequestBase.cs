using System;

namespace MajdataPlay.View.Types
{
    public partial class MajWsRequestBase
    {
        public MajWsRequestType requestType {  get; set; }
        public object requestData { get; set; }
    }
    public enum MajWsRequestType 
    {
        Reset,
        Load,
        Play,
        Pause,
        Stop,
        Status
    }
}