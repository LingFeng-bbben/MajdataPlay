using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Drawing
{
    internal static class Color32Extensions
    {
        public static SKColor ToSkColor(this Color32 color32)
        {
            return new SKColor(color32.r, color32.g, color32.b, color32.a);
        }
        public static SKColor ToSkColor(this Color color)
        {
            var color32 = (Color32)color;
            return new SKColor(color32.r, color32.g, color32.b, color32.a);
        }
        public static SKColorF ToSkColorF(this Color color)
        {
            return new SKColorF(color.r, color.g, color.b, color.a);
        }
    }
}
