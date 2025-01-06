namespace MajdataPlay.Collections
{
    public readonly ref struct UnpackJudgeInfo
    {
        public long CriticalPerfect { get; init; }
        public long Perfect { get; init; }
        public long Great { get; init; }
        public long Good { get; init; }
        public long Miss { get; init; }
        public long Fast { get; init; }
        public long Late { get; init; }
        public long All => CriticalPerfect + Perfect + Great + Good;
        public bool IsFullCombo => All != 0 && Miss == 0;
        public bool IsFullComboPlus => IsFullCombo && Good == 0;
        public bool IsAllPerfect => IsFullComboPlus && Great == 0;
        public bool IsTheoretical => IsAllPerfect && Perfect == 0;
    }
}