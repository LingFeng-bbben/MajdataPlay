using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MajdataPlay.Types;
using MajSimaiDecode;
using SkiaSharp;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay.Game
{
    public class ChartAnalyzer : MonoBehaviour
    {
        RawImage _rawImage;
        public UnityEngine.Color tapColor;
        public UnityEngine.Color slideColor;
        public UnityEngine.Color touchColor;

        public Text anaText;
        void Start()
        {
            _rawImage = GetComponent<RawImage>();
        }
        bool lockFlag = false;
        public async UniTask AnalyzeSongDetail(SongDetail songDetail, ChartLevel level)
        {
            if (lockFlag) return;
            lockFlag = true;
            try
            {
                var maidata = await songDetail.GetInnerMaidata((int)level);
                var chart = new SimaiProcess(maidata);
                var lastnoteTiming = chart.notelist.Last().time;
                AnalyzeMaidata(chart, (float)lastnoteTiming);
            }
            catch
            {
                _rawImage.texture = new Texture2D(0, 0);
                if (anaText is not null)
                {
                    anaText.text = "";
                }
            }
            finally { lockFlag = false; }
        }

        public void AnalyzeMaidata(SimaiProcess data, float totalLength)
        {
            var tapPoints = new List<Vector2>();
            var slidePoints = new List<Vector2>();
            var touchPoints = new List<Vector2>();
            var max = 0f;
            for (float time = 0; time < totalLength; time += 0.5f)
            {
                var timingPoints = data.notelist.FindAll(o => o.time > time - 0.75f && o.time <= time + 0.75f).ToList();
                float y0 = 0, y1 = 0, y2 = 0;
                foreach (var timingPoint in timingPoints)
                {
                    foreach (var note in timingPoint.noteList)
                    {
                        if (note.noteType == SimaiNoteType.Tap || note.noteType == SimaiNoteType.Hold)
                        {
                            y0++;
                        }
                        else if (note.noteType == SimaiNoteType.Slide)
                        {
                            y1 += 2;
                        }
                        else if (note.noteType == SimaiNoteType.Touch || note.noteType == SimaiNoteType.TouchHold)
                        {
                            y2++;
                        }
                    }

                }
                if (y0 + y1 + y2 > max) max = y0 + y1 + y2;

                var x = time / totalLength;
                tapPoints.Add(new Vector2(x, y0));
                slidePoints.Add(new Vector2(x, y1));
                touchPoints.Add(new Vector2(x, y2));
            }
            if (anaText is not null)
            {
                var time = TimeSpan.FromSeconds(totalLength);
                anaText.text = "Peak Density = " + max +"\n";
                var avg = tapPoints.Average(o => o.y) + 3f * slidePoints.Average(o => o.y) + 0.5f * touchPoints.Average(o => o.y);
                var esti = 7.5f * Mathf.Log10(3.8f*(avg + 0.3f * max));
                anaText.text += "Esti = Lv." + (esti) + "\n";
                anaText.text += "Length = " + ZString.Format("{0}:{1:00}.{2:000}", time.Minutes, time.Seconds, time.Milliseconds) + "\n";
            }
            //normalize
            for (var i = 0; i < tapPoints.Count; i++)
            {
                tapPoints[i] = new Vector2(tapPoints[i].x, tapPoints[i].y / max);
                slidePoints[i] = new Vector2(slidePoints[i].x, slidePoints[i].y / max);
                touchPoints[i] = new Vector2(touchPoints[i].x, touchPoints[i].y / max);
            }
            DrawGraph(tapPoints, slidePoints, touchPoints);

        }

        public void DrawGraph(List<Vector2> tapPoints, List<Vector2> slidePoints, List<Vector2> touchPoints)
        {
            var width = 1018;
            var height = 187;
            var imageInfo = new SKImageInfo(width, height);
            using (var surface = SKSurface.Create(imageInfo))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColor.Empty);
                using (var tapPaint = new SKPaint())
                using (var slidePaint = new SKPaint())
                using (var touchPaint = new SKPaint())
                {
                    tapPaint.Color = ToSkColor(tapColor);
                    tapPaint.IsAntialias = true;
                    tapPaint.Style = SKPaintStyle.Fill;
                    slidePaint.Color = ToSkColor(slideColor);
                    slidePaint.IsAntialias = true;
                    slidePaint.Style = SKPaintStyle.Fill;
                    touchPaint.Color = ToSkColor(touchColor);
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

                    _rawImage.texture = tex0;
                }
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