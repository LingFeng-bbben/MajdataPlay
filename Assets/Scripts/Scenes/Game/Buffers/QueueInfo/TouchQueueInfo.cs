using MajdataPlay.Types;

namespace MajdataPlay.Game.Buffers
{
    public sealed class TouchQueueInfo : NoteQueueInfo
    {
        public static TouchQueueInfo Default => new TouchQueueInfo()
        {
            Index = 0,
            SensorPos = SensorType.C
        };
        /// <summary>
        /// 该Touch所处的传感器编号
        /// </summary>
        public SensorType SensorPos { get; init; }
    }
}
