namespace MajdataPlay.Types
{
    public enum SensorStatus
    {
        On,
        Off
    }
    public enum SensorType
    {
        A1,
        A2,
        A3,
        A4,
        A5,
        A6,
        A7,
        A8,
        B1,
        B2,
        B3,
        B4,
        B5,
        B6,
        B7,
        B8,
        C,
        D1,
        D2,
        D3,
        D4,
        D5,
        D6,
        D7,
        D8,
        E1,
        E2,
        E3,
        E4,
        E5,
        E6,
        E7,
        E8,
        Test,
        P1,
        Service,
        P2
    }
    public enum SensorGroup
    {
        A,
        B,
        C,
        D,
        E
    }
    public enum JudgeType
    {
        Miss,
        LateGood,
        LateGreat2,
        LateGreat1,
        LateGreat,
        LatePerfect2,
        LatePerfect1,
        Perfect,
        FastPerfect1,
        FastPerfect2,
        FastGreat,
        FastGreat1,
        FastGreat2,
        FastGood
    }
    public enum NoteStatus
    {
        Start,
        Initialized,
        Scaling,
        Running,
        End,
        Destroyed
    }
    public enum ComboState
    {
        None,
        FC,
        FCPlus,
        AP,
        APPlus
    }
    public enum ScoreNoteType
    {
        Tap,
        Hold,
        Slide,
        Break,
        Touch
    }
    public enum ComponentState
    {
        Idle,
        Scanning,
        Loading,
        Parsing,
        Running,
        Backend,
        Finished,
        Failed
    }
    public enum ChartLevel
    {
        Easy,
        Basic,
        Advance,
        Expert,
        Master,
        ReMaster,
        UTAGE
    }
    public enum SoundBackendType
    {
        WaveOut, Asio, Unity
    }
    public enum JudgeMode
    {
        Classic,
        Modern
    }
    public enum JudgeDisplayType
    {
        /// <summary>
        /// CriticalPerfect, Pefect, Great, Good
        /// </summary>
        All,
        /// <summary>
        /// Pefect, Great, Good
        /// </summary>
        BelowCP,
        /// <summary>
        /// Great, Good
        /// </summary>
        BelowP,
        /// <summary>
        /// Good
        /// </summary>
        BelowGR,
        /// <summary>
        /// None
        /// </summary>
        Disable,
        /// <summary>
        /// CriticalPerfect, Pefect, Great, Good
        /// </summary>
        All_BreakOnly,
        /// <summary>
        /// Pefect, Great, Good
        /// </summary>
        BelowCP_BreakOnly,
        /// <summary>
        /// Great, Good
        /// </summary>
        BelowP_BreakOnly,
        /// <summary>
        /// Good
        /// </summary>
        BelowGR_BreakOnly,
    }
    public enum NoteLoaderStatus
    {
        Idle,
        ParsingNote,
        Backend,
        Error,
        Finished
    }
    public enum BGInfoType
    {
        CPCombo,
        PCombo,
        Combo,
        Achievement_101,
        Achievement_100,
        Achievement,
        AchievementClassical,
        AchievementClassical_100,
        DXScore,
        DXScoreRank,
        S_Board,
        SS_Board,
        SSS_Board,
        MyBest,
        Diff,
        None

    }
    public enum RendererStatus
    {
        Off,
        On
    }
}
