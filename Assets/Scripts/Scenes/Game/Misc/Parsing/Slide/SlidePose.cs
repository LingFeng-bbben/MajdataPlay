using System;
using System.Collections.Generic;
using System.Text;

namespace MajdataPlay.Scenes.Game.Parsing.Slide
{
    /// <summary>
    /// slide 箭头信息
    /// </summary>
    internal readonly struct SlidePose
    {
        public readonly float X, Y, RotZ, L;

        public SlidePose(float x, float y, float rotZ, float l)
        {
            X = x;
            Y = y;
            RotZ = rotZ;
            L = l;
        }
    }
}
