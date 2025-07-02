﻿using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using MajdataPlay.View;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using LibVLCSharp;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
#nullable enable
namespace MajdataPlay.Game
{
    public class BGManager : MonoBehaviour
    {
        public float CurrentSec
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if(_mediaPlayer is not null)
                {
                    return _mediaPlayer.Time / 1000f;
                }
                else
                {
                    return 0;
                }
            }
        }
        public TimeSpan MediaLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return TimeSpan.FromMilliseconds(_mediaLengthMs);
            }
        }
        
        [SerializeField]
        Vector3 _defaultScale;
        

        [SerializeField]
        RawImage _videoRenderer;
        [SerializeField]
        Sprite _defaultSprite = MajEnv.EmptySongCover;
        // This is the texture libVLC writes to directly. It's private.
        Texture2D? _vlcTexture = null;
        // We copy it into this texture which we actually use in unity.
        [SerializeField]
        RenderTexture? _renderTexture = null;

        SpriteRenderer _pictureCover;
        SpriteRenderer _pictureRenderer;

        MediaPlayer? _mediaPlayer = null;

        // when copying native Texture2D textures to Unity RenderTextures, the orientation mapping is incorrect on Android, so we flip it over.
        [SerializeField]
        bool _flipTextureX = true;
        [SerializeField]
        bool _flipTextureY = true;

        bool _usePictureAsBackground = false;

        long _mediaLengthMs = 0;

        void Awake()
        {
            Majdata<BGManager>.Instance = this;
            if (MajEnv.VLCLibrary != null)
            {
                _mediaPlayer = new MediaPlayer(MajEnv.VLCLibrary)
                {
                    FileCaching = 0,
                    NetworkCaching = 0,
                };
            }
            _pictureCover = GameObject.Find("BackgroundCover").GetComponent<SpriteRenderer>();
            _pictureRenderer = GetComponent<SpriteRenderer>();
            _defaultScale = transform.localScale;
        }
        void OnDestroy()
        {
            DestroyMediaPlayer();
            Majdata<BGManager>.Free();
        }

        internal void OnLateUpdate()
        {
            if (_mediaPlayer is null) return;
            if (_usePictureAsBackground)
            {
                return;
            }
            else if (!_mediaPlayer.IsPlaying)
            {
                return;
            }

            //Get size every frame
            uint height = 0;
            uint width = 0;
            IntPtr texPtr;
            _mediaPlayer.Size(0, ref width, ref height);
            //Update the vlc texture (tex)
            texPtr = _mediaPlayer.GetTexture(width, height, out var isUpdated);

            if (!isUpdated)
            {
                return;
            }

            if (_vlcTexture is null || _vlcTexture.width != width || _vlcTexture.height != height)
            {
                var px = width;
                var py = height;
                //If the currently playing video uses the Bottom Right orientation, we have to do this to avoid stretching it.
                if (GetVideoOrientation() == VideoOrientation.BottomRight)
                {
                    uint swap = px;
                    px = py;
                    py = swap;
                }

                var scale = (float)py / (float)px;

                //Make a texture of the proper size for the video to output to
                _vlcTexture = Texture2D.CreateExternalTexture((int)px, (int)py, TextureFormat.RGBA32, false, true, texPtr); 
                //Make a renderTexture the same size as vlctex
                _renderTexture = new RenderTexture(_vlcTexture.width, _vlcTexture.height, 0, RenderTextureFormat.ARGB32); 

                _videoRenderer.texture = _renderTexture;
                _videoRenderer.gameObject.transform.localScale = new Vector3(1f, scale, 1f);
            }


            _vlcTexture.UpdateExternalTexture(texPtr);

            //Copy the vlc texture into the output texture, automatically flipped over
            var flip = new Vector2(_flipTextureX ? -1 : 1, _flipTextureY ? -1 : 1);
            //If you wanted to do post processing outside of VLC you could use a shader here.
            Graphics.Blit(_vlcTexture, _renderTexture, flip, Vector2.zero); 
        }

        public void PauseVideo()
        {
            if (_usePictureAsBackground)
                return;
            _mediaPlayer?.Pause();
        }

        public void StopVideo()
        {
            if (_usePictureAsBackground)
                return;
            _mediaPlayer?.Stop();
        }

        public void PlayVideo(float time,float speed)
        {
            if (_usePictureAsBackground)
                return;
            if (_mediaPlayer is null) return;
            _mediaPlayer.SetRate(speed);
            _mediaPlayer.SeekTo(TimeSpan.FromSeconds(time));
            _mediaPlayer.Play();
        }

        public void SetVideoSpeed(float speed)
        {
            if (_mediaPlayer is null) return;
            if (speed != _mediaPlayer.Rate)
            {
                _mediaPlayer.SetRate(speed);
            }
        }

        public void SetBackgroundPic(Sprite? sprite)
        {
            DisableVideo();
            if (sprite is null) 
            { 
                _pictureRenderer.sprite = _defaultSprite;
                transform.localScale = _defaultScale;
                return; 
            }
            _pictureRenderer.sprite = sprite;
            //todo:set correct scale
            var scale = 1080f / sprite.texture.width;
            gameObject.transform.localScale = new Vector3(scale, scale, scale);
        }

        public void DisableVideo()
        {
            if(_mediaPlayer is not null)
            {
                _mediaPlayer.Media = null;
            }
            _usePictureAsBackground = true;
        }

        public void SetBackgroundDim(float dim)
        {
            _pictureCover.color = new Color(0f, 0f, 0f, dim);
        }

        public async UniTask SetBackgroundMovie(string path,Sprite? fallback)
        {
            if (_mediaPlayer is null) return;
            try
            {

                MajDebug.Log("[VLC] LoadMedia");
                if (_mediaPlayer.Media != null)
                    _mediaPlayer.Media.Dispose();

                var trimmedPath = path.Trim(new char[] { '"' });//Windows likes to copy paths with quotes but Uri does not like to open them
                var uri = new Uri(trimmedPath);
                MajDebug.Log("[VLC] Uri: " + uri.ToString());
                var media = new Media(uri);
                //media.AddOption(":start-paused");
                _mediaPlayer.Media = media;


                MajDebug.Log("[VLC] BeginParse");
                var ret = await _mediaPlayer.Media.ParseAsync(MajEnv.VLCLibrary);
                if(ret != MediaParsedStatus.Done)
                {
                    SetBackgroundPic(fallback);
                    return;
                }
                MajDebug.Log("[VLC] " + ret);
                _pictureRenderer.forceRenderingOff = true;
                _usePictureAsBackground = false;
                _mediaPlayer.Play();
                _mediaLengthMs = media.Duration;
                await UniTask.Delay(200);
                _mediaPlayer.Pause();
                _mediaPlayer.SeekTo(TimeSpan.Zero);
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                SetBackgroundPic(fallback);
            }

        }
        void DestroyMediaPlayer()
        {
            if (_mediaPlayer is null) return;
            MajDebug.Log("[VLC] DestroyMediaPlayer");
            _mediaPlayer.Stop();
            _mediaPlayer.Dispose();
        }

        public VideoOrientation? GetVideoOrientation()
        {
            var tracks = _mediaPlayer?.Tracks(TrackType.Video);

            if (tracks == null || tracks.Count == 0)
                return null;

            var orientation = tracks[0]?.Data.Video.Orientation; //At the moment we're assuming the track we're playing is the first track

            return orientation;
        }
    }
}