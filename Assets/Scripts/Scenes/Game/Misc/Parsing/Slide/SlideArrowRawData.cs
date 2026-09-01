using System.Numerics;

namespace MajdataPlay.Scenes.Game.Parsing.Slide
{
    public readonly struct SlideArrowRawData
    {
        public readonly Complex Point;
        public readonly Complex Direction;
        public readonly double PathLength;

        public SlideArrowRawData(Complex point, Complex direction, double pathLength)
        {
            Point = point;
            Direction = direction;
            PathLength = pathLength;
        }
    }
}
