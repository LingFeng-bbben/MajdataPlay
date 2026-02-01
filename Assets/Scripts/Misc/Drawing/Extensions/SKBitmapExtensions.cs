using Cysharp.Threading.Tasks;
using MajdataPlay.Utils;
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
    internal static class SKBitmapExtensions
    {
        public static async Task<Texture2D> ToTexture2DAsync(this SKBitmap bitmap, int width = 0, int height = 0, SKSamplingOptions? options = null)
        {
            await UniTask.SwitchToThreadPool();
            var resize = width != 0 || height != 0;

            width = width == 0 ? bitmap.Width : width;
            height = height == 0 ? bitmap.Height : height;

            if (resize)
            {
                bitmap = bitmap.Resize(new SKSizeI(width, height), options ?? SKSamplingOptions.Default);
            }

            Texture2D texture2D;
            var l = bitmap.ColorType.TryConvertToTextureFormat(out TextureFormat textureFormat);
            if (l > 0)
            {
                var writer = new ArrayBufferWriter<byte>(width * height * l);

                for (var i = height - 1; i >= 0; i--)
                {
                    writer.Write(bitmap.GetPixelSpan().Slice(i * width * l, width * l));
                }

                await UniTask.SwitchToMainThread();
                texture2D = new Texture2D(width, height, textureFormat, false);
                texture2D.SetPixelData(writer.WrittenSpan.ToArray(), 0);
                texture2D.Apply();
                await UniTask.SwitchToThreadPool();
            }
            else
            {
                var data = bitmap.Pixels;
                var writer = new ArrayBufferWriter<SKColor>();

                for (var i = height - 1; i >= 0; i--)
                {
                    writer.Write(data.AsSpan().Slice(i * width, width));
                }

                var data1 = writer.WrittenSpan.AsNativeArray();
                var colors = SKColorConverter.ConvertToColor32(data1, width * 64);
                var pixels32 = colors.ToArray();

                await UniTask.SwitchToMainThread();
                texture2D = new Texture2D(width, height, textureFormat, false);
                texture2D.SetPixels32(pixels32);
                texture2D.Apply();
                await UniTask.SwitchToThreadPool();

                data1.Dispose();
                colors.Dispose();
            }
            return texture2D;
        }
        static int TryConvertToTextureFormat(this SKColorType skColorType, out TextureFormat textureFormat)
        {
            switch (skColorType)
            {
                case SKColorType.Alpha8:
                    textureFormat = TextureFormat.Alpha8;
                    return 1;
                case SKColorType.Rgb565:
                    textureFormat = TextureFormat.RGB565;
                    return 2;
                case SKColorType.Rgba8888:
                case SKColorType.Rgb888x:
                    textureFormat = TextureFormat.RGBA32;
                    return 4;
                case SKColorType.Bgra8888:
                    textureFormat = TextureFormat.BGRA32;
                    return 4;
                case SKColorType.RgbaF16:
                case SKColorType.RgbaF16Clamped:
                    textureFormat = TextureFormat.RGBAHalf;
                    return 8;
                case SKColorType.RgbaF32:
                    textureFormat = TextureFormat.RGBAFloat;
                    return 16;
                case SKColorType.Rg88:
                    textureFormat = TextureFormat.RG16;
                    return 2;
                case SKColorType.RgF16:
                    textureFormat = TextureFormat.RGHalf;
                    return 4;
                case SKColorType.Rg1616:
                    textureFormat = TextureFormat.RG32;
                    return 4;
                case SKColorType.Rgba16161616:
                    textureFormat = TextureFormat.RGBA64;
                    return 8;
                case SKColorType.Gray8:
                    textureFormat = TextureFormat.RGBA32;
                    break;
                case SKColorType.AlphaF16:
                    textureFormat = TextureFormat.RGBAHalf;
                    break;
                case SKColorType.Alpha16:
                case SKColorType.Rgba1010102:
                case SKColorType.Rgb101010x:
                case SKColorType.Bgra1010102:
                case SKColorType.Bgr101010x:
                    textureFormat = TextureFormat.RGBA64;
                    break;
                default:
                    textureFormat = TextureFormat.RGBA32;
                    break;
            }

            return -1;
        }
    }
}
