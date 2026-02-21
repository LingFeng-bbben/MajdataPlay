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
using MajdataPlay.Drawing;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using MajdataPlay.Buffers;
#nullable enable
namespace MajdataPlay.Scenes.Game
{
    public class ChartAnalyzer : MonoBehaviour
    {
        
        public UnityEngine.Color tapColor;
        public UnityEngine.Color slideColor;
        public UnityEngine.Color touchColor;

        static UnityEngine.Color _colorA;
        static UnityEngine.Color _colorB;
        static UnityEngine.Color _colorC;

        public Text? anaText;

        [SerializeField]
        GameObject? _iconPrefab;

        GameObject? _loadingPrefab;
        GameObject? _errorIcon;
        GameObject? _helpIcon;

        RawImage _rawImage;
        static Texture2D? _emptyTexture;

#if UNITY_ANDROID || UNITY_IOS
        const int MAX_CACHE_COUNT = 16;
#else
        const int MAX_CACHE_COUNT = 32;
#endif
        readonly AsyncLock _lock = new();
        readonly Dictionary<SimaiChart, WeakReference<MaidataAnalyzeResult>> _cachedTextures = new(MAX_CACHE_COUNT);
        readonly RentedList<SimaiChart> _cachedMaiCharts = new(MAX_CACHE_COUNT);

        private void Awake()
        {
            Majdata<ChartAnalyzer>.Instance = this;

            if(_iconPrefab != null)
            {
                var iconTransform = _iconPrefab.transform;
                _loadingPrefab = iconTransform.GetChild(0).gameObject;
                _helpIcon = iconTransform.GetChild(1).gameObject;
                _errorIcon = iconTransform.GetChild(2).gameObject;
            }
            _rawImage = GetComponent<RawImage>();
            if(_emptyTexture is null)
            {
                _emptyTexture = new Texture2D(0, 0);
            }
        }

