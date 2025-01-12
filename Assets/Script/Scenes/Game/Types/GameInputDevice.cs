using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Game.Types
{
    public class GameInputDevice
    {
        public SensorType Area { get; init; } = SensorType.A1;
        public SensorStatus State { get; set; } = SensorStatus.Off;
        public bool IsUsedInThisFrame { get; set; } = false;
    }
}
