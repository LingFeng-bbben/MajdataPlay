using System.Numerics;

namespace MajdataPlay.Scenes.Game.Parsing.Slide.Segments
{
    /// <summary>
    /// <p>slide 直线片段</p>
    /// </summary>
    public class LineSegment : PathSegment
    {
        public readonly Complex StartPoint;
        public readonly Complex EndPoint;

        public LineSegment(Complex start, Complex end)
        {
            StartPoint = start;
            EndPoint = end;
        }

        public override bool IsCurve { get; } = false;

        public override Complex GetPointAt(double t)
        {
            return StartPoint + (EndPoint - StartPoint) * t;
        }

        public override Complex GetTangentAt(double t)
        {
            var v = EndPoint - StartPoint;
            return v / v.Magnitude;
        }

        public override double GetSegmentLength()
        {
            return (EndPoint - StartPoint).Magnitude;
        }
    }
}
