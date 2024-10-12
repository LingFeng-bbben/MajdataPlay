using System;
using System.Collections.Generic;
using System.Linq;

namespace MajdataPlay.Types
{
    public class JudgeArea
    {
        public bool On
        {
            get
            {
                return areas.Any(a => a.On);
            }
        }
        public bool CanSkip = true;
        public bool IsFinished
        {
            get
            {
                if (areas.Count == 0)
                    return false;
                else
                    return areas.Any(x => x.IsFinished);
            }
        }
        public int SlideIndex { get; set; }
        List<Area> areas = new();
        public Area[] GetAreas() => areas.ToArray();
        public SensorType[] GetSensorTypes() => areas.Select(x => x.Type).ToArray();
        public JudgeArea(Dictionary<SensorType, bool> types, int slideIndex)
        {
            SlideIndex = slideIndex;
            foreach (var item in types)
            {
                var type = item.Key;
                if (areas.Any(x => x.Type == type))
                    continue;

                areas.Add(new Area()
                {
                    Type = type,
                    IsLast = item.Value
                });
            }
            SlideIndex = slideIndex;
        }
        public JudgeArea()
        {

        }
        public void Mirror(SensorType baseLine)
        {
            foreach(var area in areas)
                area.Mirror(baseLine);
        }
        public void Diff(int diff)
        {
            foreach(var area in areas)
                area.Diff(diff);
        }
        public void SetIsLast() => areas.ForEach(x => x.IsLast = true);
        public void SetNonLast() => areas.ForEach(x => x.IsLast = false);
        public void Judge(SensorType type,in SensorStatus status)
        {
            var areaList = areas.Where(x => x.Type == type);

            if (areaList.Count() == 0)
                return;

            var area = areaList.First();
            area.Judge(status);
        }
        public void AddArea(SensorType type, bool isLast = false)
        {
            if (areas.Any(x => x.Type == type))
                return;
            areas.Add(new Area()
            {
                Type = type,
                IsLast = isLast
            });
        }
        public void Reset()
        {
            foreach (var area in areas)
                area.Reset();
        }
    }
}
