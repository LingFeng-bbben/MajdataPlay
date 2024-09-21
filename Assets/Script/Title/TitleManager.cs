using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MajdataPlay.Title
{
    public class TitleManager : MonoBehaviour
    {
        public TextMeshProUGUI echoText;
        public Animator fadeInAnim;

    // Start is called before the first frame update
    void Start()
    {
        DelayPlayVoice().Forget();
        WaitForScanningTask().Forget();
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
    async UniTaskVoid WaitForScanningTask()
    {
        bool isEmpty = false;
        float animTimer = 0;
        while(animTimer < 2f)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            animTimer += Time.deltaTime;
            if (SongStorage.State == ComponentState.Finished)
            {
                if (SongStorage.IsEmpty)
                {
                    isEmpty = true;
                    echoText.text = "No Charts";
                    return;
                }
                else
                {
                    echoText.text = "Press Any Key";
                    InputManager.Instance.BindAnyArea(OnAreaDown);
                    return;
                }
            }
        }
        fadeInAnim.enabled = false;
        int a = -1;
        float timer = 2f;
        while(true)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            var state = SongStorage.State;
            if (timer >= 2f)
                a = -1;
            else if(timer == 0)
                a = 1;
            timer += Time.deltaTime * a;
            timer = timer.Clamp(0, 2f);
            var newColor = Color.white;

            if (state >= ComponentState.Finished)
            {
                if(state == ComponentState.Failed)
                    echoText.text = "Scan Chart Failed";
                else if(SongStorage.IsEmpty)
                {
                    isEmpty = true;
                    echoText.text = "No Charts";
                }
                else
                    echoText.text = "Press Any Key";
                if (timer >= 1.8f)
                {
                    echoText.color = newColor;
                    break;
                }
            }
            newColor.a = timer / 2f;
            echoText.color = newColor;
        }
        if (isEmpty)
            return;
        else if(SongStorage.State != ComponentState.Failed)
            InputManager.Instance.BindAnyArea(OnAreaDown);
    }
    async UniTaskVoid DelayPlayVoice()
    {
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        AudioManager.Instance.PlaySFX(SFXSampleType.MAJDATA_PLAY);
        AudioManager.Instance.PlaySFX(SFXSampleType.TITLE_BGM);
    }
    void NextScene()
    {
        InputManager.Instance.UnbindAnyArea(OnAreaDown);
        AudioManager.Instance.StopSFX(SFXSampleType.TITLE_BGM);
        AudioManager.Instance.StopSFX(SFXSampleType.MAJDATA_PLAY);
        SceneManager.LoadSceneAsync(1);
    }
}