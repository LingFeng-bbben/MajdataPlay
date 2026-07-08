using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
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

        readonly static Color TapColor = new Color(1f, 0.490566f, 0.7993075f);
        readonly static Color TouchColor = new Color(1f, 0.9354098f, 0.5707547f);
        readonly static Color SlideColor = new Color(0.5330188f, 0.7586297f, 1f);

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
        static Texture DrawGraph(InternalMaidataAnalyzeResult analyzeResult,
                                 int height,
                                 int width)
        {
            EnsureSakaComponentIsInited();
            var tapPoints = analyzeResult.TapPoints;
            var slidePoints = analyzeResult.SlidePoints;
            var touchPoints = analyzeResult.TouchPoints;
            var normalizeJob = new SampleNormalizeJob()
            {
                TapPoints = tapPoints,
                SlidePoints = slidePoints,
                TouchPoints = touchPoints,
                Max = analyzeResult.PeakDensity
            };
            normalizeJob.Schedule(tapPoints.Length, 64)
                        .Complete();
            var imageInfo = new SKImageInfo(width, height);
            using var surface = SKSurface.Create(imageInfo);
            var canvas = surface.Canvas;
            canvas.Clear(SKColor.Empty);

            s_tapPath.MoveTo(0, height);
            s_slidePath.MoveTo(0, height);
            s_touchPath.MoveTo(0, height);
            for (var i = 0; i < tapPoints.Length; i++)
            {
                var x = tapPoints[i].x * width;
                var y = tapPoints[i].y;
                s_tapPath.LineTo(x, (1 - y) * height);
                y += slidePoints[i].y;
                s_slidePath.LineTo(x, (1 - y) * height);
                y += touchPoints[i].y;
                s_touchPath.LineTo(x, (1 - y) * height);
            }
            s_tapPath.LineTo(width, height);
            s_slidePath.LineTo(width, height);
            s_touchPath.LineTo(width, height);

            canvas.DrawPath(s_touchPath, s_touchPaint);
            canvas.DrawPath(s_slidePath, s_slidePaint);
            canvas.DrawPath(s_tapPath, s_tapPaint);

            return GraphHelper.GraphSnapshot(surface);
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
