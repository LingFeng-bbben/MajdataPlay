using MajdataPlay.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
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
using MajdataPlay.Recording;
using MajdataPlay.Net;
using MajdataPlay.Scenes.Result.Components;
using UnityEngine.Serialization;
using System.Threading;

#nullable enable
namespace MajdataPlay.Scenes.Result
{
    public partial class ResultScreenManager : MajBehaviour
    {
        public bool IsDebug;
        public TextMeshProUGUI title;
        public TextMeshProUGUI artist;
        public TextMeshProUGUI designer;
        public TextMeshProUGUI level;

        public TextMeshProUGUI accDX;
        public TextMeshProUGUI accHistory;
        public TextMeshProUGUI dxScore;
        public TextMeshProUGUI rank;

        public TextMeshProUGUI criticalCount;
        public TextMeshProUGUI perfectCount;
        public TextMeshProUGUI greatCount;
        public TextMeshProUGUI goodCount;
        public TextMeshProUGUI missCount;

        public TextMeshProUGUI fastCount;
        public TextMeshProUGUI lateCount;
        public TextMeshProUGUI avgJudgeTime;

        public TextMeshProUGUI omg;
        public ResultsSubDisplayManager subMonitorManager;
        public Color critColor;
        public Color perfectColor;
        public Color greatColor;
        public Color goodColor;


        public TextMeshProUGUI clearText;
        public GameObject xxlb;

        public UserInfoDisplayer userInfoDisplayer;

        public Image coverImg;

        public RawImage _noteJudgeDiffGraph;

        public FavoriteAdder favoriteAdder;

        [SerializeField]
        [FormerlySerializedAs("levelCircleDisplayer")]
        Image _levelCircleDisplayer;

        [SerializeField]
        [FormerlySerializedAs("levelBGDisplayer")]
        Image _levelBGDisplayer;

        [SerializeField]
        [FormerlySerializedAs("dxScoreDisplayer")]
        DXScoreDisplayer _dxScoreDisplayer;

        [SerializeField]
        [FormerlySerializedAs("onlineInteractionSender")]
        OnlineInteractionSender _onlineInteractionSender;

        GameInfo _gameInfo = Majdata<GameInfo>.Instance!;

        Task _scoreSaveTask = Task.CompletedTask;
        bool _isAllTaskFinished = false;
        bool _isInited = false;
        bool _isExited = false;

        const string PERFECT_COUNT_TEXT_TEMPLATE = "<line-height=60%>{0}\n<size=24>(-{1})</size>";

