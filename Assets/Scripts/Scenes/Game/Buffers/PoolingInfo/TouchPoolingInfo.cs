using MajdataPlay.Scenes.Game.Notes;
using MajdataPlay.Scenes.Game.Notes.Touch;
using MajdataPlay.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Scenes.Game.Buffers
{
    internal class TouchPoolingInfo : NotePoolingInfo, ITouchGroupInfoProvider
    {
        public char AreaPos { get; init; }
        public bool IsFirework { get; init; }
        public SensorArea SensorPos { get; init; }
        public TouchQueueInfo QueueInfo { get; init; } = TouchQueueInfo.Default;
        public TouchGroup? GroupInfo { get; set; } = null;
    }
}
