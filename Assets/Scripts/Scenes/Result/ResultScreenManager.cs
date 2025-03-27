using MajdataPlay.Types;
using MajdataPlay.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using MajdataPlay.Utils;
using MajdataPlay.Collections;
using System.Linq;
using System;
using SkiaSharp;
using System.Collections.Generic;
using MajdataPlay.Extensions;
using Random = UnityEngine.Random;
using MajdataPlay.Game;
using MajdataPlay.List;
#nullable enable
namespace MajdataPlay.Result
{
    public partial class ResultScreenManager : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI artist;
        public TextMeshProUGUI designer;
        public TextMeshProUGUI level;

        public TextMeshProUGUI accDX;
        public TextMeshProUGUI accHistory;
        public TextMeshProUGUI dxScore;
        public TextMeshProUGUI rank;

        public TextMeshProUGUI perfectCount;
        public TextMeshProUGUI greatCount;
        public TextMeshProUGUI goodCount;
        public TextMeshProUGUI missCount;

        public TextMeshProUGUI fastCount;
        public TextMeshProUGUI lateCount;
        public TextMeshProUGUI avgJudgeTime;

        public TextMeshProUGUI omg;

        public TextMeshProUGUI subMonitor;

        public Color perfectColor;
        public Color greatColor;
        public Color goodColor;


        public GameObject clearLogo;
        public GameObject xxlb;

        public Image coverImg;

        public RawImage _noteJudgeDiffGraph;

        public FavoriteAdder favoriteAdder;

        GameInfo _gameInfo = Majdata<GameInfo>.Instance!;

        UniTask OnlineSaveTask = UniTask.Delay(0);

        void Start()
        {
            if(MajInstances.RecordHelper is not null
               && MajInstances.RecordHelper.Recording
               && MajInstances.RecordHelper.Connected)
                MajInstances.RecordHelper.StopRecord();

            rank.text = "";
            var gameManager = MajInstances.GameManager;
            var result = _gameInfo.GetLastResult();
            var isClassic = gameManager.Setting.Judge.Mode == JudgeMode.Classic;

            MajInstances.LightManager.SetAllLight(Color.white);

            var totalJudgeRecord = JudgeDetail.UnpackJudgeRecord(result.JudgeRecord.TotalJudgeInfo);
            var song = result.SongDetail;
            var historyResult = MajInstances.ScoreManager.GetScore(song, gameManager.SelectedDiff);

            var intractSender = GetComponent<OnlineInteractionSender>();
            intractSender.Init(song);
            favoriteAdder.SetSong(song);

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
            designer.text = song.Designers[(int)_gameInfo.CurrentLevel] ?? "Undefined";
            level.text = _gameInfo.CurrentLevel.ToString() + " " + song.Levels[(int)_gameInfo.CurrentLevel];

            accDX.text = isClassic ? $"{result.Acc.Classic:F2}%" : $"{result.Acc.DX:F4}%";
            var nowacc = isClassic ? result.Acc.Classic : result.Acc.DX;
            var historyacc = isClassic ? historyResult.Acc.Classic : historyResult.Acc.DX;
            accHistory.text = $"{nowacc - historyacc:+0.0000;-0.0000;0}%";
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

            _noteJudgeDiffGraph.texture = DrawNoteJudgeDiffGraph(result.NoteJudgeDiffs);
            if(result.NoteJudgeDiffs.IsEmpty)
            {
                avgJudgeTime.text = $"0.000s";
            }
            else
            {
                avgJudgeTime.text = $"{result.NoteJudgeDiffs.ToArray().Average() / 1000f:F3}s";
            }

            LoadCover(song).Forget();

            var breakJudgeInfo = JudgeDetail.UnpackJudgeRecord(result.JudgeRecord[ScoreNoteType.Break]);

            if (!totalJudgeRecord.IsFullCombo)
                clearLogo.SetActive(false);
            else if (totalJudgeRecord.IsAllPerfect)
            {
                if (breakJudgeInfo.IsTheoretical)
                    clearLogo.GetComponentInChildren<TextMeshProUGUI>().text = "AP+";
                else
                    clearLogo.GetComponentInChildren<TextMeshProUGUI>().text = "AP";
            }
            else if (totalJudgeRecord.IsFullComboPlus)
                clearLogo.GetComponentInChildren<TextMeshProUGUI>().text = "FC+";

            MajInstances.AudioManager.PlaySFX("bgm_result.mp3", true);
            PlayVoice(result.Acc.DX, song).Forget();
            if (!MajInstances.GameManager.Setting.Mod.IsAnyModActive())
            {
                MajInstances.ScoreManager.SaveScore(result, result.Level);
                var score = MaiScore.CreateFromResult(result,result.Level);
                if (score is not null && song is OnlineSongDetail)
                {
                    OnlineSaveTask = intractSender.SendScore(score);
                }
            }
            
        }

