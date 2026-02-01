using SkiaSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Drawing
{
    internal static class SKColorExtensions
    {
        public static Color32 ToUnityColor32(this SKColor skColor)
        {
            return new Color32(skColor.Red, skColor.Green, skColor.Blue, skColor.Alpha);
        }
        public static Color ToUnityColor(this SKColorF skColorF)
        {
            return new Color(skColorF.Red, skColorF.Green, skColorF.Blue, skColorF.Alpha);
        }
        public static Color32[] ToUnityColor32(this ReadOnlySpan<SKColor> skColors)
        {
            var skColorBuffer = ArrayPool<SKColor>.Shared.Rent(skColors.Length);
            try
            {
                skColors.CopyTo(skColorBuffer);

                return ToUnityColor32(skColorBuffer.AsMemory().Slice(0, skColors.Length));
            }
            finally
            {
                ArrayPool<SKColor>.Shared.Return(skColorBuffer);
            }
        }
        public static Color32[] ToUnityColor32(this ReadOnlyMemory<SKColor> skColors)
        {
            var buffer = new Color32[skColors.Length];
            var loopResult = Parallel.For(0, buffer.Length, i =>
            {
                var skColor = skColors.Span[i];
                buffer[i] = new Color32(skColor.Red, skColor.Green, skColor.Blue, skColor.Alpha);
            });
            return buffer;
        }
    }
}
