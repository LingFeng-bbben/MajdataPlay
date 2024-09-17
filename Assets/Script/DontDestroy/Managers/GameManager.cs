using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using MajdataPlay.Extensions;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;
using System.Threading;
#nullable enable
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public CancellationToken AllTaskToken { get => tokenSource.Token; }
    
    public static string AssestsPath => Path.Combine(Application.dataPath,"../");
    public static string ChartPath => Path.Combine(AssestsPath, "MaiCharts");
    public static string SettingPath => Path.Combine(AssestsPath, "settings.json");
    public static string SkinPath => Path.Combine(AssestsPath, "Skins");
    public static string LogPath => Path.Combine(AssestsPath, $"MajPlayRuntime.log");
    public static string ScoreDBPath => Path.Combine(AssestsPath, "MajDatabase.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db");

    public GameSetting Setting { get; private set; } = new();
    /// <summary>
    /// 在List中选中的文件夹
    /// </summary>
    public SongCollection Collection { get; private set; } = new();
    public int SelectedDir 
    {
        get => _selectedDir; 
        set
        {
            Collection = SongStorage.Songs[value];
            _selectedDir = value;
        }
    }
    public static GameResult? LastGameResult { get; set; } = null;
    public bool UseUnityTimer { get => _useUnityTimer; set => _useUnityTimer = value; }

    CancellationTokenSource tokenSource = new();
    Task? logWritebackTask = null;
    Queue<GameLog> logQueue = new();
    /// <summary>
    /// 玩家选择的谱面难度
    /// </summary>
    public ChartLevel SelectedDiff { get; set; } = ChartLevel.Easy;

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
        logWritebackTask =  LogWriteback();
        Instance = this;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        DontDestroyOnLoad(this);

        if (File.Exists(SettingPath))
        {
            var js = File.ReadAllText(SettingPath);
            GameSetting? setting;

            if (!Serializer.Json.TryDeserialize(js, out setting,jsonReaderOption) || setting is null)
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
    }
    async void Start()
    {
        await SongStorage.ScanMusicAsync();

        SelectedDir = Setting.SelectedDir;
        Collection.Index = Setting.SelectedIndex;
        SelectedDiff = Setting.SelectedDiff;
    }
    private void OnApplicationQuit()
    {
        Save();
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        tokenSource.Cancel();
        foreach(var log in logQueue)
            File.AppendAllText(LogPath, $"[{log.Date:yyyy-MM-dd HH:mm:ss}][{log.Type}] {log.Condition}\n{log.StackTrace}");
    }
    public void Save()
    {
        Setting.SelectedDiff = SelectedDiff;
        Setting.SelectedIndex = Collection.Index;
        Setting.SelectedDir = SelectedDir;

        var json = Serializer.Json.Serialize(Setting,jsonReaderOption);
        File.WriteAllText(SettingPath, json);
    }
    async Task LogWriteback()
    {
        if (File.Exists(LogPath))
            File.Delete(LogPath);
        while(true)
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
    int _selectedDir = 0;
    int _selectedIndex = 0;
}
