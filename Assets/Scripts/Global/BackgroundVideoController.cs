using MajdataPlay.Diagnostics;
using UnityEngine;
using UnityEngine.Video;
#nullable enable
namespace MajdataPlay
{
    public sealed class BackgroundVideoController : MajComponent
    {
        [SerializeField]
        VideoPlayer _videoPlayer = null!;
        [SerializeField]
        SpriteRenderer _videoRenderer = null!;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            Majdata<BackgroundVideoController>.SetAsSingleton(this);
            if (_videoPlayer == null || _videoRenderer == null)
            {
                MajDebug.LogError("Global background video references are not configured.");
            }
        }

        public void Pause()
        {
            _videoPlayer.Pause();
        }

        public void Play()
        {
            _videoPlayer.Play();
        }

        public void Stop()
        {
            _videoPlayer.Stop();
        }

        public void Hide()
        {
            Pause();
            _videoRenderer.gameObject.layer = MajEnv.HIDDEN_LAYER;
        }

        public void Show()
        {
            _videoRenderer.gameObject.layer = MajEnv.DEFAULT_LAYER;
            Play();
        }
    }
}
