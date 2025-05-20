using MajdataPlay.Collections;
using MajdataPlay.Game;
using System;

namespace MajdataPlay
{
    public readonly struct GameResult
    {
        public Accurate Acc { get; init; }
        public ISongDetail SongDetail { get; init; }
        public JudgeDetail JudgeRecord { get; init; }
        public ChartLevel Level { get; init; }
        public long Fast { get; init; }
        public long Late { get; init; }
        public long DXScore { get; init; }
        public long TotalDXScore { get; init; }
        public ComboState ComboState { get; init; }
        public ReadOnlyMemory<float> NoteJudgeDiffs { get; init; }
    }
}
