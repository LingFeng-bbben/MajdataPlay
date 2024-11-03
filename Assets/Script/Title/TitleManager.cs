using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
#nullable enable
namespace MajdataPlay.Title
{
    public class TitleManager : MonoBehaviour
    {
        public TextMeshProUGUI echoText;
        public Animator fadeInAnim;

        Task? songStorageTask = null;
        void Start()
        {
            echoText.text = $"{Localization.GetLocalizedText("Scanning Charts")}...";
            DelayPlayVoice().Forget();
            songStorageTask = StartScanningChart();
            WaitForScanningTask().Forget();
            MajInstances.LightManager.SetAllLight(Color.white);
            
        }
        async Task StartScanningChart()
        {
            await SongStorage.ScanMusicAsync();

            if (!SongStorage.IsEmpty)
            {
                var setting = MajInstances.Setting;
                await SongStorage.SortAndFindAsync();
                SongStorage.CollectionIndex = setting.Misc.SelectedDir;
                SongStorage.WorkingCollection.Index = setting.Misc.SelectedIndex;
            }
        }

        private void OnAreaDown(object sender, InputEventArgs e)
        {
            if (!e.IsClick)
                return;
            if (e.IsButton)
                NextScene();
            else
            {
                switch (e.Type)
                {
                    case SensorType.A8:
                        MajInstances.AudioManager.OpenAsioPannel();
                        break;
                    case SensorType.E5:
                        NextScene();
                        break;
                }
            }
        }

        async UniTaskVoid WaitForScanningTask()
        {
            if (songStorageTask is null)
                return;
            bool isEmpty = false;
            float animTimer = 0;
            while (animTimer < 2f)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                animTimer += Time.deltaTime;
                if (songStorageTask.IsCompleted)
                {
                    if (SongStorage.IsEmpty)
                    {
                        isEmpty = true;
                        echoText.text = Localization.GetLocalizedText("No Charts");
                        return;
                    }
                    else
                    {
                        echoText.text = Localization.GetLocalizedText("Press Any Key");
                        MajInstances.InputManager.BindAnyArea(OnAreaDown);
                        return;
                    }
                }
            }

            fadeInAnim.enabled = false;
            int a = -1;
            float timer = 2f;
            while (true)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                if (timer >= 2f)
                    a = -1;
                else if (timer == 0)
                    a = 1;
                timer += Time.deltaTime * a;
                timer = timer.Clamp(0, 2f);
                var newColor = Color.white;

                if (songStorageTask.IsCompleted)
                {
                    if (songStorageTask.IsFaulted)
                        echoText.text = Localization.GetLocalizedText("Scan Chart Failed");
                    else if (SongStorage.IsEmpty)
                    {
                        isEmpty = true;
                        echoText.text = Localization.GetLocalizedText("No Charts");
                    }
                    else
                        echoText.text = Localization.GetLocalizedText("Press Any Key");

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
            else if (songStorageTask.Status is TaskStatus.RanToCompletion)
                MajInstances.InputManager.BindAnyArea(OnAreaDown);
        }

        async UniTaskVoid DelayPlayVoice()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            MajInstances.AudioManager.PlaySFX("MajdataPlay.wav");
            MajInstances.AudioManager.PlaySFX("bgm_title.mp3");
        }

        void NextScene()
        {
            MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
            MajInstances.AudioManager.StopSFX("bgm_title.mp3");
            MajInstances.AudioManager.StopSFX("MajdataPlay.wav");
            MajInstances.SceneSwitcher.SwitchScene("List");
        }
    }
}