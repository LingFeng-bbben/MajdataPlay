using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
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

        bool _flag = false;
        float _pressTime = 0f;
        Task? songStorageTask = null;
        void Start()
        {
            echoText.text = $"{Localization.GetLocalizedText("Scanning Charts")}...";
            DelayPlayVoice().Forget();
            songStorageTask = StartScanningChart();
            WaitForScanningTask().Forget();
            LedRing.SetAllLight(Color.white);
            if (InputManager.IsTouchPanelConnected)
            {
                Destroy(GameObject.Find("EventSystem"));
            }
        }
        async Task StartScanningChart()
        {
            var progress = new Progress<ChartScanProgress>();
            progress.ProgressChanged += (o,e) =>
            {
                switch(e.StorageType)
                {
                    case ChartStorageLocation.Local:
                        break;
                    case ChartStorageLocation.Online:
                        echoText.text = string.Format(Localization.GetLocalizedText("Scanning Charts From {0}"),e.Message);
                        break;
                }
            };
            await Task.Delay(3000);
            await SongStorage.ScanMusicAsync(progress);

            if (!SongStorage.IsEmpty)
            {
                var setting = MajInstances.Settings;
                SongStorage.CollectionIndex = setting.Misc.SelectedDir;
                var selectedCollection = SongStorage.WorkingCollection;
                var selectedIndex = setting.Misc.SelectedIndex;

                if(selectedCollection.IsEmpty)
                {
                    setting.Misc.SelectedIndex = 0;
                    return;
                }
                else if(selectedIndex >= selectedCollection.Count)
                {
                    selectedCollection.Index = 0;
                    setting.Misc.SelectedIndex = 0;
                }
                else
                {
                    selectedCollection.Index = selectedIndex;
                }
            }
        }

        private void OnAreaDown(object sender, InputEventArgs e)
        {
            if (!e.IsDown)
                return;
            if (e.IsButton)
            {
                switch (e.SArea)
                {
                    case SensorArea.Test:
                        if(_flag)
                        {
                            EnterTestMode();
                        }
                        return;
                }
                NextScene();
            }
            else
            {
                switch (e.SArea)
                {
                    case SensorArea.A8:
                        MajInstances.AudioManager.OpenAsioPannel();
                        break;
                    case SensorArea.E5:
                        NextScene();
                        break;
                }
            }
        }

        async UniTaskVoid WaitForScanningTask()
        {
            try
            {
                if (songStorageTask is null)
                    return;
                var isEmpty = false;
                while (true)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);

                    if (songStorageTask.IsCompleted)
                    {
                        if (songStorageTask.IsFaulted)
                        {
                            echoText.text = Localization.GetLocalizedText("Scan Chart Failed");
                            MajDebug.LogException(songStorageTask.Exception);
                        }
                        else if (SongStorage.IsEmpty)
                        {
                            isEmpty = true;
                            echoText.text = Localization.GetLocalizedText("No Charts");
                        }
                        else
                        {
                            if (MajInstances.Settings.Online.Enable)
                            {
                                foreach (var endpoint in MajInstances.Settings.Online.ApiEndpoints)
                                {
                                    try
                                    {
                                        if (endpoint.Username is null || endpoint.Password is null) continue;
                                        echoText.text = "Login " + endpoint.Name + " as " + endpoint.Username;
                                        await Online.Login(endpoint);
                                        await UniTask.Delay(1000);
                                    }
                                    catch (Exception ex)
                                    {
                                        MajDebug.LogError(ex);
                                        echoText.text = "Login failed for " + endpoint.Name;
                                        await UniTask.Delay(1000);
                                    }
                                }
                            }
                            echoText.text = Localization.GetLocalizedText("Press Any Key");
                            InputManager.BindAnyArea(OnAreaDown);

                        }
                        break;
                    }
                }
                fadeInAnim.SetBool("IsDone", true);
            }
            finally
            {
                _flag = true;
            }
        }
        async UniTaskVoid DelayPlayVoice()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            MajInstances.AudioManager.PlaySFX("MajdataPlay.wav");
            MajInstances.AudioManager.PlaySFX("bgm_title.mp3");
        }

        void EnterTestMode()
        {
            InputManager.UnbindAnyArea(OnAreaDown);
            _flag = false;
            MajInstances.AudioManager.StopSFX("bgm_title.mp3");
            MajInstances.AudioManager.StopSFX("MajdataPlay.wav");
            MajInstances.SceneSwitcher.SwitchScene("SensorTest");
        }
        void NextScene()
        {
            InputManager.UnbindAnyArea(OnAreaDown);
            _pressTime = 0;
            _flag = false;
            MajInstances.AudioManager.StopSFX("bgm_title.mp3");
            MajInstances.AudioManager.StopSFX("MajdataPlay.wav");
            MajInstances.SceneSwitcher.SwitchScene("List", false);
        }
    }
}