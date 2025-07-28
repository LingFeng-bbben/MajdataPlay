using HidSharp;
using MajdataPlay.Scenes.Game;
using MajdataPlay.Scenes.Game.Notes;
using MajdataPlay.IO;
using MajdataPlay.Recording;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Threading;
#nullable enable
namespace MajdataPlay.Settings
{
    public class GameSetting
    {
        public GameOptions Game { get; set; } = new();
        public JudgeOptions Judge { get; set; } = new();
        public DisplayOptions Display { get; set; } = new();
        public SoundOptions Audio { get; set; } = new();
        [JsonIgnore]
        public ModOptions Mod { get; set; } = new();
        public DebugOptions Debug { get; set; } = new();
        [SettingVisualizationIgnore]
        public OnlineOptions Online { get; set; } = new();
        [SettingVisualizationIgnore]
        public MiscOptions Misc { get; set; } = new();
        [SettingVisualizationIgnore]
        public IOOptions IO { get; set; } = new();
    }
    public class GameOptions
    {
        public float TapSpeed { get; set; } = 7.5f;
        public float TouchSpeed { get; set; } = 7.5f;
        public float SlideFadeInOffset { get; set; } = 0f;
        public float BackgroundDim { get; set; } = 0.8f;
        public bool StarRotation { get; set; } = true;
        public BGInfoOption BGInfo { get; set; } = BGInfoOption.Combo;
        public TopInfoDisplayOption TopInfo { get; set; } = TopInfoDisplayOption.None;
        public bool TrackSkip { get; set; } = true;
        public bool FastRetry { get; set; } = true;
        public MirrorOption Mirror { get; set; } = MirrorOption.Off;
        public int Rotation { get; set; } = 0;
        public RandomModeOption Random { get; set; } = RandomModeOption.Disabled;
        public RecordModeOption RecordMode { get; set; } = RecordModeOption.Disable;
    }
    public class JudgeOptions
    {
        public float AudioOffset { get; set; } = 0f;
        public float JudgeOffset { get; set; } = 0f;
        public float AnswerOffset { get; set; } = 0f;
        public float TouchPanelOffset { get; set; } = 0f;
        public JudgeModeOption Mode { get; set; } = JudgeModeOption.Modern;
    }
    public class DisplayOptions
    {
        public string Language { get; set; } = "zh-CN - Majdata";
        public string Skin { get; set; } = "default";
        public bool DisplayCriticalPerfect { get; set; } = false;
        public bool DisplayBreakScore { get; set; } = true;

        public JudgeDisplayOption FastLateType { get; set; } = JudgeDisplayOption.Disable;
        public JudgeDisplayOption NoteJudgeType { get; set; } = JudgeDisplayOption.All;
        public JudgeDisplayOption TouchJudgeType { get; set; } = JudgeDisplayOption.All;
        public JudgeDisplayOption SlideJudgeType { get; set; } = JudgeDisplayOption.All;
        public JudgeDisplayOption BreakJudgeType { get; set; } = JudgeDisplayOption.All;
        public JudgeDisplayOption BreakFastLateType { get; set; } = JudgeDisplayOption.Disable;
        public JudgeModeOption SlideSortOrder { get; set; } = JudgeModeOption.Modern;
        /// <summary>
        /// Such like Tap、Star、Hold and Break
        /// </summary>
        public float OuterJudgeDistance { get; set; } = 1f;
        /// <summary>
        /// Such like Touch and TouchHold
        /// </summary>
        public float InnerJudgeDistance { get; set; } = 1f;
        public bool DisplayHoldHeadJudgeResult { get; set; } = false;
        public float TapScale { get; set; } = 1f;
        public float HoldScale { get; set; } = 1f;
        public float TouchScale { get; set; } = 1f;
        public float SlideScale { get; set; } = 1f;
        public TouchFeedbackLevel TouchFeedback { get; set; } = TouchFeedbackLevel.Outer_Only;
        public string Resolution { get; set; } = "1080x1920";
        public float MainScreenPosition { get; set; } = 1f; 
        public RenderQualityOption RenderQuality { get; set; } = RenderQualityOption.Low;
        [SettingVisualizationIgnore]
        public bool Topmost { get; set; } = false;
        public int FPSLimit { get; set; } = 240;
        public bool VSync { get; set; } = true;
    }
    public class SoundOptions
    {
        public int Samplerate { get; set; } = 44100;
        public int AsioDeviceIndex { get; set; } = 0;
        public bool WasapiExclusive { get; set; } = true;
        public SFXVolume Volume { get; set; } = new();
        public SoundBackendOption Backend { get; set; } = SoundBackendOption.Wasapi;
    }
    public class SFXVolume
    {
        public float Global { get; set; } = 0.8f;
        public float Answer { get; set; } = 0.8f;
        public float BGM { get; set; } = 1f;
        public float Tap { get; set; } = 0.3f;
        public float Slide { get; set; } = 0.3f;
        public float Break { get; set; } = 0.3f;
        public float Touch { get; set; } = 0.3f;
        public float Voice { get; set; } = 1f;
    }

    public class ModOptions
    {
        public float PlaybackSpeed { get; set; } = 1f;
        public AutoplayModeOption AutoPlay { get; set; } = AutoplayModeOption.Disable;
        public JudgeStyleOption JudgeStyle { get; set; } = JudgeStyleOption.DEFAULT;
        public bool SubdivideSlideJudgeGrade { get; set; } = false;
        public bool AllBreak { get; set; } = false;
        public bool AllEx { get; set; } = false;
        public bool AllTouch { get; set; } = false;
        public bool SlideNoHead { get; set; } = false;
        public bool SlideNoTrack { get; set; } = false;
        public bool ButtonRingForTouch { get; set; } = false;
        public string NoteMask { get; set; } = "Disable";

