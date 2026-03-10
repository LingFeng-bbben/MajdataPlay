using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace MajdataPlay.Scenes.Title
{
    public class LoadVideoFromSA : MonoBehaviour
    {
        public VideoPlayer player;
        public string videopath;
        public bool LoadOnly;

        void Awake()
        {
            player = GetComponent<VideoPlayer>();
        }
        void Start()
        {
            DelayPlayVideo().Forget();
        }
        async UniTask DelayPlayVideo()
        {
            while(MajEnv.Settings is null)
            {
                await UniTask.Yield();
            }
            var videoPath = videopath;
            if (string.IsNullOrEmpty(videoPath) || videoPath.Length == 1)
            {
                MajDebug.LogWarning($"[{nameof(LoadVideoFromSA)}]Invalid video path: {videoPath}");
                return;
            }
            else if (videoPath[0] is '/' or '\\')
            {
                videoPath = videoPath.Substring(1);
            }
            var path = string.Empty;
            var isValid = false;
            foreach(var ext in MajEnv.SUPPORTED_VIDEO_FORMAT)
            {
                path = Path.Combine(MajEnv.AssetsPath, videoPath + ext);

                if(File.Exists(path))
                {
                    isValid = true;
                    break;
                }
            }
            if(!isValid)
            {
                MajDebug.LogError($"[{nameof(LoadVideoFromSA)}]Video does not exists: {videoPath}");
                return;
            }
            MajDebug.LogInfo($"[{nameof(LoadVideoFromSA)}]Load video from {path}");
            player.url = path;
            if (LoadOnly)
            {
                player.Prepare();
            }
            else
            {
                player.Play();
            }
        }
    }
}