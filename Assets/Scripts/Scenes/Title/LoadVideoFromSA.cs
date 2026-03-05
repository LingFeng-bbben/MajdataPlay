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
            var path = Path.Combine(MajEnv.AssetsPath, videopath);
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