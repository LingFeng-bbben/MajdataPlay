using MajdataPlay.Extensions;
using MajdataPlay.Game.Types;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Game.Utils
{
    internal static class NoteHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SensorArea GetSensor(char areaPos, int startPos)
        {
            switch (areaPos)
            {
                case 'A':
                    return (SensorArea)(startPos - 1);
                case 'B':
                    return (SensorArea)(startPos + 7);
                case 'C':
                    return SensorArea.C;
                case 'D':
                    return (SensorArea)(startPos + 16);
                case 'E':
                    return (SensorArea)(startPos + 24);
                default:
                    return SensorArea.A1;
            }
        }
        public static Vector3 GetTouchAreaPosition(SensorArea area)
        {
            var group = area.GetGroup();
            var index = area.GetIndex();
            // AreaDistance: 
            // C:   0
            // E:   3.1
            // B:   2.21
            // A,D: 4.8
            switch (group)
            {
                case SensorGroup.A:
                    {
                        var distance = GetTouchAreaDistance(group);
                        var angle = -index * (Mathf.PI / 4) + Mathf.PI * 5 / 8;
                        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                    }
                case SensorGroup.B:
                    {
                        var distance = GetTouchAreaDistance(group);
                        var angle = -index * (Mathf.PI / 4) + Mathf.PI * 5 / 8;
                        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                    }
                case SensorGroup.C:
                    return Vector3.zero;
                case SensorGroup.D:
                    {
                        var distance = GetTouchAreaDistance(group);
                        var angle = -index * (Mathf.PI / 4) + Mathf.PI * 6 / 8;
                        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                    }
                case SensorGroup.E:
                    {
                        var distance = GetTouchAreaDistance(group);
                        var angle = -index * (Mathf.PI / 4) + Mathf.PI * 6 / 8;
                        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                    }
                default:
                    return Vector3.zero;

            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetTouchAreaPosition(int index, char area)
        {
            return GetTouchAreaPosition(area switch
            {
                'A' => SensorArea.A1 + (index - 1),
                'B' => SensorArea.B1 + (index - 1),
                'C' => SensorArea.C,
                'D' => SensorArea.D1 + (index - 1),
                'E' => SensorArea.E1 + (index - 1),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetTouchAreaDistance(SensorGroup group)
        {
            switch (group)
            {
                case SensorGroup.A:
                    return 4.0f;
                case SensorGroup.B:
                    return 2.2f;
                default:
                    return 0;
                case SensorGroup.D:
                    return 4.1f;
                case SensorGroup.E:
                    return 3.1f;
            }
        }
        public static Quaternion GetTouchRoation(Vector3 position, SensorArea sensorPos)
        {
            if (sensorPos == SensorArea.C)
                return Quaternion.Euler(Vector3.zero);
            var d = Vector3.zero - position;
            var deg = 180 + Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;

            return Quaternion.Euler(new Vector3(0, 0, -deg));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetTapPosition(SensorArea area, float distance)
        {
            var group = area.GetGroup();
            switch(group)
            {
                case SensorGroup.A:
                    return GetTapPosition((int)area + 1, distance); 
                case SensorGroup.D:
                    return GetTapPosition((int)area - 16.5f, distance);
                default:
                    throw new ArgumentOutOfRangeException(nameof(area));
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetTapPosition(int position, float distance)
        {
            return new Vector3(
                distance * Mathf.Cos((position * -2f + 5f) * 0.125f * Mathf.PI),
                distance * Mathf.Sin((position * -2f + 5f) * 0.125f * Mathf.PI));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetTapPosition(float position, float distance)
        {
            return new Vector3(
                distance * Mathf.Cos((position * -2f + 5f) * 0.125f * Mathf.PI),
                distance * Mathf.Sin((position * -2f + 5f) * 0.125f * Mathf.PI));
        }
        public static SlideOKShape GetSlideOKShapeFromSlideType(string slideType)
        {
            switch(slideType)
            {
                case "circle1":
                case "circle2":
                case "circle3":
                case "circle4":
                case "circle5":
                case "circle6":
                case "circle7":
                case "circle8":
                    return SlideOKShape.Curv;
                case "wifi":
                    return SlideOKShape.Wifi;
                default:
                    return SlideOKShape.Str;
            }
        }
    }
}
