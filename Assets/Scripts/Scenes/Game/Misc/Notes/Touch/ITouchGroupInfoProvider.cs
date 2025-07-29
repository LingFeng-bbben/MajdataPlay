﻿using MajdataPlay.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Touch
{
    public interface ITouchGroupInfoProvider
    {
        SensorArea SensorPos { get; init; }
        TouchGroup? GroupInfo { get; set; }
    }
}
