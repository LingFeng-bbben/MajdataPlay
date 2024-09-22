using UnityEngine;
using MajdataPlay.Types;
using MajdataPlay.Extensions;
using System;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public abstract class TouchBase : NoteDrop
    {
        public char areaPosition;
        public bool isFirework;

        public GameObject tapEffect;
        public GameObject judgeEffect;

        public TouchGroup GroupInfo;

        protected Quaternion GetRoation()
        {
            if (sensorPos == SensorType.C)
                return Quaternion.Euler(Vector3.zero);
            var d = Vector3.zero - transform.position;
            var deg = 180 + Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;

            return Quaternion.Euler(new Vector3(0, 0, -deg));
        }
        public SensorType GetSensor() => GetSensor(areaPosition, startPosition);
        public static SensorType GetSensor(char areaPos, int startPos)
        {
            switch (areaPos)
            {
                case 'A':
                    return (SensorType)(startPos - 1);
                case 'B':
                    return (SensorType)(startPos + 7);
                case 'C':
                    return SensorType.C;
                case 'D':
                    return (SensorType)(startPos + 16);
                case 'E':
                    return (SensorType)(startPos + 24);
                default:
                    return SensorType.A1;
            }
        }
        public static Vector3 GetAreaPos(SensorType pos)
        {
            var group = pos.GetGroup();
            var index = pos.GetIndex();
            switch(group)
            {
                case SensorGroup.A:
                    {
                        var angle = -index * (Mathf.PI / 4) + Mathf.PI * 5 / 8;
                        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 4.1f;
                    }
                case SensorGroup.B:
                    {
                        var angle = -index * (Mathf.PI / 4) + Mathf.PI * 5 / 8;
                        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 2.3f;
                    }
                case SensorGroup.C:
                    return Vector3.zero;
                case SensorGroup.D:
                    {
                        var angle = -index * (Mathf.PI / 4) + Mathf.PI * 6 / 8;
                        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 4.1f;
                    }
                case SensorGroup.E:
                    {
                        var angle = -index * (Mathf.PI / 4) + Mathf.PI * 6 / 8;
                        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 3.0f;
                    }
            }
            return Vector3.zero;
        }
        public static float GetDistance(SensorGroup group)
        {
            switch (group)
            {
                case SensorGroup.D:
                case SensorGroup.A:
                    return 4.1f;
                case SensorGroup.B:
                    return 2.3f;
                default:
                    return 0;
                case SensorGroup.E:
                    return 3.0f;
            }
        }
        public static Quaternion GetRoation(Vector3 position, SensorType sensorPos)
        {
            if (sensorPos == SensorType.C)
                return Quaternion.Euler(Vector3.zero);
            var d = Vector3.zero - position;
            var deg = 180 + Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;

            return Quaternion.Euler(new Vector3(0, 0, -deg));
        }
        public static Vector3 GetAreaPos(int index, char area)
        {
            /// <summary>
            /// AreaDistance: 
            /// C:   0
            /// E:   3.1
            /// B:   2.21
            /// A,D: 4.8
            /// </summary>
            if (area == 'C') return Vector3.zero;
            if (area == 'B')
            {
                var angle = -index * (Mathf.PI / 4) + Mathf.PI * 5 / 8;
                return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 2.3f;
            }

            if (area == 'A')
            {
                var angle = -index * (Mathf.PI / 4) + Mathf.PI * 5 / 8;
                return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 4.1f;
            }

            if (area == 'E')
            {
                var angle = -index * (Mathf.PI / 4) + Mathf.PI * 6 / 8;
                return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 3.0f;
            }

            if (area == 'D')
            {
                var angle = -index * (Mathf.PI / 4) + Mathf.PI * 6 / 8;
                return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 4.1f;
            }

            return Vector3.zero;
        }
    }
}
