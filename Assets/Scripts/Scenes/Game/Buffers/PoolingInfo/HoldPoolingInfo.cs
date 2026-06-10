using MajdataPlay.Game.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Scenes.Game.Buffers
{
    internal class HoldPoolingInfo : NotePoolingInfo
    {
        public float LastFor { get; init; }
        public TapQueueInfo QueueInfo { get; init; } = TapQueueInfo.Default;
        public EachLineBinding? EachLineBinding { get; set; }
    }
}
