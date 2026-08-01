using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MajdataPlay.Collections;
using MajdataPlay.Diagnostics;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using UnityEngine.UI;
using UnityEngine.Video;
#nullable enable
namespace MajdataPlay
{
    internal sealed partial class SceneSwitcher : MajSingleton, IMainCameraProvider
    {
        public Camera MainCamera 
        { 
            get; 
            private set; 
        }
        public static event EventHandler<(MajScenes NewScene, MajScenes OldScene)>? OnSceneChanged;
        public static MajScenes CurrentScene { get; private set; } = MajScenes.Init;
        public static MajScenes LastScene { get; private set; } = MajScenes.Init;

        Canvas _canvas;
        Animator animator;
        public Image SubImage;
        public Image MainImage;
        public TMP_Text loadingText;
        public Color LoadingLightColor;

        [SerializeField]
        VideoPlayer _videoPlayer;
        [SerializeField]
        SpriteRenderer _mvRenderer;
        GameObject _bgObject;

        readonly string[] SCENE_NAMES = Enum.GetNames(typeof(MajScenes));

        const int SWITCH_ELAPSED_MS = 400;
        const int AUTO_FADE_OUT_DELAY_MS = 50;
        protected override void Awake()
        {
            base.Awake();
            Majdata<IMainCameraProvider>.Instance = this;
            SceneManager.activeSceneChanged += OnUnitySceneChanged;
            MainCamera = Camera.main;
            var currentScene = SceneManager.GetActiveScene();
            var index = Array.FindIndex(SCENE_NAMES, x => x == currentScene.name);
            if(index != -1)
            {
                CurrentScene = Enum.Parse<MajScenes>(SCENE_NAMES[index]);
            }
            _canvas = GetComponent<Canvas>();
            animator = GetComponent<Animator>();
            loadingText.gameObject.SetActive(false);
            _bgObject = _videoPlayer.gameObject;
        }
        void OnUnitySceneChanged(Scene current, Scene next)
        {
            //MajDebug.LogDebug(ZString.Format("Scene unloaded: {0}", current.name));
            MajDebug.LogDebug(ZString.Format("Scene loaded: {0}", next.name));
            MainCamera = Camera.main;
            //var currentScene = SceneManager.GetActiveScene();
            var index = Array.FindIndex(SCENE_NAMES, x => x == next.name);
            var lastScene = CurrentScene;
            if (index != -1)
            {
                CurrentScene = Enum.Parse<MajScenes>(SCENE_NAMES[index]);
            }
            LastScene = lastScene;
            if(OnSceneChanged is not null)
            {
                OnSceneChanged(this, (CurrentScene, LastScene));
            }
            CabinetLed.SetCabinetLight(1.0f);
            _canvas.worldCamera = MainCamera;
        }

        public void SwitchScene(string sceneName, bool autoFadeOut = true)
        {
            SwitchSceneInternal(sceneName, autoFadeOut).Forget();
        }
        public UniTask SwitchSceneAsync(string sceneName, bool autoFadeOut = true)
        {
            return SwitchSceneInternal(sceneName, autoFadeOut);
        }
        public void PauseMV()
        {
            _videoPlayer.Pause();
        }
        public void PlayMV()
        {
            _videoPlayer.Play();
        }
        public void StopMV()
        {
            _videoPlayer.Stop();
        }
        public void HideMV()
        {
            PauseMV();
            //_videoPlayer.enabled = false;
            //_mvRenderer.enabled = false;
            _bgObject.layer = MajEnv.HIDDEN_LAYER;
        }
        public void ShowMV()
        {
            //_mvRenderer.enabled = true;
            //_videoPlayer.enabled = true;
            _bgObject.layer = MajEnv.DEFAULT_LAYER;
            PlayMV();
        }
        public void FadeOut()
        {
            animator.SetBool("In", false);
            loadingText.gameObject.SetActive(false);
            CabinetLed.SetAllLight(Color.white);
            CabinetLed.SetCabinetLight(1.0f);
        }
        public async UniTask FadeOutAsync()
        {
            animator.SetBool("In", false);
            await UniTask.Delay(SWITCH_ELAPSED_MS);
            loadingText.gameObject.SetActive(false);
            CabinetLed.SetAllLight(Color.white);
            CabinetLed.SetCabinetLight(1.0f);
        }
        public void FadeIn()
        {
            animator.SetBool("In", true);
            loadingText.text = string.Empty;
            loadingText.gameObject.SetActive(true);
            CabinetLed.SetAllLight(LoadingLightColor);
            CabinetLed.SetCabinetLight(0.5f);
        }
        public async UniTask FadeInAsync()
        {
            animator.SetBool("In", true);
            await UniTask.Delay(SWITCH_ELAPSED_MS);
            loadingText.text = string.Empty;
            loadingText.gameObject.SetActive(true);
            CabinetLed.SetAllLight(LoadingLightColor);
            CabinetLed.SetCabinetLight(0.5f);
        }
        public void SetLoadingText(string text , Color color)
        {
            loadingText.text = text;
            loadingText.color = color;
        }
        public void SetLoadingText(string text)
        {
            loadingText.text = text;
            loadingText.color = Color.white;
        }

