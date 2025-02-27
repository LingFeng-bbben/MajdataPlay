using MajdataPlay.Types;

namespace MajdataPlay.Game.Buffers
{
    internal sealed class TouchQueueInfo : NoteQueueInfo
    {
        public static TouchQueueInfo Default => new TouchQueueInfo()
        {
            Index = 0,
            SensorPos = SensorArea.C
        };
        /// <summary>
        /// 该Touch所处的传感器编号
        /// </summary>
        public SensorArea SensorPos { get; init; }
    }
}
