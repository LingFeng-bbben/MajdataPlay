using MajdataPlay.Scenes.Game.Notes.Behaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Scenes.Game.Buffers
{
    internal class SlideQueueInfo : NoteQueueInfo
    {
        public float AppearTiming { get; init; }
        public SlideBase SlideObject { get; init; }
    }
}
