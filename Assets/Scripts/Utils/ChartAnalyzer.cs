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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Utils
{
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

        readonly static Color TapColor = new Color();
        readonly static Color TouchColor = new Color();
        readonly static Color SlideColor = new Color();

        public static MaidataAnalyzeResult AnalyzeMaidata(SimaiChart data, CancellationToken token = default)
        {
            if(data.NoteTimings.IsEmpty)
            {
                return default;
            }
            var noteTimings = data.NoteTimings;
            var length = (float)noteTimings[noteTimings.Length - 1].Timing;
            var result = AnalyzeMaidataCore(noteTimings, length);

            result.TapPoints.Dispose();
            result.TouchPoints.Dispose();
            result.SlidePoints.Dispose();
            return new MaidataAnalyzeResult()
            {
                Esti = result.Esti,
                Length = result.Length,
                MaxBPM = result.MaxBPM,
                MinBPM = result.MinBPM,
                PeakDensity = result.PeakDensity,
            };
        }
        static Texture DrawGraph(NativeArray<Vector2> tapPoints,
                                 NativeArray<Vector2> slidePoints,
                                 NativeArray<Vector2> touchPoints)
        {
            EnsureSakaComponentIsInited();
            var width = 1018;
            var height = 187;
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
            var tapPoints = new NativeArray<Vector2>((int)(length / 0.5f), Allocator.Temp);
            var slidePoints = new NativeArray<Vector2>((int)(length / 0.5f), Allocator.Temp);
            var touchPoints = new NativeArray<Vector2>((int)(length / 0.5f), Allocator.Temp);
            var max = 0f;
            var maxBPM = 0f;
            var minBPM = float.MaxValue;
            var esti = 0f;
            var y0 = 0f;
            var y1 = 0f;
            var y2 = 0f;
            var window = new Range<int>(0, 0, ContainsType.RightOpen);
            for (float time = 0; time < length; time += 0.5f)
            {
                var windowStartTiming = time - 0.75f;
                var windowEndTiming = time + 0.75f;
                for (var rIndex = 0; rIndex < data.Length; rIndex++)
                {
                    var timingPoint = data[rIndex];
                    if(timingPoint.Timing > windowEndTiming)
                    {
                        window = new Range<int>(window.Start, rIndex, ContainsType.RightOpen);
                        break;
                    }
                    maxBPM = Mathf.Max(maxBPM, timingPoint.Bpm);
                    minBPM = Mathf.Min(minBPM, timingPoint.Bpm);
                    AddSample(timingPoint.Notes, ref y0, ref y1, ref y2);
                }
                for (var lIndex = 0; lIndex < window.End; lIndex++)
                {
                    var timingPoint = data[lIndex];
                    if (timingPoint.Timing >= windowStartTiming)
                    {
                        window = new Range<int>(lIndex, window.End, ContainsType.RightOpen);
                        break;
                    }
                    DelSample(timingPoint.Notes, ref y0, ref y1, ref y2);
                }
                var sum = y0 + y1 + y2;
                max = Mathf.Max(sum, max);

                var x = time / length;
                tapPoints[pointIndex] = new Vector2(x, y0);
                slidePoints[pointIndex] = new Vector2(x, y1);
                touchPoints[pointIndex] = new Vector2(x, y2);
                pointIndex++;
            }

            return new()
            {
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
            public TimeSpan Length { get; init; }
            public float MaxBPM { get; init; }
            public float MinBPM { get; init; }

            public NativeArray<Vector2> TapPoints { get; init; }
            public NativeArray<Vector2> TouchPoints { get; init; }
            public NativeArray<Vector2> SlidePoints { get; init; }
        }
    }
}
