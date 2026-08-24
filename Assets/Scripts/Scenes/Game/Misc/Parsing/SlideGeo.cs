using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MajdataPlay.Scenes.Game.Parsing
{
    public static class SlideGeo
    {
        // Math.IEEERemainder 可以在burst直接用啊

        /// <summary>
        /// <p>这个参数定义了整个几何模型的长度单位</p>
        /// </summary>
        public const double MainRadius = 4.80;
        public const double Epsilon = 0.0001;
        public const double EpsilonRad = 0.002;

        public static readonly double CenterRadius = MainRadius * Math.Cos(Math.PI * 3 / 8);
        public static readonly double GroupBRadius = CenterRadius / Math.Cos(Math.PI / 8);

        private static readonly double _b = Math.Cos(Math.PI / 8) / 2;
        private static readonly double _a = 1 - _b;
        private static readonly double _theta = Math.PI / 4;
        private static readonly double _s = (_a * _a + _b * _b - 2 * _a * _b * Math.Cos(_theta)) /
                                            (2 * _a - 2 * _b * Math.Cos(_theta));

        public static readonly double PPQQRadius = MainRadius * _b;
        public static readonly double TransferRadius = MainRadius * (_b + _s);
        public static readonly double EdgeTransferRadian = _theta;
        public static readonly double PPQQTransferRadian =
            Math.Acos((_s * _s + _b * _b - (_a - _s) * (_a - _s)) / (2 * _b * _s));

        public static readonly double DefaultDistance = MainRadius * Math.PI / 32;

        /// <summary>
        /// <p>A 区节点坐标（判定线上的八个点）</p>
        /// <p>Note: idx is 1-based, not 0-based</p>
        /// </summary>
        public static Complex PointGroupA(int idx)
        {
            var radian = Math.PI * (5.0 / 8.0 - idx / 4.0);
            return Complex.FromPolarCoordinates(MainRadius, radian);
        }

        /// <summary>
        /// <p>B 区节点坐标（不是 B 区 Touch 的位置）</p>
        /// <p>Note: idx is 1-based, not 0-based</p>
        /// </summary>
        public static Complex PointGroupB(int idx)
        {
            var radian = Math.PI * (5.0 / 8.0 - idx / 4.0);
            return Complex.FromPolarCoordinates(GroupBRadius, radian);
        }

        /// <summary>
        /// <p>C 区节点坐标（正中心）</p>
        /// </summary>
        public static Complex PointCenter()
        {
            return Complex.Zero;
        }

        /// <summary>
        /// 获取指定判定区对应节点的坐标，判定区符合<c>SensorType</c>的定义，可取范围 0~16
        /// </summary>
        public static Complex GetPoint(int sensorIdx)
        {
            switch (sensorIdx)
            {
                case >= 0 and <= 7:
                    return PointGroupA(sensorIdx + 1);
                case >= 8 and <= 15:
                    return PointGroupB(sensorIdx - 7);
                case 16:
                    return PointCenter();
                default:
                    return Complex.Zero;
            }
        }

        /// <summary>
        /// <p>10 个圆的圆心坐标和半径</p>
        /// <p>idx 0 is center circle (pq circle),</p>
        /// <p>idx 1~8 are ppqq circles passing D[idx] panel,</p>
        /// <p>idx 9 is outer circle (judge circle / &lt;&gt; circle)</p>
        /// </summary>
        public static ComplexCircle GetCircle(int idx)
        {
            if (idx == 0)
            {
                return new ComplexCircle(Complex.Zero, CenterRadius);
            }

            if (idx == 9)
            {
                return new ComplexCircle(Complex.Zero, MainRadius);
            }

            var radian = Math.PI * (3.0 / 4.0 - idx / 4.0);
            var center = Complex.FromPolarCoordinates(PPQQRadius, radian);
            return new ComplexCircle(center, PPQQRadius);
        }

        /// <summary>
        /// 计算指定点进入指定圆的切线
        /// </summary>
        public static double CalcTangentAngle(Complex point, ComplexCircle circle, bool isCcw)
        {
            var hypot = point - circle.Center;
            var deltaRad = Math.Acos(circle.Radius / hypot.Magnitude);
            var tanRad = hypot.Phase + (isCcw ? deltaRad : -deltaRad);
            return Math.IEEERemainder(tanRad, Math.PI * 2.0);
        }

        /// <summary>
        /// <p>ppqq 圆到判定线大圆的转移路径（slidecode 专用）</p>
        /// <p>Note: idx is 1-based, not 0-based</p>
        /// </summary>
        /// <returns>CircleStruct TransferCircle, double TransferStartAngle, double TransferEndAngle</returns>
        public static (ComplexCircle, double, double) TransferOutData(int idx, bool isCcw)
        {
            var ppqqRad = Math.PI * (3.0 / 4.0 - idx / 4.0);
            double startRad, endRad;
            if (isCcw)
            {
                startRad = ppqqRad - PPQQTransferRadian;
                endRad = ppqqRad + EdgeTransferRadian;
            }
            else
            {
                startRad = ppqqRad + PPQQTransferRadian;
                endRad = ppqqRad - EdgeTransferRadian;
            }
            var d = MainRadius - TransferRadius;
            var center = Complex.FromPolarCoordinates(d, endRad);
            return (
                new ComplexCircle(center, TransferRadius),
                Math.IEEERemainder(startRad, Math.PI * 2),
                Math.IEEERemainder(endRad, Math.PI * 2)
            );
        }
    }
}
