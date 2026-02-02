using AOT;
using MajdataPlay.Collections;
using MajdataPlay.IO;
using MajdataPlay.Scenes.Test;
using MajdataPlay.Settings;
using MajdataPlay.Timer;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;// DO NOT REMOVE IT !!!

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

        readonly static List<IntPtr> _windowHandles = new ();
        readonly static ReadOnlyMemory<ITimeProvider> _builtInTimeProviders = MajTimeline.BuiltInTimeProviders;

        void Start()
        {
            MajEnv.InitPath();
            MajDebug.Init();
            var s = "\n";
            s += $"################ MajdataPlay Startup Check ################\n";
            s += $"     OS       : {SystemInfo.operatingSystem}\n";
            s += $"     Model    : {SystemInfo.deviceModel} - {SystemInfo.deviceType}\n";
            s += $"     Processor: {SystemInfo.processorType}\n";
            s += $"     Memory   : {SystemInfo.systemMemorySize} MB\n";
            s += $"     Graphices: {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsMemorySize} MB) - {SystemInfo.graphicsDeviceType}\n";
            s += $"################     Startup Check  End    ################";
            MajDebug.LogInfo(s);
            MajDebug.LogInfo($"PID: {MajEnv.GameProcess.Id}");
            MajDebug.LogInfo($"Version: {MajInstances.GameVersion}");
#if UNITY_ANDROID
            MajDebug.LogInfo($"AndroidSdkVersion: {MajEnv.AndroidSdkVersion}");
#endif
            MajEnv.Init();
            MajInstances.FPSDisplayer.Init();
            MajInstances.AudioManager.Init();
            Localization.Init();
            MajInstances.SceneSwitcher.RefreshPos();
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
            if (!availableLangs.IsEmpty())
            {
                var lang = availableLangs.Find(x => x.ToString() == Setting.Display.Language);
                if (lang is null)
                {
                    lang = availableLangs.First();
                    Setting.Display.Language = lang.ToString();
                }
                Localization.Current = lang;
            }

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
#if !(UNITY_ANDROID || UNITY_IOS)
            QualitySettings.vSyncCount = Setting.Display.VSync ? 1 : 0;
#endif
            QualitySettings.maxQueuedFrames = Setting.Debug.MaxQueuedFrames;
            DetectHWEncoder();
            if (Setting.Display.Topmost)
            {
                SetWindowTopmost();
            }

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
#if (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
            Win32API.EnumWindows(EnumWindowsCallback, Process.GetCurrentProcess().Id);
            MajDebug.LogDebug($"Found window count: {_windowHandles.Count}");
            foreach (var handle in _windowHandles)
            {
                Win32API.SetWindowPos(handle, Win32API.HWND_TOPMOST, 0, 0, 0, 0, Win32API.SWP_NOMOVE | Win32API.SWP_NOSIZE);
            }
#endif
        }
        [MonoPInvokeCallback(typeof(Win32API.EnumWindowsProc))]
        static bool EnumWindowsCallback(IntPtr hWnd, int lParam)
        {
            _windowHandles.Clear();
            Win32API.GetWindowThreadProcessId(hWnd, out int processId);

            if (processId == lParam && Win32API.IsWindowVisible(hWnd))
            {
                _windowHandles.Add(hWnd);
            }
            return true;
        }
        void EnterTestMode()
        {
            IOListener.NextScene = "Title";
#if UNITY_STANDALONE_WIN && ENABLE_MONO
            MajEnv.GameProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
#endif
            SceneManager.LoadScene("Test");
        }
        void EnterTitle()
        {
#if UNITY_STANDALONE_WIN && ENABLE_MONO
            MajEnv.GameProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
#endif
            SceneManager.LoadScene("Title");
        }
        void EnterView()
        {
#if UNITY_STANDALONE_WIN && ENABLE_MONO
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
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            MajEnv.OnApplicationQuitRequested();
        }
#if UNITY_ANDROID || UNITY_IOS
        void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                MajEnv.RequestSave();
            }
        }
        void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                MajEnv.RequestSave();
            }
        }
#endif

        public void EnableGC()
        {
#if UNITY_ANDROID && !UNITY_EDITOR //&& false
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            MajDebug.LogWarning("GC has been enabled");
#else
            GC.Collect();
#endif
        }
        public void DisableGC() 
        {
#if UNITY_ANDROID && !UNITY_EDITOR //&& false
            GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
            MajDebug.LogWarning("GC has been disabled");
#else
            GC.Collect();
#endif
        }
    }
}