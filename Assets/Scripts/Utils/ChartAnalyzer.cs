using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajSimai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Utils
{
    internal static class ChartAnalyzer
    {
        public static async UniTask<MaidataAnalyzeResult> AnalyzeMaidataAsync(SimaiChart data, CancellationToken token = default)
        {
            await UniTask.SwitchToThreadPool();
            var tapPoints = new List<Vector2>();
            var slidePoints = new List<Vector2>();
            var touchPoints = new List<Vector2>();
            var max = 0f;
            var maxBPM = 0f;
            var minBPM = 0f;
            var length = TimeSpan.Zero;
            var esti = 0f;
            using var noteTimings = new RentedList<SimaiTimingPoint>();
            noteTimings.AddRange(data.NoteTimings);
            var totalLength = (float)(noteTimings.LastOrDefault()?.Timing ?? 0f);
            if (noteTimings.Count > 0)
            {
                maxBPM = noteTimings.Max(o => o.Bpm);
                minBPM = noteTimings.Min(o => o.Bpm);
            }
            for (float time = 0; time < totalLength; time += 0.5f)
            {
                token.ThrowIfCancellationRequested();
                var timingPoints = noteTimings.Where(o => o.Timing > time - 0.75f && o.Timing <= time + 0.75f);
                float y0 = 0, y1 = 0, y2 = 0;
                foreach (var timingPoint in timingPoints)
                {
                    token.ThrowIfCancellationRequested();
                    foreach (var note in timingPoint.Notes)
                    {
                        token.ThrowIfCancellationRequested();
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
                if (y0 + y1 + y2 > max)
                {
                    max = y0 + y1 + y2;
                }

                var x = time / totalLength;
                tapPoints.Add(new Vector2(x, y0));
                slidePoints.Add(new Vector2(x, y1));
                touchPoints.Add(new Vector2(x, y2));
            }


            var avg = tapPoints.Average(o => o.y) + 3f * slidePoints.Average(o => o.y) + 0.5f * touchPoints.Average(o => o.y);
            length = TimeSpan.FromSeconds(totalLength);
            esti = 7.5f * Mathf.Log10(3.8f * (avg + 0.3f * max));


            return new MaidataAnalyzeResult()
            {
                Esti = esti,
                Length = length,
                MaxBPM = maxBPM,
                MinBPM = minBPM,
                PeakDensity = max,
            };
        }
    }
}
