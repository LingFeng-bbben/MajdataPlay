using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game.Buffers
{
    internal sealed class TapPoolingInfo : NotePoolingInfo
    {
        public bool IsStar { get; init; }
        public bool IsDouble { get; init; }
        public float RotateSpeed { get; set; } = 0f;
        public TapQueueInfo QueueInfo { get; init; } = TapQueueInfo.Default;
    }
}
