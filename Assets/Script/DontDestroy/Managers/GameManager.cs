using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using MajdataPlay.Types;
#nullable enable
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static string AssestsPath => Path.Combine(Application.dataPath,"../");
    public static string ChartPath => Path.Combine(AssestsPath, "MaiCharts");
    public static string SettingPath => Path.Combine(AssestsPath, "settings.json");
    public static string SkinPath => Path.Combine(AssestsPath, "Skins");
    public static string ScoreDBPath => Path.Combine(AssestsPath, "MajDatabase.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db.db");


    public static GameResult? LastGameResult { get; set; } = null;
    public List<SongDetail> songList = new List<SongDetail> ();
    public int selectedIndex = 0;
    /// <summary>
    /// 玩家选择的谱面难度
    /// </summary>
    public ChartLevel SelectedDiff { get; set; } = 0;
    //public float lastGameResult = -1f; //this should be a struct in future
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }
    void Start()
    {
        selectedIndex = SettingManager.Instance.SettingFile.lastSelectedSongIndex;
        SelectedDiff = SettingManager.Instance.SettingFile.lastSelectedSongDifficulty;
        songList = SongLoader.ScanMusic();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
