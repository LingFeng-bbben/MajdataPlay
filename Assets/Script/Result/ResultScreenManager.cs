using MajdataPlay.Types;
using MajdataPlay.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using MajdataPlay.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace MajdataPlay.Result
{
    public partial class ResultScreenManager : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI artist;
        public TextMeshProUGUI designer;
        public TextMeshProUGUI level;

        public TextMeshProUGUI accDX;
        public TextMeshProUGUI accClassic;
        public TextMeshProUGUI dxScore;

        public TextMeshProUGUI perfectCount;
        public TextMeshProUGUI greatCount;
        public TextMeshProUGUI goodCount;
        public TextMeshProUGUI missCount;

        public TextMeshProUGUI fastCount;
        public TextMeshProUGUI lateCount;

        public TextMeshProUGUI omg;

        public TextMeshProUGUI subMonitor;


        public GameObject clearLogo;
        public GameObject xxlb;

        public Image coverImg;


        void Start()
        {
            var gameManager = MajInstances.GameManager;
            var result = (GameResult)GameManager.LastGameResult;
            GameManager.LastGameResult = null;

            MajInstances.LightManager.SetAllLight(Color.white);
            MajInstances.LightManager.SetButtonLight(Color.green, 3);

            var totalJudgeRecord = JudgeDetail.UnpackJudgeRecord(result.JudgeRecord.TotalJudgeInfo);
            var song = result.SongInfo;

            GetComponent<OnlineInteractionSender>().Init(song);

            if (result.Acc.DX < 70)
            {
                omg.text = "您输了";
                xxlb.GetComponent<Animator>().SetTrigger("Bad");
            }
            else
            {
                omg.text = "您赢了";
                xxlb.GetComponent<Animator>().SetTrigger("Good");
            }

            title.text = song.Title;
            artist.text = song.Artist;
            designer.text = song.Designers[(int)gameManager.SelectedDiff];
            level.text = gameManager.SelectedDiff.ToString() + " " + song.Levels[(int)gameManager.SelectedDiff];

            accDX.text = $"{result.Acc.DX:F4}%";
            accClassic.text = $"{result.Acc.Classic:F2}%";
            var dxScoreRank = new DXScoreRank(result.DXScore, result.TotalDXScore);
            if (dxScoreRank.Rank > 0)
                dxScore.text = $"*{dxScoreRank.Rank} {result.DXScore}/{result.TotalDXScore}";
            else
                dxScore.text = $"{result.DXScore}/{result.TotalDXScore}";

            perfectCount.text = $"{totalJudgeRecord.CriticalPerfect + totalJudgeRecord.Perfect}";
            greatCount.text = $"{totalJudgeRecord.Great}";
            goodCount.text = $"{totalJudgeRecord.Good}";
            missCount.text = $"{totalJudgeRecord.Miss}";

            fastCount.text = $"{result.Fast}";
            lateCount.text = $"{result.Late}";

            subMonitor.text = BuildSubDisplayText(result.JudgeRecord);

            LoadCover(song).Forget();

            var breakJudgeInfo = JudgeDetail.UnpackJudgeRecord(result.JudgeRecord[ScoreNoteType.Break]);

            if (!totalJudgeRecord.IsFullCombo)
                clearLogo.SetActive(false);
            else if (totalJudgeRecord.ISAllPerfectPlus)
            {
                clearLogo.GetComponentInChildren<TextMeshProUGUI>().text = "AP+";
            }
            else if (totalJudgeRecord.IsAllPerfect)
            {
                clearLogo.GetComponentInChildren<TextMeshProUGUI>().text = "AP";
            }
            else if (totalJudgeRecord.IsFullComboPlus)
                clearLogo.GetComponentInChildren<TextMeshProUGUI>().text = "FC+";

            MajInstances.AudioManager.PlaySFX("bgm_result.mp3", true);
            PlayVoice(result.Acc.DX,song).Forget();
            MajInstances.ScoreManager.SaveScore(result, result.ChartLevel);
        }

        async UniTask LoadCover(SongDetail song)
        {
            var task = song.GetSpriteAsync();
            while (!task.IsCompleted)
            {
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            }
            coverImg.sprite = task.Result;
        }

        async UniTask PlayVoice(double dxacc, SongDetail song)
        {
            if (dxacc >= 97)
            {
                MajInstances.AudioManager.PlaySFX("Rank.wav");
                await UniTask.WaitForSeconds(1);
            }
            if (dxacc >= 100.5f)
            {
                MajInstances.AudioManager.PlaySFX("SSS+.wav");
            }else if (dxacc >= 100f)
            {
                MajInstances.AudioManager.PlaySFX("SSS.wav");
            }
            else if (dxacc >= 99.5f)
            {
                MajInstances.AudioManager.PlaySFX("SS+.wav");
            }
            else if (dxacc >= 99f)
            {
                MajInstances.AudioManager.PlaySFX("SS.wav");
            }
            else if (dxacc >= 98f)
            {
                MajInstances.AudioManager.PlaySFX("S+.wav");
            }
            else if (dxacc >= 97f)
            {
                MajInstances.AudioManager.PlaySFX("S.wav");
            }
            if (dxacc > 97)
            {
                await UniTask.WaitForSeconds(2);
                var list = new string[] { "good.wav", "good_2.wav", "good_3.wav", "good_4.wav", "good_5.wav", "good_6.wav" };
                MajInstances.AudioManager.PlaySFX(list[UnityEngine.Random.Range(0, list.Length)]);
                await UniTask.WaitForSeconds(3);
                if (song.ApiEndpoint != null)
                {
                    MajInstances.AudioManager.PlaySFX("dian_zan.wav");
                }
            }
            else
            {
                var list = new string[] { "wuyu.wav", "wuyu_2.wav", "wuyu_3.wav"};
                MajInstances.AudioManager.PlaySFX(list[UnityEngine.Random.Range(0,list.Length)]);
                await UniTask.WaitForSeconds(2);
            }
            MajInstances.InputManager.BindAnyArea(OnAreaDown);
        }


        string BuildSubDisplayText(JudgeDetail judgeRecord)
        {
            var tapJudge = JudgeDetail.UnpackJudgeRecord(judgeRecord[ScoreNoteType.Tap]);
            var holdJudge = JudgeDetail.UnpackJudgeRecord(judgeRecord[ScoreNoteType.Hold]);
            var slideJudge = JudgeDetail.UnpackJudgeRecord(judgeRecord[ScoreNoteType.Slide]);
            var touchJudge = JudgeDetail.UnpackJudgeRecord(judgeRecord[ScoreNoteType.Touch]);
            var breakJudge = JudgeDetail.UnpackJudgeRecord(judgeRecord[ScoreNoteType.Break]);
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
        
        private void OnAreaDown(object sender, InputEventArgs e)
        {
            if (e.IsClick && e.IsButton && e.Type == SensorType.A4)
            {
                MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
                MajInstances.AudioManager.StopSFX("bgm_result.mp3");
                MajInstances.SceneSwitcher.SwitchScene("List");
            }
        }
    }
}