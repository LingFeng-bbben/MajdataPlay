namespace MajdataPlay.Game.Buffers
{
    internal sealed class TapQueueInfo : NoteQueueInfo
    {
        public static TapQueueInfo Default => new TapQueueInfo()
        {
            Index = 0,
            KeyIndex = 1
        };
        /// <summary>
        /// Tap、Hold、Star的键编号
        /// </summary>
        public int KeyIndex { get; init; }
    }
}
