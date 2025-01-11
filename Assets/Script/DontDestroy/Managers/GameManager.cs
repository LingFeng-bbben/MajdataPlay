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

namespace MajdataPlay
{
#nullable enable
    public class GameManager : MonoBehaviour
    {
        public static CancellationToken GlobalCT { get; }
        public GameSetting Setting
        {
            get => MajInstances.Setting;
            set => MajInstances.Setting = value;
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

        //public bool isDanMode = false;
        //public int DanHP = 500;
        //public List<GameResult> DanResults = new();

        readonly static CancellationTokenSource _globalCTS;
        [SerializeField]
        TimerType _timer = MajTimeline.Timer;

        static GameManager()
        {
            _globalCTS = new();
            GlobalCT = _globalCTS.Token;
        }
        void Awake()
        {
            //HttpTransporter.Timeout = TimeSpan.FromMilliseconds(10000);
#if !UNITY_EDITOR
            if(Process.GetProcessesByName("MajdataPlay").Length > 1)
            {
                Application.Quit();
            }
#endif
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

            if (File.Exists(MajEnv.SettingPath))
            {
                var js = File.ReadAllText(MajEnv.SettingPath);
                GameSetting? setting;

                if (!Serializer.Json.TryDeserialize(js, out setting, MajEnv.UserJsonReaderOption) || setting is null)
                {
                    Setting = new();
                    MajDebug.LogError("Failed to read setting from file");
                }
                else
                {
                    Setting = setting;
                    //Reset Mod option after reboot
                    Setting.Mod = new ModOptions();
                }
            }
            else
            {
                Setting = new GameSetting();
                Save();
            }
            MajInstances.Setting = Setting;
            Setting.Misc.InputDevice.ButtonRing.PollingRateMs = Math.Max(0, Setting.Misc.InputDevice.ButtonRing.PollingRateMs);
            Setting.Misc.InputDevice.TouchPanel.PollingRateMs = Math.Max(0, Setting.Misc.InputDevice.TouchPanel.PollingRateMs);
            Setting.Misc.InputDevice.ButtonRing.DebounceThresholdMs = Math.Max(0, Setting.Misc.InputDevice.ButtonRing.DebounceThresholdMs);
            Setting.Misc.InputDevice.TouchPanel.DebounceThresholdMs = Math.Max(0, Setting.Misc.InputDevice.TouchPanel.DebounceThresholdMs);
            Setting.Display.InnerJudgeDistance = Setting.Display.InnerJudgeDistance.Clamp(0, 1);
            Setting.Display.OuterJudgeDistance = Setting.Display.OuterJudgeDistance.Clamp(0, 1);

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
            var thiss = Process.GetCurrentProcess();
            thiss.PriorityClass = ProcessPriorityClass.RealTime;
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
        }
        void Start()
        {
            SelectedDiff = Setting.Misc.SelectedDiff;
            SongStorage.OrderBy = Setting.Misc.OrderBy;
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
            _globalCTS.Cancel();
            MajDebug.OnApplicationQuit();
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