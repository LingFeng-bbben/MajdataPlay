using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable
namespace MajdataPlay.Game.Types
{
    public class SlideArea
    {
        public bool On => _isOn;
        public bool IsSkippable { get; set; } = true;
        public bool IsFinished
        {
            get
            {
                if (_areas.Length == 0)
                    return false;
                else
                    return _isFinished;
            }
        }
        public ReadOnlySpan<SensorType> IncludedAreas => _includedAreas.Span;
        public AreaPolicy Policy { get; init; } = AreaPolicy.OR;
        public int SlideIndex { get; set; }

        bool _isFinished = false;
        bool _isOn = false;

        Memory<Area> _areas = Memory<Area>.Empty;
        ReadOnlyMemory<SensorType> _includedAreas = Memory<SensorType>.Empty;
        
        public SlideArea(Dictionary<SensorType, bool> types, int slideIndex)
        {
            SlideIndex = slideIndex;
            if (types is null || types.Count == 0)
                return;
            Span<SensorType?> registeredAreas = stackalloc SensorType?[types.Count];
            Span<Area?> areas = stackalloc Area?[types.Count];
            var i = 0;
            var i2 = 0;
            foreach (var item in types)
            {
                var area = item.Key;
                var isLast = item.Value;
                if (registeredAreas.Any(x => x == area))
                    continue;

                registeredAreas[i++] = area;
                areas[i2++] = new Area()
                {
                    TargetArea = area,
                    IsLast = isLast
                };
            }

            Span<SensorType> _registeredAreas = stackalloc SensorType[i];
            Span<Area> _areas = stackalloc Area[i2];

            for (var j = 0; j < i; j++)
            {
                _registeredAreas[j] = (SensorType)registeredAreas[j]!;
            }
            for (var j = 0; j < i2; j++)
            {
                _areas[j] = (Area)areas[j]!;
            }
            this._areas = _areas.ToArray();
            _includedAreas = _registeredAreas.ToArray();
            SlideIndex = slideIndex;
        }
        public SlideArea()
        {

        }
        public void Mirror(SensorType baseLine)
        {
            Area[] newAreas = new Area[_areas.Length];
            SensorType[] includedAreas = new SensorType[_includedAreas.Length];
            foreach (var (i, area) in _areas.Span.WithIndex())
            {
                newAreas[i] = new Area()
                {
                    TargetArea = area.TargetArea.Mirror(baseLine),
                    IsLast = area.IsLast
                };
            }
            foreach(var (i,area) in _includedAreas.Span.WithIndex())
            {
                includedAreas[i] = area.Mirror(baseLine);
            }
            _areas = newAreas;
            _includedAreas = includedAreas;
        }
        public void Diff(int diff)
        {
            Area[] newAreas = new Area[_areas.Length];
            SensorType[] includedAreas = new SensorType[_includedAreas.Length];
            foreach (var (i, area) in _areas.Span.WithIndex())
            {
                newAreas[i] = new Area()
                {
                    TargetArea = area.TargetArea.Diff(diff),
                    IsLast = area.IsLast
                };
            }
            foreach (var (i, area) in _includedAreas.Span.WithIndex())
            {
                includedAreas[i] = area.Diff(diff);
            }
            _areas = newAreas;
            _includedAreas = includedAreas;
        }
        public void SetIsLast()
        {
            var span = _areas.Span;
            for (var i = 0; i < span.Length; i++)
            {
                ref var area = ref span[i];
                area.IsLast = true;
            }
        }
        public void SetNonLast()
        {
            var span = _areas.Span;
            for (var i = 0; i < span.Length; i++)
            {
                ref var area = ref span[i];
                area.IsLast = false;
            }
        }
        public void Check(in SensorType targetArea, in SensorStatus state)
        {
            if (_areas.Length == 0)
                return;
            var span = _areas.Span;
            var isOn = false;
            var isFinished = Policy switch
            {
                AreaPolicy.AND => true,
                _ => false
            };
            for (var i = 0;i < span.Length; i++)
            {
                ref var area = ref span[i];
                area.Check(targetArea, state);

                isOn = isOn || area.On;
                if(Policy == AreaPolicy.AND)
                    isFinished = isFinished && area.IsFinished;
                else
                    isFinished = isFinished || area.IsFinished;
            }
            _isFinished = isFinished;
            _isOn = isOn;
        }
        struct Area
        {
            public bool On { get; private set; }
            public bool Off { get; private set; }
            public SensorType TargetArea { get; init; }
            public bool IsLast { get; set; }
            public bool IsFinished
            {
                get
                {
                    if (IsLast)
                        return On;
                    else
                        return On && Off;
                }
            }
            public void Check(in SensorType area, in SensorStatus status)
            {
                if (area != TargetArea)
                    return;
                if (status == SensorStatus.Off)
                {
                    if (On)
                    {
                        Off = true;
                    }
                }
                else
                {
                    On = true;
                }
            }
        }
    }
}
