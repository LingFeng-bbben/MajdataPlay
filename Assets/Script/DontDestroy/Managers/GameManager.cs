using MajdataPlay.Types;
using MajdataPlay.Utils;
using MajdataPlay.Extensions;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json.Serialization;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Scripting;
using MajdataPlay.Types.Attribute;
using MajdataPlay.Net;
using System.Collections.Concurrent;
using MychIO;

namespace MajdataPlay
{
#nullable enable
    public class GameManager : MonoBehaviour
    {
        public static GameResult? LastGameResult { get; set; } = null;
        public CancellationToken AllTaskToken { get => _tokenSource.Token; }
        public static DateTime StartAt { get; } = DateTime.Now;
        public static ConcurrentQueue<Action> ExecutionQueue { get; } = IOManager.ExecutionQueue;
        public static string AssestsPath { get; } = Path.Combine(Application.dataPath, "../");
        public static string ChartPath { get; } = Path.Combine(AssestsPath, "MaiCharts");
        public static string SettingPath { get; } = Path.Combine(AssestsPath, "settings.json");
        public static string SkinPath { get; } = Path.Combine(AssestsPath, "Skins");
        public static string CachePath { get; } = Path.Combine(AssestsPath, "Cache");
        public static string LogsPath { get; } = Path.Combine(AssestsPath, $"Logs");
        public static string LangPath { get; } = Path.Combine(Application.streamingAssetsPath, "Langs");
        public static string ModPath { get; } = Path.Combine(Application.streamingAssetsPath, "Mods");
        public static string ScoreDBPath { get; } = Path.Combine(AssestsPath, "MajDatabase.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db");
        public static string LogPath { get; } = Path.Combine(LogsPath, $"MajPlayRuntime_{DateTime.Now:yyyy-MM-dd_HH_mm_ss}.log");
        public static JsonSerializerOptions JsonReaderOption { get; } = new()
        {

            Converters =
            {
                new JsonStringEnumConverter()
            },
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };
        public GameSetting Setting
        {
            get => MajInstances.Setting;
            set => MajInstances.Setting = value;
        }
        /// <summary>
        /// Current difficult
        /// </summary>
        public ChartLevel SelectedDiff { get; set; } = ChartLevel.Easy;
        public bool UseUnityTimer 
        { 
            get => _useUnityTimer; 
            set => _useUnityTimer = value; 
        }

        CancellationTokenSource _tokenSource = new();
        Task? _logWritebackTask = null;
        Queue<GameLog> _logQueue = new();
        

        void Awake()
        {
            //HttpTransporter.Timeout = TimeSpan.FromMilliseconds(10000);
            Application.logMessageReceived += (c, trace, type) =>
            {
                _logQueue.Enqueue(new GameLog()
                {
                    Date = DateTime.Now,
                    Condition = c,
                    StackTrace = trace,
                    Type = type
                });
            };
            _logWritebackTask = LogWriteback();
            Debug.Log($"Version: {MajInstances.GameVersion}");
            MajInstances.GameManager = this;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            DontDestroyOnLoad(this);

            if (File.Exists(SettingPath))
            {
                var js = File.ReadAllText(SettingPath);
                GameSetting? setting;

                if (!Serializer.Json.TryDeserialize(js, out setting, JsonReaderOption) || setting is null)
                {
                    Setting = new();
                    Debug.LogError("Failed to read setting from file");
                }
                else
                    Setting = setting;
            }
            else
            {
                Setting = new GameSetting();
                Save();
            }
            MajInstances.Setting = Setting;
            Setting.Misc.IOPollingRateMs = Math.Max(0, Setting.Misc.IOPollingRateMs);
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
            Localization.Initialize();
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
            GameModHelper.Initialize();
        }
        void Start()
        {
            SelectedDiff = Setting.Misc.SelectedDiff;
            SongStorage.OrderBy = Setting.Misc.OrderBy;
        }
        private void OnApplicationQuit()
        {
            Save();
            GameModHelper.Save();
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            _tokenSource.Cancel();
            foreach (var log in _logQueue)
                File.AppendAllText(LogPath, $"[{log.Date:yyyy-MM-dd HH:mm:ss}][{log.Type}] {log.Condition}\n{log.StackTrace}");
        }
        public void Save()
        {
            Setting.Misc.SelectedDiff = SelectedDiff;
            Setting.Misc.SelectedIndex = SongStorage.WorkingCollection.Index;
            Setting.Misc.SelectedDir = SongStorage.CollectionIndex;
            SongStorage.OrderBy.Keyword = string.Empty;
            Setting.Misc.OrderBy = SongStorage.OrderBy;

            var json = Serializer.Json.Serialize(Setting, JsonReaderOption);
            File.WriteAllText(SettingPath, json);
        }
        public void EnableGC()
        {
            if (!Setting.Debug.DisableGCInGameing)
                return;
#if !UNITY_EDITOR
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            Debug.LogWarning("GC has been enabled");
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
            Debug.LogWarning("GC has been disabled");
#endif
        }
        async Task LogWriteback()
        {
            var oldLogPath = Path.Combine(AssestsPath, "MajPlayRuntime.log");
            if (!Directory.Exists(LogsPath))
                Directory.CreateDirectory(LogsPath);
            if (File.Exists(oldLogPath))
                File.Delete(oldLogPath);
            if (File.Exists(LogPath))
                File.Delete(LogPath);
            while (true)
            {
                if (_logQueue.Count == 0)
                {
                    if (AllTaskToken.IsCancellationRequested)
                        return;
                    await Task.Delay(50);
                    continue;
                }
                var log = _logQueue.Dequeue();
                await File.AppendAllTextAsync(LogPath, $"[{log.Date:yyyy-MM-dd HH:mm:ss}][{log.Type}] {log.Condition}\n{log.StackTrace}");
            }
        }
        class GameLog
        {
            public DateTime Date { get; set; }
            public string? Condition { get; set; }
            public string? StackTrace { get; set; }
            public LogType? Type { get; set; }
        }
        [SerializeField]
        bool _useUnityTimer = false;
    }
}