#nullable enable
using MajdataPlay.Types;

namespace MajdataPlay.IO
{
    public readonly struct InputEventArgs
    {
        public SensorArea Type { get; init; }
        public SensorStatus OldStatus { get; init; }
        public SensorStatus Status { get; init; }
        public bool IsButton { get; init; }
        public bool IsDown => OldStatus == SensorStatus.Off && Status == SensorStatus.On;
        public bool IsUp=> OldStatus == SensorStatus.On && Status == SensorStatus.Off;
    }
}
