using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MajdataPlay.Scenes.Game.Parsing
{
    public readonly struct ComplexCircle
    {
        public readonly Complex Center;
        public readonly double Radius;

        public ComplexCircle(Complex center, double radius)
        {
            Center = center;
            Radius = radius;
        }
    }
}
