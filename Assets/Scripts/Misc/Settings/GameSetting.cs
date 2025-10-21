using HidSharp;
using MajdataPlay.Scenes.Game;
using MajdataPlay.Scenes.Game.Notes;
using MajdataPlay.IO;
using MajdataPlay.Recording;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine.Scripting;
using Newtonsoft.Json;
#nullable enable
namespace MajdataPlay.Settings
{
    [Preserve]
    public class GameSetting
    {
        [Preserve]
        public GameOptions Game { get; init; } = new();
        [Preserve]
        public JudgeOptions Judge { get; init; } = new();
        [Preserve]
        public DisplayOptions Display { get; init; } = new();
        [Preserve]
        public SoundOptions Audio { get; init; } = new();
        [JsonIgnore, Preserve]
        public ModOptions Mod { get; init; } = new();
        [Preserve]
        public DebugOptions Debug { get; init; } = new();
        [SettingVisualizationIgnore]
        public OnlineOptions Online { get; init; } = new();
        [SettingVisualizationIgnore]
        public IOOptions IO { get; init; } = new();
    }
    [Preserve]
    public class GameOptions
    {
        [Preserve]
        public float TapSpeed { get; set; } = 7.5f;
        [Preserve]
        public float TouchSpeed { get; set; } = 7.5f;
        [Preserve]
        public float SlideFadeInOffset { get; set; } = 0f;
        [Preserve]
        public float BackgroundDim { get; set; } = 0.8f;
        [Preserve]
        public bool StarRotation { get; set; } = true;
        [Preserve]
        public BGInfoOption BGInfo { get; set; } = BGInfoOption.Combo;
        [Preserve]
        public TopInfoDisplayOption TopInfo { get; set; } = TopInfoDisplayOption.None;
        [Preserve]
        public bool TrackSkip { get; set; } = true;
        [Preserve]
        public bool FastRetry { get; set; } = true;
        [Preserve]
        public MirrorOption Mirror { get; set; } = MirrorOption.Off;
        [Preserve]
        public int Rotation { get; set; } = 0;
        [Preserve]
        public RandomModeOption Random { get; set; } = RandomModeOption.Disabled;
        [Preserve]
        public RecordModeOption RecordMode { get; set; } = RecordModeOption.Disable;
    }
    [Preserve]
    public class JudgeOptions
    {
        [Preserve]
        public float AudioOffset { get; set; } = 0f;
        [Preserve]
        public float JudgeOffset { get; set; } = 0f;
        [Preserve]
        public float AnswerOffset { get; set; } = 0f;
        [Preserve]
        public float TouchPanelOffset { get; set; } = 0f;
        [Preserve]
        public JudgeModeOption Mode { get; set; } = JudgeModeOption.Modern;
    }
    [Preserve]
    public class DisplayOptions
    {
        [Preserve]
        public string Language { get; set; } = "zh-CN - Majdata";
        [Preserve]
        public string Skin { get; set; } = "default";
        [Preserve]
        public bool DisplayCriticalPerfect { get; set; } = false;
        [Preserve]
        public bool DisplayBreakScore { get; set; } = true;

