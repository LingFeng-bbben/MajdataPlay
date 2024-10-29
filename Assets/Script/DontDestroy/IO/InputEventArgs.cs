#nullable enable
using MajdataPlay.Types;

namespace MajdataPlay.IO
{
    public readonly struct InputEventArgs
    {
        public SensorType Type { get; init; }
        public SensorStatus OldStatus { get; init; }
        public SensorStatus Status { get; init; }
        public bool IsButton { get; init; }
        public bool IsClick => OldStatus == SensorStatus.Off && Status == SensorStatus.On;
    }
}
