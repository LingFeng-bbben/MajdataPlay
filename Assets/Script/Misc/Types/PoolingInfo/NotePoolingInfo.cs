
namespace MajdataPlay.Types
{
    public abstract class NotePoolingInfo
    {
        public int StartPos { get; init; }
        public float Timing { get; init; }
        public int NoteSortOrder { get; init; }
        public float Speed { get; init; }
        public bool IsEach { get; init; }
        public bool IsBreak { get; init; }
        public bool IsEX { get; init; }
    }
}
