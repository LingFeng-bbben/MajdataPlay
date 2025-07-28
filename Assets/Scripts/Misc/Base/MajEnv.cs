using Cysharp.Threading.Tasks;
using HidSharp.Platform.Windows;
using LibVLCSharp;
using MajdataPlay.Extensions;
using MajdataPlay.Numerics;
using MychIO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MajdataPlay.Settings;
using MajdataPlay.Utils;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    public static class MajEnv
    {
        public const int DEFAULT_LAYER = 0;
        public const int HIDDEN_LAYER = 3;
        public const int HTTP_BUFFER_SIZE = 8192;
        public const int HTTP_REQUEST_MAX_RETRY = 4;
        public const int HTTP_TIMEOUT_MS = 8000;
        public const float FRAME_LENGTH_SEC = 1f / 60;
        public const float FRAME_LENGTH_MSEC = FRAME_LENGTH_SEC * 1000;

        public static event Action? OnApplicationQuit;
        public static LibVLC? VLCLibrary { get; private set; }
        public static ConcurrentQueue<Action> ExecutionQueue { get; } = IOManager.ExecutionQueue;
        internal static HardwareEncoder HWEncoder { get; } = HardwareEncoder.None;
        internal static RunningMode Mode { get; set; } = RunningMode.Play;
#if UNITY_EDITOR
        public static bool IsEditor { get; } = true;
#else
        public static bool IsEditor { get; } = false;
#endif
#if UNITY_STANDALONE_WIN
        public static string RootPath { get; } = Path.Combine(Application.dataPath, "../");
        public static string AssetsPath { get; } = Application.streamingAssetsPath;
        public static string CachePath { get; } = Path.Combine(RootPath, "Cache");
#else
        public static string RootPath { get; } = Application.persistentDataPath;
        public static string AssetsPath { get; } = Path.Combine(Application.persistentDataPath, "ExtStreamingAssets/");
        public static string CachePath { get; } = Application.temporaryCachePath;
#endif

        public static string ChartPath { get; } = Path.Combine(RootPath, "MaiCharts");
        public static string SettingPath { get; } = Path.Combine(RootPath, "settings.json");
        public static string SkinPath { get; } = Path.Combine(RootPath, "Skins");
        public static string LogsPath { get; } = Path.Combine(RootPath, $"Logs");
        public static string LangPath { get; } = Path.Combine(AssetsPath, "Langs");
        public static string ScoreDBPath { get; } = Path.Combine(RootPath, "MajDatabase.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db");
        public static string LogPath { get; } = Path.Combine(LogsPath, $"MajPlayRuntime.log");
        public static string RecordOutputsPath { get; } = Path.Combine(RootPath, "RecordOutputs");
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
        })
        {
            Timeout = TimeSpan.FromMilliseconds(HTTP_TIMEOUT_MS),
            DefaultRequestHeaders = 
            {
                UserAgent = { new ProductInfoHeaderValue("MajPlay", MajInstances.GameVersion.ToString()) },
            }
        };
        public static GameSetting UserSettings { get; }
        public static CancellationToken GlobalCT
        {
            get
            {
                return _globalCTS.Token;
            }
        }
        public static JsonSerializerOptions UserJsonReaderOption { get; } = new()
        {
            Converters =
            {
                new JsonStringEnumConverter()
            },
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };

        readonly static CancellationTokenSource _globalCTS = new();

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
            CheckNoteSkinFolder();

            var netCachePath = Path.Combine(CachePath, "Net");
            var runtimeCachePath = Path.Combine(CachePath, "Runtime");

            if (File.Exists(SettingPath))
            {
                var js = File.ReadAllText(SettingPath);
                GameSetting? setting;

                if (!Serializer.Json.TryDeserialize(js, out setting, UserJsonReaderOption) || setting is null)
                {
                    UserSettings = new();
                    MajDebug.LogError("Failed to read setting from file");
                    var bakFileName = $"{SettingPath}.bak";
                    while(File.Exists(bakFileName))
                    {
                        bakFileName = $"{bakFileName}.bak";
                    }
                    try
                    {
                        File.Copy(SettingPath, bakFileName, true);
                    }
                    catch { }
                }
                else
                {
                    UserSettings = setting;
                    //Reset Mod option after reboot
                    UserSettings.Mod = new ModOptions();
                }
            }
            else
            {
                UserSettings = new GameSetting();

                var json = Serializer.Json.Serialize(UserSettings, UserJsonReaderOption);
                File.WriteAllText(SettingPath, json);
            }

            UserSettings.IO.InputDevice.ButtonRing.PollingRateMs = Math.Max(0, UserSettings.IO.InputDevice.ButtonRing.PollingRateMs);
            UserSettings.IO.InputDevice.TouchPanel.PollingRateMs = Math.Max(0, UserSettings.IO.InputDevice.TouchPanel.PollingRateMs);
            UserSettings.IO.InputDevice.ButtonRing.DebounceThresholdMs = Math.Max(0, UserSettings.IO.InputDevice.ButtonRing.DebounceThresholdMs);
            UserSettings.IO.InputDevice.TouchPanel.DebounceThresholdMs = Math.Max(0, UserSettings.IO.InputDevice.TouchPanel.DebounceThresholdMs);
            UserSettings.Display.InnerJudgeDistance = UserSettings.Display.InnerJudgeDistance.Clamp(0, 1);
            UserSettings.Display.OuterJudgeDistance = UserSettings.Display.OuterJudgeDistance.Clamp(0, 1);

            CreateDirectoryIfNotExists(CachePath);
            CreateDirectoryIfNotExists(runtimeCachePath);
            CreateDirectoryIfNotExists(netCachePath);
            CreateDirectoryIfNotExists(ChartPath);
            CreateDirectoryIfNotExists(RecordOutputsPath);
            SharedHttpClient.Timeout = TimeSpan.FromMilliseconds(HTTP_TIMEOUT_MS);
            MainThread.Priority = UserSettings.Debug.MainThreadPriority;
        }
        internal static void Init()
        {

#if UNITY_STANDALONE_WIN
            MajDebug.Log("[VLC] init");
            if (VLCLibrary != null)
            {
                VLCLibrary.Dispose();
            }
            Core.Initialize(Path.Combine(Application.dataPath, "Plugins")); //Load VLC dlls
            VLCLibrary = new LibVLC(enableDebugLogs: true, "--no-audio"); // we dont need it to produce sound here
#else
            VLCLibrary = null;
#endif
        }
        internal static void OnApplicationQuitRequested()
        {
            SharedHttpClient.CancelPendingRequests();
            SharedHttpClient.Dispose();
            if( VLCLibrary != null ) 
                VLCLibrary.Dispose();
            _globalCTS.Cancel();
            if (OnApplicationQuit is not null)
            {
                OnApplicationQuit();
            }
            WinHidManager.QuitThisBs();
        }
        static void CheckNoteSkinFolder()
        {
            if (!Directory.Exists(SkinPath))
                Directory.CreateDirectory(SkinPath);
        }
        static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
