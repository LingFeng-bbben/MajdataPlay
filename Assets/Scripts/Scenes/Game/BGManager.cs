using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using MajdataPlay.View;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
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
        bool _usePictureAsBackground = true;
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
            if (_usePictureAsBackground)
                return;
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
            if (_usePictureAsBackground)
                return;
            videoPlayer.Pause();
        }

        public void StopVideo()
        {
            if (_usePictureAsBackground)
                return;
            CancelTimeRef();
            videoPlayer.Stop();
            videoPlayer.time = 0;
        }

        public void PlayVideo(float time,float speed)
        {
            if (_usePictureAsBackground)
                return;
            videoPlayer.playbackSpeed = speed;
            videoPlayer.timeReference = VideoTimeReference.ExternalTime;
            videoPlayer.Play();
            videoPlayer.time = time;
        }

        public void SetBackgroundPic(Sprite? sprite)
        {
            _usePictureAsBackground = true;
            videoPlayer.enabled = false;
            spriteRender.enabled = true;
            if (sprite is null) 
            { 
                spriteRender.sprite = DefaultSprite;
                transform.localScale = DefaultScale;
                return; 
            }
            spriteRender.sprite = sprite;
            //todo:set correct scale
            var scale = 1080f / sprite.texture.width;
            gameObject.transform.localScale = new Vector3(scale, scale, scale);
        }

        public void DisableVideo()
        {
            videoPlayer.url = null;
        }

        public void SetBackgroundDim(float dim)
        {
            GameObject.Find("BackgroundCover").GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, dim);
        }

        public async UniTask SetBackgroundMovie(string path,Sprite? fallback)
        {
            var sceneSwitcher = MajInstances.SceneSwitcher;
            try
            {
                //var isErrorThrown = false;
                var isPrepared = false;
                videoPlayer.url = "file://" + path;
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                spriteRender.sprite =
                    Sprite.Create(new Texture2D(1080, 1080), new Rect(0, 0, 1080, 1080), new Vector2(0.5f, 0.5f));
                //videoPlayer.errorReceived += (s,m) =>
                //{
                //    isErrorThrown = true;
                //    MajDebug.LogError(m);
                //};
                videoPlayer.prepareCompleted += (s) => isPrepared = true;
                videoPlayer.Prepare();
                var startAt = MajTimeline.UnscaledTime;
                var timeout = TimeSpan.FromSeconds(12);
                while (true)
                {
                    try
                    {
                        var remainingTime = timeout - (MajTimeline.UnscaledTime - startAt);
                        if (remainingTime.TotalSeconds > 10)
                        {
                            var text1 = string.Format(Localization.GetLocalizedText("Waiting for the video player...{0}"), "");
                            sceneSwitcher.SetLoadingText($"{text1}");
                        }
                        else if (remainingTime.TotalSeconds > 0)
                        {
                            var text1 = string.Format(Localization.GetLocalizedText("Waiting for the video player...{0}"), $"{remainingTime.TotalSeconds:F0}s");
                            var text2 = Localization.GetLocalizedText("Press the 4th button to use default cover");
                            sceneSwitcher.SetLoadingText($"{text1}\n{text2}");
                        }
                        else if (remainingTime.TotalSeconds > -2)
                        {
                            var text1 = Localization.GetLocalizedText("Video loading timeout, fall back to default cover");
                            sceneSwitcher.SetLoadingText($"{text1}",Color.red);
                            continue;
                        }
                        else
                        {
                            throw new Exception("SB Unity");
                        }
                        if (InputManager.CheckButtonStatus(SensorArea.A4, SensorStatus.On))
                        {
                            throw new Exception("SB Unity");
                        }
                        if (videoPlayer.isPrepared)
                            break;
                    }
                    finally
                    {
                        await UniTask.Yield();
                    }
                }
                var scale = videoPlayer.height / (float)videoPlayer.width;
                gameObject.transform.localScale = new Vector3(1f, 1f * scale);
                _usePictureAsBackground = false;
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                SetBackgroundPic(fallback);
            }
            finally
            {
                sceneSwitcher.SetLoadingText("", Color.white);
            }

        }
    }
}