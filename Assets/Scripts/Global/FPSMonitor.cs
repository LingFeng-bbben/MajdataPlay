using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

#nullable enable
namespace MajdataPlay
{
    internal sealed class FPSMonitor : MajComponent
    {
        const int FPS_SAMPLE_COUNT = 120;
        const int ONE_PERCENT_LOW_FPS_SAMPLE_COUNT = 120;

        uint _avgFPSIndex;
        uint _avgFPSSampleCount;
        uint _onePercentLowFrameSampleCount;

        long _totalFrameTimeTicks;

        readonly long[] _avgFPSData = new long[FPS_SAMPLE_COUNT];
        readonly (long FrameTimeTicks, ulong FrameIndex)[] _onePercentLowFrameData =
            new (long FrameTimeTicks, ulong FrameIndex)[ONE_PERCENT_LOW_FPS_SAMPLE_COUNT];

        TimeSpan _lastUpdateTiming = TimeSpan.Zero;

        internal double AverageFPS
        {
            get
            {
                if (_avgFPSSampleCount == 0 || _totalFrameTimeTicks <= 0)
                {
                    return 0;
                }

                var averageFrameTime = (TimeSpan.FromTicks(_totalFrameTimeTicks) / _avgFPSSampleCount).TotalSeconds;
                return averageFrameTime <= 0 ? 0 : 1 / averageFrameTime;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _lastUpdateTiming = MajTimeline.UnscaledTime;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        void LateUpdate()
        {
            var currentTime = MajTimeline.UnscaledTime;
            var delta = currentTime - _lastUpdateTiming;
            AddSample(delta.Ticks);
            _lastUpdateTiming = currentTime;
        }

        internal bool TryGetOnePercentLowFPS(out double fps)
        {
            if (_onePercentLowFrameSampleCount != ONE_PERCENT_LOW_FPS_SAMPLE_COUNT)
            {
                fps = 0;
                return false;
            }

            var totalLowFrameTime = 0L;
            var sampleCount = Math.Max(1, (int)(ONE_PERCENT_LOW_FPS_SAMPLE_COUNT * 0.01));
            for (var i = 0; i < sampleCount; i++)
            {
                totalLowFrameTime += _onePercentLowFrameData[i].FrameTimeTicks;
            }

            var averageLowFrameTime = (TimeSpan.FromTicks(totalLowFrameTime) / sampleCount).TotalSeconds;
            fps = averageLowFrameTime <= 0 ? 0 : 1 / averageLowFrameTime;
            return true;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void AddSample(in long frameTicks)
        {
            if (_avgFPSIndex >= FPS_SAMPLE_COUNT)
            {
                _avgFPSIndex = 0;
            }
            if (_avgFPSSampleCount != FPS_SAMPLE_COUNT)
            {
                _avgFPSSampleCount++;
            }
            ref var lastSample = ref _avgFPSData[_avgFPSIndex++];
            _totalFrameTimeTicks -= lastSample;
            _totalFrameTimeTicks += frameTicks;
            lastSample = frameTicks;

            fixed ((long FrameTimeTicks, ulong FrameIndex)* lowFrameDataPtr = _onePercentLowFrameData)
            {
                const int BYTES_SIZE = 16;
                if (_onePercentLowFrameSampleCount < ONE_PERCENT_LOW_FPS_SAMPLE_COUNT)
                {
                    _onePercentLowFrameSampleCount++;
                }

                var flag = 0;
                var thisFrameIndex = MajTimeline.FrameCount;
                var oldestFrameIndex = ulong.MaxValue;
                var oldestRecordIndex = -1;
                for (var i = 0; i < ONE_PERCENT_LOW_FPS_SAMPLE_COUNT; i++)
                {
                    ref var data = ref *(lowFrameDataPtr + i);

                    switch (flag)
                    {
                        case 0:
                        {
                            if (data.FrameTimeTicks == 0)
                            {
                                data.FrameTimeTicks = frameTicks;
                                data.FrameIndex = thisFrameIndex;
                                return;
                            }
                            if (data.FrameTimeTicks < frameTicks)
                            {
                                var bytesToCopy = (ONE_PERCENT_LOW_FPS_SAMPLE_COUNT - i - 1) * BYTES_SIZE;
                                Buffer.MemoryCopy(lowFrameDataPtr + i, lowFrameDataPtr + i + 1, bytesToCopy, bytesToCopy);
                                data.FrameTimeTicks = frameTicks;
                                data.FrameIndex = thisFrameIndex;
                                flag = 1;
                                continue;
                            }

                            goto case 2;
                        }
                        case 1:
                        {
                            if (data.FrameTimeTicks == 0)
                            {
                                return;
                            }

                            goto case 2;
                        }
                        case 2:
                        {
                            if (data.FrameIndex < oldestFrameIndex)
                            {
                                oldestRecordIndex = i;
                                oldestFrameIndex = data.FrameIndex;
                            }
                            break;
                        }
                    }
                }
                if (oldestRecordIndex != -1)
                {
                    var bytesToCopy = (ONE_PERCENT_LOW_FPS_SAMPLE_COUNT - oldestRecordIndex - 1) * BYTES_SIZE;
                    Buffer.MemoryCopy(lowFrameDataPtr + oldestRecordIndex + 1, lowFrameDataPtr + oldestRecordIndex,
                        bytesToCopy, bytesToCopy);
                    *(lowFrameDataPtr + (ONE_PERCENT_LOW_FPS_SAMPLE_COUNT - 1)) = (0, 0);
                }
            }
        }
    }
}
