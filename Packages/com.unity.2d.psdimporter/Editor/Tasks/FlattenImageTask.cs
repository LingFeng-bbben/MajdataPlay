using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.U2D.PSD
{
    static class FlattenImageTask
    {
        struct LayerData
        {
            public IntPtr layerBuffer;
            public int4 layerRect;
        }

        public static unsafe void Execute(in PSDExtractLayerData[] layer, ref NativeArray<Color32> output, bool importHiddenLayer, Vector2Int documentSize)
        {
            UnityEngine.Profiling.Profiler.BeginSample("FlattenImage");

            List<LayerData> layerData = new List<LayerData>();
            for (int i = layer.Length - 1; i >= 0; --i)
            {
                GetLayerDataToMerge(in layer[i], ref layerData, importHiddenLayer);
            }

            if (layerData.Count == 0)
                return;

            int layersPerJob = layerData.Count / (SystemInfo.processorCount == 0 ? 8 : SystemInfo.processorCount);
            layersPerJob = Mathf.Max(layersPerJob, 1);

            FlattenImageInternalJob job = new FlattenImageInternalJob();
            FlattenImageInternalJob combineJob = new FlattenImageInternalJob();

            job.inputTextures = new NativeArray<IntPtr>(layerData.Count, Allocator.TempJob);
            job.inputTextureRects = new NativeArray<int4>(layerData.Count, Allocator.TempJob);

            for (int i = 0; i < layerData.Count; ++i)
            {
                job.inputTextures[i] = layerData[i].layerBuffer;
                job.inputTextureRects[i] = layerData[i].layerRect;
            }

            job.layersPerJob = layersPerJob;
            job.flipY = false;
            combineJob.flipY = true;

            int jobCount = layerData.Count / layersPerJob + (layerData.Count % layersPerJob > 0 ? 1 : 0);
            combineJob.layersPerJob = jobCount;

            NativeArray<byte>[] premergedBuffer = new NativeArray<byte>[jobCount];
            job.outputTextureSizes = new NativeArray<int2>(jobCount, Allocator.TempJob);
            job.outputTextures = new NativeArray<IntPtr>(jobCount, Allocator.TempJob);
            combineJob.inputTextures = new NativeArray<IntPtr>(jobCount, Allocator.TempJob);
            combineJob.inputTextureRects = new NativeArray<int4>(jobCount, Allocator.TempJob);

            for (int i = 0; i < jobCount; ++i)
            {
                premergedBuffer[i] = new NativeArray<byte>(documentSize.x * documentSize.y * 4, Allocator.TempJob);
                job.outputTextureSizes[i] = new int2(documentSize.x, documentSize.y);
                job.outputTextures[i] = new IntPtr(premergedBuffer[i].GetUnsafePtr());
                combineJob.inputTextures[i] = new IntPtr(premergedBuffer[i].GetUnsafeReadOnlyPtr());
                combineJob.inputTextureRects[i] = new int4(0, 0, documentSize.x, documentSize.y);
            }

            combineJob.outputTextureSizes = new NativeArray<int2>(new[] { new int2(documentSize.x, documentSize.y) }, Allocator.TempJob);
            combineJob.outputTextures = new NativeArray<IntPtr>(new[] { new IntPtr(output.GetUnsafePtr()) }, Allocator.TempJob);

            JobHandle handle = job.Schedule(jobCount, 1);
            combineJob.Schedule(1, 1, handle).Complete();

            foreach (NativeArray<byte> b in premergedBuffer)
            {
                if (b.IsCreated)
                    b.Dispose();
            }

            UnityEngine.Profiling.Profiler.EndSample();
        }

        static unsafe void GetLayerDataToMerge(in PSDExtractLayerData layer, ref List<LayerData> layerData, bool importHiddenLayer)
        {
            PDNWrapper.BitmapLayer bitmapLayer = layer.bitmapLayer;
            PSDLayerImportSetting importSetting = layer.importSetting;
            if (!bitmapLayer.Visible && importHiddenLayer == false || importSetting.importLayer == false)
                return;

            if (bitmapLayer.IsGroup)
            {
                for (int i = layer.children.Length - 1; i >= 0; --i)
                    GetLayerDataToMerge(layer.children[i], ref layerData, importHiddenLayer);
            }

            if (bitmapLayer.Surface == null || bitmapLayer.localRect == default)
                return;

            PDNWrapper.Rectangle layerRect = bitmapLayer.documentRect;
            LayerData data = new LayerData()
            {
                layerBuffer = new IntPtr(bitmapLayer.Surface.color.GetUnsafeReadOnlyPtr()),
                layerRect = new int4(layerRect.X, layerRect.Y, layerRect.Width, layerRect.Height)
            };
            layerData.Add(data);
        }

        [BurstCompile]
        struct FlattenImageInternalJob : IJobParallelFor
        {
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<IntPtr> inputTextures;
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<int4> inputTextureRects;
            [ReadOnly]
            public int layersPerJob;
            [ReadOnly]
            public bool flipY;

            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<int2> outputTextureSizes;
            [DeallocateOnJobCompletion]
            public NativeArray<IntPtr> outputTextures;

            public unsafe void Execute(int index)
            {
                Color32* outputColor = (Color32*)outputTextures[index].ToPointer();
                for (int layerIndex = index * layersPerJob; layerIndex < (index * layersPerJob) + layersPerJob; ++layerIndex)
                {
                    if (inputTextures.Length <= layerIndex)
                        break;

                    Color32* inputColor = (Color32*)inputTextures[layerIndex].ToPointer();
                    int inStartPosX = inputTextureRects[layerIndex].x;
                    int inStartPosY = inputTextureRects[layerIndex].y;
                    int inWidth = inputTextureRects[layerIndex].z;
                    int inHeight = inputTextureRects[layerIndex].w;

                    int outWidth = outputTextureSizes[index].x;
                    int outHeight = outputTextureSizes[index].y;

                    for (int y = 0; y < inHeight; ++y)
                    {
                        int outPosY = y + inStartPosY;
                        // If pixel is outside of output texture's Y, move to the next pixel.
                        if (outPosY < 0 || outPosY >= outHeight)
                            continue;

                        int inRow = y * inWidth;
                        int outRow = flipY ? (outHeight - 1 - y - inStartPosY) * outWidth : (y + inStartPosY) * outWidth;

                        for (int x = 0; x < inWidth; ++x)
                        {
                            int outPosX = x + inStartPosX;
                            // If pixel is outside of output texture's X, move to the next pixel.
                            if (outPosX < 0 || outPosX >= outWidth)
                                continue;

                            int inBufferIndex = inRow + x;
                            int outBufferIndex = outRow + outPosX;

                            Color inColor = inputColor[inBufferIndex];
                            Color prevOutColor = outputColor[outBufferIndex];
                            Color outColor = new Color();

                            float destAlpha = prevOutColor.a * (1 - inColor.a);
                            outColor.a = inColor.a + prevOutColor.a * (1 - inColor.a);

                            float premultiplyAlpha = outColor.a > 0.0f ? 1 / outColor.a : 1f;
                            outColor.r = (inColor.r * inColor.a + prevOutColor.r * destAlpha) * premultiplyAlpha;
                            outColor.g = (inColor.g * inColor.a + prevOutColor.g * destAlpha) * premultiplyAlpha;
                            outColor.b = (inColor.b * inColor.a + prevOutColor.b * destAlpha) * premultiplyAlpha;

                            outputColor[outBufferIndex] = outColor;
                        }
                    }
                }
            }
        }
    }
}
