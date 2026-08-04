using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Drawing
{
    public static class SKSurfaceExtensions
    {
        public static Texture2D ToTexture2D(this SKSurface surface, SKImageInfo imageInfo)
        {
            using var bitmap = new SKBitmap(imageInfo);

            surface.ReadPixels(
                            bitmap.Info,
                            bitmap.GetPixels(),
                            bitmap.RowBytes,
                            0,
                            0);

            return bitmap.ToTexture2D();
        }
    }
}
