using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public class EachLineDrop : MonoBehaviour,IPoolableNote<EachLinePoolingInfo,NoteQueueInfo>,IRendererContainer
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
        public NoteQueueInfo QueueInfo => TapQueueInfo.Default;
        public GameObject GameObject => gameObject;
        public bool IsInitialized => State >= NoteStatus.Initialized;

        public float timing;
        public int startPosition = 1;
        public int curvLength = 1;
        public float speed = 1;
        public Sprite[] curvSprites;
        private SpriteRenderer sr;

        private GamePlayManager gpManager;

        public void Initialize(EachLinePoolingInfo poolingInfo)
        {
            if (State >= NoteStatus.Initialized && State < NoteStatus.Destroyed)
                return;
            startPosition = poolingInfo.StartPos;
            timing = poolingInfo.Timing;
            speed = poolingInfo.Speed;
            curvLength = poolingInfo.CurvLength;
            if (State == NoteStatus.Start)
                Start();
            sr.sprite = curvSprites[curvLength - 1];
            State = NoteStatus.Initialized;
        }
        public void End(bool forceEnd = false)
        {
            State = NoteStatus.Destroyed;
            if (forceEnd)
                return;
            RendererState = RendererStatus.Off;
            NoteA = null;
            NoteB = null;
            DistanceProvider = null;
        }
        private void Start()
        {
            if (IsInitialized)
                return;
            gpManager = GamePlayManager.Instance;

            sr = gameObject.GetComponent<SpriteRenderer>();
            sr.sprite = curvSprites[curvLength - 1];
            sr.forceRenderingOff = true;
        }
        private void Update()
        {
            var timing = gpManager.AudioTime - this.timing;
            float distance;

            if (DistanceProvider is not null)
                distance = DistanceProvider.Distance;
            else
                distance = timing * speed + 4.8f;
            var destScale = distance * 0.4f + 0.51f;
            if(NoteA is not null && NoteB is not null)
            {
                if(NoteA.State == NoteStatus.Destroyed || NoteB.State == NoteStatus.Destroyed)
                    Destroy(gameObject);
            }
            else if (timing > 0)
                Destroy(gameObject);

            if (distance <= 1.225f)
            {
                distance = 1.225f;
                if (destScale > 0.3f) sr.forceRenderingOff = false;
            }
            if (destScale > 0.3f) 
                sr.forceRenderingOff = false;
            var lineScale = Mathf.Abs(distance / 4.8f);
            transform.localScale = new Vector3(lineScale, lineScale, 1f);
            transform.rotation = Quaternion.Euler(0, 0, -45f * (startPosition - 1));
        }

        RendererStatus _rendererState = RendererStatus.Off;
    }
}