        async UniTask SwitchSceneInternal(string sceneName, bool autoFadeOut)
        {
            InputManager.ClearAllSubscriber();
            SubImage.sprite = MajInstances.SkinManager?.SelectedSkin?.SubDisplay!;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            loadingText.text = "";
            loadingText.gameObject.SetActive(true);
            animator.SetBool("In", true);
            await UniTask.Delay(SWITCH_ELAPSED_MS);
            CabinetLed.SetAllLight(LoadingLightColor);
            CabinetLed.SetCabinetLight(0.5f);
            await SwitchSceneCoreAsync(sceneName, autoFadeOut);
        }
        async UniTask SwitchSceneCoreAsync(string sceneName, bool autoFadeOut)
        {
            //await SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            //await Resources.UnloadUnusedAssets();
            await SceneManager.LoadSceneAsync(sceneName);
            await UniTask.DelayFrame(1);
            await UniTask.Delay(AUTO_FADE_OUT_DELAY_MS);
            if (autoFadeOut)
            {
                animator.SetBool("In", false);
                CabinetLed.SetAllLight(Color.white);
                CabinetLed.SetCabinetLight(1.0f);
                loadingText.gameObject.SetActive(false);
            }
        }
    }
    internal sealed partial class SceneSwitcher : MajSingleton
    {
        // Task
        async UniTask SwitchSceneInternalAsync(string sceneName, Task taskToRun)
        {
            InputManager.ClearAllSubscriber();
            SubImage.sprite = MajInstances.SkinManager?.SelectedSkin?.SubDisplay!;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            await UniTask.Delay(SWITCH_ELAPSED_MS);
            while (!taskToRun.IsCompleted)
            {
                await UniTask.Yield();
            }
            if (taskToRun.IsFaulted)
            {
                throw taskToRun.Exception;
            }
            CabinetLed.SetAllLight(LoadingLightColor);
            CabinetLed.SetCabinetLight(0.5f);
            await SwitchSceneCoreAsync(sceneName, true);
        }
        public async UniTaskVoid SwitchSceneAfterTaskAsync(string sceneName, Task taskToRun)
        {
            await SwitchSceneInternalAsync(sceneName, taskToRun);
        }
        // ValueTasl
        async UniTask SwitchSceneInternalAsync(string sceneName, ValueTask taskToRun)
        {
            InputManager.ClearAllSubscriber();
            SubImage.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            await UniTask.Delay(SWITCH_ELAPSED_MS);
            while (!taskToRun.IsCompleted)
            {
                await UniTask.Yield();
            }
            if(taskToRun.IsFaulted)
            {
                throw taskToRun.AsTask().Exception;
            }
            CabinetLed.SetAllLight(LoadingLightColor);
            CabinetLed.SetCabinetLight(0.5f);
            await SwitchSceneCoreAsync(sceneName, true);
        }
        public async UniTaskVoid SwitchSceneAfterTaskAsync(string sceneName, ValueTask taskToRun)
        {
            await SwitchSceneInternalAsync(sceneName, taskToRun);
        }
        // UniTask
        async UniTask SwitchSceneInternalAsync(string sceneName, UniTask taskToRun)
        {
            InputManager.ClearAllSubscriber();
            SubImage.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            await UniTask.Delay(SWITCH_ELAPSED_MS);
            while (taskToRun.Status is not (UniTaskStatus.Succeeded or UniTaskStatus.Faulted or UniTaskStatus.Canceled))
            {
                await UniTask.Yield();
            }
            switch (taskToRun.Status)
            {
                case UniTaskStatus.Canceled:
                case UniTaskStatus.Faulted:
                    throw taskToRun.AsTask().Exception;
            }            
            CabinetLed.SetAllLight(LoadingLightColor);
            CabinetLed.SetCabinetLight(0.5f);
            await SwitchSceneCoreAsync(sceneName, true);
        }
        public async UniTaskVoid SwitchSceneAfterTaskAsync(string sceneName, UniTask taskToRun)
        {
            await SwitchSceneInternalAsync(sceneName, taskToRun);
        }


