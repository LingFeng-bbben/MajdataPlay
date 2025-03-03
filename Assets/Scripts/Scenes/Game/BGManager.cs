using Cysharp.Threading.Tasks;
using MajdataPlay.Utils;
using MajdataPlay.View;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace MajdataPlay.Game
{
    public class BGManager : MonoBehaviour
    {
        public Sprite DefaultSprite;
        public Vector3 DefaultScale;
        private GamePlayManager _gpManager;
        private ViewManager _viewManager;

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
            DefaultScale = transform.localScale;
            spriteRender = GetComponent<SpriteRenderer>();
            videoPlayer = GetComponent<VideoPlayer>();
            _gpManager = Majdata<GamePlayManager>.Instance!;
            _viewManager = Majdata<ViewManager>.Instance!;
        }

        private void Update()
        {
            if (videoPlayer.isPlaying)
            {
                if (MajEnv.Mode == RunningMode.Play)
                    videoPlayer.externalReferenceTime = _gpManager.AudioTimeNoOffset;
                else if (MajEnv.Mode == RunningMode.View)
                    videoPlayer.externalReferenceTime = _viewManager.AudioTimeNoOffset;
            }
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

        public void StopVideo()
        {
            CancelTimeRef();
            videoPlayer.Stop();
            videoPlayer.time = 0;
        }

        public void PlayVideo(float time,float speed)
        {
            videoPlayer.playbackSpeed = speed;
            videoPlayer.timeReference = VideoTimeReference.ExternalTime;
            videoPlayer.Play();
            videoPlayer.time = time;
        }

        public void SetBackgroundPic(Sprite sprite)
        {
            if (sprite == null) { 
                spriteRender.sprite = DefaultSprite;
                transform.localScale = DefaultScale;
                return; 
            }
            spriteRender.sprite = sprite;
            //todo:set correct scale
            var scale = 1080f / sprite.texture.width;
            gameObject.transform.localScale = new Vector3(scale, scale, scale);
        }

        public void SetBackgroundDim(float dim)
        {
            GameObject.Find("BackgroundCover").GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, dim);
        }

        public async UniTask SetBackgroundMovie(string path)
        {
            videoPlayer.url = "file://" + path;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            spriteRender.sprite =
                Sprite.Create(new Texture2D(1080, 1080), new Rect(0, 0, 1080, 1080), new Vector2(0.5f, 0.5f));
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared) await UniTask.Yield();
            var scale = videoPlayer.height / (float)videoPlayer.width;
            gameObject.transform.localScale = new Vector3(1f, 1f * scale);

        }
    }
}