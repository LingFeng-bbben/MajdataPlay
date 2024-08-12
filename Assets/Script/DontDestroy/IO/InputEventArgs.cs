#nullable enable
using MajdataPlay.Types;

namespace MajdataPlay.IO
{
    public struct InputEventArgs
    {
        public SensorType Type { get; set; }
        public SensorStatus OldStatus { get; set; }
        public SensorStatus Status { get; set; }
        public bool IsButton { get; set; }
        public bool IsClick => OldStatus == SensorStatus.Off && Status == SensorStatus.On;

    }
}
