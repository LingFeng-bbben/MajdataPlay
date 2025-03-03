using MajdataPlay.Game.Buffers;
using MajdataPlay.Utils;
using MajdataPlay.View;
using System;
#nullable enable
namespace MajdataPlay.Game
{
    public class SlideUpdater : NoteUpdater
    {
        SlideQueueInfo?[] _queueInfos = Array.Empty<SlideQueueInfo>();

        INoteTimeProvider _noteTimeProvider;
        private void Awake()
        {
            Majdata<SlideUpdater>.Instance = this;
        }
        private void Start()
        {
            if (MajEnv.Mode == RunningMode.Play)
            {
                _noteTimeProvider = Majdata<GamePlayManager>.Instance!;
            }
            else
            {
                _noteTimeProvider = Majdata<ViewManager>.Instance!;
            }
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
            _queueInfos = infos;
        }
        internal override void OnFixedUpdate() => base.OnFixedUpdate();
        internal override void OnLateUpdate() => base.OnLateUpdate();
        internal override void OnUpdate()
        {
            var gameTime = _noteTimeProvider.ThisFrameSec;
            for (var i = 0; i < _queueInfos.Length; i++)
            {
                ref var info = ref _queueInfos[i];
                if (info is null)
                    continue;
                var appearTiming = info.AppearTiming;
                if(gameTime >= appearTiming)
                {
                    info.SlideObject.SetActive(true);
                    info = null;
                }

            }
            base.OnUpdate();
        }
        private void OnDestroy()
        {
            Majdata<SlideUpdater>.Free();
        }
    }
}
