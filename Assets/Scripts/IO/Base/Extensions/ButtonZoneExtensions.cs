using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.IO
{
    internal static class ButtonZoneExtensions
    {
        public static SensorArea ToSensorArea(this ButtonZone zone)
        {
            if (zone < ButtonZone.A1 || zone > ButtonZone.P2)
            {
                return ThrowHelper.Throw<ArgumentOutOfRangeException, SensorArea>(new ArgumentOutOfRangeException(nameof(zone)));
            }
            return (SensorArea)((int)zone);
        }
    }
}
