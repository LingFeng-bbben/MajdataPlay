using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace MajdataPlay.Settings
{
    [Preserve]
    internal class ChartSetting
    {
        [SettingVisualizationIgnore, Preserve]
        public string Hash { get; init; }
        [SettingVisualizationIgnore, Preserve]
        public OffsetUnitOption Unit { get; set; } = OffsetUnitOption.Second;
        [Preserve]
        public float AudioOffset { get; set; } = 0f;
        [Preserve]
        public float TrackVolumeOffset { get; set; } = 0f;
    }
}
