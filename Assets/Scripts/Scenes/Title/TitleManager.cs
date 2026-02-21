using Cysharp.Threading.Tasks;
using MajdataPlay.Settings;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
#nullable enable
namespace MajdataPlay.Scenes.Title
{
    public class TitleManager : MonoBehaviour
    {
        public TextMeshProUGUI echoText;
        public Animator fadeInAnim;

        bool _flag = false;
        float _pressTime = 0f;
        void Start()
        {
#if UNITY_ANDROID || UNITY_IOS
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



            echoText.text = $"{Localization.GetLocalizedText("MAJTEXT_LOADING_SCORE_STORAGE")}...";
            await UniTask.DelayFrame(9);
            var task1 = ScoreManager.InitAsync().AsValueTask();
            while (!task1.IsCompleted)
            {
                await UniTask.Yield();
            }

            task1 = ChartSettingStorage.InitAsync();

            while (!task1.IsCompleted)
            {
                await UniTask.Yield();
            }
            await UniTask.Delay(2000);
            echoText.text = $"{Localization.GetLocalizedText("MAJTEXT_LOADING_SKIN")}...";
            var task2 = MajInstances.SkinManager.InitAsync();
            while (!task2.IsCompleted)
            {
                await UniTask.Yield();
            }

            echoText.text = $"{"MAJTEXT_SCANNING_CHARTS".i18n()}...";
            var task3 = StartScanningChart();
            try
            {
                if (task3 is null)
                {
                    return;
                }
                var isEmpty = false;
                while (true)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);

                    if (task3.IsCompleted)
                    {
                        if (task3.IsFaulted)
                        {
                            echoText.text = "MAJTEXT_SCAN_CHARTS_FAILED".i18n();
                            MajDebug.LogException(task3.Exception);
                        }
                        else if (SongStorage.IsEmpty)
                        {
                            isEmpty = true;
                            echoText.text = "MAJTEXT_NO_CHART".i18n();
                        }
                        else
                        {
                            //if (MajInstances.Settings.Online.Enable)
                            //{
                            //    foreach (var endpoint in MajInstances.Settings.Online.ApiEndpoints)
                            //    {
                            //        try
                            //        {
                            //            if (endpoint.Username is null || endpoint.Password is null) continue;
                            //            echoText.text = "Login " + endpoint.Name + " as " + endpoint.Username;
                            //            await Online.LoginAsync(endpoint);
                            //            await UniTask.Delay(1000);
                            //        }
                            //        catch (Exception ex)
                            //        {
                            //            MajDebug.LogError(ex);
                            //            echoText.text = "Login failed for " + endpoint.Name;
                            //            await UniTask.Delay(1000);
                            //        }
                            //    }
                            //}
                            echoText.text = "MAJTEXT_PRESS_ANY_KEY".i18n();
                            InputManager.BindAnyArea(OnAreaDown);

                            var list = new string[] { "game_init.wav", "game_init_2.wav", "game_init_3.wav" };
                            MajInstances.AudioManager.PlaySFX(list[UnityEngine.Random.Range(0, list.Length)]);

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
#if UNITY_ANDROID // Android Only (Extract Assets)
            var extractRoot = MajEnv.AssetsPath;
            echoText.text = $"Extracting Assets...";
            Directory.CreateDirectory(extractRoot);
            List<string> filePathsList = new List<string>();
            TextAsset paths = Resources.Load<TextAsset>("StreamingAssetPaths");
            string fs = paths.text;
            MajDebug.LogInfo(fs);
            string[] fLines = fs.Replace("\\", "/").Split("\n");
            foreach (string line in fLines)
            {
                if (line.Trim().Length <= 1) continue;
                var path = Path.Combine(Application.streamingAssetsPath, line.Trim());
                echoText.text = $"Extracting {path}...";
                MajDebug.LogInfo($"Extracting {path}");
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
#elif UNITY_IOS // iOS Only (Extract Assets)
            var extractRoot = MajEnv.AssetsPath;
            echoText.text = $"Extracting Assets...";
            Directory.CreateDirectory(extractRoot);
            List<string> filePathsList = new List<string>();
            TextAsset paths = Resources.Load<TextAsset>("StreamingAssetPaths");
            string fs = paths.text;
            MajDebug.LogInfo(fs);
            string[] fLines = fs.Replace("\\", "/").Split("\n");
            foreach (string line in fLines)
            {
                var srcPath = Path.Combine(Application.streamingAssetsPath,line);
                var dstPath = Path.Combine(extractRoot, line);
                var dstDir = Path.GetDirectoryName(dstPath);
                if (!string.IsNullOrEmpty(dstDir)) Directory.CreateDirectory(dstDir);

                echoText.text = $"Extracting {line}...";
                MajDebug.LogInfo($"Extracting(iOS direct): {srcPath} -> {dstPath}");

                try
                {
                    byte[] data = File.ReadAllBytes(srcPath);
                    if (data == null || data.Length == 0)
                    {
                        MajDebug.LogError($"Extract failed(iOS): empty data: {line}\nsrc={srcPath}");
                        continue;
                    }

                    File.WriteAllBytes(dstPath, data);
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"Extract failed(iOS): {line}\nsrc={srcPath}\n{e}");
                }

                yield return null;
            }
            echoText.text = "Please Reboot The Game";
#else
            yield return new WaitForEndOfFrame();
#endif
        }

        async Task StartScanningChart()
        {
            var progress = new Progress<string>();
            progress.ProgressChanged += (o, e) =>
            {
                echoText.text = e;
            };
            await Task.Delay(3000);
            await SongStorage.InitAsync(progress);

            if (!SongStorage.IsEmpty)
            {
                var setting = MajInstances.Settings;
                var listConfig = MajEnv.RuntimeConfig.List;
                var dirId = listConfig.SelectedDirGuid;
                var selectedSongHash = listConfig.SelectedSongHash;
                var isDirMatched = false;

                if (dirId != Guid.Empty)
                {
                    var dirIndex = Array.FindIndex(SongStorage.Collections, x => x.Id == dirId);
                    if (dirIndex != -1)
                    {
                        listConfig.SelectedDir = dirIndex;
                        isDirMatched = true;
                    }
                }
                SongStorage.CollectionIndex = listConfig.SelectedDir;
                listConfig.SelectedDir = SongStorage.CollectionIndex;
                var selectedCollection = SongStorage.WorkingCollection;
                var selectedIndex = listConfig.SelectedSongIndex;
                listConfig.SelectedDirGuid = selectedCollection.Id;

                if (isDirMatched && !string.IsNullOrEmpty(selectedSongHash))
                {
                    selectedIndex = Array.FindIndex(selectedCollection.ToArray(), x => x.Hash == selectedSongHash);
                    if (selectedIndex == -1)
                    {
                        selectedIndex = 0;
                    }
                }

                if (selectedCollection.IsEmpty)
                {
                    listConfig.SelectedSongIndex = 0;
                    return;
                }
                else if (selectedIndex >= selectedCollection.Count)
                {
                    selectedCollection.Index = 0;
                    listConfig.SelectedSongIndex = 0;
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
                        if (_flag)
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
            if (MajInstances.Settings.Online.Enable)
            {
                MajInstances.SceneSwitcher.SwitchScene("Login", false);
            }
            else
            {
                MajInstances.SceneSwitcher.SwitchScene("List", false);
            }
        }
    }
}