        protected override void Awake()
        {
            base.Awake();
            InputManager.TouchButtonRingEdge = 4.8f;
        }
        async Task Start()
        {
            if (IsDebug)
            {
                var rect1 = _noteJudgeDiffGraph.GetComponent<RectTransform>().rect;
                var width = (int)rect1.width;
                var height = (int)rect1.height;


                // 生成随机样本用于测试
                const int sampleCount = 1000; // 可调整样本数量
                var arr = new float[sampleCount];
                var rng = new System.Random();
                for (int i = 0; i < sampleCount; i++)
                {
                    // 在 -150 .. 150 范围内生成随机浮点数
                    arr[i] = (float)(rng.NextDouble() * 300.0 - 150.0);
                }
                var testSpan = new ReadOnlySpan<float>(arr);
                

                _noteJudgeDiffGraph.texture = DrawNoteJudgeDiffGraph(testSpan, height, width);
                return;
            }


            rank.text = "";
            var listConfig = MajEnv.RuntimeConfig.List;
            var result = _gameInfo.GetLastResult();
            var isClassic = MajEnv.Settings.Judge.Mode == JudgeModeOption.Classic;

            CabinetLed.SetAllLight(Color.white);

            var totalJudgeRecord = JudgeDetail.UnpackJudgeRecord(result.JudgeRecord.TotalJudgeInfo);
            var song = result.SongDetail;
            var historyResult = ScoreManager.GetScore(song, listConfig.SelectedDiff);
            var score = MaiScore.CreateFromResult(result, result.Level);
            var diffColor = RuntimeDatabase.DifficultyColors[(int)_gameInfo.CurrentLevel];
            _levelCircleDisplayer.color = diffColor;
            _levelBGDisplayer.color = diffColor;
            _onlineInteractionSender.Init(song, score);
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
            level.text = song.Levels[(int)_gameInfo.CurrentLevel];

            // TODO: more animation here
            accDX.text = isClassic ? $"{Math.Floor(result.Acc.Classic * 100) / 100:F2}%" : $"{Math.Floor(result.Acc.DX * 10000) / 10000:F4}%";
            var nowAcc = isClassic ? result.Acc.Classic : result.Acc.DX;
            var historyAcc = isClassic ? historyResult.Acc.Classic : historyResult.Acc.DX;
            accHistory.text = $"{nowAcc - historyAcc:+0.0000;-0.0000;0}%";

            _dxScoreDisplayer.SetScore(result);

            criticalCount.text = $"{totalJudgeRecord.CriticalPerfect}";
            perfectCount.text = $"{totalJudgeRecord.Perfect}";
            greatCount.text = $"{totalJudgeRecord.Great}";
            goodCount.text = $"{totalJudgeRecord.Good}";
            missCount.text = $"{totalJudgeRecord.Miss}";

            fastCount.text = $"{result.Fast}";
            lateCount.text = $"{result.Late}";

            subMonitorManager.SetJudgeDetail(result.JudgeRecord);

            var rect = _noteJudgeDiffGraph.GetComponent<RectTransform>().rect;
            _noteJudgeDiffGraph.texture = DrawNoteJudgeDiffGraph(result.NoteJudgeDiffs.Span, (int)rect.height, (int)rect.width);
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


            if (totalJudgeRecord.IsAllPerfect)
            {
                if (breakJudgeInfo.IsTheoretical)
                {
                    clearText.text = "AP+";
                }
                else
                {
                    clearText.text = "AP";
                }
            }
            else if (totalJudgeRecord.IsFullCombo)
            {
                if (totalJudgeRecord.IsFullComboPlus) { 
                    clearText.text = "FC+";
                }
                else
                {
                    clearText.text = "FC";
                }
            }
            else
            {
                clearText.text = "";
            }

            MajInstances.AudioManager.PlaySFX("bgm_result.mp3", true);
            PlayVoice(result.Acc.DX, 
                song,
                totalJudgeRecord.IsAllPerfect, 
                totalJudgeRecord.IsFullCombo, 
                gameObject.GetCancellationTokenOnDestroy()).Forget();
            if (!MajInstances.GameManager.Settings.Mod.IsAnyModActive())
            {
                var localScoreSaveTask = ScoreManager.SaveScore(result, result.Level);
                if (song is OnlineSongDetail onlineSong && onlineSong.ServerInfo.RuntimeConfig.AuthMethod != NetAuthMethodOption.None)
                {
                    var task = _onlineInteractionSender.SendScoreAsync();
                    _scoreSaveTask = Task.WhenAll(localScoreSaveTask, task);
                }
                else
                {
                    _scoreSaveTask = localScoreSaveTask;
                }
            }
            await UniTask.Delay(3000);
            _isInited = true;
            CabinetLed.SetButtonLight(Color.yellow, 4);
            var t1 = _scoreSaveTask;
            var t2 = RecordHelper.StopRecordAsync();
            while (!t1.IsCompleted || !t2.IsCompleted)
            {
                await UniTask.Yield();
            }
            _isAllTaskFinished = true;
            CabinetLed.SetButtonLight(Color.green, 3);
        }

        async UniTask LoadCover(ISongDetail song)
        {
            var cover = await song.GetCoverAsync(true);
            await UniTask.SwitchToMainThread();
            coverImg.sprite = cover;
        }

