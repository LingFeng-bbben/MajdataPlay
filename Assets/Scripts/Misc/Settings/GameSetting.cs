using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine.Scripting;
using Newtonsoft.Json;
using MajdataPlay.Net;
using MajdataPlay.Settings.OptionEnumerators;
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
        [HideInSettingUI]
        public OnlineOptions Online { get; init; } = new();
        [HideInSettingUI]
        public IOOptions IO { get; init; } = new();
    }
    [Preserve]
    public class GameOptions
    {
        [Preserve]
        [Step("0.25")]
        [Range(HasMax = false, HasMin = false)]
        public float TapSpeed { get; set; } = 7.5f;
        [Preserve]
        [Step("0.25")]
        [Range(HasMax = false, HasMin = false)]
        public float TouchSpeed { get; set; } = 7.5f;
        [Preserve]
        [OptionEnumerator(typeof(GameOffsetEnumerator))]
        public float SlideFadeInOffset { get; set; } = 0f;
        [Preserve]
        [Step("0.05")]
        [Range("0", "1" ,HasMax = true, HasMin = true)]
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
        [Step("1")]
        [Range("-7", "7", HasMax = true, HasMin = true)]
        public int Rotation { get; set; } = 0;
        [Preserve]
        public RandomModeOption Random { get; set; } = RandomModeOption.Disabled;
#if UNITY_ANDROID || UNITY_IOS
        [Preserve]
        public bool ButtonRingForTouch { get; set; } = true;
#endif

#if UNITY_STANDALONE
        [Preserve]
        public RecordModeOption RecordMode { get; set; } = RecordModeOption.Disable;
#endif
    }
    [Preserve]
    public class JudgeOptions
    {
        [Preserve]
        [OptionEnumerator(typeof(GameOffsetEnumerator))]
        public float AudioOffset { get; set; } = 0f;
        [Preserve]
        [OptionEnumerator(typeof(GameOffsetEnumerator))]
        public float JudgeOffset { get; set; } = 0f;
        [Preserve]
        [OptionEnumerator(typeof(GameOffsetEnumerator))]
        public float AnswerOffset { get; set; } = 0f;
        [Preserve]
        [OptionEnumerator(typeof(GameOffsetEnumerator))]
        public float TouchPanelOffset { get; set; } = 0f;
        [Preserve]
        public JudgeModeOption Mode { get; set; } = JudgeModeOption.Modern;
    }
    [Preserve]
    public class DisplayOptions
    {
        [Preserve]
        [OptionEnumerator(typeof(LanguageEnumerator))]
        public string Language { get; set; } = "zh-CN - Majdata";
        [Preserve]
        [OptionEnumerator(typeof(SkinEnumerator))]
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
        [Step("0.05")]
        [Range("0", "1", HasMax = true, HasMin = true)]
        public float OuterJudgeDistance { get; set; } = 1f;
        /// <summary>
        /// Such like Touch and TouchHold
        /// </summary>
        [Preserve]
        [Step("0.05")]
        [Range("0", "1", HasMax = true, HasMin = true)]
        public float InnerJudgeDistance { get; set; } = 1f;
        [Preserve]
        public bool DisplayHoldHeadJudgeResult { get; set; } = false;
        [Preserve]
        [Step("0.01")]
        [Range("0", "2", HasMax = true, HasMin = true)]
        public float TapScale { get; set; } = 1f;
        [Preserve]
        [Step("0.01")]
        [Range("0", "2", HasMax = true, HasMin = true)]
        public float HoldScale { get; set; } = 1f;
        [Preserve]
        [Step("0.01")]
        [Range("0", "2", HasMax = true, HasMin = true)]
        public float TouchScale { get; set; } = 1f;
        [Preserve]
        [Step("0.01")]
        [Range("0", "2", HasMax = true, HasMin = true)]
        public float SlideScale { get; set; } = 1f;
        [Preserve]
        public TouchFeedbackLevel TouchFeedback { get; set; } = TouchFeedbackLevel.Outer_Only;
#if UNITY_STANDALONE
        [HideInSettingUI]
        public string Resolution { get; set; } = "1080x1920";
#endif
        [Preserve]
#if UNITY_ANDROID || UNITY_IOS
        public bool MainScreenTransform { get; set; } = true;
#else
        public bool MainScreenTransform { get; set; } = false;
#endif
        [Preserve]
        [Step("0.01")]
        [Range("0.05", "1.5", HasMax = true, HasMin = true)]
        public float MainScreenScale { get; set; } = 1f;
        [Preserve]
        [Step("0.1")]
        [Range("-1", "1", HasMax = true, HasMin = true)]
        public float MainScreenOffset { get; set; } = 1f;
        [HideInSettingUI]
        public float MainScreenCachedScreenCenterY { get; set; } = 960f;
        [Preserve]
        [Step("0.01")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float SubDisplayOffset { get; set; } = 0f;
        [Preserve]
        [Step("0.01")]
        public float SubDisplayScale { get; set; } = 1f;
        [Preserve]
        [OptionEnumerator(typeof(EngineEnumSettingEnumerator))]
        public RenderQualityOption RenderQuality { get; set; } = RenderQualityOption.Low;
#if UNITY_STANDALONE
        [HideInSettingUI]
        public bool Topmost { get; set; } = false;
#endif
        [Preserve]
        [Step("1")]
        [Range("-1", null, HasMax = false, HasMin = true)]
        [OptionEnumerator(typeof(EngineNumberSettingEnumerator))]
        public int FPSLimit { get; set; } = 120;
#if !(UNITY_ANDROID || UNITY_IOS)
        [Preserve]
        [OptionEnumerator(typeof(EngineBooleanSettingEnumerator))]
        public bool VSync { get; set; } = true;
#endif
        [Preserve]
        public bool SkipVideoDownload { get; set; } = false;
    }
    [Preserve]
    public class SoundOptions
    {
#if UNITY_IOS || UNITY_ANDROID
        readonly static SoundBackendOption DEFAULT_SOUND_BACKEND = SoundBackendOption.BassSimple;
#else
        readonly static SoundBackendOption DEFAULT_SOUND_BACKEND = SoundBackendOption.Wasapi;
#endif
        [Preserve]
        public bool ForceMono { get; set; } = false;
        [Preserve]
        public SFXVolume Volume { get; set; } = new();
#if !(UNITY_ANDROID || UNITY_IOS)
        [Preserve]
        public WasapiOptions Wasapi { get; set; } = new();
        [Preserve]
        public AsioOptions Asio { get; set; } = new();
        [Preserve]
        public ChannelOptions Channel { get; set; } = new();
#else
        public MobileAudioOptions Mobile { get; set; } = new();
#endif
        [Preserve]
        public SoundBackendOption Backend { get; set; } = DEFAULT_SOUND_BACKEND;
    }
    [Preserve]
    public class SFXVolume
    {
        [Preserve]
        [Step("0.05")]
        [Range("0", "1", HasMax = true, HasMin = true)]
        [OptionEnumerator(typeof(AudioVolumeEnumerator))]
        public float Global { get; set; } = 0.3f;
        [Preserve]
        [Step("0.05")]
        [Range("0", "1", HasMax = true, HasMin = true)]
        [OptionEnumerator(typeof(AudioVolumeEnumerator))]
        public float Answer { get; set; } = 0.8f;
        [Preserve]
        [Step("0.05")]
        [Range("0", "1", HasMax = true, HasMin = true)]
        [OptionEnumerator(typeof(AudioVolumeEnumerator))]
        public float BGM { get; set; } = 1f;
        [Preserve]
        [Step("0.05")]
        [Range("0", "1", HasMax = true, HasMin = true)]
        [OptionEnumerator(typeof(AudioVolumeEnumerator))]
        public float Track { get; set; } = 1f;
        [Preserve]
        [Step("0.05")]
        [Range("0", "1", HasMax = true, HasMin = true)]
        [OptionEnumerator(typeof(AudioVolumeEnumerator))]
        public float Tap { get; set; } = 0.3f;
        [Preserve]
        [Step("0.05")]
        [Range("0", "1", HasMax = true, HasMin = true)]
        [OptionEnumerator(typeof(AudioVolumeEnumerator))]
        public float Slide { get; set; } = 0.3f;
        [Preserve]
        [Step("0.05")]
        [Range("0", "1", HasMax = true, HasMin = true)]
        [OptionEnumerator(typeof(AudioVolumeEnumerator))]
        public float Break { get; set; } = 0.3f;
        [Preserve]
        [Step("0.05")]
        [Range("0", "1", HasMax = true, HasMin = true)]
        [OptionEnumerator(typeof(AudioVolumeEnumerator))]
        public float Touch { get; set; } = 0.3f;
        [Preserve]
        [Step("0.05")]
        [Range("0", "1", HasMax = true, HasMin = true)]
        [OptionEnumerator(typeof(AudioVolumeEnumerator))]
        public float Voice { get; set; } = 1f;
    }
    [Preserve]
    public class ModOptions
    {
        [Preserve]
        [Step("0.05")]
        [Range("0", null, HasMax = false, HasMin = true)]
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
#if !(UNITY_ANDROID || UNITY_IOS)
        [Preserve]
        public bool ButtonRingForTouch { get; set; } = false;
#endif
        [Preserve]
        [OptionEnumerator(typeof(NoteMaskEnumerator))]
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
#if UNITY_IOS
        [JsonIgnore]
#endif
        public bool Enable { get; set; } = false;
#if UNITY_STANDALONE && ENABLE_MONO
        public bool UseProxy { get; init; } = true;
        public string Proxy { get; init; } = string.Empty;
#endif
        [Preserve]
        public ApiEndpoint[] ApiEndpoints { get; set; } = new ApiEndpoint[]
        {
#if !UNITY_IOS
            new ApiEndpoint()
            {
                Name = "MajdataNET",
                Url = new("https://majdata.net/api3/api/"),
                Username = "YourUsername",
                Password = "YourPassword"
            }
#endif
        };
    }
    [Preserve]
    public class DebugOptions
    {
        [Preserve]
        public bool DisplaySensor { get; set; } = false;
#if UNITY_ANDROID
        [Preserve]
        [Step("0.05")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float TouchSimulationRadius { get; set; } = 0.5f;
        [Preserve]
        [Step("0.05")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float TouchAAreaExtraRadius { get; set; } = 0f;
        [Preserve]
        [Step("0.05")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float TouchBAreaExtraRadius { get; set; } = 0f;
        [Preserve]
        [Step("0.05")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float TouchCAreaExtraRadius { get; set; } = 0.23f;
        [Preserve]
        [Step("0.05")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float TouchDAreaExtraRadius { get; set; } = 0.2f;
        [Preserve]
        [Step("0.05")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float TouchEAreaExtraRadius { get; set; } = 0.12f;
        [Preserve]
        [Step("0.05")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float TouchRadiusAdjust { get; set; } = 0f;
#else
        [Preserve]
        [Step("0.05")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float TouchSimulationRadius { get; set; } = 0.5f;
        [Preserve]
        [Step("0.05")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float TouchAAreaExtraRadius { get; set; } = 0f;
        [Preserve]
        [Step("0.05")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float TouchBAreaExtraRadius { get; set; } = 0f;
        [Preserve]
        [Step("0.05")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float TouchCAreaExtraRadius { get; set; } = 0.23f;
        [Preserve]
        [Step("0.05")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float TouchDAreaExtraRadius { get; set; } = 0.2f;
        [Preserve]
        [Step("0.05")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float TouchEAreaExtraRadius { get; set; } = 0.12f;
        [Preserve]
        [Step("0.05")]
        [Range("-5", "5", HasMax = true, HasMin = true)]
        public float TouchRadiusAdjust { get; set; } = 0f;
#endif
        [Preserve]
        public bool DisplayFPS { get; set; } = true;
#if UNITY_STANDALONE
        [HideInSettingUI]
        public bool FullScreen { get; set; } = true;
#endif
        [HideInSettingUI]
        public int MenuOptionIterationSpeed { get; set; } = 45;
        [Preserve]
        [Range("0", null, HasMax = false, HasMin = true)]
        [OptionEnumerator(typeof(GameOffsetEnumerator))]
        public float DisplayOffset { get; set; } = 0f;
        [Preserve]
        [Step("0.001")]
        public float NoteAppearRate { get; set; } = 0.265f;
        [Preserve]
        public OffsetUnitOption OffsetUnit { get; set; } = OffsetUnitOption.Frame;
#if UNITY_STANDALONE
        [HideInSettingUI]
        public bool HideCursorInGame { get; set; } = true;
#endif
        [HideInSettingUI]
#if UNITY_IOS
        [JsonIgnore]
#endif
        public bool NoteFolding { get; set; } = true;
        [Preserve]
        public DJAutoPolicyOption DJAutoPolicy { get; set; } = DJAutoPolicyOption.Strict;
        [HideInSettingUI]
        public int MaxQueuedFrames { get; set; } = 2;
#if UNITY_IOS || UNITY_ANDROID
        [HideInSettingUI]
        public int TapPoolCapacity { get; set; } = 48;
        [HideInSettingUI]
        public int HoldPoolCapacity { get; set; } = 48;
        [HideInSettingUI]
        public int TouchPoolCapacity { get; set; } = 64;
        [HideInSettingUI]
        public int TouchHoldPoolCapacity { get; set; } = 64;
        [HideInSettingUI]
        public int EachLinePoolCapacity { get; set; } = 24;
#else
        [HideInSettingUI]
        public int TapPoolCapacity { get; set; } = 96;
        [HideInSettingUI]
        public int HoldPoolCapacity { get; set; } = 96;
        [HideInSettingUI]
        public int TouchPoolCapacity { get; set; } = 64;
        [HideInSettingUI]
        public int TouchHoldPoolCapacity { get; set; } = 64;
        [HideInSettingUI]
        public int EachLinePoolCapacity { get; set; } = 48;
#endif
        [Preserve]
        [HideInSettingUI]
#if UNITY_IOS
        [JsonIgnore]
#endif
        public LogLevel DebugLevel { get; set; } = LogLevel.Info;
    }
    [Preserve]
    public class IOOptions
    {
#if UNITY_STANDALONE
        [Preserve]
        public DeviceManufacturerOption? Manufacturer { get; set; } = null;
#endif
        [Preserve]
        public InputDeviceOptions InputDevice { get; set; } = new();
#if UNITY_STANDALONE
        [Preserve]
        public OutputDeviceOptions OutputDevice { get; set; } = new();
#endif
    }
    [Preserve]
    public class InputDeviceOptions
    {
#if UNITY_STANDALONE
        [Preserve]
        public int Player { get; set; } = 1;
        [Preserve]
        public ButtonRingOptions ButtonRing { get; set; } = new();
        [Preserve]
        public TouchPanelOptions TouchPanel { get; set; } = new();
#else
        [Preserve]
        public bool EnableKeyboardInput { get; set; } = false;
        public bool EnableGamepadInput { get; set; } = false;
#endif
    }
#if UNITY_STANDALONE
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
        public float Brightness { get; set; } = 1.0f;
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
        public SerialPortOptions SerialPortOptions { get; set; } = new();
        [Preserve]
        public UsbOptions UsbOptions { get; set; } = new();
        [Preserve]
        public CapacitiveTouchPanelOptions CapacitivePanelOptions { get; set; } = new();
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
        public HidOpenPriority OpenPriority { get; set; } = HidOpenPriority.VeryHigh;
    }
    [Preserve]
    public class UsbOptions
    {
        [Preserve]
        public string? DeviceName { get; set; } = null;
        [Preserve]
        public int? ProductId { get; set; } = null;
        [Preserve]
        public int? VendorId { get; set; } = null;
        [Preserve]
        public bool Exclusice { get; set; } = false;
    }
    [Preserve]
    public class CapacitiveTouchPanelOptions
    {
        [Preserve]
        public int TouchRadius { get; set; } = 30;
    }
    [Preserve]
    public class SerialPortOptions
    {
        [Preserve]
#if UNITY_STANDALONE_WIN
        public int? Port { get; set; } = null;
#else
        public string? Port { get; set; } = null;
#endif
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
#endif
    public class MobileAudioOptions
    {
#if UNITY_ANDROID // Android Only (AAudio)
        public bool EnableAAudio { get; set; } = true;
#endif
        public int BufferLengthMs { get; set; } = 128;
        public int UpdatePeriodMs { get; set; } = 16;
        public int DeviceBufferLengthMs { get; set; } = 32;
        public int DeviceUpdatePeriodMs { get; set; } = 4;
    }
}