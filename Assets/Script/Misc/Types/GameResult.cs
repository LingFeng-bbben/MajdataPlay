
namespace MajdataPlay.Types
{
    public readonly struct GameResult
    {
        public Accurate Acc { get; init; }
        public SongDetail SongInfo { get; init; }
        public JudgeDetail JudgeRecord { get; init; }
        public ChartLevel ChartLevel { get; init; }
        public long Fast { get; init; }
        public long Late { get; init; }
        public long DXScore { get; init; }
        public long TotalDXScore { get; init; }
        public ComboState ComboState { get; init; }
    }
}