        // Task
        async UniTask<T> SwitchSceneInternalAsync<T>(string sceneName, Task<T> taskToRun)
        {
            InputManager.ClearAllSubscriber();
            SubImage.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            await UniTask.Delay(SWITCH_ELAPSED_MS);
            while (!taskToRun.IsCompleted)
            {
                await UniTask.Yield();
            }
            if (taskToRun.IsFaulted)
            {
                throw taskToRun.Exception;
            }
            CabinetLed.SetAllLight(LoadingLightColor);
            CabinetLed.SetCabinetLight(0.5f);
            await SwitchSceneCoreAsync(sceneName, true);
            return taskToRun.Result;
        }
        public async UniTask<T> SwitchSceneAfterTaskAsync<T>(string sceneName, Task<T> taskToRun)
        {
            return await SwitchSceneInternalAsync(sceneName, taskToRun);
        }
        // ValueTasl
        async UniTask<T> SwitchSceneInternalAsync<T>(string sceneName, ValueTask<T> taskToRun)
        {
            InputManager.ClearAllSubscriber();
            SubImage.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            await UniTask.Delay(SWITCH_ELAPSED_MS);
            while (!taskToRun.IsCompleted)
            {
                await UniTask.Yield();
            }
            if (taskToRun.IsFaulted)
            {
                throw taskToRun.AsTask().Exception;
            }
            CabinetLed.SetAllLight(LoadingLightColor);
            CabinetLed.SetCabinetLight(0.5f);
            await SwitchSceneCoreAsync(sceneName, true);
            return taskToRun.Result;
        }
        public async UniTask<T> SwitchSceneAfterTaskAsync<T>(string sceneName, ValueTask<T> taskToRun)
        {
            return await SwitchSceneInternalAsync(sceneName, taskToRun);
        }
        // UniTask
        async UniTask<T> SwitchSceneInternalAsync<T>(string sceneName, UniTask<T> taskToRun)
        {
            InputManager.ClearAllSubscriber();
            SubImage.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            await UniTask.Delay(SWITCH_ELAPSED_MS);
            while (taskToRun.Status is not (UniTaskStatus.Succeeded or UniTaskStatus.Faulted or UniTaskStatus.Canceled))
            {
                await UniTask.Yield();
            }
            switch (taskToRun.Status)
            {
                case UniTaskStatus.Canceled:
                case UniTaskStatus.Faulted:
                    throw taskToRun.AsTask().Exception;
            }
            CabinetLed.SetAllLight(LoadingLightColor);
            CabinetLed.SetCabinetLight(0.5f);
            await SwitchSceneCoreAsync(sceneName, true);

            return taskToRun.AsValueTask().Result;
        }
        public async UniTask<T> SwitchSceneAfterTaskAsync<T>(string sceneName, UniTask<T> taskToRun)
        {
            return await SwitchSceneInternalAsync(sceneName, taskToRun);
        }
    }
}
