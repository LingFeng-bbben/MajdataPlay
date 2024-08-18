using MajdataPlay.Types;
using MajdataPlay.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class ResultScreenManager : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI accDX;
    public TextMeshProUGUI accClassic;

    public TextMeshProUGUI perfectCount;
    public TextMeshProUGUI greatCount;
    public TextMeshProUGUI goodCount;
    public TextMeshProUGUI missCount;

    public TextMeshProUGUI fastCount;
    public TextMeshProUGUI lateCount;

    public TextMeshProUGUI subMonitor;


    public GameObject clearLogo;

    public Image coverImg;


    void Start()
    {
        var gameManager = GameManager.Instance;
        var result = (GameResult)GameManager.LastGameResult;
        GameManager.LastGameResult = null;

        var totalJudgeRecord = UnpackJudgeRecord(result.JudgeRecord.TotalJudgeInfo);
        var song = result.SongInfo;
        
        title.text = song.Title;
        accDX.text = $"{result.Acc.DX:F4}%";
        accClassic.text = $"{result.Acc.Classic:F2}%";

        perfectCount.text = $"{totalJudgeRecord.CriticalPerfect + totalJudgeRecord.Perfect}";
        greatCount.text = $"{totalJudgeRecord.Great}";
        goodCount.text = $"{totalJudgeRecord.Good}";
        missCount.text = $"{totalJudgeRecord.Miss}";

        fastCount.text = $"{result.Fast}";
        lateCount.text = $"{result.Late}";

        subMonitor.text = BuildSubDisplayText(result.JudgeRecord);

        coverImg.sprite = song.SongCover;

        if(!totalJudgeRecord.IsNoMiss)
            clearLogo.SetActive(false);
        else if(totalJudgeRecord.IsNoGood)
            clearLogo.GetComponentInChildren<TextMeshProUGUI>().text = "FC+";
        else if(totalJudgeRecord.IsAllPerfect)
            clearLogo.GetComponentInChildren<TextMeshProUGUI>().text = "AP";

        IOManager.Instance.BindAnyArea(OnAreaDown);
        AudioManager.Instance.PlaySFX("Sugoi.wav");
    }
    string BuildSubDisplayText(JudgeDetail judgeRecord)
    {
        var tapJudge = UnpackJudgeRecord(judgeRecord[ScoreNoteType.Tap]);
        var holdJudge = UnpackJudgeRecord(judgeRecord[ScoreNoteType.Hold]);
        var slideJudge = UnpackJudgeRecord(judgeRecord[ScoreNoteType.Slide]);
        var touchJudge = UnpackJudgeRecord(judgeRecord[ScoreNoteType.Touch]);
        var breakJudge = UnpackJudgeRecord(judgeRecord[ScoreNoteType.Break]);
        string[] nmsl = new string[] 
        {
            "NOTES\t\tCP    \t\tP    \t\tGr    \t\tGd   \t\tM",
            $"Tap  \t\t{tapJudge.CriticalPerfect}\t\t{tapJudge.Perfect}\t\t{tapJudge.Great}\t\t{tapJudge.Good}\t\t{tapJudge.Miss}",
            $"Hold\t\t{holdJudge.CriticalPerfect}\t\t{holdJudge.Perfect}\t\t{holdJudge.Great}\t\t{holdJudge.Good}\t\t{holdJudge.Miss}",
            $"Slide\t\t{slideJudge.CriticalPerfect}\t\t{slideJudge.Perfect}\t\t{slideJudge.Great}\t\t{slideJudge.Good}\t\t{slideJudge.Miss}",
            $"Touch\t\t{touchJudge.CriticalPerfect}\t\t{touchJudge.Perfect}\t\t{touchJudge.Great}\t\t{touchJudge.Good}\t\t{touchJudge.Miss}",
            $"Break\t\t{breakJudge.CriticalPerfect}\t\t{breakJudge.Perfect}\t\t{breakJudge.Great}\t\t{breakJudge.Good}\t\t{breakJudge.Miss}"
        };
        return string.Join("\n", nmsl);
    }
    UnpackJudgeInfo UnpackJudgeRecord(JudgeInfo judgeInfo)
    {
        long cPerfect = 0;
        long perfect = 0;
        long great = 0;
        long good = 0;
        long miss = 0;

        long fast = 0;
        long late = 0;

        foreach(var kv in judgeInfo)
        {
            if (kv.Key > JudgeType.Perfect)
                fast += kv.Value;
            else if(kv.Key is not (JudgeType.Miss or JudgeType.Perfect))
                late += kv.Value;
            switch(kv.Key)
            {
                case JudgeType.Miss:
                    miss += kv.Value;
                    break;
                case JudgeType.FastGood:
                case JudgeType.LateGood:
                    good += kv.Value;
                    break;
                case JudgeType.LateGreat2:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat:
                case JudgeType.FastGreat:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat2:
                    great += kv.Value;
                    break;
                case JudgeType.LatePerfect2:
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                case JudgeType.FastPerfect2:
                    perfect += kv.Value;
                    break;
                case JudgeType.Perfect:
                    cPerfect += kv.Value;
                    break;
            }
        }
        return new UnpackJudgeInfo()
        {
            CriticalPerfect = cPerfect,
            Perfect = perfect,
            Great = great,
            Good = good,
            Miss = miss,
            Fast = fast,
            Late = late,
        };

    }
    private void OnAreaDown(object sender, InputEventArgs e)
    {
        if (e.IsClick && e.IsButton && e.Type == MajdataPlay.Types.SensorType.A4)
        {
            SceneManager.LoadScene(1);
        }
    }

    private void OnDestroy()
    {
        IOManager.Instance.UnbindAnyArea(OnAreaDown);
    }
    readonly ref struct UnpackJudgeInfo
    {
        public long CriticalPerfect { get; init; }
        public long Perfect { get; init; }
        public long Great { get; init; }
        public long Good { get; init; }
        public long Miss { get; init; }
        public long Fast { get; init; }
        public long Late { get; init; }
        public bool IsNoMiss => Miss == 0;
        public bool IsNoGood => Good == 0;
        public bool IsAllPerfect => IsNoMiss && IsNoGood && Great == 0;
    }
}
