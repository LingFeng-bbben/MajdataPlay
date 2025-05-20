using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay
{
    internal static class SKColorExtensions
    {
        public static Color32 ToUnityColor32(this SKColor skColor)
        {
            return new Color32(skColor.Red, skColor.Green, skColor.Blue, skColor.Alpha);
        }
    }
}
