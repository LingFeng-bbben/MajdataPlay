using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Types
{
    public sealed class TapPoolingInfo : NotePoolingInfo
    {
        public bool IsStar { get; init; }
        public bool IsDouble { get; init; }
        public bool IsFakeStar { get; init; }
        public bool IsFakeStarRotate { get; init; }
        public float RotateSpeed { get; init; } = 1f;
        public GameObject? Slide { get; init; } = null;
    }
}
