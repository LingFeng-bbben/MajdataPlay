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
using MajdataPlay.Drawing;
using Random = UnityEngine.Random;
using MajdataPlay.Scenes.Game;
using MajdataPlay.Scenes.List;
using MajdataPlay.Numerics;
using MajdataPlay.Scenes.Game.Notes;
using MajdataPlay.Settings;
using System.Threading.Tasks;

#nullable enable
namespace MajdataPlay.Scenes.Result
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

        public UserInfoDisplayer userInfoDisplayer;

        public Image coverImg;

        public RawImage _noteJudgeDiffGraph;

        public FavoriteAdder favoriteAdder;

        GameInfo _gameInfo = Majdata<GameInfo>.Instance!;

        Task _scoreSaveTask = Task.CompletedTask;
        bool _isAllTaskFinished = false;
        bool _isInited = false;
        bool _isExited = false;

        void Start()
        {
            rank.text = "";
            var listConfig = MajEnv.RuntimeConfig.List;
            var result = _gameInfo.GetLastResult();
            var isClassic = MajEnv.Settings.Judge.Mode == JudgeModeOption.Classic;

            LedRing.SetAllLight(Color.white);

            var totalJudgeRecord = JudgeDetail.UnpackJudgeRecord(result.JudgeRecord.TotalJudgeInfo);
            var song = result.SongDetail;
            var historyResult = ScoreManager.GetScore(song, listConfig.SelectedDiff);
            var score = MaiScore.CreateFromResult(result, result.Level);
            var intractSender = GetComponent<OnlineInteractionSender>();
            intractSender.Init(song, score);
            favoriteAdder.SetSong(song);
            userInfoDisplayer.DisplayFromSong(song);


            if (result.Acc.DX < 97)
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

            accDX.text = isClassic ? $"{Math.Floor(result.Acc.Classic * 100) / 100:F2}%" : $"{Math.Floor(result.Acc.DX * 10000) / 10000:F4}%";
            var nowAcc = isClassic ? result.Acc.Classic : result.Acc.DX;
            var historyAcc = isClassic ? historyResult.Acc.Classic : historyResult.Acc.DX;
            accHistory.text = $"{nowAcc - historyAcc:+0.0000;-0.0000;0}%";
            var dxScoreRank = new DXScoreRank(result.DXScore, result.TotalDXScore);
            if (dxScoreRank.Rank > 0)
            {
                dxScore.text = $"*{dxScoreRank.Rank} {result.DXScore}/{result.TotalDXScore}";
            }
            else
            {
                dxScore.text = $"{result.DXScore}/{result.TotalDXScore}";
            }

            perfectCount.text = $"{totalJudgeRecord.CriticalPerfect + totalJudgeRecord.Perfect}";
            greatCount.text = $"{totalJudgeRecord.Great}";
            goodCount.text = $"{totalJudgeRecord.Good}";
            missCount.text = $"{totalJudgeRecord.Miss}";

            fastCount.text = $"{result.Fast}";
            lateCount.text = $"{result.Late}";

            subMonitor.text = BuildSubDisplayText(result.JudgeRecord);

            _noteJudgeDiffGraph.texture = DrawNoteJudgeDiffGraph(result.NoteJudgeDiffs);
            if(MajEnv.Settings.Debug.OffsetUnit == OffsetUnitOption.Second)
            {
                if (result.NoteJudgeDiffs.IsEmpty)
                {
                    avgJudgeTime.text = $"0.000s";
                }
                else
                {
                    avgJudgeTime.text = $"{result.NoteJudgeDiffs.ToArray().Average() / 1000f:F3}s";
                }
            }
            else
            {
                if (result.NoteJudgeDiffs.IsEmpty)
                {
                    avgJudgeTime.text = $"0.0f";
                }
                else
                {
                    avgJudgeTime.text = $"{result.NoteJudgeDiffs.ToArray().Average() / MajEnv.FRAME_LENGTH_MSEC:F1}f";
                }
            }

            
            LoadCover(song).Forget();

            var breakJudgeInfo = JudgeDetail.UnpackJudgeRecord(result.JudgeRecord[ScoreNoteType.Break]);

            if (!totalJudgeRecord.IsFullCombo)
            {
                clearLogo.SetActive(false);
            }
            else if (totalJudgeRecord.IsAllPerfect)
            {
                if (breakJudgeInfo.IsTheoretical)
                {
                    clearLogo.GetComponentInChildren<TextMeshProUGUI>().text = "AP+";
                }
                else
                {
                    clearLogo.GetComponentInChildren<TextMeshProUGUI>().text = "AP";
                }
            }
            else if (totalJudgeRecord.IsFullComboPlus)
            {
                clearLogo.GetComponentInChildren<TextMeshProUGUI>().text = "FC+";
            }

            MajInstances.AudioManager.PlaySFX("bgm_result.mp3", true);
            PlayVoice(result.Acc.DX, song,totalJudgeRecord.IsAllPerfect, totalJudgeRecord.IsFullCombo).Forget();
            if (!MajInstances.GameManager.Setting.Mod.IsAnyModActive())
            {
                var localScoreSaveTask = ScoreManager.SaveScore(result, result.Level);
                if (song is OnlineSongDetail onlineSong && onlineSong.ServerInfo.RuntimeConfig.AuthMethod != NetAuthMethodOption.None)
                {
                    var task = intractSender.SendScoreAsync();
                    _scoreSaveTask = Task.WhenAll(localScoreSaveTask, task);
                }
                else
                {
                    _scoreSaveTask = localScoreSaveTask;
                }
            }

        }

        async UniTask LoadCover(ISongDetail song)
        {
            var cover = await song.GetCoverAsync(true);
            await UniTask.SwitchToMainThread();
            coverImg.sprite = cover;
        }

        async UniTask PlayVoice(double dxacc, ISongDetail song, bool isAP, bool isFC)
        {
            try
            {
                AudioSampleWrap? lastSample = null;
                if (dxacc >= 97)
                {
                    lastSample = MajInstances.AudioManager.PlaySFX("clear.wav")!;
                    while (lastSample.IsPlaying) await UniTask.Yield();
                }
                if (dxacc >= 100.5f)
                {
                    lastSample = MajInstances.AudioManager.PlaySFX("SSS+.wav")!;
                    rank.text = "SSS+";
                }
                else if (dxacc >= 100f)
                {
                    lastSample = MajInstances.AudioManager.PlaySFX("SSS.wav")!;
                    rank.text = "SSS";
                }
                else if (dxacc >= 99.5f)
                {
                    lastSample = MajInstances.AudioManager.PlaySFX("SS+.wav")!;
                    rank.text = "SS+";
                }
                else if (dxacc >= 99f)
                {
                    lastSample = MajInstances.AudioManager.PlaySFX("SS.wav")!;
                    rank.text = "SS";
                }
                else if (dxacc >= 98f)
                {
                    lastSample = MajInstances.AudioManager.PlaySFX("S+.wav")!;
                    rank.text = "S+";
                }
                else if (dxacc >= 97f)
                {
                    lastSample = MajInstances.AudioManager.PlaySFX("S.wav")!;
                    rank.text = "S";
                }

                while (lastSample != null && lastSample.IsPlaying)
                {
                    await UniTask.Yield();
                }

                if (isAP)
                {
                    var list = new string[] { "ap_comment.wav", "ap_comment_2.wav" };
                    lastSample = MajInstances.AudioManager.PlaySFX(list[Random.Range(0, list.Length)]);
                }
                else if(isFC)
                {
                    var list = new string[] { "fc_comment.wav", "fc_comment_2.wav" };
                    lastSample = MajInstances.AudioManager.PlaySFX(list[Random.Range(0, list.Length)]);
                }else if (dxacc >= 97f)
                {
                    var list = new string[] { "clear_comment.wav", "clear_comment_2.wav", "clear_comment_3.wav", "clear_comment_4.wav" };
                    lastSample = MajInstances.AudioManager.PlaySFX(list[Random.Range(0, list.Length)]);
                }
                else
                {
                    var list = new string[] { "fail_comment.wav", "fail_comment_2.wav", "fail_comment_3.wav", "fail_comment_4.wav", "fail_comment_5.wav" };
                    lastSample = MajInstances.AudioManager.PlaySFX(list[Random.Range(0, list.Length)]);
                }

                while (lastSample != null && lastSample.IsPlaying)
                {
                    await UniTask.Yield();
                }

                if (song is OnlineSongDetail)
                {
                    MajInstances.AudioManager.PlaySFX("dian_zan.wav");
                }
            }
            catch (Exception e)
            { 
                MajDebug.LogException(e); 
            }
            _isInited = true;
            LedRing.SetButtonLight(Color.yellow, 4);
            var t1 = _scoreSaveTask;
            var t2 = RecordHelper.StopRecordAsync();
            while(!t1.IsCompleted || !t2.IsCompleted)
            {
                await UniTask.Yield();
            }
            _isAllTaskFinished = true;
            LedRing.SetButtonLight(Color.green, 3);
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


        void Update()
        {
            if(!_isInited || _isExited)
            {
                return;
            }
            if(InputManager.IsButtonClickedInThisFrame(ButtonZone.A5))
            {
                favoriteAdder.FavoratePressed();
            }
            if(!_isAllTaskFinished)
            {
                return;
            }
            if(InputManager.IsButtonClickedInThisFrame(ButtonZone.A4))
            {
                var canNextRound = _gameInfo.NextRound();
                if (_gameInfo.IsDanMode)
                {
                    if (!canNextRound)
                    {
                        _isExited = true;
                        MajInstances.SceneSwitcher.SwitchScene("TotalResult");
                        return;
                    }
                    else
                    {
                        MajInstances.AudioManager.StopSFX("bgm_result.mp3");

                        //TODO: Add Animation to show that
                        //SongStorage.WorkingCollection.Index++;
                        //MajInstances.GameManager.DanHP += SongStorage.WorkingCollection.DanInfo.RestoreHP;
                        _isExited = true;
                        MajInstances.SceneSwitcher.SwitchScene("Game", false);
                        return;
                    }
                }
                _isExited = true;
                MajInstances.AudioManager.StopSFX("bgm_result.mp3");
                MajInstances.SceneSwitcher.SwitchScene("List", false);
            }
        }
        void OnDestroy()
        {
            DestroyImmediate(_noteJudgeDiffGraph.texture, true);
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
            goodPaint.Color = goodColor.ToSkColor();
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
                {
                    maxSampleCount = y;
                }

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