using MajdataPlay.Types;
using MajdataPlay.Utils;
using MajdataPlay.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;// DO NOT REMOVE IT !!!
using MajdataPlay.Timer;
using MajdataPlay.Collections;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using MajdataPlay.SensorTest;
using System.Collections.Generic;

namespace MajdataPlay
{
#nullable enable
    public class GameManager : MonoBehaviour
    {
        public static CancellationToken GlobalCT { get; }
        public GameSetting Setting
        {
            get => MajInstances.Setting;
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
        public int LastSettingPage { get; set; } = 0;


        readonly static CancellationTokenSource _globalCTS;
        [SerializeField]
        TimerType _timer = MajTimeline.Timer;
        [SerializeField]
        Sprite _emptySongCover;
        [SerializeField]
        Material _holdShineMaterial;
        [SerializeField]
        Material _breakMaterial;
        [SerializeField]
        Material _defaultMaterial;
        public bool IsEnterView = false;
        static GameManager()
        {
            _globalCTS = new();
            GlobalCT = _globalCTS.Token;
        }
        void Awake()
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
            MajDebug.Log($"Version: {MajInstances.GameVersion}");
            MajInstances.GameManager = this;
            _timer = MajTimeline.Timer;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            DontDestroyOnLoad(this);

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
                    break;
                }
            }

#if UNITY_EDITOR
            if (IsEnterView)
            {
                MajEnv.Mode = RunningMode.View;
            }
#endif

            ApplyScreenConfig();

            var availableLangs = Localization.Available;
            if (availableLangs.IsEmpty())
                return;
            var lang = availableLangs.Find(x => x.ToString() == Setting.Game.Language);
            if (lang is null)
            {
                lang = availableLangs.First();
                Setting.Game.Language = lang.ToString();
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
            QualitySettings.SetQualityLevel((int)Setting.Game.RenderQuality, true);
            QualitySettings.vSyncCount = Setting.Display.VSync ? 1 : 0;
            QualitySettings.maxQueuedFrames = Setting.Debug.MaxQueuedFrames;

            if(Setting.Display.Topmost)
            {
                SetWindowTopmost();
            }
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
            SceneManager.LoadScene("SensorTest");
        }
        void EnterTitle()
        {
            SceneManager.LoadScene("Title");
        }
        void EnterView()
        {
            SceneManager.LoadScene("View");
        }
        void ApplyScreenConfig()
        {
            if (MajEnv.Mode != RunningMode.View)
            {
                var fullScreen = Setting.Debug.FullScreen;
                Screen.fullScreen = fullScreen;

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
            Application.targetFrameRate = Setting.Display.FPSLimit;
        }
        void Start()
        {
            SelectedDiff = Setting.Misc.SelectedDiff;
            SongStorage.OrderBy = Setting.Misc.OrderBy;

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
            if(MajTimeline.Timer != _timer)
            {
                MajDebug.LogWarning($"Time provider changed:\nOld:{MajTimeline.Timer}\nNew:{_timer}");
                MajTimeline.Timer = _timer;
            }
        }
        void OnApplicationQuit()
        {
            Save();
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            MajDebug.OnApplicationQuit();
            MajInstances.LightManager.SetAllLight(Color.black);
            _globalCTS.CancelAfter(2000);
        }
        public void Save()
        {
            Setting.Misc.SelectedDiff = SelectedDiff;
            Setting.Misc.SelectedIndex = SongStorage.WorkingCollection.Index;
            Setting.Misc.SelectedDir = SongStorage.CollectionIndex;
            SongStorage.OrderBy.Keyword = string.Empty;
            Setting.Misc.OrderBy = SongStorage.OrderBy;

            var json = Serializer.Json.Serialize(Setting, MajEnv.UserJsonReaderOption);
            File.WriteAllText(MajEnv.SettingPath, json);
        }
        public void EnableGC()
        {
            if (!Setting.Debug.DisableGCInGameing)
                return;
#if !UNITY_EDITOR
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            MajDebug.LogWarning("GC has been enabled");
#endif
            GC.Collect();
        }
        public void DisableGC() 
        {
            if (!Setting.Debug.DisableGCInGameing)
                return;
            GC.Collect();
#if !UNITY_EDITOR
            GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
            MajDebug.LogWarning("GC has been disabled");
#endif
        }
    }
}