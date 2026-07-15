using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using SkiaSharp;
using SkiaSharp.Unity;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace MajdataPlay.Drawing
{
    [BurstCompile]
    public static class SKBitmapExtensions
    {
        const int BYTES_PER_PIXEL = 4;
        const GraphicsFormat GFX_FORMAT = GraphicsFormat.R8G8B8A8_UNorm;
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

            texture.wrapMode = TextureWrapMode.Clamp;
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
            var rawBuffer = new NativeArray<byte>(totalBytes, Allocator.TempJob);

            GenerateMipMaps(texture, converted, rawBuffer, mipCount);
            
            texture.LoadRawTextureData(rawBuffer);
            texture.Apply(false, false);

            return texture;
        }

        static unsafe void WriteSKBitmapToRaw(SKBitmap srcBitmap, NativeArray<byte> raw, int levelWidth, int levelHeight, int offset)
        {
            var srcRowBytes = srcBitmap.RowBytes;
            var dstRowBytes = levelWidth * BYTES_PER_PIXEL;            
            var srcPixels = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray(srcBitmap.GetPixelSpan(),
                                                                                      Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var safety = AtomicSafetyHandle.Create();
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref srcPixels, safety);
#endif
            var jobHandle = new CovertSKBitmapPixelToTexturePixelJob()
            {
                SrcPixels = srcPixels,
                DstPixels = raw,
                LevelWidth = levelWidth,
                LevelHeight = levelHeight,
                SrcRowBytes = srcRowBytes,
                DstRowBytes = dstRowBytes,
                DstOffset = offset
            }.Schedule(levelHeight, 64);

            jobHandle.Complete();
        }

        public unsafe static void GenerateMipMaps(Texture2D texture, SKBitmap bitmap, NativeArray<byte> raw, int mipCount)
        {
             // RGBA32
            int width = bitmap.Width;
            int height = bitmap.Height;

            // 辅助：把一个 SKBitmap 的像素写入 raw 的指定偏移（只写有效 width*4 部分，按 Unity 的行顺序）
            

            // 3) 生成并写入每一层 mip
            // 首先把 base level（mip 0）写入 raw 的偏移 0
            // 注意：确保 bitmap 的 ColorType 是 RGBA8888；若是 BGRA，需要在写入时交换 R/B
            // 我们先把 base 写入 raw
            WriteSKBitmapToRaw(bitmap, raw, width, height, 0);

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
                WriteSKBitmapToRaw(mipBitmap, raw, mipW, mipH, mipOffset);

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

        [BurstCompile]
        struct CovertSKBitmapPixelToTexturePixelJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<byte> SrcPixels;

            [NativeDisableParallelForRestriction]
            public NativeArray<byte> DstPixels;

            public int SrcRowBytes;
            public int DstRowBytes;
            public int LevelWidth;
            public int LevelHeight;
            public int DstOffset;

            public void Execute(int y)
            {
                var srcRow = y;
                var dstRow = LevelHeight - 1 - y;
                var srcRowStart = srcRow * SrcRowBytes;
                var dstRowStart = DstOffset + (dstRow * DstRowBytes);

                var srcFragment = SrcPixels.Slice(srcRowStart, DstRowBytes);
                var dstFragment = DstPixels.Slice(dstRowStart, DstRowBytes);

                dstFragment.CopyFrom(srcFragment);
            }
        }
    }
}
