namespace MajdataPlay.Types
{
    public class GameSetting
    {
        public GameOptions Game { get; set; } = new();
        public JudgeOptions Judge { get; set; } = new();
        public DisplayOptions Display { get; set; } = new();
        public SoundOptions Audio { get; set; } = new();
        public DebugOptions Debug { get; set; } = new();
        public int SelectedIndex { get; set; } = 0;
        public ChartLevel SelectedDiff { get; set; } = ChartLevel.Easy;
    }
    public class GameOptions
    {
        public float TapSpeed { get; set; } = 7.5f;
        public float TouchSpeed { get; set; } = 7.5f;
        public float SlideFadeInOffset { get; set; } = 0f;
        public float BackgroundDim { get; set; } = 0.8f;
        public bool StarRotation { get; set; } = true;
    }
    public class JudgeOptions
    {
        public float AudioOffset { get; set; } = 0f;
        public float JudgeOffset { get; set; } = 0f;
        public JudgeMode Mode { get; set; } = JudgeMode.Modern;
    }
    public class DisplayOptions
    {
        public string Skin { get; set; } = "default";
        public bool DisplayCriticalPerfect { get; set; } = false;
        public JudgeDisplayType FastLateType { get; set; } = JudgeDisplayType.Disable;
        public JudgeDisplayType NoteJudgeType { get; set; } = JudgeDisplayType.All;
        public JudgeDisplayType TouchJudgeType { get; set; } = JudgeDisplayType.All;
        public JudgeDisplayType SlideJudgeType { get; set; } = JudgeDisplayType.All;
        /// <summary>
        /// Such like Tap、Star、Hold and Break
        /// </summary>
        public float OuterJudgeDistance { get; set; } = 1f;
        /// <summary>
        /// Such like Touch and TouchHold
        /// </summary>
        public float InnerJudgeDistance { get; set; } = 1f;
    }
    public class SoundOptions
    {
        public int Samplerate { get; set; } = 44100;
        public int AsioDeviceIndex { get; set; } = 0;
        public SFXVolume Volume { get; set; } = new();
        public SoundBackendType Backend { get; set; } = SoundBackendType.WaveOut;
    }
    public class SFXVolume
    {
        public float Anwser { get; set; } = 0.8f;
        public float BGM { get; set; } = 1f;
        public float Judge { get; set; } = 0.3f;
        public float Slide { get; set; } = 0.3f;
        public float Break { get; set; } = 0.3f;
        public float Touch { get; set; } = 0.3f;
        public float Voice { get; set; } = 1f;
    }
    public class DebugOptions
    {
        public bool DisplaySensor { get; set; } = false;
        public bool DisplayFPS { get; set; } = false;
        public bool TryFixAudioSync { get; set; } = true;
        public float NoteAppearRate { get; set; } = 0.36f;
    }
}