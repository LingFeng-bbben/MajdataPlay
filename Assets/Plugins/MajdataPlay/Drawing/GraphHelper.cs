using SkiaSharp;
using System;
using System.Buffers;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace MajdataPlay.Drawing
{
    [BurstCompile]
    public static class GraphHelper
    {
        public unsafe static Texture2D GraphSnapshot(SKSurface surface)
        {
            //sort it into rawimage
            using var image = surface.Snapshot();
            using var bitmap = SKBitmap.FromImage(image);
            var pixelCount = bitmap.Width * bitmap.Height;
            var skcolors = bitmap.Pixels.AsSpan();
            using var dst = new NativeArray<Color32>(bitmap.Width * bitmap.Height, Allocator.TempJob);
            fixed (SKColor* unsafePixelData = &MemoryMarshal.GetReference(skcolors))
            {
                var pixelData = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<SKColor>((void*)unsafePixelData, skcolors.Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                var safety = AtomicSafetyHandle.Create();
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref pixelData, safety);
#endif
                var job = new ConvertBitmapJob
                {
                    Source = pixelData,
                    Destination = dst,
                    Width = bitmap.Width,
                    Height = bitmap.Height
                };
                job.Schedule(pixelCount, 256)
                   .Complete();
            }

            var tex0 = new Texture2D(bitmap.Width, bitmap.Height);
            tex0.SetPixelData(dst, 0);
            tex0.Apply();

            return tex0;
        }
        [BurstCompile]
        internal struct ConvertBitmapJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<SKColor> Source;

            [WriteOnly]
            public NativeArray<Color32> Destination;

            public int Width;
            public int Height;

            public void Execute(int index)
            {
                var x = index % Width;
                var y = index / Width;

                var srcIndex = ((Height - 1 - y) * Width) + x;

                var c = Source[srcIndex];

                Destination[index] = new Color32(
                    c.Red,
                    c.Green,
                    c.Blue,
                    c.Alpha);
            }
        }
    }
}
