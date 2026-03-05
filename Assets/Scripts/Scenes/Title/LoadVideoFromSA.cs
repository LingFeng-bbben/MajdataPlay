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
            var videoPath = videopath;
            if (string.IsNullOrEmpty(videoPath) || videoPath.Length == 1)
            {
                return;
            }
            else if (videoPath[0] is '/' or '\\')
            {
                videoPath = videoPath.Substring(1);
            }
            var path = Path.Combine(MajEnv.AssetsPath, videoPath);
            MajDebug.LogInfo($"[{nameof(LoadVideoFromSA)}]Load video from {path}");
            player.url = path;
            if(LoadOnly)
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