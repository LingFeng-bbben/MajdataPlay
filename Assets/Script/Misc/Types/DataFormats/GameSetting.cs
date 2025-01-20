using System.Collections.Generic;
using System.Text.Json.Serialization;
using UnityEngine.Rendering;
#nullable enable
namespace MajdataPlay.Types
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
        public OnlineOptions Online { get; set; } = new();
        public MiscOptions Misc { get; set; } = new();
    }
    public class GameOptions
    {
        public float TapSpeed { get; set; } = 7.5f;
        public float TouchSpeed { get; set; } = 7.5f;
        public float SlideFadeInOffset { get; set; } = 0f;
        public float BackgroundDim { get; set; } = 0.8f;
        public bool StarRotation { get; set; } = true;
        public BGInfoType BGInfo { get; set; } = BGInfoType.Combo;
        public MirrorType Mirror { get; set; } = MirrorType.Off;
        public int Rotation { get; set; } = 0;
        public string Language { get; set; } = "zh-CN - Majdata";
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
        public bool DisplayBreakScore { get; set; } = true;
        public JudgeDisplayType FastLateType { get; set; } = JudgeDisplayType.Disable;
        public JudgeDisplayType NoteJudgeType { get; set; } = JudgeDisplayType.All;
        public JudgeDisplayType TouchJudgeType { get; set; } = JudgeDisplayType.All;
        public JudgeDisplayType SlideJudgeType { get; set; } = JudgeDisplayType.All;
        public JudgeDisplayType BreakJudgeType { get; set; } = JudgeDisplayType.All;
        public JudgeDisplayType BreakFastLateType { get; set; } = JudgeDisplayType.Disable;
        public JudgeMode SlideSortOrder { get; set; } = JudgeMode.Modern;
        /// <summary>
        /// Such like Tap、Star、Hold and Break
        /// </summary>
        public float OuterJudgeDistance { get; set; } = 1f;
        /// <summary>
        /// Such like Touch and TouchHold
        /// </summary>
        public float InnerJudgeDistance { get; set; } = 1f;
        public TouchFeedbackLevel TouchFeedback { get; set; } = TouchFeedbackLevel.All;
        public string Resolution { get; set; } = "Auto";
        public int TargetFPS = 240;
    }
    public class SoundOptions
    {
        public int Samplerate { get; set; } = 44100;
        public int AsioDeviceIndex { get; set; } = 0;
        public bool WasapiExclusive { get; set; } = true;
        public SFXVolume Volume { get; set; } = new();
        public SoundBackendType Backend { get; set; } = SoundBackendType.Wasapi;
    }
    public class SFXVolume
    {
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
        public bool AllBreak { get; set; } = false;
        public bool AllEx { get; set; } = false;
        public bool AllTouch { get; set; } = false;
        //public bool SlideNoHead { get; set; } = false;
        //public bool SlideNoTrack { get; set; } = false;
        public string NoteMask { get; set; } = "Disable";
        public bool AutoPlay { get; set; } = false;
        public JudgeStyleType JudgeStyle { get; set; } = JudgeStyleType.DEFAULT;

        public bool IsAnyModActive()
        {
            return !(PlaybackSpeed == 1f &&
                !AllBreak&&!AllEx&&!AllTouch&&!AutoPlay&& JudgeStyle == JudgeStyleType.DEFAULT);
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
        public bool FullScreen { get; set; } = true;
        public bool TryFixAudioSync { get; set; } = false;
        public float NoteAppearRate { get; set; } = 0.265f;
        public bool DisableGCInGameing { get; set; } = true;
    }
    public class MiscOptions
    {
        public InputDeviceOptions InputDevice { get; set; } = new();
        public OutputDeviceOptions OutputDevice { get; set; } = new();
        public int SelectedIndex { get; set; } = 0;
        public int SelectedDir { get; set; } = 0;
        public ChartLevel SelectedDiff { get; set; } = ChartLevel.Easy;
        public SongOrder OrderBy { get; set; } = new();
    }
    public class InputDeviceOptions
    {
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
        public int COMPort { get; set; } = 21;
        public int RefreshRateMs { get; set; } = 16;
    }
    public class ButtonRingOptions
    {
        public DeviceType Type { get; set; } = DeviceType.Keyboard;
        public int ProductId { get; set; } = 0x0021;
        public int VendorId { get; set; } = 0x0CA3;
        public bool Debounce { get; set; } = true;
        public int PollingRateMs { get; set; } = 0;
        public int DebounceThresholdMs { get; set; } = 5;
    }
    public class TouchPanelOptions
    {
        public int COMPort { get; set; } = 3;
        public bool Debounce { get; set; } = true;
        public int PollingRateMs { get; set; } = 2;
        public int DebounceThresholdMs { get; set; } = 2;
    }
}