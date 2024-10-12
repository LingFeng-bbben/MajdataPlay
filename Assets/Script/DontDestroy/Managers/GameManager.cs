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

namespace MajdataPlay
{
#nullable enable
    public class GameManager : MonoBehaviour
    {
        public static GameResult? LastGameResult { get; set; } = null;
        public CancellationToken AllTaskToken { get => tokenSource.Token; }
        
        public static string AssestsPath { get; } = Path.Combine(Application.dataPath, "../");
        public static string ChartPath { get; } = Path.Combine(AssestsPath, "MaiCharts");
        public static string SettingPath { get; } = Path.Combine(AssestsPath, "settings.json");
        public static string SkinPath { get; } = Path.Combine(AssestsPath, "Skins");
        public static string CachePath { get; } = Path.Combine(AssestsPath, "Cache");
        public static string LogPath { get; } = Path.Combine(AssestsPath, $"MajPlayRuntime.log");
        public static string LangPath { get; } = Path.Combine(Application.streamingAssetsPath, "Langs");
        public static string ScoreDBPath { get; } = Path.Combine(AssestsPath, "MajDatabase.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db");

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

        CancellationTokenSource tokenSource = new();
        Task? logWritebackTask = null;
        Queue<GameLog> logQueue = new();
        readonly JsonSerializerOptions jsonReaderOption = new()
        {

            Converters =
            {
                new JsonStringEnumConverter()
            },
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };

        void Awake()
        {
            Application.logMessageReceived += (c, trace, type) =>
            {
                logQueue.Enqueue(new GameLog()
                {
                    Date = DateTime.Now,
                    Condition = c,
                    StackTrace = trace,
                    Type = type
                });
            };
            logWritebackTask = LogWriteback();
            MajInstances.GameManager = this;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            DontDestroyOnLoad(this);

            if (File.Exists(SettingPath))
            {
                var js = File.ReadAllText(SettingPath);
                GameSetting? setting;

                if (!Serializer.Json.TryDeserialize(js, out setting, jsonReaderOption) || setting is null)
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
        }
        async void Start()
        {
            await SongStorage.ScanMusicAsync();

            SelectedDiff = Setting.Misc.SelectedDiff;
            SongStorage.OrderBy = Setting.Misc.OrderBy;
            if (!SongStorage.IsEmpty)
            {
                SongStorage.SortAndFind();
                SongStorage.CollectionIndex = Setting.Misc.SelectedDir;
                SongStorage.WorkingCollection.Index = Setting.Misc.SelectedIndex;
            }
        }
        private void OnApplicationQuit()
        {
            Save();
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            tokenSource.Cancel();
            foreach (var log in logQueue)
                File.AppendAllText(LogPath, $"[{log.Date:yyyy-MM-dd HH:mm:ss}][{log.Type}] {log.Condition}\n{log.StackTrace}");
        }
        public void Save()
        {
            Setting.Misc.SelectedDiff = SelectedDiff;
            Setting.Misc.SelectedIndex = SongStorage.WorkingCollection.Index;
            Setting.Misc.SelectedDir = SongStorage.CollectionIndex;
            SongStorage.OrderBy.Keyword = string.Empty;
            Setting.Misc.OrderBy = SongStorage.OrderBy;

            var json = Serializer.Json.Serialize(Setting, jsonReaderOption);
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
            if (File.Exists(LogPath))
                File.Delete(LogPath);
            while (true)
            {
                if (logQueue.Count == 0)
                {
                    if (AllTaskToken.IsCancellationRequested)
                        return;
                    await Task.Delay(50);
                    continue;
                }
                var log = logQueue.Dequeue();
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