        async UniTask PlayVoice(double dxacc, ISongDetail song, bool isAP, bool isFC, CancellationToken token = default)
        {
            try
            {
                AudioSampleWrap? lastSample = null;
                if (dxacc >= 97)
                {
                    lastSample = MajInstances.AudioManager.PlaySFX("clear.wav")!;
                    while (lastSample.IsPlaying) await UniTask.Yield(token, true);
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
                    await UniTask.Yield(token, true);
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
                    await UniTask.Yield(token, true);
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
            InputManager.TouchButtonRingEdge = 5.4f;
            DestroyImmediate(_noteJudgeDiffGraph.texture, true);
        }
        Texture DrawNoteJudgeDiffGraph(ReadOnlySpan<float> dataset,
                               int height,
                               int width)
        {
            const float DIFF_MIN = -150f;
            const float DIFF_MAX = 150f;
            const float DIFF_RANGE = DIFF_MAX - DIFF_MIN;

            const int CHART_PADDING_LEFT = 0;
            const int CHART_PADDING_RIGHT = 0;
            const int CHART_PADDING_TOP = 0;
            const int CHART_PADDING_BOTTOM = 0;

            const int BAR_COUNT = 72; 

            var chartWidth = width - CHART_PADDING_LEFT - CHART_PADDING_RIGHT;
            var chartHeight = height - CHART_PADDING_TOP - CHART_PADDING_BOTTOM;

            var imageInfo = new SKImageInfo(width, height);
            Span<Point> points = stackalloc Point[BAR_COUNT];
            var maxSampleCount = 0;
            using var surface = SKSurface.Create(imageInfo);
            using var critPaint = new SKPaint();
            using var perfectPaint = new SKPaint();
            using var greatPaint = new SKPaint();
            using var goodPaint = new SKPaint();
            using var emptyPaint = new SKPaint();
            //using var linePaint = new SKPaint();
            //using var midLinePaint = new SKPaint();
            var canvas = surface.Canvas;

            canvas.Clear(SKColor.Empty);

            critPaint.Color = critColor.ToSkColor();
            critPaint.IsAntialias = true;
            critPaint.Style = SKPaintStyle.Fill;

            perfectPaint.Color = perfectColor.ToSkColor();
            perfectPaint.IsAntialias = true;
            perfectPaint.Style = SKPaintStyle.Fill;

            greatPaint.Color = greatColor.ToSkColor();
            greatPaint.IsAntialias = true;
            greatPaint.Style = SKPaintStyle.Fill;

            goodPaint.Color = goodColor.ToSkColor();
            goodPaint.IsAntialias = true;
            goodPaint.Style = SKPaintStyle.Fill;

            emptyPaint.Color = SKColors.Transparent;
            emptyPaint.IsAntialias = true;
            emptyPaint.Style = SKPaintStyle.Fill;

            //linePaint.Color = SKColors.White.WithAlpha(180);
            //linePaint.IsAntialias = true;
            //linePaint.Style = SKPaintStyle.Stroke;
            //linePaint.StrokeWidth = 1f;

            //midLinePaint.Color = SKColors.Red.WithAlpha(180);
            //midLinePaint.IsAntialias = true;
            //midLinePaint.Style = SKPaintStyle.Stroke;
            //midLinePaint.StrokeWidth = 2f;

            // 计算每个 bin 的区间
            var binWidthDiff = DIFF_RANGE / (float)BAR_COUNT; // 每个 bin 覆盖的 diff 范围
            for (int i = 0; i < BAR_COUNT; i++)
            {
                var binCenter = DIFF_MIN + (i + 0.5f) * binWidthDiff;
                var binStart = DIFF_MIN + i * binWidthDiff;
                var binEnd = binStart + binWidthDiff;
                var range = new Range<float>(binStart, binEnd, ContainsType.Closed);
                var samples = dataset.FindAll(x => range.InRange(x));
                var sampleCount = samples.Length;

                if (sampleCount > maxSampleCount) maxSampleCount = sampleCount;

                points[i] = new Point()
                {
                    X = binCenter, // 存储中心 diff 以便后续判断颜色
                    Y = sampleCount,
                    Diff = binCenter,
                    IsEmpty = samples.IsEmpty
                };
            }

            // 柱状图参数：间距与圆角
            var barSpacing = 2f; // 每个柱子之间的 padding（像素），可调整
            var totalSpacing = barSpacing * (BAR_COUNT - 1);
            var barWidth = Math.Max(1f, (chartWidth - totalSpacing) / BAR_COUNT);
            var cornerRadius = MathF.Min(barWidth, chartHeight) * 0.5f;

            // 绘制每个柱子
            for (int i = 0; i < BAR_COUNT; i++)
            {
                var origin = points[i];
                if (maxSampleCount == 0)
                {
                    // 没有样本，直接跳出（或可绘制占位）
                    continue;
                }

                // 归一化高度
                var normalizedHeight = origin.Y / (float)maxSampleCount; // 0..1
                var drawHeight = normalizedHeight * chartHeight;

                // 计算 x 位置：从左 padding 开始，按宽度+间距排列
                var x = CHART_PADDING_LEFT + i * (barWidth + barSpacing);
                var yTop = CHART_PADDING_TOP + (chartHeight - drawHeight);
                var rect = new SKRect(x, yTop, x + barWidth, CHART_PADDING_TOP + chartHeight);

                // 选择颜色（根据柱子中心 diff）
                var absDiff = Math.Abs(origin.Diff);
                SKPaint paintToUse;
                if (absDiff <= MajEnv.FRAME_LENGTH_MSEC)
                    paintToUse = critPaint;
                else if (absDiff <= MajEnv.FRAME_LENGTH_MSEC * 3)
                    paintToUse = perfectPaint;
                else if (absDiff <= MajEnv.FRAME_LENGTH_MSEC * 6)
                    paintToUse = greatPaint;
                else
                    paintToUse = goodPaint;

                // 如果你想跳过空桶：保持 continue；否则可绘制浅色占位
                if (origin.IsEmpty)
                {
                    // 跳过绘制空桶（若想显示占位，改为绘制 emptyPaint）
                    continue;
                }

                // 绘制圆角矩形柱子
                canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, paintToUse);

                // 可选细边框增强分隔感
                using var borderPaint = new SKPaint
                {
                    Color = SKColors.White.WithAlpha(0),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 0.5f,
                    IsAntialias = true
                };
                canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, borderPaint);
            }

            // 绘制底线
            //canvas.DrawLine(CHART_PADDING_LEFT,
            //                CHART_PADDING_TOP + chartHeight + 0.5f,
            //                CHART_PADDING_LEFT + chartWidth,
            //                CHART_PADDING_TOP + chartHeight + 0.5f, linePaint);

            // 绘制竖向网格线（可保留）
            //for (var i = -9; i < 10; i++)
            //{
            //    var index = i + 9f;
            //    var gridX = (chartWidth * (index / 18f)) + CHART_PADDING_LEFT;
            //    var start = new SKPoint()
            //    {
            //        X = gridX,
            //        Y = CHART_PADDING_TOP
            //    };
            //    var end = new SKPoint()
            //    {
            //        X = gridX,
            //        Y = CHART_PADDING_TOP + chartHeight
            //    };
            //    canvas.DrawLine(start, end, linePaint);
            //}

            // 绘制中线（Diff = 0）
            // 计算中线 x 坐标：Diff=0 对应的相对位置为 (0 - DIFF_MIN) / DIFF_RANGE
            //var centerRatio = (0f - DIFF_MIN) / DIFF_RANGE; // 0..1
            //var centerX = CHART_PADDING_LEFT + centerRatio * chartWidth;
            //// 中线从顶部到底部
            //canvas.DrawLine(centerX, CHART_PADDING_TOP, centerX, CHART_PADDING_TOP + chartHeight, midLinePaint);

            return surface.ToTexture2D(imageInfo);
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