        async UniTask LoadCover(ISongDetail song)
        {
            coverImg.sprite = await song.GetCoverAsync(true);
        }

        async UniTask PlayVoice(double dxacc, ISongDetail song)
        {
            if (dxacc >= 97)
            {
                MajInstances.AudioManager.PlaySFX("Rank.wav");
                await UniTask.WaitForSeconds(1);
            }
            if (dxacc >= 100.5f)
            {
                MajInstances.AudioManager.PlaySFX("SSS+.wav");
                rank.text = "SSS+";
            }
            else if (dxacc >= 100f)
            {
                MajInstances.AudioManager.PlaySFX("SSS.wav");
                rank.text = "SSS";
            }
            else if (dxacc >= 99.5f)
            {
                MajInstances.AudioManager.PlaySFX("SS+.wav");
                rank.text = "SS+";
            }
            else if (dxacc >= 99f)
            {
                MajInstances.AudioManager.PlaySFX("SS.wav");
                rank.text = "SS";
            }
            else if (dxacc >= 98f)
            {
                MajInstances.AudioManager.PlaySFX("S+.wav");
                rank.text = "S+";
            }
            else if (dxacc >= 97f)
            {
                MajInstances.AudioManager.PlaySFX("S.wav");
                rank.text = "S";
            }
            if (dxacc > 97)
            {
                await UniTask.WaitForSeconds(2);
                var list = new string[] { "good.wav", "good_2.wav", "good_3.wav", "good_4.wav", "good_5.wav", "good_6.wav" };
                MajInstances.AudioManager.PlaySFX(list[Random.Range(0, list.Length)]);
                await UniTask.WaitForSeconds(3);
                if (song is OnlineSongDetail)
                {
                    MajInstances.AudioManager.PlaySFX("dian_zan.wav");
                }
            }
            else
            {
                var list = new string[] { "wuyu.wav", "wuyu_2.wav", "wuyu_3.wav" };
                MajInstances.AudioManager.PlaySFX(list[Random.Range(0, list.Length)]);
                await UniTask.WaitForSeconds(2);
            }
            await OnlineSaveTask;
            InputManager.BindAnyArea(OnAreaDown);
            MajInstances.LightManager.SetButtonLight(Color.green, 3);
            MajInstances.LightManager.SetButtonLight(Color.yellow, 4);
        }


        string BuildSubDisplayText(JudgeDetail judgeRecord)
            {
                var tapJudgeInfo = JudgeDetail.UnpackJudgeRecord(judgeRecord[ScoreNoteType.Tap]);
                var holdJudgeInfo = JudgeDetail.UnpackJudgeRecord(judgeRecord[ScoreNoteType.Hold]);
                var slideJudgeInfo = JudgeDetail.UnpackJudgeRecord(judgeRecord[ScoreNoteType.Slide]);
                var touchJudgeInfo = JudgeDetail.UnpackJudgeRecord(judgeRecord[ScoreNoteType.Touch]);
                var breakJudgeInfo = JudgeDetail.UnpackJudgeRecord(judgeRecord[ScoreNoteType.Break]);
                string[] nmsl = new string[]
                {
                "NOTES\t\tCP    \t\tP    \t\tGr    \t\tGd   \t\tM",
                $"Tap  \t\t\t{tapJudgeInfo.CriticalPerfect}\t\t{tapJudgeInfo.Perfect}\t\t{tapJudgeInfo.Great}\t\t{tapJudgeInfo.Good}\t\t{tapJudgeInfo.Miss}",
                $"Hold\t\t\t{holdJudgeInfo.CriticalPerfect}\t\t{holdJudgeInfo.Perfect}\t\t{holdJudgeInfo.Great}\t\t{holdJudgeInfo.Good}\t\t{holdJudgeInfo.Miss}",
                $"Slide\t\t\t{slideJudgeInfo.CriticalPerfect}\t\t{slideJudgeInfo.Perfect}\t\t{slideJudgeInfo.Great}\t\t{slideJudgeInfo.Good}\t\t{slideJudgeInfo.Miss}",
                $"Touch\t\t\t{touchJudgeInfo.CriticalPerfect}\t\t{touchJudgeInfo.Perfect}\t\t{touchJudgeInfo.Great}\t\t{touchJudgeInfo.Good}\t\t{touchJudgeInfo.Miss}",
                $"Break\t\t\t{breakJudgeInfo.CriticalPerfect}\t\t{breakJudgeInfo.Perfect}\t\t{breakJudgeInfo.Great}\t\t{breakJudgeInfo.Good}\t\t{breakJudgeInfo.Miss}"
                };
                return string.Join("\n", nmsl);
            }


