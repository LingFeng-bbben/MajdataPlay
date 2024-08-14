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
        IOManager.Instance.BindAnyArea(OnAreaDown);
        StartCoroutine(DelayPlayVoice());
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
        yield return new WaitForSeconds(0.3f);
        AudioManager.Instance.PlaySFX("MajdataPlay.wav");
        yield return new WaitForSeconds(2.5f);
        AudioManager.Instance.PlaySFX("titlebgm.mp3");
    }
    void NextScene()
    {
        AudioManager.Instance.StopSFX("titlebgm.mp3");
        SceneManager.LoadSceneAsync(1);
    }
    private void OnDestroy()
    {
        IOManager.Instance.UnbindAnyArea(OnAreaDown);
    }
}
