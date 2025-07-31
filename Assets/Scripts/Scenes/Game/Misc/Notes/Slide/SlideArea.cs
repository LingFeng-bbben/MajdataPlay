using MajdataPlay.Collections;
using MajdataPlay.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Slide
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
                {
                    return false;
                }
                else
                {
                    return _isFinished;
                }
            }
        }
        public ReadOnlySpan<SensorArea> IncludedAreas => _includedAreas.Span;
        public AreaPolicy Policy { get; init; } = AreaPolicy.OR;
        public int ArrowProgressWhenFinished
        {
            get => _arrowProgressWhenFinished;
        }
        public int ArrowProgressWhenOn
        {
            get => _arrowProgressWhenOn;
        }
        bool _isFinished = false;
        bool _isOn = false;
        int _arrowProgressWhenFinished = 0;
        int _arrowProgressWhenOn = 0;

        Memory<Area> _areas = Memory<Area>.Empty;
        ReadOnlyMemory<SensorArea> _includedAreas = Memory<SensorArea>.Empty;

        public SlideArea(ReadOnlySpan<(SensorArea, bool)> types, int progressWhenOn, int progressWhenFinished)
        {
            if (types.Length == 0)
            {
                return;
            }
            Span<SensorArea?> registeredAreas = stackalloc SensorArea?[types.Length];
            Span<Area?> areas = stackalloc Area?[types.Length];
            var i = 0;
            var i2 = 0;
            foreach (var item in types)
            {
                var (area, isLast) = item;
                if (registeredAreas.Any(x => x == area))
                {
                    continue;
                }

                registeredAreas[i++] = area;
                areas[i2++] = new Area()
                {
                    TargetArea = area,
                    IsLast = isLast
                };
            }

            Span<SensorArea> _registeredAreas = stackalloc SensorArea[i];
            Span<Area> _areas = stackalloc Area[i2];

            for (var j = 0; j < i; j++)
            {
                _registeredAreas[j] = (SensorArea)registeredAreas[j]!;
            }
            for (var j = 0; j < i2; j++)
            {
                _areas[j] = (Area)areas[j]!;
            }
            this._areas = _areas.ToArray();
            _includedAreas = _registeredAreas.ToArray();
            _arrowProgressWhenFinished = progressWhenFinished;
            _arrowProgressWhenOn = progressWhenOn;
        }
        public SlideArea(ReadOnlySpan<(SensorArea, bool)> types, int arrowProgress) : this(types, arrowProgress, arrowProgress)
        {

        }
        public SlideArea()
        {

        }
        public void Mirror(SensorArea baseLine)
        {
            var newAreas = new Area[_areas.Length];
            var includedAreas = new SensorArea[_includedAreas.Length];
            foreach (var (i, area) in _areas.Span.WithIndex())
            {
                newAreas[i] = new Area()
                {
                    TargetArea = area.TargetArea.Mirror(baseLine),
                    IsLast = area.IsLast
                };
            }
            foreach (var (i, area) in _includedAreas.Span.WithIndex())
            {
                includedAreas[i] = area.Mirror(baseLine);
            }
            _areas = newAreas;
            _includedAreas = includedAreas;
        }
        public void Diff(int diff)
        {
            var newAreas = new Area[_areas.Length];
            var includedAreas = new SensorArea[_includedAreas.Length];
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
        public void Check(in SensorArea targetArea, in SwitchStatus state)
        {
            if (_areas.Length == 0)
            {
                return;
            }
            var span = _areas.Span;
            var isOn = false;
            var isFinished = Policy switch
            {
                AreaPolicy.AND => true,
                _ => false
            };
            for (var i = 0; i < span.Length; i++)
            {
                ref var area = ref span[i];
                area.Check(targetArea, state);

                isOn = isOn || area.On;
                if (Policy == AreaPolicy.AND)
                {
                    isFinished = isFinished && area.IsFinished;
                }
                else
                {
                    isFinished = isFinished || area.IsFinished;
                }
            }
            _isFinished = isFinished;
            _isOn = isOn;
        }
        struct Area
        {
            /// <summary>
            /// True if the target area has been triggered.
            /// </summary>
            public bool On
            {
                get
                {
                    return _wasOn;
                }
            }
            /// <summary>
            /// True if the target area has been untriggered and has been triggered.
            /// </summary>
            public bool Off
            {
                get
                {
                    return _wasOff;
                }
            }
            public SensorArea TargetArea { get; init; }
            public bool IsLast { get; set; }
            public bool IsFinished
            {
                get
                {
                    if (IsLast)
                    {
                        return _wasOn;
                    }
                    else
                    {
                        return _wasOn && _wasOff;
                    }
                }
            }

            bool _wasOn;
            bool _wasOff;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Check(in SensorArea area, in SwitchStatus status)
            {
                if (area != TargetArea)
                {
                    return;
                }
                if (status == SwitchStatus.Off)
                {
                    if (_wasOn)
                    {
                        _wasOff = true;
                    }
                }
                else
                {
                    _wasOn = true;
                }
            }
        }
    }
}
