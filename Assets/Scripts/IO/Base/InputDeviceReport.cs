using System;

namespace MajdataPlay.IO
{
    internal readonly struct InputDeviceReport
    {
        public int Index { get; init; }
        public SwitchStatus State { get; init; }
        public TimeSpan Timestamp { get; init; }
    }
}
