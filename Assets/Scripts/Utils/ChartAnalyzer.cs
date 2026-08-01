using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.Diagnostics;
using MajdataPlay.Drawing;
using MajdataPlay.Numerics;
using MajSimai;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Utils
{
    [BurstCompile]
    internal static class ChartAnalyzer
    {
        [ThreadStatic]
        static SKPaint? s_tapPaint;
        [ThreadStatic]
        static SKPaint? s_slidePaint;
        [ThreadStatic]
        static SKPaint? s_touchPaint;
        [ThreadStatic]
        static SKPath? s_tapPath;
        [ThreadStatic]
        static SKPath? s_slidePath;
        [ThreadStatic]
        static SKPath? s_touchPath;

        [ThreadStatic]
        static SKPoint[]? s_radii;
        [ThreadStatic]
        static SKRoundRect? s_roundRect;

        readonly static Color TapColor = new Color(0.8980393f, 0.3176471f, 0.5607843f);
        readonly static Color TouchColor = new Color(0.9450981f, 0.654902f, 0.2235294f);
        readonly static Color SlideColor = new Color(0.2431373f, 0.5568628f, 0.6784314f);

        public static MaidataAnalyzeResult AnalyzeMaidata(SimaiChart data, float? chartLength = null)
        {
            if(data.NoteTimings.IsEmpty)
            {
                return default;
            }
            var noteTimings = data.NoteTimings;
            var length = chartLength ?? (float)noteTimings[noteTimings.Length - 1].Timing;
            var result = AnalyzeMaidataCore(noteTimings, length);

            result.TapPoints.Dispose();
            result.TouchPoints.Dispose();
            result.SlidePoints.Dispose();
            return new ()
            {
                Esti = result.Esti,
                Length = result.Length,
                MaxBPM = result.MaxBPM,
                MinBPM = result.MinBPM,
                PeakDensity = result.PeakDensity,
            };
        }
        public static MaidataLineGraphAnalyzeResult AnalyzeMaidataWithGraph(SimaiChart data, 
                                                                            int height, 
                                                                            int width, 
                                                                            float? chartLength = null)
        {
            if (data.NoteTimings.IsEmpty)
            {
                return default;
            }
            var noteTimings = data.NoteTimings;
            var length = chartLength ?? (float)noteTimings[noteTimings.Length - 1].Timing;
            var result = AnalyzeMaidataCore(noteTimings, length);
            var graph = DrawGraph(result, height, width);

            result.TapPoints.Dispose();
            result.TouchPoints.Dispose();
            result.SlidePoints.Dispose();
            return new ()
            {
                Esti = result.Esti,
                Length = result.Length,
                MaxBPM = result.MaxBPM,
                MinBPM = result.MinBPM,
                PeakDensity = result.PeakDensity,
                LineGraph = graph
            };
        }
        static unsafe Texture DrawGraph(InternalMaidataAnalyzeResult analyzeResult,
                                 int height,
                                 int width)
        {
            const int BarCount = 64;

            EnsureSakaComponentIsInited();
            var tapPoints = analyzeResult.TapPoints;
            var slidePoints = analyzeResult.SlidePoints;
            var touchPoints = analyzeResult.TouchPoints;

            var imageInfo = new SKImageInfo(width, height);
            using var surface = SKSurface.Create(imageInfo);
            var canvas = surface.Canvas;
            canvas.Clear(SKColor.Empty);

            var count = tapPoints.Length;

            var bars = BuildBars(
                tapPoints,
                slidePoints,
                touchPoints,
                BarCount);

            var step = (float)width / BarCount;

            var barWidth = step * 0.72f;
            var radius = barWidth * 0.5f;

            for (var i = 0; i < BarCount; i++)
            {
                var x = (i + 0.5f) * step;
                var barInfo = bars[i];
                var tapBarHeight = barInfo.Tap * height;
                var touchBarHeight = barInfo.Touch * height;
                var slideBarHeight = barInfo.Slide * height;

                //MajDebug.LogInfo(String.Format("Heights {0} {1} {2} Total:{3}", tapBarHeight, touchBarHeight, slideBarHeight, tapBarHeight + touchBarHeight + slideBarHeight));

                // Draw touch bar
                DrawRoundRectBar(
                        canvas,
                        x,
                        height,
                        touchBarHeight + slideBarHeight + tapBarHeight,
                        barWidth,
                        radius,
                        s_touchPaint);
                DrawRoundRectBar(
                        canvas,
                        x,
                        height ,
                        slideBarHeight + tapBarHeight,
                        barWidth,
                        radius,
                        s_slidePaint);
                DrawRoundRectBar(
                        canvas,
                        x,
                        height,
                        tapBarHeight,
                        barWidth,
                        radius,
                        s_tapPaint);
            }

            return surface.ToTexture2D(imageInfo);
        }
        static void DrawRoundRectBar(SKCanvas canvas,
                                     float centerX,
                                     float bottom,
                                     float barHeight,
                                     float width,
                                     float radius,
                                     SKPaint paint)
        {
            if (barHeight <= 0f)
            {
                return;
            }

            var rect = new SKRect(
                centerX - (width * 0.5f),
                bottom - barHeight,
                centerX + (width * 0.5f),
                bottom);

            // 四个角全部设置圆角
            s_radii![0] = new SKPoint(radius, radius); // 左上
            s_radii[1] = new SKPoint(radius, radius); // 右上
            s_radii[2] = new SKPoint(radius, radius); // 右下
            s_radii[3] = new SKPoint(radius, radius); // 左下

            s_roundRect!.SetRectRadii(rect, s_radii);

            canvas.DrawRoundRect(s_roundRect, paint);
        }
        static NativeArray<GraphBar> BuildBars(NativeArray<Vector2> tapPoints,
                                               NativeArray<Vector2> slidePoints,
                                               NativeArray<Vector2> touchPoints,
                                               int barCount)
        {
            var sampleCount = tapPoints.Length;

            var bars = new NativeArray<GraphBar>(barCount, Allocator.Temp);
            var max = 0f;

            for (var i = 0; i < barCount; i++)
            {
                var begin = i * sampleCount / barCount;
                var end = (i + 1) * sampleCount / barCount;

                var tap = 0f;
                var slide = 0f;
                var touch = 0f;

                for (var j = begin; j < end; j++)
                {
                    tap += tapPoints[j].y;
                    slide += slidePoints[j].y;
                    touch += touchPoints[j].y;
                }
                max = Mathf.Max(max, tap + slide + touch);

                bars[i] = new ()
                {
                    Tap = tap,
                    Slide = slide,
                    Touch = touch
                };
            }
            // normalize
            for (var i = 0; i < barCount; i++)
            {
                var bar = bars[i];
                bars[i] = new GraphBar
                {
                    Tap = bar.Tap / max,
                    Slide = bar.Slide / max,
                    Touch = bar.Touch / max
                };
                //MajDebug.LogInfo(String.Format("{0} {1} {2} {3}", bars[i].Tap, bars[i].Slide, bars[i].Touch, bars[i].Tap + bars[i].Slide + bars[i].Touch));
            }


            return bars;
        }
        static InternalMaidataAnalyzeResult AnalyzeMaidataCore(ReadOnlySpan<SimaiTimingPoint> data, float length)
        {
            var pointIndex = 0;
            var sampleCount = (int)(length / 0.5f);
            if(Mathf.Floor(length) > 0.5)
            {
                sampleCount += 1;
            }
            var tapPoints = new NativeArray<Vector2>(sampleCount, Allocator.TempJob);
            var slidePoints = new NativeArray<Vector2>(sampleCount, Allocator.TempJob);
            var touchPoints = new NativeArray<Vector2>(sampleCount, Allocator.TempJob);
            var max = 0f;
            var maxBPM = 0f;
            var minBPM = float.MaxValue;
            var y0 = 0f;
            var y1 = 0f;
            var y2 = 0f;
            var window = new Range<int>(0, 0, ContainsType.RightOpen);
            var tapYSum = 0f;
            var touchYSum = 0f;
            var slideYSum = 0f;
            for (float time = 0; time < length; time += 0.5f)
            {
                var windowStartTiming = time - 0.75f;
                var windowEndTiming = time + 0.75f;
                var rIndex = window.End;
                var lIndex = window.Start;
                for (; rIndex < data.Length; rIndex++)
                {
                    var timingPoint = data[rIndex];
                    if(timingPoint.Timing > windowEndTiming)
                    {
                        break;
                    }
                    maxBPM = Mathf.Max(maxBPM, timingPoint.Bpm);
                    minBPM = Mathf.Min(minBPM, timingPoint.Bpm);
                    AddSample(timingPoint.Notes, ref y0, ref y1, ref y2);
                }
                for (; lIndex < window.End; lIndex++)
                {
                    var timingPoint = data[lIndex];
                    if (timingPoint.Timing >= windowStartTiming)
                    {
                        break;
                    }
                    DelSample(timingPoint.Notes, ref y0, ref y1, ref y2);
                }
                window = new Range<int>(lIndex, rIndex, ContainsType.RightOpen);
                var sum = y0 + y1 + y2;
                max = Mathf.Max(sum, max);

                var x = time / length;
                tapPoints[pointIndex] = new Vector2(x, y0);
                slidePoints[pointIndex] = new Vector2(x, y1);
                touchPoints[pointIndex] = new Vector2(x, y2);
                tapYSum += y0;
                slideYSum += y1;
                touchYSum += y2;
                pointIndex++;
            }
            var tapYAvg = tapYSum / tapPoints.Length;
            var touchYAvg = touchYSum / touchPoints.Length;
            var slideYAvg = slideYSum / slidePoints.Length;
            var avg = tapYAvg + (3f * slideYAvg) + (0.5f * touchYAvg);
            var esti = 7.5f * Mathf.Log10(3.8f * (avg + (0.3f * max)));

            return new()
            {
                Average = avg,
                Esti = esti,
                Length = TimeSpan.FromSeconds(length),
                MaxBPM = maxBPM,
                MinBPM = minBPM,
                PeakDensity = max,

                TapPoints = tapPoints,
                TouchPoints = touchPoints,
                SlidePoints = slidePoints,
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void AddSample(ReadOnlySpan<SimaiNote> notes, ref float y0, ref float y1, ref float y2)
        {
            for (var i = 0; i < notes.Length; i++)
            {
                var note = notes[i];
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void DelSample(ReadOnlySpan<SimaiNote> notes, ref float y0, ref float y1, ref float y2)
        {
            for (var i = 0; i < notes.Length; i++)
            {
                var note = notes[i];
                switch (note.Type)
                {
                    case SimaiNoteType.Tap:
                    case SimaiNoteType.Hold:
                        y0--;
                        break;
                    case SimaiNoteType.Slide:
                        y1 -= 2;
                        break;
                    case SimaiNoteType.Touch:
                    case SimaiNoteType.TouchHold:
                        y2--;
                        break;
                }
            }
        }
        [MemberNotNull(nameof(s_tapPaint), nameof(s_slidePaint), nameof(s_touchPaint))]
        [MemberNotNull(nameof(s_tapPath), nameof(s_slidePath), nameof(s_touchPath))]
        [MemberNotNull(nameof(s_radii), nameof(s_roundRect))]
        static void EnsureSakaComponentIsInited()
        {
            if(s_tapPaint is null)
            {
                s_tapPaint = new();
                s_tapPaint.Color = TapColor.ToSkColor();
                s_tapPaint.IsAntialias = true;
                s_tapPaint.Style = SKPaintStyle.Fill;
            }
            if(s_slidePaint is null)
            {
                s_slidePaint = new();
                s_slidePaint.Color = SlideColor.ToSkColor();
                s_slidePaint.IsAntialias = true;
                s_slidePaint.Style = SKPaintStyle.Fill;
            }
            if(s_touchPaint is null)
            {
                s_touchPaint = new();
                s_touchPaint.Color = TouchColor.ToSkColor();
                s_touchPaint.IsAntialias = true;
                s_touchPaint.Style = SKPaintStyle.Fill;
            }
            if(s_tapPath is null)
            {
                s_tapPath = new();
            }
            else
            {
                s_tapPath.Rewind();
            }
            if (s_touchPath is null)
            {
                s_touchPath = new();
            }
            else
            {
                s_touchPath.Rewind();
            }
            if (s_slidePath is null)
            {
                s_slidePath = new();
            }
            else
            {
                s_slidePath.Rewind();
            }

            if(s_radii is null)
            {
                s_radii = new SKPoint[4]
                {
                    SKPoint.Empty,
                    SKPoint.Empty,
                    SKPoint.Empty,
                    SKPoint.Empty
                };
            }
            else
            {
                s_radii[0] = SKPoint.Empty;
                s_radii[1] = SKPoint.Empty;
                s_radii[2] = SKPoint.Empty;
                s_radii[3] = SKPoint.Empty;
            }
            if(s_roundRect is null)
            {
                s_roundRect = new();
            }
            else
            {
                s_roundRect.SetEmpty();
            }
        }
        struct InternalMaidataAnalyzeResult
        {
            public float PeakDensity { get; init; }
            public float Esti { get; init; }
            public float Average { get; init; }
            public TimeSpan Length { get; init; }
            public float MaxBPM { get; init; }
            public float MinBPM { get; init; }            

            public NativeArray<Vector2> TapPoints { get; init; }
            public NativeArray<Vector2> TouchPoints { get; init; }
            public NativeArray<Vector2> SlidePoints { get; init; }
        }
        readonly struct GraphBar
        {
            public float Tap { get; init; }
            public float Slide { get; init; }
            public float Touch { get; init; }
        }

        [BurstCompile]
        struct SampleNormalizeJob : IJobParallelFor
        {
            public NativeArray<Vector2> TapPoints;
            public NativeArray<Vector2> SlidePoints;
            public NativeArray<Vector2> TouchPoints;

            [ReadOnly]
            public float Max;

            public void Execute(int i)
            {
                var invMax = 1f / Max;
                var tapPoint = TapPoints[i];
                var slidePoint = SlidePoints[i];
                var touchPoint = TouchPoints[i];

                TapPoints[i] = new Vector2(tapPoint.x, tapPoint.y * invMax);
                SlidePoints[i] = new Vector2(slidePoint.x, slidePoint.y * invMax);
                TouchPoints[i] = new Vector2(touchPoint.x, touchPoint.y * invMax);
            }
        }
    }
}
