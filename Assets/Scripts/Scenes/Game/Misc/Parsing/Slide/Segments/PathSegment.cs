using MajdataPlay.Scenes.Game.Parsing;
using MajdataPlay.Scenes.Game.Parsing.Slide;
using System.Numerics;

namespace MajdataPlay.Scenes.Game.Parsing.Slide.Segments
{
    /// <summary>
    /// <p>一个 slide 路径片段的 abstract 类</p>
    /// </summary>
    public abstract class PathSegment
    {
        /// <summary>
        /// <p>用来控制箭头对齐的标志</p>
        /// </summary>
        public SlideParseMarker ParseMarker { get; private set; } = SlideParseMarker.None;

        /// <summary>
        /// <p>这个参数表示这段路径上每隔多少距离放一个箭头</p>
        /// <p>默认值是判定圆周长的 1/64</p>
        /// </summary>
        public double ArrowDistance { get; private set; } = SlideGeo.DefaultDistance;

        public abstract bool IsCurve { get; }

        /// <summary>
        /// <p>计算路径上某点的坐标，保证均匀插值</p>
        /// </summary>
        /// <param name="t">0 ~ 1 (both inclusive)</param>
        public abstract Complex GetPointAt(double t);

        /// <summary>
        /// <p>计算路径上某点处的有向切线，方向为路径前进方向</p>
        /// </summary>
        /// <param name="t">0 ~ 1 (both inclusive)</param>
        /// <returns>complex (magnitude = 1)</returns>
        public abstract Complex GetTangentAt(double t);

        /// <summary>
        /// <p>计算本段路径总长</p>
        /// </summary>
        public abstract double GetSegmentLength();

        /// <summary>
        /// <p>设置控制箭头对齐的标志，给 parser 用</p>
        /// </summary>
        public void SetParseMarker(SlideParseMarker marker) => ParseMarker = marker;

        /// <summary>
        /// <p>设置本段路径的箭头排列间距</p>
        /// </summary>
        public void SetArrowDistance(double distance) => ArrowDistance = distance;
    }
}
