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
#nullable enable
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static string AssestsPath => Path.Combine(Application.dataPath,"../");
    public static string ChartPath => Path.Combine(AssestsPath, "MaiCharts");
    public static string SettingPath => Path.Combine(AssestsPath, "settings.json");
    public static string SkinPath => Path.Combine(AssestsPath, "Skins");
    public static string ScoreDBPath => Path.Combine(AssestsPath, "MajDatabase.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db");

    public GameSetting Setting { get; private set; } = new();
    public int SelectedIndex { get; set; } = 0;
    public List<SongDetail> SongList { get; set; } = new List<SongDetail>();
    public static GameResult? LastGameResult { get; set; } = null;
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
    private void Awake()
    {
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
        Setting.Display.InnerJudgeDistance = Setting.Display.InnerJudgeDistance.Clamp(1, 0);
        Setting.Display.OuterJudgeDistance = Setting.Display.OuterJudgeDistance.Clamp(1, 0);

        var fullScreen = Setting.Debug.FullScreen;
        if (!fullScreen)
            Screen.fullScreen = false;
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
    void Start()
    {
        SelectedIndex = Setting.SelectedIndex;
        SelectedDiff = Setting.SelectedDiff;
        SongList = SongLoader.ScanMusic();
    }
    private void OnApplicationQuit()
    {
        Save();
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }
    public void Save()
    {
        Setting.SelectedDiff = SelectedDiff;
        Setting.SelectedIndex = SelectedIndex;

        var json = Serializer.Json.Serialize(Setting,jsonReaderOption);
        File.WriteAllText(SettingPath, json);
    }
}
