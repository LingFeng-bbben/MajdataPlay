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
        public static Color BgColor { get; set; } = new Color(0, 0, 0);

        uint _avgFPSIndex = 0;
        uint _avgFPSSampleCount = 0;
        uint _1_lowFrameSampleCount = 0;

        float _frameTimer = 1;
        float _totalFrameTimeSec = 0;

        readonly float[] _avgFPSData = new float[150];
        readonly (float FrameTimeSec, ulong FrameIndex)[] _1_lowFrameData = new (float FrameTimeSec, ulong FrameIndex)[1500];

        TextMeshPro _textDisplayer;
        GameSetting _setting;

        protected override void Awake()
        {
            base.Awake();
            MajInstances.FPSDisplayer = this;
            _textDisplayer = GetComponent<TextMeshPro>();
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
            var delta = MajTimeline.DeltaTime;
            AddSample(delta);
            if (_frameTimer <= 0)
            {
                _textDisplayer.enabled = _setting.Debug.DisplayFPS;
                var avgFPS = _totalFrameTimeSec / _avgFPSSampleCount;
                using var sb = ZString.CreateStringBuilder(true);
                if (_1_lowFrameSampleCount != 1500)
                {
                    sb.AppendFormat("FPS  {0:F2}   1%  --.--", 1 / avgFPS);
                    var a = sb.AsArraySegment();
                    _textDisplayer.SetCharArray(a.Array, a.Offset, a.Count);
                }
                else
                {
                    var totalLowFrameTime = 0f;
                    for (var i = 0U; i < 150; i++)
                    {
                        ref var data = ref _1_lowFrameData[i];
                        totalLowFrameTime += data.FrameTimeSec;
                    }
                    var avgLowFrameTime = totalLowFrameTime / 150; 
                    sb.AppendFormat("FPS  {0:F2}   1%  {1:F2}", 1 / avgFPS, 1 / avgLowFrameTime);
                    var a = sb.AsArraySegment();
                    _textDisplayer.SetCharArray(a.Array, a.Offset, a.Count);
                }
                _frameTimer = 1;
            }
            else
            {
                _frameTimer -= delta;
            }
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void AddSample(in float data)
        {
            if (_avgFPSIndex >= 150)
            {
                _avgFPSIndex = 0;
            }
            if (_avgFPSSampleCount != 150)
            {
                _avgFPSSampleCount++;
            }
            ref var lastSample = ref _avgFPSData[_avgFPSIndex++];
            _totalFrameTimeSec -= lastSample;
            _totalFrameTimeSec += data;
            lastSample = data;
            
            fixed((float FrameTime, ulong FrameIndex)* lowFrameDataPtr = _1_lowFrameData)
            {
                const int BYTES_SIZE = 16;
                if (_1_lowFrameSampleCount < 1500)
                {
                    _1_lowFrameSampleCount++;
                }

                var flag = 0;
                var thisFrameIndex = MajTimeline.FrameCount;
                var oldestFrameIndex = ulong.MaxValue;
                var oldestRecordIndex = -1;
                for (var i = 0; i < 1500; i++)
                {
                    ref var data2 = ref *(lowFrameDataPtr + i);
                    
                    switch(flag)
                    {
                        case 0:
                            {
                                if (data2.FrameTime == 0)
                                {
                                    data2.FrameTime = data;
                                    data2.FrameIndex = thisFrameIndex;
                                    return;
                                }
                                else if (data2.FrameTime < data)
                                {
                                    //Array.Copy(_1_lowFrameData, i, _1_lowFrameData, i + 1, 1500 - i - 1);
                                    var bytes2Copy = (1500 - i - 1) * BYTES_SIZE;
                                    Buffer.MemoryCopy(lowFrameDataPtr + i, lowFrameDataPtr + i + 1, bytes2Copy, bytes2Copy);
                                    data2.FrameTime = data;
                                    data2.FrameIndex = thisFrameIndex;
                                    flag = 1;
                                    continue;
                                }

                                goto case 2;
                            }
                        case 1:
                            {
                                if (data2.FrameTime == 0)
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
                    var bytes2Copy = (1500 - oldestRecordIndex - 1) * BYTES_SIZE;
                    Buffer.MemoryCopy(lowFrameDataPtr + oldestRecordIndex + 1, lowFrameDataPtr + oldestRecordIndex, bytes2Copy, bytes2Copy);
                    *(lowFrameDataPtr + 1499) = (0 , 0);
                }
            }
        }
    }
}