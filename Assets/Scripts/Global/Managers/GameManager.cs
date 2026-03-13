using AOT;
using Cysharp.Threading.Tasks;
using MajdataPlay.Collections;
using MajdataPlay.i18n;
using MajdataPlay.IO;
using MajdataPlay.Scenes.Test;
using MajdataPlay.Settings;
using MajdataPlay.Timer;
#if UNITY_STANDALONE_WIN
using MajdataPlay.Platform.Win32;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting; // DO NOT REMOVE IT !!!

namespace MajdataPlay
{
#nullable enable
    internal sealed class GameManager : MajSingleton
    {
        public static bool IsAppOnFocus { get; private set; } = true;
        public static event EventHandler<EventArgs?>? OnAppQuit;
        public static event EventHandler<EventArgs?>? OnSave;
        public static event EventHandler<bool>? OnAppFocus;
        public static event EventHandler<bool>? OnAppPause;
#if UNITY_ANDROID
        public static event EventHandler<AndroidJavaObject?>? OnNewIntent;
        public static AndroidJavaClass UnityPlayerClass { get; private set; }
        public static AndroidJavaClass MajdataPlayActivityClass { get; private set; }

        public static AndroidJavaObject CurrentActivity { get; private set; }
#endif
        public static Camera MainCamera
        {
            get => MajInstances.SceneSwitcher.MainCamera;
        }

        public GameSetting Settings
        {
            get => MajInstances.Settings;
        }

        [SerializeField] BuiltInTimeProvider _timer = BuiltInTimeProvider.Winapi;
        [SerializeField] Sprite _emptySongCover;
        [SerializeField] Material _holdShineMaterial;
        [SerializeField] Material _breakMaterial;
        [SerializeField] Material _defaultMaterial;

        [SerializeField] bool _isEnterView = false;
        [SerializeField] bool _isEnterTest = false;

        readonly static List<IntPtr> _windowHandles = new();
        readonly static ReadOnlyMemory<ITimeProvider> _builtInTimeProviders = MajTimeline.BuiltInTimeProviders;

#if UNITY_ANDROID
        OnNewIntentCallback _onNewIntentCallbackProxy;
#endif
        protected override void Awake()
        {
            base.Awake();
#if UNITY_ANDROID && !UNITY_EDITOR
            UnityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            MajdataPlayActivityClass = new AndroidJavaClass("net.majdata.majdataplay.MajdataPlayActivity");
            UnityEngine.Debug.Log("[Android]Get current activity");
            CurrentActivity = UnityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            UnityEngine.Debug.Log("[Android]Creating onNewIntent callback proxy");
            _onNewIntentCallbackProxy = new(this);
            UnityEngine.Debug.Log("[Android]Setting onNewIntent callback proxy");
            MajdataPlayActivityClass.CallStatic("registerOnNewIntentCallback", _onNewIntentCallbackProxy);
#endif
        }
        void Start()
        {
#if UNITY_IOS && !UNITY_EDITOR
            IOSNativeSettings.Init();
#endif
            MajEnv.InitPath();
            MajDebug.Init();
            var s = "\n";
            s += $"################ MajdataPlay Startup Check ################\n";
            s += $"     OS       : {SystemInfo.operatingSystem}\n";
            s += $"     Model    : {SystemInfo.deviceModel} - {SystemInfo.deviceType}\n";
            s += $"     Processor: {SystemInfo.processorType}\n";
            s += $"     Memory   : {SystemInfo.systemMemorySize} MB\n";
            s +=
                $"     Graphices: {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsMemorySize} MB) - {SystemInfo.graphicsDeviceType}\n";
            s += $"################     Startup Check  End    ################";
            MajDebug.LogInfo(s);
            MajDebug.LogInfo($"PID: {MajEnv.GameProcess.Id}");
            MajDebug.LogInfo($"Version: {MajInstances.GameVersion}");
#if UNITY_ANDROID && !UNITY_EDITOR // Android Only (Sdk Version Log)
            MajDebug.LogInfo($"AndroidSdkVersion: {MajEnv.AndroidSdkVersion}");
            using var packageManager = CurrentActivity.Call<AndroidJavaObject>("getPackageManager");
            var packageName = CurrentActivity.Call<string>("getPackageName");
            using var packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
            var androidVersionCode = 0L;
            if(MajEnv.AndroidSdkVersion >= 28)
            {
                androidVersionCode = packageInfo.Call<long>("getLongVersionCode");
            }
            else
            {
                androidVersionCode = packageInfo.Get<int>("versionCode");
            }
            MajDebug.LogInfo($"AndroidVerCode: {androidVersionCode}");
#endif
            MajEnv.Init();
            if (!Directory.Exists(MajEnv.AssetsPath))
            {
#if UNITY_ANDROID
                ExtractAssetsAndroid();
                MoveCharts();
                MoveSkins();
#elif UNITY_IOS
                ExtractAssetsIos();
                MoveCharts();
                MoveSkins();
#endif
            }

            MajInstances.FPSDisplayer.Init();
            MajInstances.AudioManager.Init();
            Localization.Init();
#if UNITY_STANDALONE_WIN
            _timer = BuiltInTimeProvider.Winapi;
#else
            _timer = BuiltInTimeProvider.Stopwatch;
#endif
            MajTimeline.TimeProvider = _builtInTimeProviders.Span[(int)_timer];
#if UNITY_STANDALONE
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif

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
                    Settings.Mod.AutoPlay = AutoplayModeOption.Enable;
                    break;
                }
            }

#if UNITY_EDITOR
            if (_isEnterTest)
            {
                MajEnv.Mode = RunningMode.Test;
            }
            else if (_isEnterView)
            {
                MajEnv.Mode = RunningMode.View;
                Settings.Mod.AutoPlay = AutoplayModeOption.Enable;
            }
#endif

