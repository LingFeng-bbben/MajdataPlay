using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;

namespace MajdataPlay.IO
{
    [BurstCompile]
    internal static unsafe class MobileTouchPanelHelper
    {
        [BurstCompile]
        public static void PositionHandle(in Vector4 cubePos,
                in float userRad,
                in float a_extraRad,
                in float b_extraRad,
                in float c_extraRad,
                in float d_extraRad,
                in float e_extraRad,
                in float FINGER_RADIUS_SEGMENT_LENGTH,
                in int TOUCH_ANGLE_SMAPLE_COUNT,
                in ulong* posSamples,
                in Vector4* circleSamples,
                ref ulong newStates)
        {
            const ulong A_AREA_MASK = 0b00000000_00000000_00000000_00000000_00000000_00001111_11110000_00000000;
            const ulong B_AREA_MASK = 0b00000000_00000000_00000000_00000000_00001111_11110000_00000000_00000000;
            const ulong C_AREA_MASK = 0b00000000_00000000_00000000_00000000_00110000_00000000_00000000_00000000;
            const ulong D_AREA_MASK = 0b00000000_00000000_00000000_00111111_11000000_00000000_00000000_00000000;
            const ulong E_AREA_MASK = 0b00000000_00000000_00111111_11000000_00000000_00000000_00000000_00000000;
            var BURST_CONST_FINGER_RADIUS_SEGMENT_LENGTH = (float4)FINGER_RADIUS_SEGMENT_LENGTH;
            var radStepCount = (int)(userRad / FINGER_RADIUS_SEGMENT_LENGTH);
            var aAreaRad = math.max(userRad, userRad + a_extraRad);
            var bAreaRad = math.max(userRad, userRad + b_extraRad);
            var cAreaRad = math.max(userRad, userRad + c_extraRad);
            var dAreaRad = math.max(userRad, userRad + d_extraRad);
            var eAreaRad = math.max(userRad, userRad + e_extraRad);

            for (var i = 0; i < radStepCount; i++)
            {
                var rad = BURST_CONST_FINGER_RADIUS_SEGMENT_LENGTH * (int4)(i + 1);
                for (int j = 0; j < TOUCH_ANGLE_SMAPLE_COUNT; j++)
                {
                    var circular = (Vector4)(circleSamples[j] * rad);
                    var pos = cubePos + circular;
                    //Debug.DrawLine(lastCircular, pos, Color.red, MajEnv.FRAME_LENGTH_SEC);
                    //lastCircular = pos;

                    ReadPostionData(pos, posSamples, ref newStates);
                }
            }
            ReadPostionData(cubePos, posSamples, ref newStates);
            ReadOnlySpan<(ulong Mask, float Radius)> areaData = stackalloc (ulong, float)[]
            {
                    (A_AREA_MASK, aAreaRad),
                    (B_AREA_MASK, bAreaRad),
                    (C_AREA_MASK, cAreaRad),
                    (D_AREA_MASK, dAreaRad),
                    (E_AREA_MASK, eAreaRad)
                };
            for (var a = 0; a < areaData.Length; a++)
            {
                ref readonly var data = ref areaData[a];
                var mask = data.Mask;
                var radius = data.Radius;
                var subP = 0UL;
                var segLength = radius / FINGER_RADIUS_SEGMENT_LENGTH;
                var radius4 = (float4)radius;
                for (var i = 0; i < segLength; i++)
                {
                    var rad = BURST_CONST_FINGER_RADIUS_SEGMENT_LENGTH * (int4)(i + 1);
                    for (int j = 0; j < TOUCH_ANGLE_SMAPLE_COUNT; j++)
                    {
                        var circular = (Vector4)(circleSamples[j] * rad);
                        var pos = cubePos + circular;

                        ReadPostionData(pos, posSamples, ref subP);
                    }
                }
                subP &= mask;
                newStates |= subP;
            }
        }
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadPostionData(in Vector4 pos, in ulong* posSamples, ref ulong newP)
        {
            var x = (int)(pos.x * 100);
            var y = (int)(pos.y * 100);
            if (x < -540 || y < -540 || x > 539 || y > 539)
            {
                return;
            }
            ref readonly var posData = ref posSamples[(x + 540) * 1280 + (y + 540)];
            newP |= posData;
        }
    }
}
