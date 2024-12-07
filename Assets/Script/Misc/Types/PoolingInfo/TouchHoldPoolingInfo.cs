using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Types
{
    public class TouchHoldPoolingInfo : NotePoolingInfo, ITouchGroupInfoProvider
    {
        public float LastFor { get; init; }
        public char AreaPos { get; init; }
        public bool IsFirework { get; init; }
        public SensorType SensorPos { get; init; }
        public TouchQueueInfo QueueInfo { get; init; } = TouchQueueInfo.Default;
        public TouchGroup? GroupInfo { get; set; } = null;
    }
}