        void Start()
        {
            _colorA = tapColor;
            _colorB = slideColor;
            _colorC = touchColor;
        }
        void OnDestroy()
        {
            Majdata<ChartAnalyzer>.Free();
            foreach(var (k, v) in _cachedTextures)
            {
                if(v.TryGetTarget(out var result))
                {
                    UnityEngine.Object.DestroyImmediate(result.LineGraph, true);
                    v.SetTarget(null!);
                }
            }
            _cachedMaiCharts.Clear();
            _cachedTextures.Clear();
        }
        public async UniTask AnalyzeAndDrawGraphAsync(ISongDetail songDetail, ChartLevel level, float length = -1, bool noCache = false, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext(cancellationToken: token))
            {
                await UniTask.SwitchToThreadPool();
                using (await _lock.LockAsync(token))
                {
                    try
                    {
                        await UniTask.SwitchToMainThread(token);
                        SetLoading();
                        await UniTask.SwitchToThreadPool();
                        var simaiFile = await songDetail.GetMaidataAsync(token: token);
                        var maiChart = simaiFile.Charts[(int)level];
                        if (maiChart.IsEmpty)
                        {
                            await UniTask.SwitchToMainThread(token);
                            SetHelp();
                            return;
                        }
                        double lastnoteTiming;
                        using (var noteTimings = new RentedList<SimaiTimingPoint>())
                        {
                            noteTimings.AddRange(maiChart.NoteTimings);
                            lastnoteTiming = length == -1 ? noteTimings.LastOrDefault()?.Timing ?? length : length;
                        }
                        var result = await AnalyzeMaidataAsync(maiChart, (float)lastnoteTiming, noCache);
                        await UniTask.SwitchToMainThread(token);
                        token.ThrowIfCancellationRequested();
                        if (_cachedTextures.Count == MAX_CACHE_COUNT)
                        {
                            var c = _cachedMaiCharts[0];
                            var @ref = _cachedTextures[c];
                            _cachedTextures.Remove(c);
                            _cachedMaiCharts.RemoveAt(0);
                            if(@ref.TryGetTarget(out var r))
                            {
                                var tex = r.LineGraph;
                                if(tex.IsNativeAlive())
                                {
                                    UnityEngine.Object.DestroyImmediate(tex, true);
                                }
                            }
                        }
                        if(!_cachedTextures.ContainsKey(maiChart))
                        {
                            _cachedTextures.Add(maiChart, new(result));
                            _cachedMaiCharts.Add(maiChart);
                        }
                        SetTexture(result.LineGraph);
                        if (anaText is not null)
                        {
                            var max = result.PeakDensity;
                            var esti = result.Esti;
                            var minBPM = result.MinBPM;
                            var maxBPM = result.MaxBPM;
                            var time = result.Length;
                            using var sb = ZString.CreateStringBuilder();
                            sb.Append("Peak Density = "); sb.Append(max);
                            sb.AppendLine();
                            sb.Append("Esti = Lv."); sb.Append(esti);
                            sb.AppendLine();
                            sb.Append("Length = "); sb.AppendFormat("{0}:{1:00}.{2:000}", time.Minutes, time.Seconds, time.Milliseconds);
                            sb.AppendLine();

                            if (minBPM == maxBPM)
                            {
                                sb.Append("BPM = "); sb.Append(minBPM);
                            }
                            else
                            {
                                sb.Append("BPM = "); sb.Append(minBPM); sb.Append(" - "); sb.Append(maxBPM);
                            }
                            //anaText.text = "Peak Density = " + max + "\n";
                            //anaText.text += "Esti = Lv." + (esti) + "\n";
                            //anaText.text += "Length = " + ZString.Format("{0}:{1:00}.{2:000}", time.Minutes, time.Seconds, time.Milliseconds) + "\n";
                            //anaText.text += "BPM = " + minBPM + " - " + maxBPM;
                            anaText.text = sb.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is not OperationCanceledException)
                        {
                            MajDebug.LogException(ex);
                        }
                        await UniTask.SwitchToMainThread(token);
                        SetError();
                        //_rawImage.texture = new Texture2D(0, 0);
                        if (anaText is not null)
                        {
                            anaText.text = string.Empty;
                        }
                    }
                    finally
                    {
                        await UniTask.SwitchToMainThread();
                    }
                }
            }
        }
        public async UniTask AnalyzeAndDrawGraphAsync(SimaiChart data, float totalLength, CancellationToken token = default)
        {
            try
            {
                await UniTask.SwitchToMainThread();
                SetLoading();
                if (data.IsEmpty)
                {
                    await UniTask.SwitchToMainThread();
                    SetHelp();
                    return;
                }
                await UniTask.SwitchToThreadPool();
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
                    using var sb = ZString.CreateStringBuilder();
                    sb.Append("Peak Density = "); sb.Append(max);
                    sb.AppendLine();
                    sb.Append("Esti = Lv."); sb.Append(esti);
                    sb.AppendLine();
                    sb.Append("Length = "); sb.AppendFormat("{0}:{1:00}.{2:000}", time.Minutes, time.Seconds, time.Milliseconds);
                    sb.AppendLine();

                    if (minBPM == maxBPM)
                    {
                        sb.Append("BPM = "); sb.Append(minBPM);
                    }
                    else
                    {
                        sb.Append("BPM = "); sb.Append(minBPM); sb.Append(" - "); sb.Append(maxBPM);
                    }
                    //anaText.text = "Peak Density = " + max + "\n";
                    //anaText.text += "Esti = Lv." + (esti) + "\n";
                    //anaText.text += "Length = " + ZString.Format("{0}:{1:00}.{2:000}", time.Minutes, time.Seconds, time.Milliseconds) + "\n";
                    //anaText.text += "BPM = " + minBPM + " - " + maxBPM;
                    anaText.text = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                MajDebug.LogException(ex);
                await UniTask.SwitchToMainThread();
                SetError();
                _rawImage.texture = _emptyTexture!;
                if (anaText is not null)
                {
                    anaText.text = "";
                }
            }
        }

        void SetLoading()
        {
            _errorIcon?.SetActive(false);
            _helpIcon?.SetActive(false);
            _loadingPrefab?.SetActive(true);
            _rawImage.texture = _emptyTexture!;
        }
        void SetHelp()
        {
            _errorIcon?.SetActive(false);
            _helpIcon?.SetActive(true);
            _loadingPrefab?.SetActive(false);
            _rawImage.texture = _emptyTexture!;
        }
        void SetError()
        {
            _errorIcon?.SetActive(true);
            _helpIcon?.SetActive(false);
            _loadingPrefab?.SetActive(false);
            _rawImage.texture = _emptyTexture!;
        }
        void SetTexture(Texture texture)
        {
            _errorIcon?.SetActive(false);
            _helpIcon?.SetActive(false);
            _loadingPrefab?.SetActive(false);
            _rawImage.texture = texture;
        }
        async UniTask<MaidataAnalyzeResult> AnalyzeMaidataAsync(SimaiChart data, float totalLength, bool noCache = false)
        {
            await UniTask.SwitchToThreadPool();
            if (!noCache)
            {
                if(_cachedTextures.TryGetValue(data, out var @ref) && @ref.TryGetTarget(out var r))
                {
                    return r;
                }
            }

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
            for (float time = 0; time < totalLength; time += 0.5f)
            {
                var timingPoints = noteTimings.Where(o => o.Timing > time - 0.75f && o.Timing <= time + 0.75f);
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
                maxBPM = noteTimings.Max(o => o.Bpm);
                minBPM = noteTimings.Min(o => o.Bpm);
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