using MajdataPlay.Buffers;
using MajdataPlay.Game.Buffers;
using MajdataPlay.Game.Notes.Controllers;
using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using UnityEngine;
using UnityEngine.U2D;
#nullable enable
namespace MajdataPlay.Game.Notes.Behaviours
{
    internal class EachLineDrop : MajComponent, IPoolableNote<EachLinePoolingInfo, NoteQueueInfo>, IStateful<NoteStatus>, IRendererContainer
    {
        public RendererStatus RendererState
        {
            get => _rendererState;
            set
            {
                if (State < NoteStatus.Initialized)
                    return;
                switch (value)
                {
                    case RendererStatus.Off:
                        sr.forceRenderingOff = true;
                        break;
                    case RendererStatus.On:
                        sr.forceRenderingOff = false;
                        break;
                }
            }
        }
        public IDistanceProvider? DistanceProvider { get; set; }
        public IStatefulNote? NoteA { get; set; }
        public IStatefulNote? NoteB { get; set; }
        public NoteStatus State { get; set; } = NoteStatus.Start;
        public bool IsDestroyed => State == NoteStatus.End;
        public NoteQueueInfo QueueInfo => TapQueueInfo.Default;
        public bool IsInitialized => State >= NoteStatus.Initialized;

        public float timing;
        public int startPosition = 1;
        public int curvLength = 1;
        public float speed = 1;
        float _noteAppearRate = 0.265f;
        public Sprite[] curvSprites;
        private SpriteRenderer sr;
        GameSetting gameSetting = new();
        INoteController _noteController;
        NotePoolManager poolManager;
        public void Initialize(EachLinePoolingInfo poolingInfo)
        {
            if (State >= NoteStatus.Initialized && State < NoteStatus.End)
                return;
            startPosition = poolingInfo.StartPos;
            timing = poolingInfo.Timing;
            speed = poolingInfo.Speed;
            curvLength = poolingInfo.CurvLength;
            sr.sprite = curvSprites[curvLength - 1];
            Transform.localScale = new Vector3(1.225f / 4.8f, 1.225f / 4.8f, 1f);
            Transform.rotation = Quaternion.Euler(0, 0, -45f * (startPosition - 1));
            State = NoteStatus.Initialized;
            RendererState = RendererStatus.Off;
            if (DistanceProvider is null)
                MajDebug.LogWarning("DistanceProvider not found");
        }
        public void End()
        {
            State = NoteStatus.End;
            RendererState = RendererStatus.Off;
            NoteA = null;
            NoteB = null;
            DistanceProvider = null;
            poolManager.Collect(this);
        }
        protected override void Awake()
        {
            base.Awake();
            _noteController = Majdata<INoteController>.Instance!;
            poolManager = FindObjectOfType<NotePoolManager>();
            gameSetting = MajInstances.Settings;
            _noteAppearRate = gameSetting.Debug.NoteAppearRate;
            sr = gameObject.GetComponent<SpriteRenderer>();
            sr.sprite = curvSprites[curvLength - 1];
            RendererState = RendererStatus.Off;
            sr.forceRenderingOff = true;
            Active = true;
        }
        [OnLateUpdate]
        void OnLateUpdate()
        {
            if (State < NoteStatus.Initialized || IsDestroyed)
                return;
            var timing = _noteController.ThisFrameSec - this.timing;
            var distance = DistanceProvider is not null ? DistanceProvider.Distance : timing * speed + 4.8f;
            var scaleRate = _noteAppearRate;
            var destScale = distance * scaleRate + (1 - scaleRate * 1.225f);
            var lineScale = Mathf.Abs(distance / 4.8f);

            switch (State)
            {
                case NoteStatus.Initialized:
                    if (destScale >= 0f)
                    {
                        RendererState = RendererStatus.Off;

                        State = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    return;
                case NoteStatus.Scaling:
                    if (destScale > 0.3f)
                        RendererState = RendererStatus.On;
                    if (distance < 1.225f)
                        Transform.localScale = new Vector3(1.225f / 4.8f, 1.225f / 4.8f, 1f);
                    else
                    {
                        State = NoteStatus.Running;
                        goto case NoteStatus.Running;
                    }
                    break;
                case NoteStatus.Running:
                    Transform.localScale = new Vector3(lineScale, lineScale, 1f);
                    if (NoteA is not null && NoteB is not null)
                    {
                        if (NoteA.State == NoteStatus.End || NoteB.State == NoteStatus.End)
                        {
                            End();
                            return;
                        }
                    }
                    else if (timing > 0)
                    {
                        End();
                        return;
                    }
                    break;
            }
        }
        void OnDestroy()
        {
            Active = false;
        }
        RendererStatus _rendererState = RendererStatus.Off;
    }
}