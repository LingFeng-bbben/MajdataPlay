using MajdataPlay.Types;
using MychIO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Utils
{
    public static class MajEnv
    {
        public const int DEFAULT_LAYER = 0;
        public const int HIDDEN_LAYER = 3;
        public static ConcurrentQueue<Action> ExecutionQueue { get; } = IOManager.ExecutionQueue;
        public static string AssestPath { get; } = Path.Combine(Application.dataPath, "../");
        public static string ChartPath { get; } = Path.Combine(AssestPath, "MaiCharts");
        public static string SettingPath { get; } = Path.Combine(AssestPath, "settings.json");
        public static string SkinPath { get; } = Path.Combine(AssestPath, "Skins");
        public static string CachePath { get; } = Path.Combine(AssestPath, "Cache");
        public static string LogsPath { get; } = Path.Combine(AssestPath, $"Logs");
        public static string LangPath { get; } = Path.Combine(Application.streamingAssetsPath, "Langs");
        public static string ScoreDBPath { get; } = Path.Combine(AssestPath, "MajDatabase.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db");
        public static string LogPath { get; } = Path.Combine(LogsPath, $"MajPlayRuntime_{DateTime.Now:yyyy-MM-dd_HH_mm_ss}.log");
        public static Thread MainThread { get; } = Thread.CurrentThread;
        public static GameSetting UserSetting => MajInstances.Setting;
        public static CancellationToken GlobalCT => GameManager.GlobalCT;
        public static JsonSerializerOptions UserJsonReaderOption { get; } = new()
        {
            Converters =
            {
                new JsonStringEnumConverter()
            },
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };
    }
}
