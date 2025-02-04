namespace MajdataPlay.Types
{
    public ref struct NoteScore
    {
        public long TotalScore { get; set; }
        public long TotalExtraScore { get; set; }
        public long TotalExtraScoreClassic { get; set; }
        public long LostScore { get; set; }
        public long LostExtraScore { get; set; }
        public long LostExtraScoreClassic { get; set; }
    }
}
