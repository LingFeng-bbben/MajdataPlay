using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Interfaces
{
    public interface ITouchGroupInfoProvider
    {
        SensorType SensorPos { get; init; }
        TouchGroup? GroupInfo { get; set; }
    }
}
