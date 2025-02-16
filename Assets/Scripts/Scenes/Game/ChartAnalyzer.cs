using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MajdataPlay.Types;
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
namespace MajdataPlay.Game
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

        void Start()
        {
            _colorA = tapColor;
            _colorB = slideColor;
            _colorC = touchColor;
            _rawImage = GetComponent<RawImage>();
        }
        
        public async UniTask AnalyzeAndDrawGraphAsync(ISongDetail songDetail, ChartLevel level, float length = -1, CancellationToken token = default)
        {
            try
            {
                try
                {
                    var simaiFile = await songDetail.GetMaidataAsync();
                    var maiChart = simaiFile.Charts[(int)level];
                    var lastnoteTiming = length == -1 ? maiChart.NoteTimings.Last().Timing : length;
                    var result = await AnalyzeMaidataAsync(maiChart, (float)lastnoteTiming);

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
            finally
            {
                await UniTask.Yield();
            }
        }
        public async UniTask AnalyzeAndDrawGraphAsync(SimaiChart data, float totalLength, CancellationToken token = default)
        {
            try
            {
                try
                {
                    var result = await AnalyzeMaidataAsync(data, totalLength);

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
            finally
            {
                await UniTask.Yield();
            }
        }
        internal static async UniTask<MaidataAnalyzeResult> AnalyzeMaidataAsync(SimaiChart data, float totalLength)
        {
            try
            {
                if(MajCache<SimaiChart, MaidataAnalyzeResult>.TryGetValue(data, out var cachedResult))
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

                await Task.Run(() =>
                {
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
                });
                var tex = await DrawGraphAsync(tapPoints, slidePoints, touchPoints);

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
            finally
            {
                await UniTask.Yield();
            }
        }
        static async ValueTask<Texture> DrawGraphAsync(List<Vector2> tapPoints, List<Vector2> slidePoints, List<Vector2> touchPoints)
        {
            var width = 1018;
            var height = 187;
            var imageInfo = new SKImageInfo(width, height);
            using var surface = SKSurface.Create(imageInfo);

            await Task.Run(() =>
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColor.Empty);
                using (var tapPaint = new SKPaint())
                using (var slidePaint = new SKPaint())
                using (var touchPaint = new SKPaint())
                {
                    tapPaint.Color = ToSkColor(_colorA);
                    tapPaint.IsAntialias = true;
                    tapPaint.Style = SKPaintStyle.Fill;
                    slidePaint.Color = ToSkColor(_colorB);
                    slidePaint.IsAntialias = true;
                    slidePaint.Style = SKPaintStyle.Fill;
                    touchPaint.Color = ToSkColor(_colorC);
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
            });
            await UniTask.Yield();
            return GraphSnapshot(surface);
        }
        static Texture GraphSnapshot(SKSurface surface)
        {
            //sort it into rawimage
            using (var image = surface.Snapshot())
            {
                var bitmap = SKBitmap.FromImage(image);
                var skcolors = bitmap.Pixels.AsSpan();
                var writer = new ArrayBufferWriter<SKColor>(bitmap.Width * bitmap.Height);
                for (var i = bitmap.Height - 1; i >= 0; i--) writer.Write(skcolors.Slice(i * bitmap.Width, bitmap.Width));
                var colors = writer.WrittenSpan.ToArray().AsParallel().AsOrdered().Select(s => ToUnityColor32(s)).ToArray();

                var tex0 = new Texture2D(bitmap.Width, bitmap.Height);
                tex0.SetPixels32(colors);
                tex0.Apply();

                return tex0;
            }
        }
        public static Color32 ToUnityColor32(SKColor skColor)
        {
            return new Color32(skColor.Red, skColor.Green, skColor.Blue, skColor.Alpha);
        }

        public static SKColor ToSkColor(Color32 color32)
        {
            return new SKColor(color32.r, color32.g, color32.b, color32.a);
        }
    }
}