        [Preserve]
        public JudgeDisplayOption FastLateType { get; set; } = JudgeDisplayOption.Disable;
        [Preserve]
        public JudgeDisplayOption NoteJudgeType { get; set; } = JudgeDisplayOption.All;
        [Preserve]
        public JudgeDisplayOption TouchJudgeType { get; set; } = JudgeDisplayOption.All;
        [Preserve]
        public JudgeDisplayOption SlideJudgeType { get; set; } = JudgeDisplayOption.All;
        [Preserve]
        public JudgeDisplayOption BreakJudgeType { get; set; } = JudgeDisplayOption.All;
        [Preserve]
        public JudgeDisplayOption BreakFastLateType { get; set; } = JudgeDisplayOption.Disable;
        [Preserve]
        public JudgeModeOption SlideSortOrder { get; set; } = JudgeModeOption.Modern;
        /// <summary>
        /// Such like Tap、Star、Hold and Break
        /// </summary>
        [Preserve]
        public float OuterJudgeDistance { get; set; } = 1f;
        /// <summary>
        /// Such like Touch and TouchHold
        /// </summary>
        [Preserve]
        public float InnerJudgeDistance { get; set; } = 1f;
        [SettingVisualizationIgnore]
        public bool DisplayHoldHeadJudgeResult { get; set; } = false;
        [Preserve]
        public float TapScale { get; set; } = 1f;
        [Preserve]
        public float HoldScale { get; set; } = 1f;
        [Preserve]
        public float TouchScale { get; set; } = 1f;
        [Preserve]
        public float SlideScale { get; set; } = 1f;
        [Preserve]
        public TouchFeedbackLevel TouchFeedback { get; set; } = TouchFeedbackLevel.Outer_Only;
        [SettingVisualizationIgnore]
        public string Resolution { get; set; } = "1080x1920";
        [SettingVisualizationIgnore]
        public float MainScreenPosition { get; set; } = 1f;
        [Preserve]
        public RenderQualityOption RenderQuality { get; set; } = RenderQualityOption.Low;
        [SettingVisualizationIgnore]
        public bool Topmost { get; set; } = false;
        [Preserve]
        public int FPSLimit { get; set; } = 120;
#if !UNITY_ANDROID
        [Preserve]
        public bool VSync { get; set; } = true;
#endif
    }
    [Preserve]
    public class SoundOptions
    {
        [Preserve]
        public bool ForceMono { get; set; } = false;
        [Preserve]
        public SFXVolume Volume { get; set; } = new();
        [Preserve]
        public WasapiOptions Wasapi { get; set; } = new();
        [Preserve]
        public AsioOptions Asio { get; set; } = new();
        [Preserve]
        public ChannelOptions Channel { get; set; } = new();
        [Preserve]
        public SoundBackendOption Backend { get; set; } = SoundBackendOption.Wasapi;
    }
    [Preserve]
    public class SFXVolume
    {
        [Preserve]
        public float Global { get; set; } = 0.3f;
        [Preserve]
        public float Answer { get; set; } = 0.8f;
        [Preserve]
        public float BGM { get; set; } = 1f;
        [Preserve]
        public float Track { get; set; } = 1f;
        [Preserve]
        public float Tap { get; set; } = 0.3f;
        [Preserve]
        public float Slide { get; set; } = 0.3f;
        [Preserve]
        public float Break { get; set; } = 0.3f;
        [Preserve]
        public float Touch { get; set; } = 0.3f;
        [Preserve]
        public float Voice { get; set; } = 1f;
    }
    [Preserve]
    public class ModOptions
    {
        [Preserve]
        public float PlaybackSpeed { get; set; } = 1f;
        [Preserve]
        public AutoplayModeOption AutoPlay { get; set; } = AutoplayModeOption.Disable;
        [Preserve]
        public JudgeStyleOption JudgeStyle { get; set; } = JudgeStyleOption.DEFAULT;
        [Preserve]
        public bool SubdivideSlideJudgeGrade { get; set; } = false;
        [Preserve]
        public bool AllBreak { get; set; } = false;
        [Preserve]
        public bool AllEx { get; set; } = false;
        [Preserve]
        public bool AllTouch { get; set; } = false;
        [Preserve]
        public bool SlideNoHead { get; set; } = false;
        [Preserve]
        public bool SlideNoTrack { get; set; } = false;
#if !UNITY_ANDROID
        [Preserve]
        public bool ButtonRingForTouch { get; set; } = false;
#endif
        [Preserve]
        public string NoteMask { get; set; } = "Disable";

