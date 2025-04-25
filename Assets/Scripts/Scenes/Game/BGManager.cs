using Cysharp.Threading.Tasks;
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
using UnityEngine.Rendering;
using UnityEngine.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace MajdataPlay.Game
{
    public class BGManager : MonoBehaviour
    {
        public Sprite DefaultSprite;
        public Vector3 DefaultScale;
        private INoteController _gpManager;

        public RawImage videoRenderer;
        private SpriteRenderer picture;
        private MediaPlayer mediaPlayer;

        Texture2D _vlcTexture = null; //This is the texture libVLC writes to directly. It's private.
        public RenderTexture texture = null; //We copy it into this texture which we actually use in unity.
        // when copying native Texture2D textures to Unity RenderTextures, the orientation mapping is incorrect on Android, so we flip it over.
        public bool flipTextureX = true;
        public bool flipTextureY = true;

        public float CurrentSec => mediaPlayer.Time / 1000f;

        bool _usePictureAsBackground = false;
        void Awake()
        {
            Majdata<BGManager>.Instance = this;
            if (mediaPlayer != null)
            {
                DestroyMediaPlayer();
            }
            mediaPlayer = new MediaPlayer(MajEnv.libVLC) { 
                FileCaching = 0,
                NetworkCaching = 0,
            };
        }
        void OnDestroy()
        {
            DestroyMediaPlayer();
            Majdata<BGManager>.Free();
        }

        // Start is called before the first frame update
        private void Start()
        {
            DefaultScale = transform.localScale;
            picture = GetComponent<SpriteRenderer>();
            _gpManager = Majdata<INoteController>.Instance!;
        }

        private void Update()
        {
            if (_usePictureAsBackground)
                return;

            //print("VideoTime: " + (mediaPlayer.Time));

            //Get size every frame
            uint height = 0;
            uint width = 0;
            mediaPlayer.Size(0, ref width, ref height);

            if (_vlcTexture == null || _vlcTexture.width != width || _vlcTexture.height != height)
            {
                ResizeOutputTextures(width, height);
            }

            if (_vlcTexture != null)
            {
                //Update the vlc texture (tex)
                var texptr = mediaPlayer.GetTexture(width, height, out bool updated);
                if (updated)
                {
                    _vlcTexture.UpdateExternalTexture(texptr);

                    //Copy the vlc texture into the output texture, automatically flipped over
                    var flip = new Vector2(flipTextureX ? -1 : 1, flipTextureY ? -1 : 1);
                    Graphics.Blit(_vlcTexture, texture, flip, Vector2.zero); //If you wanted to do post processing outside of VLC you could use a shader here.
                }
            }
        }

        public void PauseVideo()
        {
            if (_usePictureAsBackground)
                return;
            mediaPlayer.Pause();
        }

        public void StopVideo()
        {
            if (_usePictureAsBackground)
                return;
            mediaPlayer.Stop();
        }

        public void PlayVideo(float time,float speed)
        {
            if (_usePictureAsBackground)
                return;
            mediaPlayer.SetRate(speed);
            mediaPlayer.SeekTo(TimeSpan.FromSeconds(time));
            mediaPlayer.Play();
        }

        public void SetVideoSpeed(float speed)
        {
            if(speed != mediaPlayer.Rate)
                mediaPlayer.SetRate(speed);
        }

        public void SetBackgroundPic(Sprite? sprite)
        {
            DisableVideo();
            if (sprite is null) 
            { 
                picture.sprite = DefaultSprite;
                transform.localScale = DefaultScale;
                return; 
            }
            picture.sprite = sprite;
            //todo:set correct scale
            var scale = 1080f / sprite.texture.width;
            gameObject.transform.localScale = new Vector3(scale, scale, scale);
        }

        public void DisableVideo()
        {
            mediaPlayer.Media = null;
            _usePictureAsBackground = true;
        }

        public void SetBackgroundDim(float dim)
        {
            GameObject.Find("BackgroundCover").GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, dim);
        }

        public async UniTask SetBackgroundMovie(string path,Sprite? fallback)
        {
            try
            {
                MajDebug.Log("[VLC] LoadMedia");
                if (mediaPlayer.Media != null)
                    mediaPlayer.Media.Dispose();

                var trimmedPath = path.Trim(new char[] { '"' });//Windows likes to copy paths with quotes but Uri does not like to open them
                var uri = new Uri(trimmedPath);
                MajDebug.Log("[VLC] Uri: " + uri.ToString());
                var media = new Media(uri);
                //media.AddOption(":start-paused");
                mediaPlayer.Media = media;


                MajDebug.Log("[VLC] BeginParse");
                var ret = await mediaPlayer.Media.ParseAsync(MajEnv.libVLC);
                MajDebug.Log("[VLC] " + ret);
                picture.forceRenderingOff = true;
                _usePictureAsBackground = false;
                mediaPlayer.Play();
                await UniTask.Delay(200);
                mediaPlayer.Pause();
                mediaPlayer.SeekTo(TimeSpan.Zero);
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                SetBackgroundPic(fallback);
            }

        }
        void DestroyMediaPlayer()
        {
            MajDebug.Log("[VLC] DestroyMediaPlayer");
            mediaPlayer?.Stop();
            mediaPlayer?.Dispose();
            mediaPlayer = null;
        }

        void ResizeOutputTextures(uint px, uint py)
        {
            var texptr = mediaPlayer.GetTexture(px, py, out bool updated);
            if (px != 0 && py != 0 && updated && texptr != IntPtr.Zero)
            {
                //If the currently playing video uses the Bottom Right orientation, we have to do this to avoid stretching it.
                if (GetVideoOrientation() == VideoOrientation.BottomRight)
                {
                    uint swap = px;
                    px = py;
                    py = swap;
                }

                _vlcTexture = Texture2D.CreateExternalTexture((int)px, (int)py, TextureFormat.RGBA32, false, true, texptr); //Make a texture of the proper size for the video to output to
                texture = new RenderTexture(_vlcTexture.width, _vlcTexture.height, 0, RenderTextureFormat.ARGB32); //Make a renderTexture the same size as vlctex

                if (videoRenderer != null)
                    videoRenderer.texture = texture;


                var scale = (float)py / (float)px;
                videoRenderer.gameObject.transform.localScale = new Vector3(1f, scale, 1f);
            }
        }

        public VideoOrientation? GetVideoOrientation()
        {
            var tracks = mediaPlayer?.Tracks(TrackType.Video);

            if (tracks == null || tracks.Count == 0)
                return null;

            var orientation = tracks[0]?.Data.Video.Orientation; //At the moment we're assuming the track we're playing is the first track

            return orientation;
        }
    }
}