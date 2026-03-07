using Cysharp.Text;
using MajdataPlay.Settings;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    internal sealed class FPSDisplayer : MajSingleton
    {
        const int FPS_SAMPLE_COUNT = 120;
        const int _1_LOW_FPS_SAMPLE_COUNT = 120;
        public static Color BgColor { get; set; } = new Color(0, 0, 0);

        uint _avgFPSIndex = 0;
        uint _avgFPSSampleCount = 0;
        uint _1_lowFrameSampleCount = 0;

        float _frameTimer = 1;
        long _totalFrameTimeTicks = 0;

        readonly long[] _avgFPSData = new long[FPS_SAMPLE_COUNT];
        readonly (long FrameTimeTicks, ulong FrameIndex)[] _1_lowFrameData = new (long FrameTimeTicks, ulong FrameIndex)[_1_LOW_FPS_SAMPLE_COUNT];

        TextMeshPro _textDisplayer;
        GameSetting _setting;

        TimeSpan _lastUpdateTiming = TimeSpan.Zero;

        protected override void Awake()
        {
            base.Awake();
            MajInstances.FPSDisplayer = this;
            _textDisplayer = GetComponent<TextMeshPro>();
            _lastUpdateTiming = MajTimeline.UnscaledTime;
        }
        internal void Init()
        {
            _setting = MajInstances.Settings;
            _textDisplayer.enabled = _setting.Debug.DisplayFPS;
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        void LateUpdate()
        {
            var currentTime = MajTimeline.UnscaledTime;
            var delta = currentTime - _lastUpdateTiming;
            AddSample(delta.Ticks);
            _lastUpdateTiming = currentTime;
            if (_frameTimer <= 0)
            {
                _textDisplayer.enabled = _setting.Debug.DisplayFPS;
                var avgFPS = (TimeSpan.FromTicks(_totalFrameTimeTicks) / _avgFPSSampleCount).TotalSeconds;
                using var sb = ZString.CreateStringBuilder(true);
                if (_1_lowFrameSampleCount != _1_LOW_FPS_SAMPLE_COUNT)
                {
                    sb.AppendFormat("FPS  {0:F2}   1%  --.--", 1 / avgFPS);
                    var a = sb.AsArraySegment();
                    _textDisplayer.SetCharArray(a.Array, a.Offset, a.Count);
                }
                else
                {
                    var totalLowFrameTime = 0L;
                    var sampleCount = (int)(_1_LOW_FPS_SAMPLE_COUNT * 0.01);
                    for (var i = 0U; i < sampleCount; i++)
                    {
                        ref var data = ref _1_lowFrameData[i];
                        totalLowFrameTime += data.FrameTimeTicks;
                    }
                    var avgLowFrameTime = (TimeSpan.FromTicks(totalLowFrameTime) / sampleCount).TotalSeconds; 
                    sb.AppendFormat("FPS  {0:F2}   1%  {1:F2}", 1 / avgFPS, 1 / avgLowFrameTime);
                    var a = sb.AsArraySegment();
                    _textDisplayer.SetCharArray(a.Array, a.Offset, a.Count);
                }
                _frameTimer = 1;
            }
            else
            {
                _frameTimer -= (float)delta.TotalSeconds;
            }
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
            
            fixed((long FrameTimeTicks, ulong FrameIndex)* lowFrameDataPtr = _1_lowFrameData)
            {
                const int BYTES_SIZE = 16;
                if (_1_lowFrameSampleCount < _1_LOW_FPS_SAMPLE_COUNT)
                {
                    _1_lowFrameSampleCount++;
                }

                var flag = 0;
                var thisFrameIndex = MajTimeline.FrameCount;
                var oldestFrameIndex = ulong.MaxValue;
                var oldestRecordIndex = -1;
                for (var i = 0; i < _1_LOW_FPS_SAMPLE_COUNT; i++)
                {
                    ref var data2 = ref *(lowFrameDataPtr + i);
                    
                    switch(flag)
                    {
                        case 0:
                            {
                                if (data2.FrameTimeTicks == 0)
                                {
                                    data2.FrameTimeTicks = frameTicks;
                                    data2.FrameIndex = thisFrameIndex;
                                    return;
                                }
                                else if (data2.FrameTimeTicks < frameTicks)
                                {
                                    //Array.Copy(_1_lowFrameData, i, _1_lowFrameData, i + 1, 1500 - i - 1);
                                    var bytes2Copy = (_1_LOW_FPS_SAMPLE_COUNT - i - 1) * BYTES_SIZE;
                                    Buffer.MemoryCopy(lowFrameDataPtr + i, lowFrameDataPtr + i + 1, bytes2Copy, bytes2Copy);
                                    data2.FrameTimeTicks = frameTicks;
                                    data2.FrameIndex = thisFrameIndex;
                                    flag = 1;
                                    continue;
                                }

                                goto case 2;
                            }
                        case 1:
                            {
                                if (data2.FrameTimeTicks == 0)
                                {
                                    return;
                                }

                                goto case 2;
                            }
                        case 2:
                            {
                                if (data2.FrameIndex < oldestFrameIndex)
                                {
                                    oldestRecordIndex = i;
                                    oldestFrameIndex = data2.FrameIndex;
                                }
                            }
                            break;
                    }
                }
                if(oldestRecordIndex != -1)
                {
                    var bytes2Copy = (_1_LOW_FPS_SAMPLE_COUNT - oldestRecordIndex - 1) * BYTES_SIZE;
                    Buffer.MemoryCopy(lowFrameDataPtr + oldestRecordIndex + 1, lowFrameDataPtr + oldestRecordIndex, bytes2Copy, bytes2Copy);
                    *(lowFrameDataPtr + (_1_LOW_FPS_SAMPLE_COUNT - 1)) = (0 , 0);
                }
            }
        }
    }
}