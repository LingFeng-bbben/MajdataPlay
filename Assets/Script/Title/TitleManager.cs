using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        IOManager.Instance.OnButtonDown += OnButtonPress;
        StartCoroutine(DelayPlayVoice());
    }

    IEnumerator DelayPlayVoice()
    {
        yield return new WaitForSeconds(0.3f);
        AudioManager.Instance.PlaySFX("MajdataPlay.wav");
        yield return new WaitForSeconds(2f);
        AudioManager.Instance.PlaySFX("titlebgm.mp3");
    }

    void OnButtonPress(object sender,ButtonEventArgs e)
    {
        AudioManager.Instance.StopSFX("titlebgm.mp3");
        SceneManager.LoadSceneAsync(1);
    }

    private void OnDestroy()
    {
        IOManager.Instance.OnButtonDown -= OnButtonPress;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