            ApplyScreenConfig();

            var availableLangs = Localization.Available;
            if (!availableLangs.IsEmpty())
            {
                if(string.IsNullOrEmpty(Settings.Display.Language))
                {
                    if(Localization.SetLangByCode(Application.systemLanguage.ToLocale()))
                    {
                        Settings.Display.Language = Localization.Current.ToString();
                    }
                }
                else
                {
                    var lang = availableLangs.Find(x => x.ToString() == Settings.Display.Language);
                    if (lang is null)
                    {
                        lang = availableLangs.First();
                        Settings.Display.Language = lang.ToString();
                    }

                    Localization.Current = lang;
                }  
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
            QualitySettings.SetQualityLevel((int)Settings.Display.RenderQuality, true);
#if !(UNITY_ANDROID || UNITY_IOS)
            QualitySettings.vSyncCount = Settings.Display.VSync ? 1 : 0;
#endif
            QualitySettings.maxQueuedFrames = Settings.Debug.MaxQueuedFrames;
            DetectHWEncoder();
#if (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
            if (Settings.Display.Topmost)
            {
                SetWindowTopmost();
            }
#endif

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
            if (deviceName.Contains("nvidia"))
            {
                encoder = HardwareEncoder.NVENC;
            }
            else if (deviceName.Contains("amd"))
            {
                encoder = HardwareEncoder.AMF;
            }
            else if (deviceName.Contains("intel"))
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
#if (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
        void SetWindowTopmost()
        {

            Win32API.EnumWindows(EnumWindowsCallback, Process.GetCurrentProcess().Id);
            MajDebug.LogDebug($"Found window count: {_windowHandles.Count}");
            foreach (var handle in _windowHandles)
            {
                Win32API.SetWindowPos(handle, Win32API.HWND_TOPMOST, 0, 0, 0, 0, Win32API.SWP_NOMOVE | Win32API.SWP_NOSIZE);
            }

        }
#endif
#if UNITY_STANDALONE_WIN
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
#endif
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
                var fullScreen = Settings.Debug.FullScreen;
                Screen.fullScreen = fullScreen;
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;

                var resolution = Settings.Display.Resolution.ToLower();
                if (resolution is not "auto")
                {
                    var param = resolution.Split("x");
                    int width, height;

                    if (param.Length != 2)
                    {
                        return;
                    }
                    else if (!int.TryParse(param[0], out width) || !int.TryParse(param[1], out height))
                    {
                        return;
                    }
                    Screen.SetResolution(width, height, fullScreen);
                }
            }
#endif
            Application.targetFrameRate = Settings.Display.FPSLimit;
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
            RequestSave(this);
            try
            {
                if (OnAppQuit is not null)
                {
                    OnAppQuit(this, null);
                }
            }
            catch (Exception ex)
            {
                MajDebug.LogException(ex);
            }
            OnAppQuit = null;
            OnSave = null;
        }
        void OnApplicationFocus(bool focus)
        {
            IsAppOnFocus = focus;
            if (OnAppFocus is not null)
            {
                OnAppFocus(this, focus);
            }
#if UNITY_ANDROID || UNITY_IOS
            if (!focus)
            {
                RequestSave(this);
            }
#endif
        }

        void OnApplicationPause(bool pause)
        {
            if (OnAppPause is not null)
            {
                OnAppPause(this, pause);
            }
#if UNITY_ANDROID || UNITY_IOS  
            if (pause)
            {
                RequestSave(this);
            }
#endif
        }
#if UNITY_ANDROID
        [Preserve]
        void Android_OnNewIntent(AndroidJavaObject intent)
        {
            //var intentObject = CurrentActivity.Call<AndroidJavaObject>("getIntent");
            if (OnNewIntent is not null)
            {
                OnNewIntent(this, intent);
            }
        }
