using MajdataPlay.Game.Types;
using MajdataPlay.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Types
{
    public class TouchPoolingInfo: NotePoolingInfo, ITouchGroupInfoProvider
    {
        public char AreaPos { get; init; }
        public bool IsFirework { get; init; }
        public SensorType SensorPos { get; init; }
        public TouchQueueInfo QueueInfo { get; init; } = TouchQueueInfo.Default;
        public TouchGroup? GroupInfo { get; set; } = null;
    }
}
