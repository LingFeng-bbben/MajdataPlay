using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class BGManager : MonoBehaviour
{
    private float playSpeed;
    private GamePlayManager gamePlayManager;

    private float smoothRDelta;

    private SpriteRenderer spriteRender;

    private VideoPlayer videoPlayer;

    private float originalScaleX;

    // Start is called before the first frame update
    private void Start()
    {
        originalScaleX = gameObject.transform.localScale.x;
        spriteRender = GetComponent<SpriteRenderer>();
        videoPlayer = GetComponent<VideoPlayer>();
        gamePlayManager = GamePlayManager.Instance;
    }

    private void Update()
    {
        //videoPlayer.externalReferenceTime = provider.AudioTime;
        var delta = (float)videoPlayer.clockTime - gamePlayManager.AudioTime;
        smoothRDelta += (Time.unscaledDeltaTime - smoothRDelta) * 0.01f;
        if (gamePlayManager.AudioTime < 0) return;
        var realSpeed = Time.deltaTime / smoothRDelta;

        if (Time.captureFramerate != 0)
        {
            //print("speed="+realSpeed+" delta="+delta);
            videoPlayer.playbackSpeed = realSpeed - delta;
            return;
        }

        if (delta < -0.01f)
            videoPlayer.playbackSpeed = playSpeed + 0.2f;
        else if (delta > 0.01f)
            videoPlayer.playbackSpeed = playSpeed - 0.2f;
        else
            videoPlayer.playbackSpeed = playSpeed;
    }

    public void PauseVideo()
    {
        videoPlayer.Pause();
    }

    public void ContinueVideo(float speed)
    {
        videoPlayer.playbackSpeed = speed;
        playSpeed = speed;
        videoPlayer.Play();
    }

    //Moved to songloader
    //public void LoadBGFromPath(string path, float speed)

    public void SetBackgroundPic(Sprite sprite)
    {
        if (sprite == null) return;
        spriteRender.sprite = sprite;
        //todo:set correct scale
        var scale = 1080f / sprite.texture.width;
        gameObject.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetBackgroundDim(float dim)
    {
        GameObject.Find("BackgroundCover").GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, dim);
    }

    public void SetBackgroundMovie(string path, float speed=1f)
    {
        videoPlayer.url = "file://" + path;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.playbackSpeed = speed;
        playSpeed = speed;
        spriteRender.sprite =
            Sprite.Create(new Texture2D(1080, 1080), new Rect(0, 0, 1080, 1080), new Vector2(0.5f, 0.5f));
        videoPlayer.Prepare();
        StartCoroutine(waitFumenStart());
    }

    private IEnumerator waitFumenStart()
    {
        
        //videoPlayer.timeReference = VideoTimeReference.ExternalTime;
        while (gamePlayManager.AudioTime <= 0) yield return new WaitForEndOfFrame();
        while (!videoPlayer.isPrepared) yield return new WaitForEndOfFrame();
        videoPlayer.Play();
        //videoPlayer.time = gamePlayManager.AudioTime;

        var scale = videoPlayer.height / (float)videoPlayer.width;
        
        
        gameObject.transform.localScale = new Vector3(1f, 1f * scale);
    }
}