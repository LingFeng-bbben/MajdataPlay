using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Types;
using MajdataPlay.Types;
using MychIO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Utils
{
    public static class MajEnv
    {
        public const int DEFAULT_LAYER = 0;
        public const int HIDDEN_LAYER = 3;
        public const int HTTP_BUFFER_SIZE = 8192;
        public const int HTTP_REQUEST_MAX_RETRY = 4;
        public const int HTTP_TIMEOUT_MS = 4000;
        public static ConcurrentQueue<Action> ExecutionQueue { get; } = IOManager.ExecutionQueue;
        internal static RunningMode Mode { get; set; } = RunningMode.Play;
        public static string RootPath { get; } = Path.Combine(Application.dataPath, "../");
        public static string AssetsPath { get; } = Application.streamingAssetsPath;
        public static string ChartPath { get; } = Path.Combine(RootPath, "MaiCharts");
        public static string SettingPath { get; } = Path.Combine(RootPath, "settings.json");
        public static string SkinPath { get; } = Path.Combine(RootPath, "Skins");
        public static string CachePath { get; } = Path.Combine(RootPath, "Cache");
        public static string LogsPath { get; } = Path.Combine(RootPath, $"Logs");
        public static string LangPath { get; } = Path.Combine(Application.streamingAssetsPath, "Langs");
        public static string ScoreDBPath { get; } = Path.Combine(RootPath, "MajDatabase.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db");
        public static string LogPath { get; } = Path.Combine(LogsPath, $"MajPlayRuntime_{DateTime.Now:yyyy-MM-dd_HH_mm_ss}.log");
        public static Sprite EmptySongCover { get; }
        public static Material BreakMaterial { get; }
        public static Material DefaultMaterial { get; }
        public static Material HoldShineMaterial { get; }
        public static Thread MainThread { get; } = Thread.CurrentThread;
        public static Process GameProcess { get; } = Process.GetCurrentProcess();
        public static HttpClient SharedHttpClient { get; } = new HttpClient(new HttpClientHandler()
        {
            Proxy = WebRequest.GetSystemWebProxy(),
            UseProxy = true,
            UseCookies = true,
            CookieContainer = new CookieContainer(),
        });
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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ChangedSynchronizationContext()
        {
#if !UNITY_EDITOR
            SynchronizationContext.SetSynchronizationContext(new UniTaskSynchronizationContext());
#endif
        }
        static MajEnv()
        {
            ChangedSynchronizationContext();
            CheckAndLoadUserSetting();
            CheckNoteSkinFolder();

            var netCachePath = Path.Combine(CachePath, "Net");
            var runtimeCachePath = Path.Combine(CachePath, "Runtime");
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);
            if (!Directory.Exists(runtimeCachePath))
                Directory.CreateDirectory(runtimeCachePath);
            if (!Directory.Exists(netCachePath))
                Directory.CreateDirectory(netCachePath);
            if (!Directory.Exists(ChartPath))
                Directory.CreateDirectory(ChartPath);
            SharedHttpClient.Timeout = TimeSpan.FromMilliseconds(HTTP_TIMEOUT_MS);
            GameProcess.PriorityClass = ProcessPriorityClass.High;
        }
        static void CheckAndLoadUserSetting()
        {
            if (File.Exists(SettingPath))
            {
                var js = File.ReadAllText(SettingPath);
                GameSetting? setting;

                if (!Serializer.Json.TryDeserialize(js, out setting, UserJsonReaderOption) || setting is null)
                {
                    MajInstances.Setting = new();
                    MajDebug.LogError("Failed to read setting from file");
                }
                else
                {
                    MajInstances.Setting = setting;
                    //Reset Mod option after reboot
                    MajInstances.Setting.Mod = new ModOptions();
                }
            }
            else
            {
                MajInstances.Setting = new GameSetting();

                var json = Serializer.Json.Serialize(UserSetting, UserJsonReaderOption);
                File.WriteAllText(SettingPath, json);
            }

            UserSetting.Misc.InputDevice.ButtonRing.PollingRateMs = Math.Max(0, UserSetting.Misc.InputDevice.ButtonRing.PollingRateMs);
            UserSetting.Misc.InputDevice.TouchPanel.PollingRateMs = Math.Max(0, UserSetting.Misc.InputDevice.TouchPanel.PollingRateMs);
            UserSetting.Misc.InputDevice.ButtonRing.DebounceThresholdMs = Math.Max(0, UserSetting.Misc.InputDevice.ButtonRing.DebounceThresholdMs);
            UserSetting.Misc.InputDevice.TouchPanel.DebounceThresholdMs = Math.Max(0, UserSetting.Misc.InputDevice.TouchPanel.DebounceThresholdMs);
            UserSetting.Display.InnerJudgeDistance = UserSetting.Display.InnerJudgeDistance.Clamp(0, 1);
            UserSetting.Display.OuterJudgeDistance = UserSetting.Display.OuterJudgeDistance.Clamp(0, 1);
        }
        static void CheckNoteSkinFolder()
        {
            if (!Directory.Exists(SkinPath))
                Directory.CreateDirectory(SkinPath);
        }
    }
}
