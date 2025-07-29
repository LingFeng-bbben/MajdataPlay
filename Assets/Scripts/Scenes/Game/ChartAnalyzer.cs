using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MajdataPlay.Utils;
using MajSimai;
using NeoSmart.AsyncLock;
using SkiaSharp;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
#nullable enable
namespace MajdataPlay.Scenes.Game
{
    public class ChartAnalyzer : MonoBehaviour
    {
        RawImage _rawImage;
        public UnityEngine.Color tapColor;
        public UnityEngine.Color slideColor;
        public UnityEngine.Color touchColor;

        static UnityEngine.Color _colorA;
        static UnityEngine.Color _colorB;
        static UnityEngine.Color _colorC;

        public Text? anaText;
        readonly AsyncLock _locker = new();

        private void Awake()
        {
            Majdata<ChartAnalyzer>.Instance = this;
        }

        void Start()
        {
            _colorA = tapColor;
            _colorB = slideColor;
            _colorC = touchColor;
            _rawImage = GetComponent<RawImage>();
        }
        void OnDestroy()
        {
            Majdata<ChartAnalyzer>.Free();
        }
        public async UniTask AnalyzeAndDrawGraphAsync(ISongDetail songDetail, ChartLevel level, float length = -1, bool noCache = false, CancellationToken token = default)
        {
            try
            {
                try
                {
                    var simaiFile = await songDetail.GetMaidataAsync();
                    var maiChart = simaiFile.Charts[(int)level];
                    if (maiChart.IsEmpty)
                    {
                        return;
                    }
                    var lastnoteTiming = length == -1 ? maiChart.NoteTimings.LastOrDefault()?.Timing ?? length : length;
                    var result = await AnalyzeMaidataAsync(maiChart, (float)lastnoteTiming, noCache);
                    await UniTask.SwitchToMainThread();
                    token.ThrowIfCancellationRequested();
                    _rawImage.texture = result.LineGraph;
                    if (anaText is not null)
                    {
                        var max = result.PeakDensity;
                        var esti = result.Esti;
                        var minBPM = result.MinBPM;
                        var maxBPM = result.MaxBPM;
                        var time = result.Length;

                        anaText.text = "Peak Density = " + max + "\n";
                        anaText.text += "Esti = Lv." + (esti) + "\n";
                        anaText.text += "Length = " + ZString.Format("{0}:{1:00}.{2:000}", time.Minutes, time.Seconds, time.Milliseconds) + "\n";
                        anaText.text += "BPM = " + minBPM + " - " + maxBPM;
                    }
                }
                catch (Exception ex)
                {
                    MajDebug.LogException(ex);
                    await UniTask.Yield();
                    //_rawImage.texture = new Texture2D(0, 0);
                    if (anaText is not null)
                    {
                        anaText.text = "";
                    }
                }
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public async UniTask AnalyzeAndDrawGraphAsync(SimaiChart data, float totalLength, CancellationToken token = default)
        {
            try
            {
                var result = await AnalyzeMaidataAsync(data, totalLength);
                await UniTask.SwitchToMainThread();
                token.ThrowIfCancellationRequested();
                _rawImage.texture = result.LineGraph;
                if (anaText is not null)
                {
                    var max = result.PeakDensity;
                    var esti = result.Esti;
                    var minBPM = result.MinBPM;
                    var maxBPM = result.MaxBPM;
                    var time = result.Length;

                    anaText.text = "Peak Density = " + max + "\n";
                    anaText.text += "Esti = Lv." + (esti) + "\n";
                    anaText.text += "Length = " + ZString.Format("{0}:{1:00}.{2:000}", time.Minutes, time.Seconds, time.Milliseconds) + "\n";
                    anaText.text += "BPM = " + minBPM + " - " + maxBPM;
                }
            }
            catch (Exception ex)
            {
                MajDebug.LogException(ex);
                await UniTask.Yield();
                _rawImage.texture = new Texture2D(0, 0);
                if (anaText is not null)
                {
                    anaText.text = "";
                }
            }
        }
        internal static async UniTask<MaidataAnalyzeResult> AnalyzeMaidataAsync(SimaiChart data, float totalLength, bool noCache = false)
        {
            await UniTask.SwitchToThreadPool();
            if ((!noCache) && MajCache<SimaiChart, MaidataAnalyzeResult>.TryGetValue(data, out var cachedResult))
            {
                return cachedResult;
            }

            var tapPoints = new List<Vector2>();
            var slidePoints = new List<Vector2>();
            var touchPoints = new List<Vector2>();
            var max = 0f;
            var maxBPM = 0f;
            var minBPM = 0f;
            var length = TimeSpan.Zero;
            var esti = 0f;

            for (float time = 0; time < totalLength; time += 0.5f)
            {
                var timingPoints = data.NoteTimings.ToList().FindAll(o => o.Timing > time - 0.75f && o.Timing <= time + 0.75f);
                float y0 = 0, y1 = 0, y2 = 0;
                foreach (var timingPoint in timingPoints)
                {
                    foreach (var note in timingPoint.Notes)
                    {
                        switch (note.Type)
                        {
                            case SimaiNoteType.Tap:
                            case SimaiNoteType.Hold:
                                y0++;
                                break;
                            case SimaiNoteType.Slide:
                                y1 += 2;
                                break;
                            case SimaiNoteType.Touch:
                            case SimaiNoteType.TouchHold:
                                y2++;
                                break;
                        }
                    }

                }
                if (y0 + y1 + y2 > max) max = y0 + y1 + y2;

                var x = time / totalLength;
                tapPoints.Add(new Vector2(x, y0));
                slidePoints.Add(new Vector2(x, y1));
                touchPoints.Add(new Vector2(x, y2));
                maxBPM = data.NoteTimings.Max(o => o.Bpm);
                minBPM = data.NoteTimings.Min(o => o.Bpm);
            }


            var avg = tapPoints.Average(o => o.y) + 3f * slidePoints.Average(o => o.y) + 0.5f * touchPoints.Average(o => o.y);
            length = TimeSpan.FromSeconds(totalLength);
            esti = 7.5f * Mathf.Log10(3.8f * (avg + 0.3f * max));

            //normalize
            for (var i = 0; i < tapPoints.Count; i++)
            {
                tapPoints[i] = new Vector2(tapPoints[i].x, tapPoints[i].y / max);
                slidePoints[i] = new Vector2(slidePoints[i].x, slidePoints[i].y / max);
                touchPoints[i] = new Vector2(touchPoints[i].x, touchPoints[i].y / max);
            }

            var tex = await DrawGraphAsync(tapPoints, slidePoints, touchPoints);

            await UniTask.SwitchToThreadPool();

            var result = new MaidataAnalyzeResult()
            {
                Esti = esti,
                Length = length,
                MaxBPM = maxBPM,
                MinBPM = minBPM,
                PeakDensity = max,
                LineGraph = tex
            };
            MajCache<SimaiChart, MaidataAnalyzeResult>.Replace(data, result);
            return result;
        }
        static async ValueTask<Texture> DrawGraphAsync(List<Vector2> tapPoints, List<Vector2> slidePoints, List<Vector2> touchPoints)
        {
            var width = 1018;
            var height = 187;
            var imageInfo = new SKImageInfo(width, height);
            using var surface = SKSurface.Create(imageInfo);

            var canvas = surface.Canvas;
            canvas.Clear(SKColor.Empty);
            using (var tapPaint = new SKPaint())
            using (var slidePaint = new SKPaint())
            using (var touchPaint = new SKPaint())
            {
                tapPaint.Color = _colorA.ToSkColor();
                tapPaint.IsAntialias = true;
                tapPaint.Style = SKPaintStyle.Fill;
                slidePaint.Color = _colorB.ToSkColor();
                slidePaint.IsAntialias = true;
                slidePaint.Style = SKPaintStyle.Fill;
                touchPaint.Color = _colorC.ToSkColor();
                touchPaint.IsAntialias = true;
                touchPaint.Style = SKPaintStyle.Fill;
                using (var tapPath = new SKPath())
                using (var slidePath = new SKPath())
                using (var touchPath = new SKPath())
                {
                    tapPath.MoveTo(0, height);
                    slidePath.MoveTo(0, height);
                    touchPath.MoveTo(0, height);
                    for (var i = 0; i < tapPoints.Count; i++)
                    {
                        var x = tapPoints[i].x * width;
                        var y = tapPoints[i].y;
                        tapPath.LineTo(x, (1 - y) * height);
                        y += slidePoints[i].y;
                        slidePath.LineTo(x, (1 - y) * height);
                        y += touchPoints[i].y;
                        touchPath.LineTo(x, (1 - y) * height);
                    }
                    tapPath.LineTo(width, height);
                    slidePath.LineTo(width, height);
                    touchPath.LineTo(width, height);
                    tapPath.Close();
                    slidePath.Close();
                    touchPath.Close();

                    canvas.DrawPath(touchPath, touchPaint);
                    canvas.DrawPath(slidePath, slidePaint);
                    canvas.DrawPath(tapPath, tapPaint);
                }
            }
            await UniTask.Yield();
            return GraphHelper.GraphSnapshot(surface);
        }
    }
}