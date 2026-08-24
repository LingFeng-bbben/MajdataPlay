using MajdataPlay.Scenes.Game.Parsing.Slide.Segments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MajdataPlay.Scenes.Game.Parsing.Slide
{

    /// <summary>
    /// <p>参数化的 slide 路径曲线</p>
    /// </summary>
    public class ParametricSlidePath
    {
        /// <summary>
        /// <p>组成这条路径的所有片段</p>
        /// </summary>
        public readonly PathSegment[] Segments;

        /// <summary>
        /// <p>片段之间的切割点，<c>Fractions[i]</c>是第 i 个片段的终点的 t 值</p>
        /// </summary>
        public readonly double[] Fractions;

        /// <summary>
        /// <p>片段累积长度，<c>AccumulatedLengths[i]</c>是到第 i 个片段的终点为止经过的总长度</p>
        /// </summary>
        public readonly double[] AccumulatedLengths;

        public ParametricSlidePath(IEnumerable<PathSegment> pathSegments)
        {
            Segments = pathSegments.ToArray();
            if (Segments.Length == 0)
            {
                throw new ArgumentException("At least one path segment is required.");
            }

            var lengths = Segments.Select(s => s.GetSegmentLength());
            var sum = 0.0;
            AccumulatedLengths = lengths.Select(x => (sum += x)).ToArray();
            Fractions = AccumulatedLengths.Select(x => x / sum).ToArray();
        }

        /// <summary>
        /// <p>获取 t 位置所在的片段</p>
        /// </summary>
        /// <param name="t">0 ~ 1 (both inclusive)</param>
        /// <param name="segmentT">这个点在返回片段上的 t 值，0 ~ 1 (both inclusive)</param>
        /// <returns>片段对象</returns>
        public PathSegment GetSegmentAt(double t, out double segmentT)
        {
            if (t <= 0.0)
            {
                segmentT = 0.0;
                return Segments[0];
            }

            if (t >= 1.0)
            {
                segmentT = 1.0;
                return Segments[^1];
            }

            // The index of the specified value in the specified array, if value is found;
            // otherwise, a negative number.
            //
            // If value is not found and value is less than one or more elements in array,
            // the negative number returned is the bitwise complement of the index of the first element
            // that is larger than value.
            //
            // If value is not found and value is greater than all elements in array,
            // the negative number returned is the bitwise complement of (the index of the last element plus 1).
            var idx = Array.BinarySearch(Fractions, t);

            if (idx < 0)
            {
                // t 不在 Fractions 里，此时 idx 的按位取反是 第一个比 t 大的元素 的索引
                idx = ~idx;
            }
            // 如果 idx >= 0 说明 t 在 Fractions 里，此时 idx 就是 t 的索引
            // 所以无论怎样都有 Fractions[idx-1] < t && Fractions[idx] >= t
            // Note: Fractions[i] 是 Segments[i] 的终点

            if (idx >= Segments.Length)
            {
                // t 超出了最后一段的终点，按理来说这不可能，因为越界情况已经处理掉了
                // 但可能会有舍入误差什么的……
                segmentT = 1.0;
                return Segments[^1];
            }

            if (idx == 0)
            {
                segmentT = t / Fractions[0];
                return Segments[0];
            }

            segmentT = (t - Fractions[idx - 1]) / (Fractions[idx] - Fractions[idx - 1]);
            return Segments[idx];
        }

        /// <summary>
        /// <p>计算路径总长</p>
        /// </summary>
        public double GetPathLength() => AccumulatedLengths[^1];

        /// <summary>
        /// 是否关于“A1 - C - A5”反射路径
        /// </summary>
        public bool Reflected { get; private set; } = false;
        /// <summary>
        /// 顺时针旋转路径，多少个 45 度
        /// </summary>
        public int Rotated45CW { get; private set; } = 0;

        private Complex _rotor = Complex.One;

        /// <summary>
        /// 反射并旋转整条路径，先反射再旋转
        /// </summary>
        /// <param name="reflect">是否关于“A1 - C - A5”反射</param>
        /// <param name="rotate45CW">顺时针旋转多少个 45 度，0~7</param>
        public void SetRotoreflection(bool reflect, int rotate45CW)
        {
            Rotated45CW = rotate45CW;
            Reflected = reflect;
            var angle = reflect ? (135 - rotate45CW * 45) : (-rotate45CW * 45);
            _rotor = Complex.FromPolarCoordinates(1, angle / 180.0 * Math.PI);
        }

        private Complex CalcRotoreflection(Complex z)
        {
            var x = Reflected ? Complex.Conjugate(z) : z;
            return x * _rotor;
        }

        /// <summary>
        /// <p>计算路径上某点的坐标，保证均匀插值</p>
        /// </summary>
        /// <param name="t">0 ~ 1 (both inclusive)</param>
        public virtual Complex GetPointAt(double t)
        {
            var segment = GetSegmentAt(t, out var segT);
            return CalcRotoreflection(segment.GetPointAt(segT));
        }

        /// <summary>
        /// <p>计算路径上某点处的有向切线，方向为路径前进方向</p>
        /// </summary>
        /// <param name="t">0 ~ 1 (both inclusive)</param>
        /// <returns>complex (magnitude = 1)</returns>
        public virtual Complex GetTangentAt(double t)
        {
            var segment = GetSegmentAt(t, out var segT);
            return CalcRotoreflection(segment.GetTangentAt(segT));
        }

        public virtual SlideEndShape GetEndShape()
        {
            var lastSegment = Segments[^1];
            if (lastSegment is CircleSegment circle)
            {
                return circle.IsCcw != Reflected ? SlideEndShape.CircleCCW : SlideEndShape.CircleCW;
            }

            if (lastSegment is ArcSegment arc)
            {
                return (arc.EndRadian > arc.StartRadian) != Reflected ? SlideEndShape.CircleCCW : SlideEndShape.CircleCW;
            }

            return SlideEndShape.Straight;
        }
    }
}
