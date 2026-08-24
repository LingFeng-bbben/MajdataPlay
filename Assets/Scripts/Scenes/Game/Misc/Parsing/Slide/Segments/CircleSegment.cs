using MajdataPlay.Scenes.Game.Parsing;
using System;
using System.Numerics;

namespace MajdataPlay.Scenes.Game.Parsing.Slide.Segments
{
    /// <summary>
    /// <p>slide 圆周片段，总之就是转一整圈</p>
    /// </summary>
    public class CircleSegment : PathSegment
    {
        public readonly ComplexCircle Circle;
        public readonly double StartRadian;
        public readonly bool IsCcw;

        public CircleSegment(ComplexCircle circle, double startRadian, bool isCcw)
        {
            Circle = circle;
            StartRadian = startRadian;
            IsCcw = isCcw;
        }

        public override bool IsCurve { get; } = true;

        public override Complex GetPointAt(double t)
        {
            double angle;
            if (IsCcw)
            {
                angle = StartRadian + t * Math.PI * 2.0;
            }
            else
            {
                angle = StartRadian - t * Math.PI * 2.0;
            }

            return Circle.Center + Complex.FromPolarCoordinates(Circle.Radius, angle);
        }

        public override Complex GetTangentAt(double t)
        {
            double angle;
            if (IsCcw)
            {
                angle = StartRadian + t * Math.PI * 2.0;
                return Complex.FromPolarCoordinates(1, angle) * Complex.ImaginaryOne;
            }
            else
            {
                angle = StartRadian - t * Math.PI * 2.0;
                return Complex.FromPolarCoordinates(-1, angle) * Complex.ImaginaryOne;
            }
        }

        public override double GetSegmentLength()
        {
            return Math.PI * Circle.Radius * 2.0;
        }
    }
}
