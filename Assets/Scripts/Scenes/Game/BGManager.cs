using MajdataPlay.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace MajdataPlay.Game
{
    public class BGManager : MonoBehaviour
    {
        private float playSpeed;
        private GamePlayManager _gpManager;

        private SpriteRenderer spriteRender;

        private VideoPlayer videoPlayer;
        void Awake()
        {
            Majdata<BGManager>.Instance = this;
        }
        void OnDestroy()
        {
            Majdata<BGManager>.Free();
        }

        // Start is called before the first frame update
        private void Start()
        {
            spriteRender = GetComponent<SpriteRenderer>();
            videoPlayer = GetComponent<VideoPlayer>();
            _gpManager = Majdata<GamePlayManager>.Instance!;
        }

        private void Update()
        {
            if (videoPlayer.isPlaying)
                videoPlayer.externalReferenceTime = _gpManager.AudioTimeNoOffset;
            /*var delta = (float)videoPlayer.clockTime - gamePlayManager.AudioTimeNoOffset;

            if (delta < -0.01f)
                videoPlayer.playbackSpeed = playSpeed + 0.2f;
            else if (delta > 0.01f)
                videoPlayer.playbackSpeed = playSpeed - 0.2f;
            else
                videoPlayer.playbackSpeed = playSpeed;*/
        }

        public void CancelTimeRef()
        {
            videoPlayer.timeReference = VideoTimeReference.Freerun;
        }

        public void PauseVideo()
        {
            videoPlayer.Pause();
        }

        public void ContinueVideo(float speed)
        {
            videoPlayer.playbackSpeed = speed;
            playSpeed = speed;
            videoPlayer.Play();
        }

        public void SetBackgroundPic(Sprite sprite)
        {
            if (sprite == null) return;
            spriteRender.sprite = sprite;
            //todo:set correct scale
            var scale = 1080f / sprite.texture.width;
            gameObject.transform.localScale = new Vector3(scale, scale, scale);
        }

        public void SetBackgroundDim(float dim)
        {
            GameObject.Find("BackgroundCover").GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, dim);
        }

        public void SetBackgroundMovie(string path, float speed = 1f)
        {
            videoPlayer.url = "file://" + path;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.playbackSpeed = speed;
            playSpeed = speed;
            spriteRender.sprite =
                Sprite.Create(new Texture2D(1080, 1080), new Rect(0, 0, 1080, 1080), new Vector2(0.5f, 0.5f));
            videoPlayer.Prepare();
            StartCoroutine(waitFumenStart());
        }

        private IEnumerator waitFumenStart()
        {

            videoPlayer.timeReference = VideoTimeReference.ExternalTime;

            while (_gpManager.AudioTimeNoOffset <= 0) yield return new WaitForEndOfFrame();
            while (!videoPlayer.isPrepared) yield return new WaitForEndOfFrame();
            videoPlayer.Play();
            //videoPlayer.time = gamePlayManager.AudioTimeNoOffset;

            var scale = videoPlayer.height / (float)videoPlayer.width;


            gameObject.transform.localScale = new Vector3(1f, 1f * scale);
        }
    }
}