        public bool IsAnyModActive()
        {
            return !(PlaybackSpeed == 1f &&
                !AllBreak && !AllEx && !AllTouch && AutoPlay == AutoplayModeOption.Disable && JudgeStyle == JudgeStyleOption.DEFAULT);
        }

    }
    [Preserve]
    public class OnlineOptions
    {
        [Preserve]
        public bool Enable { get; set; } = false;
#if UNITY_STANDALONE && ENABLE_MONO
        public bool UseProxy { get; init; } = true;
        public string Proxy { get; init; } = string.Empty;
#endif
        [Preserve]
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
    [Preserve]
    public class ApiEndpoint
    {
        [Preserve]
        public string Name { get; set; } = string.Empty;
        [Preserve]
        public string Url { get; set; } = string.Empty;
        [Preserve]
        public string? Username { get; set; }
        [Preserve]
        public string? Password { get; set; }
    }
    [Preserve]
    public class DebugOptions
    {
        [Preserve]
        public bool DisplaySensor { get; set; } = false;
        [Preserve]
        public bool DisplayFPS { get; set; } = true;
        [SettingVisualizationIgnore]
        public bool FullScreen { get; set; } = true;
        [SettingVisualizationIgnore]
        public int MenuOptionIterationSpeed { get; set; } = 45;
        [Preserve]
        public float DisplayOffset { get; set; } = 0f;
        [Preserve]
        public float NoteAppearRate { get; set; } = 0.265f;
        [Preserve]
        public OffsetUnitOption OffsetUnit { get; set; } = OffsetUnitOption.Second;
        [SettingVisualizationIgnore]
        public bool HideCursorInGame { get; set; } = true;
        [SettingVisualizationIgnore]
        public bool NoteFolding { get; set; } = true;
        [Preserve]
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
        [Preserve]
        [SettingVisualizationIgnore]
        [JsonProperty]
        internal MajDebug.LogLevel DebugLevel { get; set; } = MajDebug.LogLevel.Info;
    }
    [Preserve]
    public class IOOptions
    {
        [Preserve]
        public DeviceManufacturerOption? Manufacturer { get; set; } = null;
        [Preserve]
        public InputDeviceOptions InputDevice { get; set; } = new();
        [Preserve]
        public OutputDeviceOptions OutputDevice { get; set; } = new();
    }
    [Preserve]
    public class InputDeviceOptions
    {
        [Preserve]
        public int Player { get; set; } = 1;
        [Preserve]
        public ButtonRingOptions ButtonRing { get; set; } = new();
        [Preserve]
        public TouchPanelOptions TouchPanel { get; set; } = new();
    }
    [Preserve]
    public class OutputDeviceOptions
    {
        [Preserve]
        public LedOptions Led { get; set; } = new();
    }
    [Preserve]
    public class LedOptions
    {
        [Preserve]
        public bool Enable { get; set; } = true;
        [Preserve]
        public int RefreshRateMs { get; set; } = 100;
        [Preserve]
        public bool Throttler { get; set; } = false;
        [Preserve]
        public SerialPortOptions SerialPortOptions { get; set; } = new();
        [Preserve]
        public HidOptions HidOptions { get; set; } = new();
    }
    [Preserve]
    public class ButtonRingOptions
    {
        [Preserve]
        public ButtonRingDeviceOption? Type { get; set; } = null;
        [Preserve]
        public bool Debounce { get; set; } = false;
        [Preserve]
        public int PollingRateMs { get; set; } = 0;
        [Preserve]
        public int DebounceThresholdMs { get; set; } = 0;
        [Preserve]
        public HidOptions HidOptions { get; set; } = new();
    }
    [Preserve]
    public class TouchPanelOptions
    {
        [Preserve]
        public bool Debounce { get; set; } = false;
        [Preserve]
        public TouchPanelSensitivityConfig Sensitivities { get; set; } = default;
        [Preserve]
        public int PollingRateMs { get; set; } = 0;
        [Preserve]
        public int DebounceThresholdMs { get; set; } = 0;
        [Preserve]
        public float TouchSimulationRadius { get; set; } = 0.5f;
        [Preserve]
        public SerialPortOptions SerialPortOptions { get; set; } = new();
    }
    [Preserve]
    public class HidOptions
    {
        [Preserve]
        public string? DeviceName { get; set; } = null;
        [Preserve]
        public int? ProductId { get; set; } = null;
        [Preserve]
        public int? VendorId { get; set; } = null;
        [Preserve]
        public bool Exclusice { get; set; } = false;
        [Preserve]
        public OpenPriority OpenPriority { get; set; } = OpenPriority.VeryHigh;
    }
    [Preserve]
    public class SerialPortOptions
    {
        [Preserve]
        public int? Port { get; set; } = null;
        [Preserve]
        public int? BaudRate { get; set; } = null;
    }
    [Preserve]
    public struct TouchPanelSensitivityConfig
    {
        [Preserve]
        public short A { get; set; }
        [Preserve]
        public short B { get; set; }
        [Preserve]
        public short C { get; set; }
        [Preserve]
        public short D { get; set; }
        [Preserve]
        public short E { get; set; }
    }
    [Preserve]
    public class ChannelOptions
    {
        // Front (LF / RF)
        // Rear (LR / RR)
        // Side (LS / RS) (rear center)
        // CenterAndLFE (LFE / Center)
        public string Main { get; set; } = "Front";
    }
    public class AsioOptions
    {
        public int DeviceIndex { get; set; } = 0;
        public int SampleRate { get; set; } = 44100;
    }
    public class WasapiOptions
    {
        public bool Exclusive { get; set; } = true;
        public bool RawMode { get; set; } = true;
        public float BufferSize { get; set; } = 0.02f;
        public float Period { get; set; } = 0.005f;
    }
}