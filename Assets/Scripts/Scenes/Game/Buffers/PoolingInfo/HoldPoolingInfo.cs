using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Game.Buffers
{
    internal class HoldPoolingInfo : NotePoolingInfo
    {
        public float LastFor { get; init; }
        public TapQueueInfo QueueInfo { get; init; } = TapQueueInfo.Default;
    }
}
