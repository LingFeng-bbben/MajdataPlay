using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public class EachLineDrop : MonoBehaviour
    {
        public float time;
        public int startPosition = 1;
        public int curvLength = 1;
        public float speed = 1;

        public IDistanceProvider? DistanceProvider { get; set; }
        public IStatefulNote? NoteA { get; set; }
        public IStatefulNote? NoteB { get; set; }

        public Sprite[] curvSprites;
        private SpriteRenderer sr;

        private GamePlayManager gpManager;

        // Start is called before the first frame update
        private void Start()
        {
            gpManager = GamePlayManager.Instance;

            sr = gameObject.GetComponent<SpriteRenderer>();
            sr.sprite = curvSprites[curvLength - 1];
            sr.forceRenderingOff = true;
        }

        // Update is called once per frame
        private void Update()
        {
            var timing = gpManager.AudioTime - time;
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
    }
}