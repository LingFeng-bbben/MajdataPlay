
namespace MajdataPlay.Types
{
    public readonly ref struct GameResult
    {
        public double Accurate { get; init; }
        public double Accurate_Classic { get; init; }
        public SongDetail SongInfo { get; init; }
        public JudgeDetail JudgeRecord { get; init; }
        public long Fast { get; init; }
        public long Late { get; init; }
        public long DXScore { get; init; }
        public ComboState ComboState { get; init; }
    }
}
