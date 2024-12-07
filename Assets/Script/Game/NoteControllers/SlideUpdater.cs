using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
#nullable enable
namespace MajdataPlay.Game
{
    public class SlideUpdater : NoteUpdater
    {
        SlideQueueInfo?[] _queueInfos = Array.Empty<SlideQueueInfo>();

        GamePlayManager _gpManager;
        private void Awake()
        {
            MajInstanceHelper<SlideUpdater>.Instance = this;
        }
        private void Start()
        {
            _gpManager = MajInstanceHelper<GamePlayManager>.Instance!;
        }
        public void AddSlideQueueInfos(SlideQueueInfo[] infos)
        {
            if (infos is null)
                throw new ArgumentNullException();
            _queueInfos = infos;
        }
        protected override void FixedUpdate() => base.FixedUpdate();
        protected override void LateUpdate() => base.LateUpdate();
        protected override void Update()
        {
            var gameTime = _gpManager.AudioTime;
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
            base.Update();
        }
        private void OnDestroy()
        {
            MajInstanceHelper<SlideUpdater>.Free();
        }
    }
}