#endif
        public void EnableGC()
        {
            GC.Collect();
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR //Android/iOS Only (GC Enable)
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            MajDebug.LogWarning("GC has been enabled");
#endif
        }

        public void DisableGC()
        {
            GC.Collect();
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR //Android/iOS Only (GC Disable)
            GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
            MajDebug.LogWarning("GC has been disabled");
#endif
        }
        public static void RequestSave(object? sender)
        {
            try
            {
                if (OnSave is not null)
                {
                    OnSave(sender, null);
                }
            }
            catch (Exception ex)
            {
                MajDebug.LogException(ex);
            }
        }
        private static void ExtractAssetsIos()
        {
            var extractRoot = Path.Combine(MajEnv.RootPath, "ExtStreamingAssets/");
            Directory.CreateDirectory(extractRoot);
            var paths = Resources.Load<TextAsset>("StreamingAssetPaths");
            var fs = paths.text;
            MajDebug.LogInfo(fs);
            string[] fLines = fs.Replace("\\", "/").Split("\n");
            foreach (var rawLine in fLines)
            {
                var line = rawLine.Trim();
                var srcPath = Path.Combine(Application.streamingAssetsPath, line);
                var dstPath = Path.Combine(extractRoot, line);
                var dstDir = Path.GetDirectoryName(dstPath);
                if (!string.IsNullOrEmpty(dstDir)) Directory.CreateDirectory(dstDir);
                MajDebug.LogInfo($"Extracting(iOS direct): {srcPath} -> {dstPath}");

                try
                {
                    var data = File.ReadAllBytes(srcPath);
                    if (data.Length == 0)
                    {
                        MajDebug.LogError($"Extract failed(iOS): empty data: {line}\nsrc={srcPath}");
                        continue;
                    }

                    File.WriteAllBytes(dstPath, data);
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"Extract failed(iOS): {line}\nsrc={srcPath}\n{e}");
                }
            }
        }

        private static void ExtractAssetsAndroid()
        {
            var extractRoot = Path.Combine(MajEnv.RootPath, "ExtStreamingAssets");
            Directory.CreateDirectory(extractRoot);

            var paths = Resources.Load<TextAsset>("StreamingAssetPaths");
            if (paths == null)
            {
                MajDebug.LogError("StreamingAssetPaths not found in Resources.");
                return;
            }

            var fs = paths.text;
            MajDebug.LogInfo(fs);

            string[] fLines = fs.Replace("\\", "/").Split('\n');

            foreach (var rawLine in fLines)
            {
                var line = rawLine.Trim();
                if (line.Length == 0)
                    continue;

                var srcUrl = Path.Combine(Application.streamingAssetsPath, line).Replace("\\", "/");

                var dstPath = Path.Combine(extractRoot, line);
                var dstDir = Path.GetDirectoryName(dstPath);
                if (!string.IsNullOrEmpty(dstDir))
                    Directory.CreateDirectory(dstDir);

                MajDebug.LogInfo($"Extracting(Android jar sync): {srcUrl} -> {dstPath}");

                try
                {
                    using var req = UnityWebRequest.Get(srcUrl);
                    req.downloadHandler = new DownloadHandlerBuffer();
                    var op = req.SendWebRequest();
                    while (!op.isDone)
                    {
                        System.Threading.Thread.Sleep(1);
                    }

                    if (req.result != UnityWebRequest.Result.Success)
                    {
                        MajDebug.LogError($"Extract failed(Android): {line}\nsrc={srcUrl}\nerr={req.error}");
                        continue;
                    }

                    var data = req.downloadHandler.data;
                    if (data == null || data.Length == 0)
                    {
                        MajDebug.LogError($"Extract failed(Android): empty data: {line}\nsrc={srcUrl}");
                        continue;
                    }

                    File.WriteAllBytes(dstPath, data);
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"Extract failed(Android): {line}\nsrc={srcUrl}\n{e}");
                }
            }
        }

        private static void MoveCharts()
        {
            var src = Path.Combine(MajEnv.RootPath, "ExtStreamingAssets", "MaiCharts", "Original");
            var dst = Path.Combine(MajEnv.RootPath, "MaiCharts", "Original");

            if (!Directory.Exists(src))
            {
                MajDebug.LogError($"Move failed: source not found: {src}");
                return;
            }

            if (Directory.Exists(dst))
                Directory.Delete(dst, recursive: true);

            Directory.Move(sourceDirName: src, destDirName: dst);
            MajDebug.LogInfo($"Moved: {src} -> {dst}");
        }

        private static void MoveSkins()
        {
            var src = Path.Combine(MajEnv.RootPath, "ExtStreamingAssets", "Skins", "Light2");
            var dst = Path.Combine(MajEnv.RootPath, "Skins", "Light2");

            if (!Directory.Exists(src))
            {
                MajDebug.LogError($"Move failed: source not found: {src}");
                return;
            }

            if (Directory.Exists(dst))
                Directory.Delete(dst, recursive: true);

            Directory.Move(sourceDirName: src, destDirName: dst);
            MajDebug.LogInfo($"Moved: {src} -> {dst}");
        }

#if UNITY_ANDROID
        class OnNewIntentCallback : AndroidJavaProxy
        {
            readonly GameManager _gameManager;
            public OnNewIntentCallback(GameManager gm) : base("net.majdata.majdataplay.CSharpOnNewIntentCallback")
            {
                _gameManager = gm;
            }

            public void OnNewIntent(AndroidJavaObject intent)
            {
                _gameManager.Android_OnNewIntent(intent);
            }
        }
#endif
    }
}
