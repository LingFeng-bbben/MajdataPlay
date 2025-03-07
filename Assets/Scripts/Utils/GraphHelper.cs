using MajdataPlay.Extensions;
using SkiaSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Utils
{
    internal static class GraphHelper
    {
        public static Texture GraphSnapshot(SKSurface surface)
        {
            //sort it into rawimage
            using (var image = surface.Snapshot())
            {
                var bitmap = SKBitmap.FromImage(image);
                var skcolors = bitmap.Pixels.AsSpan();
                var writer = new ArrayBufferWriter<SKColor>(bitmap.Width * bitmap.Height);
                for (var i = bitmap.Height - 1; i >= 0; i--) writer.Write(skcolors.Slice(i * bitmap.Width, bitmap.Width));
                var colors = writer.WrittenSpan.ToArray().AsParallel().AsOrdered().Select(s => s.ToUnityColor32()).ToArray();

                var tex0 = new Texture2D(bitmap.Width, bitmap.Height);
                tex0.SetPixels32(colors);
                tex0.Apply();

                return tex0;
            }
        }
    }
}
