using MajdataPlay.Utils;
using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;// DO NOT REMOVE IT !!!
using MajdataPlay.Settings;
using MajdataPlay.Timer;
using MajdataPlay.Collections;
using System.Reflection;
using UnityEngine.SceneManagement;
using MajdataPlay.IO;
using MajdataPlay.Scenes.Test;
using System.Collections.Generic;

namespace MajdataPlay
{
#nullable enable
    internal sealed class GameManager : MajSingleton
    {
        public static Camera MainCamera { get; private set; }
        public GameSetting Setting
        {
            get => MajInstances.Settings;
        }
        /// <summary>
        /// Current difficult
        /// </summary>
        public ChartLevel SelectedDiff
        {
            get
            {
                return _selectedDiff;
            }
            set 
            {
                _selectedDiff = value;
            }
        }
        private ChartLevel _selectedDiff = ChartLevel.Easy;


        [SerializeField]
        BuiltInTimeProvider _timer = BuiltInTimeProvider.Winapi;
        [SerializeField]
        Sprite _emptySongCover;
        [SerializeField]
        Material _holdShineMaterial;
        [SerializeField]
        Material _breakMaterial;
        [SerializeField]
        Material _defaultMaterial;

        [SerializeField]
        bool _isEnterView = false;
        [SerializeField]
        bool _isEnterTest = false;

        readonly static ReadOnlyMemory<ITimeProvider> _builtInTimeProviders = MajTimeline.BuiltInTimeProviders;

        protected override void Awake()
        {
            //HttpTransporter.Timeout = TimeSpan.FromMilliseconds(10000);
            var s = "\n";
            s += $"################ MajdataPlay Startup Check ################\n";
            s += $"     OS       : {SystemInfo.operatingSystem}\n";
            s += $"     Model    : {SystemInfo.deviceModel} - {SystemInfo.deviceType}\n";
            s += $"     Processor: {SystemInfo.processorType}\n";
            s += $"     Memory   : {SystemInfo.systemMemorySize} MB\n";
            s += $"     Graphices: {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsMemorySize} MB) - {SystemInfo.graphicsDeviceType}\n";
            s += $"################     Startup Check  End    ################";
            MajDebug.Log(s);
            MajDebug.Log($"PID: {MajEnv.GameProcess.Id}");
            MajDebug.Log($"Version: {MajInstances.GameVersion}");
            MajEnv.Init();
            base.Awake();
#if UNITY_STANDALONE_WIN
            _timer = BuiltInTimeProvider.Winapi;
#else
            _timer = BuiltInTimeProvider.Stopwatch;
#endif
            MajTimeline.TimeProvider = _builtInTimeProviders.Span[(int)_timer];
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (arg == "--test-mode")
                {
                    MajEnv.Mode = RunningMode.Test;
                    break;
                }
                if (arg == "--view-mode")
                {
                    MajEnv.Mode = RunningMode.View;
                    Setting.Mod.AutoPlay = AutoplayModeOption.Enable;
                    break;
                }
            }

#if UNITY_EDITOR
            if(_isEnterTest)
            {
                MajEnv.Mode = RunningMode.Test;
            }
            else if (_isEnterView)
            {
                MajEnv.Mode = RunningMode.View;
                Setting.Mod.AutoPlay = AutoplayModeOption.Enable;
            }
#endif

            ApplyScreenConfig();

            var availableLangs = Localization.Available;
            if (availableLangs.IsEmpty())
                return;
            var lang = availableLangs.Find(x => x.ToString() == Setting.Display.Language);
            if (lang is null)
            {
                lang = availableLangs.First();
                Setting.Display.Language = lang.ToString();
            }
            Localization.Current = lang;

            var envType = typeof(MajEnv);