        public bool IsAnyModActive()
        {
            return !(PlaybackSpeed == 1f &&
                !AllBreak && !AllEx && !AllTouch && AutoPlay == AutoplayModeOption.Disable && JudgeStyle == JudgeStyleOption.DEFAULT);
        }

    }
    public class OnlineOptions
    {
        public bool Enable { get; set; } = false;
        public List<ApiEndpoint> ApiEndpoints { get; set; } = new List<ApiEndpoint>
        {
            {
                new ApiEndpoint()
                {
                    Name = "Majnet",
                    Url = "https://majdata.net/api3/api" ,
                    Username = "YourUsername",
                    Password = "YourPassword"
                }
            },
            {
                new ApiEndpoint()
                {
                    Name = "Contest",
                    Url = "https://majdata.net/api1/api"
                }
            }
        };
    }

    public class ApiEndpoint
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class DebugOptions
    {
        public bool DisplaySensor { get; set; } = false;
        public bool DisplayFPS { get; set; } = true;
        [SettingVisualizationIgnore]
        public bool FullScreen { get; set; } = true;
        public int MenuOptionIterationSpeed { get; set; } = 45;
        public float DisplayOffset { get; set; } = 0f;
        public bool TryFixAudioSync { get; set; } = false;
        public float NoteAppearRate { get; set; } = 0.265f;
        public bool DisableGCInGame { get; set; } = false;
        public bool HideCursorInGame { get; set; } = true;
        public bool NoteFolding { get; set; } = true;
        public DJAutoPolicyOption DJAutoPolicy { get; set; } = DJAutoPolicyOption.Strict;
        [SettingVisualizationIgnore]
        public int MaxQueuedFrames { get; set; } = 2;
        [SettingVisualizationIgnore]
        public int TapPoolCapacity { get; set; } = 96;
        [SettingVisualizationIgnore]
        public int HoldPoolCapacity { get; set; } = 48;
        [SettingVisualizationIgnore]
        public int TouchPoolCapacity { get; set; } = 64;
        [SettingVisualizationIgnore]
        public int TouchHoldPoolCapacity { get; set; } = 16;
        [SettingVisualizationIgnore]
        public int EachLinePoolCapacity { get; set; } = 64;
        [SettingVisualizationIgnore]
        public ProcessPriorityClass ProcessPriority { get; set; } = ProcessPriorityClass.AboveNormal;
        [SettingVisualizationIgnore]
        public ThreadPriority MainThreadPriority { get; set; } = ThreadPriority.Normal;
        [SettingVisualizationIgnore]
        public ThreadPriority IOThreadPriority { get; set; } = ThreadPriority.AboveNormal;
    }
    public class MiscOptions
    {
        public int SelectedIndex { get; set; } = 0;
        public int SelectedDir { get; set; } = 0;
        public ChartLevel SelectedDiff { get; set; } = ChartLevel.Easy;
        public SongOrder OrderBy { get; set; } = new();
    }
    public class IOOptions
    {
        public DeviceManufacturerOption Manufacturer { get; set; } = DeviceManufacturerOption.General;
        public InputDeviceOptions InputDevice { get; set; } = new();
        public OutputDeviceOptions OutputDevice { get; set; } = new();
    }
    public class InputDeviceOptions
    {
        public int Player { get; set; } = 1;
        public ButtonRingOptions ButtonRing { get; set; } = new();
        public TouchPanelOptions TouchPanel { get; set; } = new();
    }
    public class OutputDeviceOptions
    {
        public LedOptions Led { get; set; } = new();
    }
    public class LedOptions
    {
        public bool Enable { get; set; } = true;
        public int RefreshRateMs { get; set; } = 100;
        public bool Throttler { get; set; } = false;
        public SerialPortOptions SerialPortOptions { get; set; } = new()
        {
            Port = 21,
            BaudRate = 115200
        };
        public HidOptions HidOptions { get; set; } = new()
        {
            ProductId = 0x1224,
            VendorId = 0x0E8F
        };
    }
    public class ButtonRingOptions
    {
        public ButtonRingDeviceOption Type { get; set; } = ButtonRingDeviceOption.Keyboard;
        public bool Debounce { get; set; } = false;
        public int PollingRateMs { get; set; } = 0;
        public int DebounceThresholdMs { get; set; } = 0;
        public HidOptions HidOptions { get; set; } = new();
    }
    public class TouchPanelOptions
    {
        public bool Debounce { get; set; } = false;
        public int Sensitivity { get; set; } = 0;
        public int PollingRateMs { get; set; } = 0;
        public int DebounceThresholdMs { get; set; } = 0;
        public float TouchSimulationRadius { get; set; } = 0.5f;
        public SerialPortOptions SerialPortOptions { get; set; } = new();
    }
    public class HidOptions
    {
        public string DeviceName { get; set; } = string.Empty;
        public int ProductId { get; set; } = 0x0021;
        public int VendorId { get; set; } = 0x0CA3;
        public bool Exclusice { get; set; } = false;
        public OpenPriority OpenPriority { get; set; } = OpenPriority.VeryHigh;
    }
    public class SerialPortOptions
    {
        public int Port { get; set; } = 3;
        public int BaudRate { get; set; } = 9600;
    }
}