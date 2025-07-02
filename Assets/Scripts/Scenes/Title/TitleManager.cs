using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
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
        void Start()
        {
#if UNITY_ANDROID 
            //we extract the streaming assets files here and let the user to restart the app
            if (!Directory.Exists(MajEnv.AssetsPath))
            {
                StartCoroutine(ExtractStreamingAss());
                return;
            }
#endif
            InitAsync().Forget();
            LedRing.SetAllLight(Color.white);
            if (InputManager.IsTouchPanelConnected)
            {
                Destroy(GameObject.Find("EventSystem"));
            }
        }
        async UniTaskVoid InitAsync()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            MajInstances.AudioManager.PlaySFX("MajdataPlay.wav");
            MajInstances.AudioManager.PlaySFX("bgm_title.mp3");

            echoText.text = $"{Localization.GetLocalizedText("Loading Score Storage")}...";
            await UniTask.DelayFrame(9);
            var task1 = ScoreManager.InitAsync().AsValueTask();
            while(!task1.IsCompleted)
            {
                await UniTask.Yield();
            }

            echoText.text = $"{Localization.GetLocalizedText("Scanning Charts")}...";
            var task2 = StartScanningChart();
            try
            {
                if (task2 is null)
                {
                    return;
                }
                var isEmpty = false;
                while (true)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);

                    if (task2.IsCompleted)
                    {
                        if (task2.IsFaulted)
                        {
                            echoText.text = Localization.GetLocalizedText("Scan Chart Failed");
                            MajDebug.LogException(task2.Exception);
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
        IEnumerator ExtractStreamingAss()
        {
            var extractRoot = MajEnv.AssetsPath;
            echoText.text = $"Extracting Assets...";
            Directory.CreateDirectory(extractRoot);
            List<string> filePathsList = new List<string>();
            TextAsset paths = Resources.Load<TextAsset>("StreamingAssetPaths");
            string fs = paths.text;
            MajDebug.Log(fs);
            string[] fLines = fs.Replace("\\","/").Split("\n");
            foreach (string line in fLines)
            {
                if (line.Trim().Length <= 1) continue;
                var path = Path.Combine( Application.streamingAssetsPath, line.Trim());
                echoText.text = $"Extracting {path}...";
                MajDebug.Log($"Extracting {path}");
                yield return new WaitForEndOfFrame();
                byte[] data = null;
                int dataLen = 0;
                UnityWebRequest webRequest = UnityWebRequest.Get(path);
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    dataLen = webRequest.downloadHandler.data.Length;
                    data = webRequest.downloadHandler.data;
                    var file = Path.Combine(extractRoot, line.Trim());
                    var dir = Path.GetDirectoryName(file);
                    Directory.CreateDirectory(dir);
                    File.WriteAllBytes(file, data);
                }
                else
                {
                    MajDebug.LogError("Extract failed");
                }
            }
            echoText.text = $"Please Reboot The Game";
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
            await SongStorage.InitAsync(progress);

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
                switch (e.BZone)
                {
                    case ButtonZone.Test:
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