using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class LoadVideoFromSA : MonoBehaviour
{
    public VideoPlayer player;
    public string videopath;
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<VideoPlayer>();
        player.url = Application.streamingAssetsPath + videopath;
        player.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
