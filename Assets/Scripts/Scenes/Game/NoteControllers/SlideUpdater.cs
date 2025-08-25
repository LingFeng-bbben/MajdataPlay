﻿using MajdataPlay.Scenes.Game.Buffers;
using MajdataPlay.Utils;
using MajdataPlay.Scenes.View;
using System;
using System.Linq;
using UnityEngine.Profiling;
using MajdataPlay.Buffers;
using System.Collections.Generic;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Controllers
{
    internal sealed class SlideUpdater : NoteUpdater
    {
        ReadOnlyMemory<SlideQueueInfo> _queueInfos = ReadOnlyMemory<SlideQueueInfo>.Empty;

        SlideQueueInfo[] _rentedArrayForQueueInfos = Array.Empty<SlideQueueInfo>();
        INoteTimeProvider _noteTimeProvider;

        const string UPDATER_NAME = "SlideUpdater";
        const string PRE_UPDATE_METHOD_NAME = UPDATER_NAME + ".PreUpdate";
        const string UPDATE_METHOD_NAME = UPDATER_NAME + ".Update";
        const string FIXED_UPDATE_METHOD_NAME = UPDATER_NAME + ".FixedUpdate";
        const string LATE_UPDATE_METHOD_NAME = UPDATER_NAME + ".LateUpdate";

        void Awake()
        {
            Majdata<SlideUpdater>.Instance = this;
        }
        protected override void OnDestroy()
        {
            Majdata<SlideUpdater>.Free();
            base.OnDestroy();
            _queueInfos = ReadOnlyMemory<SlideQueueInfo>.Empty;
            Pool<SlideQueueInfo>.ReturnArray(_rentedArrayForQueueInfos, true);
            _rentedArrayForQueueInfos = Array.Empty<SlideQueueInfo>();
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
            buffer.AddRange(infos.OrderBy(x => x.AppearTiming));
            _rentedArrayForQueueInfos = Pool<SlideQueueInfo>.RentArray(buffer.Count, true);
            var queueInfos = _rentedArrayForQueueInfos.AsMemory(0, buffer.Count);
            buffer.CopyTo(queueInfos.Span);
            _queueInfos = queueInfos;
        }
        internal override void OnFixedUpdate()
        {
            Profiler.BeginSample(FIXED_UPDATE_METHOD_NAME);
            base.OnFixedUpdate();
            Profiler.EndSample();
        }
        internal override void OnLateUpdate()
        {
            Profiler.BeginSample(LATE_UPDATE_METHOD_NAME);
            base.OnLateUpdate();
            Profiler.EndSample();
        }
        internal override void OnUpdate()
        {
            Profiler.BeginSample(UPDATE_METHOD_NAME);
            base.OnUpdate();
            Profiler.EndSample();
        }
        internal override void OnPreUpdate()
        {
            Profiler.BeginSample(PRE_UPDATE_METHOD_NAME);
            var thisFrameSec = _noteTimeProvider.ThisFrameSec;
            if (!_queueInfos.IsEmpty)
            {
                var i = 0;
                var queueInfos = _queueInfos.Span;
                for (; i < queueInfos.Length; i++)
                {
                    var info = queueInfos[i];
                    if (info is null)
                    {
                        continue;
                    }
                    var appearTiming = info.AppearTiming;
                    if (thisFrameSec >= appearTiming)
                    {
                        info.SlideObject.SetActive(true);
                    }
                    else
                    {
                        break;
                    }
                }
                _queueInfos = _queueInfos.Slice(i);
            }
            base.OnPreUpdate();
            Profiler.EndSample();
        }
    }
}