        private void OnAreaDown(object sender, InputEventArgs e)
        {
            if (e.IsDown && e.IsButton )
            {
                switch (e.Type)
                {
                    case SensorArea.A4:
                        var canNextRound = _gameInfo.NextRound();
                        if (_gameInfo.IsDanMode)
                        {
                            if (!canNextRound)
                            {
                                InputManager.UnbindAnyArea(OnAreaDown);
                                MajInstances.SceneSwitcher.SwitchScene("TotalResult");
                                return;

                            }
                            else
                            {
                                InputManager.UnbindAnyArea(OnAreaDown);
                                MajInstances.AudioManager.StopSFX("bgm_result.mp3");

                                //TODO: Add Animation to show that
                                //SongStorage.WorkingCollection.Index++;
                                //MajInstances.GameManager.DanHP += SongStorage.WorkingCollection.DanInfo.RestoreHP;

                                MajInstances.SceneSwitcher.SwitchScene("Game", false);
                                return;
                            }
                        }
                        InputManager.UnbindAnyArea(OnAreaDown);
                        MajInstances.AudioManager.StopSFX("bgm_result.mp3");
                        MajInstances.SceneSwitcher.SwitchScene("List", false);
                        return;
                    case SensorArea.A5:
                        favoriteAdder.FavoratePressed();
                        break;
                }
                
            }
        }
        Texture DrawNoteJudgeDiffGraph(ReadOnlyMemory<float> noteJudgeDiffs)
        {
            ReadOnlySpan<float> dataset = noteJudgeDiffs.Span;
            const float SAMPLE_DIFF_STEP = 1.6667f / 2;
            const int CHART_PADDING_LEFT = 20;
            const int CHART_PADDING_RIGHT = 20;
            const int CHART_PADDING_TOP = 0;
            const int CHART_PADDING_BOTTOM = 30;

            var width = 690;
            var height = 139;
            var chartWidth = width - CHART_PADDING_LEFT - CHART_PADDING_RIGHT;
            var chartHeight = height - CHART_PADDING_TOP - CHART_PADDING_BOTTOM;
            
            var imageInfo = new SKImageInfo(width, height);
            Span<Point> points = stackalloc Point[180];
            var maxSampleCount = 0;
            var textFont = new SKFont(SKTypeface.Default);
            using var surface = SKSurface.Create(imageInfo);
            using var perfectPaint = new SKPaint();
            using var greatPaint = new SKPaint();
            using var goodPaint = new SKPaint();
            using var linePaint = new SKPaint();
            using var textPaint = new SKPaint();
            using var perfectPath = new SKPath();
            using var greatPath = new SKPath();
            using var goodPath = new SKPath();
            var canvas = surface.Canvas;
            
            canvas.Clear(SKColor.Empty);
            perfectPaint.Color = perfectColor.ToSkColor();
            perfectPaint.IsAntialias = true;
            perfectPaint.Style = SKPaintStyle.Fill;
            greatPaint.Color = greatColor.ToSkColor();
            greatPaint.IsAntialias = true;
            greatPaint.Style = SKPaintStyle.Fill;
            goodPaint.Color = greatColor.ToSkColor();
            goodPaint.IsAntialias = true;
            goodPaint.Style = SKPaintStyle.Fill;
            linePaint.Color = SKColors.White;
            linePaint.IsAntialias = true;
            linePaint.Style = SKPaintStyle.Fill;
            linePaint.StrokeWidth = 1f;
            textPaint.Color = SKColors.White;
            textPaint.IsAntialias = true;
            textPaint.Style = SKPaintStyle.Fill;
            textPaint.StrokeWidth = 4f;
            textFont.Size = 20;

            for (float sampleDiff = -150f,i = 0; sampleDiff <= 150f; sampleDiff += SAMPLE_DIFF_STEP * 2,i++)
            {
                var range = new Range<float>(sampleDiff - SAMPLE_DIFF_STEP, sampleDiff + SAMPLE_DIFF_STEP, ContainsType.Closed);
                var samples = dataset.FindAll(x => range.InRange(x));
                var sampleCount = samples.Length;
                var x = (sampleDiff + 150f) / 300f;
                var y = sampleCount;

                if (y > maxSampleCount)
                    maxSampleCount = y;

                points[(int)i] = new Point()
                {
                    X = x,
                    Y = y,
                    Diff = sampleDiff,
                    IsEmpty = samples.IsEmpty
                };
            }
            perfectPath.MoveTo(CHART_PADDING_LEFT, chartHeight);
            greatPath.MoveTo(CHART_PADDING_LEFT, chartHeight);
            goodPath.MoveTo(CHART_PADDING_LEFT, chartHeight);
            for (var i = 0; i < points.Length; i++)
            {
                var origin = points[i];
                var point = new Point()
                {
                    X = origin.X,
                    Y = origin.Y / maxSampleCount,
                    Diff = origin.Diff,
                    IsEmpty = origin.IsEmpty
                };
                var x = chartWidth * point.X + CHART_PADDING_LEFT;
                var y = chartHeight * (1 - point.Y) - CHART_PADDING_TOP;
                var isPerfect = Math.Abs(point.Diff) <= 50f;
                var isGreat = Math.Abs(point.Diff) <= 100f;
                var isGood = Math.Abs(point.Diff) <= 150f;

                if(point.IsEmpty)
                {
                    perfectPath.LineTo(x, chartHeight + CHART_PADDING_TOP);
                    greatPath.LineTo(x, chartHeight + CHART_PADDING_TOP);
                    goodPath.LineTo(x, chartHeight + CHART_PADDING_TOP);
                }
                else if (isPerfect)
                {
                    perfectPath.LineTo(x, y);
                    greatPath.LineTo(x, chartHeight + CHART_PADDING_TOP);
                    goodPath.LineTo(x, chartHeight + CHART_PADDING_TOP);
                }
                else if (isGreat)
                {
                    perfectPath.LineTo(x, chartHeight + CHART_PADDING_TOP);
                    greatPath.LineTo(x, y);
                    goodPath.LineTo(x, chartHeight + CHART_PADDING_TOP);
                }
                else if (isGood)
                {
                    perfectPath.LineTo(x, chartHeight + CHART_PADDING_TOP);
                    greatPath.LineTo(x, chartHeight + CHART_PADDING_TOP);
                    goodPath.LineTo(x, y);
                }
            }
            perfectPath.LineTo(chartWidth + CHART_PADDING_LEFT, chartHeight + CHART_PADDING_TOP);
            greatPath.LineTo(chartWidth + CHART_PADDING_LEFT, chartHeight + CHART_PADDING_TOP);
            goodPath.LineTo(chartWidth + CHART_PADDING_LEFT, chartHeight + CHART_PADDING_TOP);
            perfectPath.Close();
            greatPath.Close();
            goodPath.Close();

            canvas.DrawPath(perfectPath, perfectPaint);
            canvas.DrawPath(greatPath, greatPaint);
            canvas.DrawPath(goodPath, goodPaint);
            canvas.DrawLine(CHART_PADDING_LEFT, 
                            CHART_PADDING_TOP + chartHeight + 0.5f, 
                            CHART_PADDING_LEFT + chartWidth, 
                            CHART_PADDING_TOP + chartHeight + 0.5f, linePaint);
            
            for (var i = -9; i < 10; i++)
            {
                var index = i + 9f;
                var x =  (chartWidth  * (index / 18)) + CHART_PADDING_LEFT;
                var start = new SKPoint()
                {
                    X = x,
                    Y = CHART_PADDING_TOP
                };
                var end = new SKPoint()
                {
                    X = x,
                    Y = CHART_PADDING_TOP + chartHeight
                };
                var textPoint = new SKPoint()
                {
                    X = x + 6f,
                    Y = CHART_PADDING_TOP + chartHeight + 18f
                };
                canvas.DrawLine(start, end, linePaint);
                canvas.DrawText($"{i}f", textPoint,SKTextAlign.Right,textFont, textPaint);
            }
            return GraphHelper.GraphSnapshot(surface);
        }
        readonly struct Point
        {
            public float X { get; init; }
            public float Y { get; init; }
            public float Diff { get; init; }
            public bool IsEmpty { get; init; }
        }
    }
}