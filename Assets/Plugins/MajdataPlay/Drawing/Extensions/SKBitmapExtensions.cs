using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using SkiaSharp;
using SkiaSharp.Unity;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MajdataPlay.Drawing
{
    public static class SKBitmapExtensions
    {
        public static Texture2D ToTexture2D(this SKBitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;

            using var converted = new SKBitmap();

            var info = new SKImageInfo(
                width,
                height,
                SKColorType.Rgba8888,
                SKAlphaType.Premul
            );

            if (!converted.TryAllocPixels(info))
            {
                throw new Exception("Failed to allocate SKBitmap.");
            }

            bitmap.CopyTo(converted, SKColorType.Rgba8888);

            var texture = new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                true
            );

            UploadBitmap(texture, converted);

            texture.Apply(true, false);

            return texture;
        }

        private unsafe static void UploadBitmap(Texture2D texture, SKBitmap bitmap)
        {
            const int BYTES_PE_PIXELS = 4;

            var width = bitmap.Width;
            var height = bitmap.Height;            

            var srcRowBytes = bitmap.RowBytes;
            var dstRowBytes = width * BYTES_PE_PIXELS;

            var size = dstRowBytes * height;
            using var raw = new NativeArray<byte>(size, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            var src = (byte*)bitmap.GetPixels().ToPointer();
            var dst = (byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(raw);

            for (int y = 0; y < height; y++)
            {
                UnsafeUtility.MemCpy(
                    dst + ((height - 1 - y) * dstRowBytes),
                    src + (y * srcRowBytes),
                    dstRowBytes
                );
            }

            texture.LoadRawTextureData(raw);
        }
    }
}
