#nullable enable
using MajdataPlay.IO;

namespace MajdataPlay.IO
{
    public readonly struct InputEventArgs
    {
        public ButtonZone BZone { get; init; }
        public SensorArea SArea { get; init; }
        public SwitchStatus OldStatus { get; init; }
        public SwitchStatus Status { get; init; }
        public bool IsButton { get; init; }
        public bool IsDown => OldStatus == SwitchStatus.Off && Status == SwitchStatus.On;
        public bool IsUp=> OldStatus == SwitchStatus.On && Status == SwitchStatus.Off;
    }
}