            envType.GetField("<EmptySongCover>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic)
                   .SetValue(null, _emptySongCover);
            envType.GetField("<BreakMaterial>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic)
                   .SetValue(null, _breakMaterial);
            envType.GetField("<DefaultMaterial>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic)
                   .SetValue(null, _defaultMaterial);
            envType.GetField("<HoldShineMaterial>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic)
                   .SetValue(null, _holdShineMaterial);
            QualitySettings.SetQualityLevel((int)Setting.Display.RenderQuality, true);
            QualitySettings.vSyncCount = Setting.Display.VSync ? 1 : 0;
            QualitySettings.maxQueuedFrames = Setting.Debug.MaxQueuedFrames;
            DetectHWEncoder();
            if (Setting.Display.Topmost)
            {
                SetWindowTopmost();
            }
        }
        void DetectHWEncoder()
        {
            var deviceName = SystemInfo.graphicsDeviceName.ToLower();
            HardwareEncoder encoder;
            if(deviceName.Contains("nvidia"))
            {
                encoder = HardwareEncoder.NVENC;
            }
            else if(deviceName.Contains("amd"))
            {
                encoder = HardwareEncoder.AMF;
            }
            else if(deviceName.Contains("intel"))
            {
                encoder = HardwareEncoder.QSV;
            }
            else
            {
                encoder = HardwareEncoder.None;
            }
            var envType = typeof(MajEnv);

            envType.GetField("<HWEncoder>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic)
                   .SetValue(null, encoder);
        }
        void SetWindowTopmost()
        {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            var handles = new List<IntPtr>();
            Win32API.EnumWindows((hWnd, lParam) =>
            {
                Win32API.GetWindowThreadProcessId(hWnd, out int processId);

                if (processId == lParam && Win32API.IsWindowVisible(hWnd))
                {
                    handles.Add(hWnd);
                }
                return true;
            }, Process.GetCurrentProcess().Id);
            MajDebug.Log($"Found window count: {handles.Count}");
            foreach (var handle in handles)
            {
                Win32API.SetWindowPos(handle, Win32API.HWND_TOPMOST, 0, 0, 0, 0, Win32API.SWP_NOMOVE | Win32API.SWP_NOSIZE);
            }
#endif
        }
        void EnterTestMode()
        {
            IOListener.NextScene = "Title";
            #if UNITY_STANDALONE_WIN
            MajEnv.GameProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
            #endif
            SceneManager.LoadScene("Test");
        }
        void EnterTitle()
        {
            #if UNITY_STANDALONE_WIN
            MajEnv.GameProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
            #endif
            SceneManager.LoadScene("Title");
        }
        void EnterView()
        {
            #if UNITY_STANDALONE_WIN
            MajEnv.GameProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
            #endif
            SceneManager.LoadScene("View");
        }
        public void ApplyScreenConfig()
        {
#if UNITY_STANDALONE_WIN
            if (MajEnv.Mode != RunningMode.View)
            {
                var fullScreen = Setting.Debug.FullScreen;
                Screen.fullScreen = fullScreen;
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;

                var resolution = Setting.Display.Resolution.ToLower();
                if (resolution is not "auto")
                {
                    var param = resolution.Split("x");
                    int width, height;

                    if (param.Length != 2)
                        return;
                    else if (!int.TryParse(param[0], out width) || !int.TryParse(param[1], out height))
                        return;
                    Screen.SetResolution(width, height, fullScreen);
                }
            }
#endif
            Application.targetFrameRate = Setting.Display.FPSLimit;
        }
        void Start()
        {
            SelectedDiff = Setting.Misc.SelectedDiff;
            SongStorage.OrderBy = Setting.Misc.OrderBy;
            InputManager.Init(Majdata<DummyTouchPanelRenderer>.Instance!.InstanceID2SensorIndexMappingTable);
            if (MajEnv.Mode == RunningMode.Test)
            {
                EnterTestMode();
                return;
            }
            if (MajEnv.Mode == RunningMode.View)
            {
                EnterView();
                return;
            }

            EnterTitle();
        }
        void Update()
        {
            ChangeTimerIfRequested();
        }
        [Conditional("DEBUG")]
        void ChangeTimerIfRequested()
        {
            var builtInTimeProviders = _builtInTimeProviders.Span;
            var selectedTimer = builtInTimeProviders[(int)_timer];
            if (MajTimeline.TimeProvider != selectedTimer)
            {
                MajDebug.LogWarning($"Time provider changed:\nOld:{MajTimeline.TimeProvider}\nNew:{selectedTimer}");
                MajTimeline.TimeProvider = selectedTimer;
            }
        }
        void OnApplicationQuit()
        {
            Save();
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            MajEnv.OnApplicationQuitRequested();
        }
        public void Save()
        {
            Setting.Misc.SelectedDiff = SelectedDiff;
            Setting.Misc.SelectedIndex = SongStorage.WorkingCollection.Index;
            Setting.Misc.SelectedDir = SongStorage.CollectionIndex;
            //SongStorage.OrderBy.Keyword = string.Empty;
            Setting.Misc.OrderBy = SongStorage.OrderBy;

            var json = Serializer.Json.Serialize(Setting, MajEnv.UserJsonReaderOption);
            File.WriteAllText(MajEnv.SettingPath, json);
        }
        public void EnableGC()
        {
            GC.Collect();
            return;
#if !UNITY_EDITOR
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            MajDebug.LogWarning("GC has been enabled");
#endif
        }
        public void DisableGC() 
        {
            GC.Collect();
            return;
#if !UNITY_EDITOR
            GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
            MajDebug.LogWarning("GC has been disabled");
#endif
        }
    }
}