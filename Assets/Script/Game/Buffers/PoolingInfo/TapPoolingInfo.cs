using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Buffers
{
    public sealed class TapPoolingInfo : NotePoolingInfo
    {
        public bool IsStar { get; init; }
        public bool IsDouble { get; init; }
        public float RotateSpeed { get; set; } = 1f;
        public TapQueueInfo QueueInfo { get; init; } = TapQueueInfo.Default;
    }
}
