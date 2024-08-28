using MajdataPlay.IO;
using MajdataPlay.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InputManager.Instance.BindAnyArea(OnAreaDown);
        StartCoroutine(DelayPlayVoice());
        LightManager.Instance.SetAllLight(Color.white);
    }

    private void OnAreaDown(object sender, InputEventArgs e)
    {
        if (!e.IsClick)
            return;
        if(e.IsButton)
            NextScene();
        else
        {
            switch(e.Type)
            {
                case SensorType.A8:
                    AudioManager.Instance.OpenAsioPannel();
                    break;
                case SensorType.E5:
                    NextScene();
                    break;
            }
        }
    }

    IEnumerator DelayPlayVoice()
    {
        yield return new WaitForEndOfFrame();
        AudioManager.Instance.PlaySFX("MajdataPlay.wav");
        AudioManager.Instance.PlaySFX("titlebgm.mp3");
    }
    void NextScene()
    {
        SceneManager.LoadSceneAsync(1);
    }
    private void OnDestroy()
    {
        InputManager.Instance.UnbindAnyArea(OnAreaDown);
        AudioManager.Instance.StopSFX("titlebgm.mp3");
        AudioManager.Instance.StopSFX("MajdataPlay.wav");
    }
}
