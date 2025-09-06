using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Settings
{
    internal class ChartSetting
    {
        [SettingVisualizationIgnore]
        public string Hash { get; init; }
        [SettingVisualizationIgnore]
        public OffsetUnitOption Unit { get; set; } = OffsetUnitOption.Second;
        public float AudioOffset { get; set; } = 0f;
        public float TrackVolumeOffset { get; set; } = 0f;
    }
}
