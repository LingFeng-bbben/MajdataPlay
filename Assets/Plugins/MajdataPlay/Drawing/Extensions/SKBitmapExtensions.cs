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
using UnityEngine.Experimental.Rendering;

namespace MajdataPlay.Drawing
{
    public static class SKBitmapExtensions
    {
        const int BYTES_PER_PIXEL = 4;
        const GraphicsFormat GFX_FORMAT = GraphicsFormat.R8G8B8A8_UNorm;
        public async static UniTask<Texture2D> ToTexture2D(this SKBitmap bitmap)
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

            
            int mipCount = texture.mipmapCount;

            // 1) 计算所有 mip 层总字节数
            long totalSize = 0;
            int w = width, h = height;
            for (int mip = 0; mip < mipCount; mip++)
            {
                // 使用你之前的 ComputeMipmapSize(width, height, gfxFormat) 形式
                uint levelSize = GraphicsFormatUtility.ComputeMipmapSize(w, h, GFX_FORMAT);
                totalSize += levelSize;
                w = Math.Max(1, w / 2);
                h = Math.Max(1, h / 2);
            }

            if (totalSize > int.MaxValue) throw new Exception("Texture too large for single buffer.");
            int totalBytes = (int)totalSize;

            // 2) 分配 raw 缓冲区
            byte[] raw = new byte[totalBytes];

            await UniTask.SwitchToThreadPool();
            GenerateMipMaps(texture, converted, ref raw, mipCount);
            
            await UniTask.SwitchToMainThread();
            texture.LoadRawTextureData(raw);
            texture.Apply(true, false);

            return texture;
        }

        static unsafe void WriteSKBitmapToRaw(SKBitmap srcBitmap, ref byte[]raw, int levelWidth, int levelHeight, int offset)
        {
            int dstRowBytes = levelWidth * BYTES_PER_PIXEL;
            byte* srcPtr = (byte*)srcBitmap.GetPixels().ToPointer();
            int srcRowBytes = srcBitmap.RowBytes;

            // 我们把 Skia 的行翻转写入 Unity（Unity 的纹理通常从底到顶）
            for (int y = 0; y < levelHeight; y++)
            {
                int srcRow = y; // Skia 行索引（0..h-1）
                int dstRow = levelHeight - 1 - y; // 写入 Unity 时翻转
                int srcRowStart = srcRow * srcRowBytes;
                int dstRowStart = offset + dstRow * dstRowBytes;

                // 只拷贝每行的有效像素部分（levelWidth * 4），忽略 Skia 的 padding
                for (int x = 0; x < dstRowBytes; x++)
                {
                    raw[dstRowStart + x] = srcPtr[srcRowStart + x];
                }
            }
        }

        public unsafe static void GenerateMipMaps(Texture2D texture, SKBitmap bitmap, ref byte[] raw, int mipCount)
        {
             // RGBA32
            int width = bitmap.Width;
            int height = bitmap.Height;

            // 辅助：把一个 SKBitmap 的像素写入 raw 的指定偏移（只写有效 width*4 部分，按 Unity 的行顺序）
            

            // 3) 生成并写入每一层 mip
            // 首先把 base level（mip 0）写入 raw 的偏移 0
            // 注意：确保 bitmap 的 ColorType 是 RGBA8888；若是 BGRA，需要在写入时交换 R/B
            // 我们先把 base 写入 raw
            WriteSKBitmapToRaw(bitmap, ref raw, width, height, 0);

            // 计算并写入后续 mip 层（使用 Skia 的 Resize）
            int prevW = width;
            int prevH = height;
            // 为了质量，从上一层缩放到下一层
            SKBitmap prevBitmap = bitmap;

            // 计算每层偏移的辅助函数（累加每层大小）
            int ComputeOffsetForMip(int targetMip)
            {
                uint acc = 0;
                for (int m = 0; m < targetMip; m++)
                {
                    int mw = Math.Max(1, width >> m);
                    int mh = Math.Max(1, height >> m);
                    acc += GraphicsFormatUtility.ComputeMipmapSize(mw, mh, GFX_FORMAT);
                }
                return (int)acc;
            }

            for (int mip = 1; mip < mipCount; mip++)
            {
                int mipW = Math.Max(1, prevW / 2);
                int mipH = Math.Max(1, prevH / 2);

                // 使用 Skia 缩放（高质量滤波）
                var info = new SKImageInfo(mipW, mipH, SKColorType.Rgba8888, SKAlphaType.Premul);

                // 从 prevBitmap 缩放到 mipBitmap
                SKBitmap mipBitmap = prevBitmap.Resize(info, SKSamplingOptions.Default);
                

                int mipOffset = ComputeOffsetForMip(mip);
                Debug.Log($"Mip {mip}: {mipW}x{mipH}, offset={mipOffset}, rowBytes={mipBitmap.RowBytes}");

                // 写入该 mip 到 raw
                WriteSKBitmapToRaw(mipBitmap,ref raw, mipW, mipH, mipOffset);

                // 准备下一层：prevBitmap 指向当前 mipBitmap（注意释放上一个非 base 的 bitmap）
                if (prevBitmap != bitmap)
                {
                    prevBitmap.Dispose();
                }
                prevBitmap = mipBitmap;
                prevW = mipW;
                prevH = mipH;
            }

            // 如果我们在循环中创建了新的 prevBitmap（非 base），最后需要释放它（但不要释放 base bitmap）
            if (prevBitmap != bitmap)
            {
                prevBitmap.Dispose();
            }

            //Debug.Log($"UploadBitmapWithMips: prepared raw buffer length={raw.Length}, expected={totalBytes}");

            // 4) 把 raw 一次性上传给 Unity（我们已经提供了所有 mip 层）
            
            //texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);

            //Debug.Log("UploadBitmapWithMips: LoadRawTextureData + Apply complete. Mipmaps provided by CPU.");
        }
    }
}
