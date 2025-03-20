using MajdataPlay.Game.Buffers;
using MajdataPlay.Utils;
using MajdataPlay.View;
using System;
using System.Linq;
using UnityEngine.Profiling;
#nullable enable
namespace MajdataPlay.Game.Notes.Controllers
{
    public class SlideUpdater : NoteUpdater
    {
        Memory<SlideQueueInfo> _queueInfos = Memory<SlideQueueInfo>.Empty;

        INoteTimeProvider _noteTimeProvider;

        const string UPDATER_NAME = "SlideUpdater";
        const string PRE_UPDATE_METHOD_NAME = UPDATER_NAME + ".PreUpdate";
        const string UPDATE_METHOD_NAME = UPDATER_NAME + ".Update";
        const string FIXED_UPDATE_METHOD_NAME = UPDATER_NAME + ".FixedUpdate";
        const string LATE_UPDATE_METHOD_NAME = UPDATER_NAME + ".LateUpdate";
        private void Awake()
        {
            Majdata<SlideUpdater>.Instance = this;
        }
        private void Start()
        {
            _noteTimeProvider = Majdata<INoteController>.Instance!;
        }
        internal override void Clear()
        {
            base.Clear();
            _queueInfos = Array.Empty<SlideQueueInfo>();
        }
        internal void AddSlideQueueInfos(SlideQueueInfo[] infos)
        {
            if (infos is null)
                throw new ArgumentNullException();
            _queueInfos = infos.OrderBy(x => x.AppearTiming)
                               .ToArray();
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
                        continue;
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
        private void OnDestroy()
        {
            Majdata<SlideUpdater>.Free();
        }
    }
}
