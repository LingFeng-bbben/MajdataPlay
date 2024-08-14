using System.Collections.Generic;
using System.Linq;

namespace MajdataPlay.Types
{
    public class JudgeAreaGroup
    {
        List<JudgeArea> areas = new();
        public bool IsFinished
        {
            get => areas.All(x => x.IsFinished);
        }
        public bool On
        {
            get
            {
                return areas.All(a => a.On);
            }
        }
        public int SlideIndex { get; set; }

        public JudgeAreaGroup(List<JudgeArea> areas, int slideIndex)
        {
            this.areas = areas;
            SlideIndex = slideIndex;
            foreach (JudgeArea area in areas)
                area.Reset();
        }
        public void Judge(SensorType type, SensorStatus status)
        {
            foreach (var area in areas)
                area.Judge(type, status);
        }
        public SensorType[] GetSensorTypes() => areas.SelectMany(x => x.GetSensorTypes()).ToArray();
    }
}
