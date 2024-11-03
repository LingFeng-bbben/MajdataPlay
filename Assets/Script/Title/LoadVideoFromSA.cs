using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace MajdataPlay.Title
{
    public class LoadVideoFromSA : MonoBehaviour
    {
        public VideoPlayer player;
        public string videopath;
        public bool LoadOnly;
        // Start is called before the first frame update
        void Start()
        {
            player = GetComponent<VideoPlayer>();
            player.url = Application.streamingAssetsPath + videopath;
            if(LoadOnly)
                player.Prepare();
            else
                player.Play();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}