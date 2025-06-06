﻿using MajdataPlay.Game.Notes;
using MajdataPlay.Game.Notes.Touch;
using MajdataPlay.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Game.Buffers
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
