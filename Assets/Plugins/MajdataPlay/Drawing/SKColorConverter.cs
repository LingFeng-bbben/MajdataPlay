using SkiaSharp;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MajdataPlay.Drawing
{
    public static class SKColorConverter
    {
        /// <summary>
        ///   批量转换到 Color32
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="batchCount"></param>
        /// <returns></returns>
        public static NativeArray<Color32> ConvertToColor32(NativeArray<SKColor> colors, int batchCount = 512)
        {
            var handle = FastColorConverter(colors.Reinterpret<uint>(), out var data, batchCount);
            handle.Complete();
            return data.Reinterpret<Color32>();
        }

        /// <summary>
        ///   批量转换到 SKColor
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="batchCount"></param>
        /// <returns></returns>
        public static NativeArray<SKColor> ConvertToSkColor(NativeArray<Color32> colors, int batchCount = 512)
        {
            var handle = FastColorConverter(colors.Reinterpret<uint>(), out var data, batchCount);
            handle.Complete();
            return data.Reinterpret<SKColor>();
        }

        /// <summary>
        ///   快速转换颜色
        /// </summary>
        /// <param name="dataIn"></param>
        /// <param name="dataOut"></param>
        /// <param name="batchCount"></param>
        public static JobHandle FastColorConverter(NativeArray<uint> dataIn, out NativeArray<uint> dataOut, int batchCount = 512)
        {
            dataOut = new NativeArray<uint>(dataIn.Length, Allocator.TempJob);

            var job = new ColorConverterJob
            {
                DataIn = dataIn,
                DataOut = dataOut
            };
            return job.Schedule(dataIn.Length, batchCount);
        }

        [BurstCompile]
        private struct ColorConverterJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<uint> DataIn;
            public NativeArray<uint> DataOut;

            private const uint Mask0 = 0x00FF0000;
            private const uint Mask1 = 0x000000FF;

            public void Execute(int index)
            {
                var color = DataIn[index];

                DataOut[index] = ((color & Mask0) >> 16) | ((color & Mask1) << 16) | (color & ~(Mask0 | Mask1));
            }
        }
    }
}