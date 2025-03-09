using MajdataPlay.Game.Buffers;
using MajdataPlay.Utils;
using MajdataPlay.View;
using System;
using System.Linq;
#nullable enable
namespace MajdataPlay.Game
{
    public class SlideUpdater : NoteUpdater
    {
        Memory<SlideQueueInfo> _queueInfos = Memory<SlideQueueInfo>.Empty;

        INoteTimeProvider _noteTimeProvider;
        private void Awake()
        {
            Majdata<SlideUpdater>.Instance = this;
        }
        private void Start()
        {
            _noteTimeProvider = Majdata<INoteTimeProvider>.Instance!;
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
        internal override void OnFixedUpdate() => base.OnFixedUpdate();
        internal override void OnLateUpdate() => base.OnLateUpdate();
        internal override void OnUpdate()
        {
            var thisFrameSec = _noteTimeProvider.ThisFrameSec;
            if(!_queueInfos.IsEmpty)
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
            base.OnUpdate();
        }
        private void OnDestroy()
        {
            Majdata<SlideUpdater>.Free();
        }
    }
}
