using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    internal class MaidataAnalyzeResult
    {
        public float PeakDensity { get; init; }
        public float Esti { get; init; }
        public TimeSpan Length { get; init; }
        public float MaxBPM { get; init; }
        public float MinBPM { get; init; }
        public required Texture LineGraph { get; init; }
    }
}
