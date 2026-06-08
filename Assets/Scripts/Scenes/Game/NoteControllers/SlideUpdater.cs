using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.Collections;
using MajdataPlay.Scenes.Game.Buffers;
using MajdataPlay.Scenes.Game.Notes.Behaviours;
using MajdataPlay.Scenes.View;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Profiling;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Controllers
{
    internal sealed class SlideUpdater : NoteUpdater
    {
        const string UPDATER_NAME = "SlideUpdater";
        const string PRE_UPDATE_METHOD_NAME = UPDATER_NAME + ".PreUpdate";
        const string UPDATE_METHOD_NAME = UPDATER_NAME + ".Update";
        const string FIXED_UPDATE_METHOD_NAME = UPDATER_NAME + ".FixedUpdate";
        const string LATE_UPDATE_METHOD_NAME = UPDATER_NAME + ".LateUpdate";

        ReadOnlyMemory<SlideQueueInfo> _queueInfos = ReadOnlyMemory<SlideQueueInfo>.Empty;

        readonly RentedList<SlideBinding> _slideBindings = new RentedList<SlideBinding>();

        readonly RentedList<NoteInfo> _slidePreUpdatableComponents = new RentedList<NoteInfo>(1024);
        readonly RentedList<NoteInfo> _slideUpdatableComponents = new RentedList<NoteInfo>(1024);
        readonly RentedList<NoteInfo> _slideLateUpdatableComponents = new RentedList<NoteInfo>(1024);

        readonly RentedList<NoteInfo> _otherPreUpdatableComponents = new RentedList<NoteInfo>();
        readonly RentedList<NoteInfo> _otherUpdatableComponents = new RentedList<NoteInfo>();
        readonly RentedList<NoteInfo> _otherLateUpdatableComponents = new RentedList<NoteInfo>();

        int _slideBindingCursor = 0;

        SlideQueueInfo[] _rentedArrayForQueueInfos = Array.Empty<SlideQueueInfo>();
        INoteTimeProvider _noteTimeProvider;
        

        void Awake()
        {
            Majdata<SlideUpdater>.Instance = this;
        }
        public override async UniTask InitAsync()
        {
            await base.InitAsync();
            using var slideComponents = new RentedList<NoteInfo>();
            for (var i = 0; i < Components.Length; i++)
            {
                var component = Components.Span[i];
                if (component.Component is SlideBase)
                {
                    slideComponents.Add(component);
                }
                else
                {
                    if (component.IsPreUpdatable)
                    {
                        _otherPreUpdatableComponents.Add(component);
                    }
                    if (component.IsUpdatable)
                    {
                        _otherUpdatableComponents.Add(component);
                    }
                    if (component.IsLateUpdatable)
                    {
                        _otherLateUpdatableComponents.Add(component);
                    }
                }
            }

            for (var i = 0; i < _queueInfos.Length; i++)
            {
                var queueInfo = _queueInfos.Span[i];
                var compo = slideComponents.Find(x => (object?)x.Component == (object?)queueInfo.SlideObject);
                if(compo is null)
                {
                    continue;
                }
                _slideBindings.Add(new()
                {
                    AppearTiming = queueInfo.AppearTiming - 0.15f,
                    QueueInfo = queueInfo,
                    NoteInfo = compo
                });
            }
        }
        protected override void OnDestroy()
        {
            Majdata<SlideUpdater>.Free();
            base.OnDestroy();
            Clear();
        }
        private void Start()
        {
            _noteTimeProvider = Majdata<INoteController>.Instance!;
        }
        internal override void Clear()
        {
            base.Clear();
            _queueInfos = ReadOnlyMemory<SlideQueueInfo>.Empty;
            Pool<SlideQueueInfo>.ReturnArray(_rentedArrayForQueueInfos, true);
            _rentedArrayForQueueInfos = Array.Empty<SlideQueueInfo>();
        }
        internal void AddSlideQueueInfos(IEnumerable<SlideQueueInfo> infos)
        {
            if (infos is null)
            {
                throw new ArgumentNullException();
            }
            using var buffer = new RentedList<SlideQueueInfo>();
            buffer.AddRange(infos.Where(x => x is not null).OrderBy(x => x.AppearTiming));
            _rentedArrayForQueueInfos = Pool<SlideQueueInfo>.RentArray(buffer.Count, true);
            var queueInfos = _rentedArrayForQueueInfos.AsMemory(0, buffer.Count);
            buffer.CopyTo(queueInfos.Span);
            _queueInfos = queueInfos;
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal new void OnFixedUpdate()
        {
            Profiler.BeginSample(FIXED_UPDATE_METHOD_NAME);
            base.OnFixedUpdate();
            Profiler.EndSample();
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal new void OnLateUpdate()
        {
            using (UnityProfiler.Create(LATE_UPDATE_METHOD_NAME))
            {
                var start = MajTimeline.UnscaledTime;
                var slideComponents = _slideLateUpdatableComponents;
                var len = slideComponents.Count;
                for (var i = 0; i < len; i++)
                {
                    var component = slideComponents[i];
                    if (component.State == NoteStatus.End)
                    {
                        slideComponents.RemoveAt(i);
                        i--;
                        len--;
                        continue;
                    }
                    component.OnLateUpdate();
                }
                var otherComponents = _otherLateUpdatableComponents;
                len = otherComponents.Count;
                for (var i = 0; i < len; i++) 
                {
                    var component = otherComponents[i];
                    if(component.State is NoteStatus.Start)
                    {
                        continue;
                    }
                    else if (component.State == NoteStatus.End)
                    {
                        otherComponents.RemoveAt(i);
                        i--;
                        len--;
                        continue;
                    }
                    component.OnLateUpdate();
                }

                var end = MajTimeline.UnscaledTime;
                var timeSpan = end - start;
                IntUpdateElapsedMs = timeSpan.TotalMilliseconds;
            }
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal new void OnUpdate()
        {
            using (UnityProfiler.Create(UPDATE_METHOD_NAME))
            {
                var start = MajTimeline.UnscaledTime;
                var slideComponents = _slideUpdatableComponents;
                var len = slideComponents.Count;
                for (var i = 0; i < len; i++)
                {
                    var component = slideComponents[i];
                    if (component.State == NoteStatus.End)
                    {
                        slideComponents.RemoveAt(i);
                        i--;
                        len--;
                        continue;
                    }
                    component.OnUpdate();
                }

                var otherComponents = _otherUpdatableComponents;
                len = otherComponents.Count;
                for (var i = 0; i < len; i++)
                {
                    var component = otherComponents[i];
                    if (component.State is NoteStatus.Start)
                    {
                        continue;
                    }
                    else if (component.State == NoteStatus.End)
                    {
                        otherComponents.RemoveAt(i);
                        i--;
                        len--;
                        continue;
                    }
                    component.OnUpdate();
                }

                var end = MajTimeline.UnscaledTime;
                var timeSpan = end - start;
                IntUpdateElapsedMs = timeSpan.TotalMilliseconds;
            }
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal new void OnPreUpdate()
        {
            using (UnityProfiler.Create(PRE_UPDATE_METHOD_NAME))
            {
                var thisFrameSec = _noteTimeProvider.ThisFrameSec;
                if (_queueInfos.Length - _slideBindingCursor > 0)
                {
                    for (ref var i = ref _slideBindingCursor; i < _slideBindings.Count; i++)
                    {
                        var binding = _slideBindings[i];
                        var queueInfo = binding.QueueInfo;
                        var noteInfo = binding.NoteInfo;
                        var appearTiming = binding.AppearTiming;
                        if (thisFrameSec >= appearTiming)
                        {
                            queueInfo.SlideObject.SetActive(true);
                            if (noteInfo.IsPreUpdatable)
                            {
                                _slidePreUpdatableComponents.Add(noteInfo);
                            }
                            if (noteInfo.IsUpdatable)
                            {
                                _slideUpdatableComponents.Add(noteInfo);
                            }
                            if (noteInfo.IsLateUpdatable)
                            {
                                _slideLateUpdatableComponents.Add(noteInfo);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                var start = MajTimeline.UnscaledTime;
                var slideComponents = _slidePreUpdatableComponents;
                var len = slideComponents.Count;
                for (var i = 0; i < len; i++)
                {
                    var component = slideComponents[i];
                    if (component.State is NoteStatus.Start)
                    {
                        continue;
                    }
                    else if (component.State == NoteStatus.End)
                    {
                        slideComponents.RemoveAt(i);
                        i--;
                        len--;
                        continue;
                    }
                    component.OnPreUpdate();
                }

                var otherComponents = _otherPreUpdatableComponents;
                len = otherComponents.Count;
                for (var i = 0; i < len; i++)
                {
                    var component = otherComponents[i];
                    if (component.State == NoteStatus.End)
                    {
                        otherComponents.RemoveAt(i);
                        i--;
                        len--;
                        continue;
                    }
                    component.OnPreUpdate();
                }

                var end = MajTimeline.UnscaledTime;
                var timeSpan = end - start;
                IntPreUpdateElapsedMs = timeSpan.TotalMilliseconds;
            }
        }
        struct SlideBinding
        {
            public required float AppearTiming { get; init; }
            public required SlideQueueInfo QueueInfo { get; init; }
            public required NoteInfo NoteInfo { get; init; }
        }
    }
}
