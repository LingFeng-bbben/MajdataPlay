using Cysharp.Threading.Tasks;
using MajdataPlay.Collections;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
        public static MajScenes CurrentScene { get; private set; } = MajScenes.Init;

        Animator animator;
        public Image SubImage;
        public Image MainImage;
        public TMP_Text loadingText;
        public Color LoadingLightColor;

        readonly string[] SCENE_NAMES = Enum.GetNames(typeof(MajScenes));

        const int SWITCH_ELAPSED = 400;
        protected override void Awake()
        {
            base .Awake();
            Majdata<IMainCameraProvider>.Instance = this;
            SceneManager.activeSceneChanged += OnUnitySceneChanged;
            MainCamera = Camera.main;
            var currentScene = SceneManager.GetActiveScene();
            var index = Array.FindIndex(SCENE_NAMES, x => x == currentScene.name);
            if(index != -1)
            {
                CurrentScene = Enum.Parse<MajScenes>(SCENE_NAMES[index]);
            }
            animator = GetComponent<Animator>();
            loadingText.gameObject.SetActive(false);
        }
        void CurrentSceneUpdate()
        {
            var currentScene = SceneManager.GetActiveScene();
            var index = Array.FindIndex(SCENE_NAMES, x => x == currentScene.name);
            if (index != -1)
            {
                CurrentScene = Enum.Parse<MajScenes>(SCENE_NAMES[index]);
            }
        }
        void OnUnitySceneChanged(Scene scene1, Scene scene2)
        {
            MainCamera = Camera.main;
        }

        public void SwitchScene(string sceneName, bool autoFadeOut = true)
        {
            SwitchSceneInternal(sceneName,autoFadeOut).Forget();
        }

        public void FadeOut()
        {
            animator.SetBool("In", false);
            loadingText.gameObject.SetActive(false);
            MajInstances.LightManager.SetAllLight(Color.white);
        }
        public void FadeIn()
        {
            animator.SetBool("In", true);
            loadingText.text = string.Empty;
            loadingText.gameObject.SetActive(true);
            MajInstances.LightManager.SetAllLight(LoadingLightColor);
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
            SubImage.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            loadingText.text = "";
            loadingText.gameObject.SetActive(true);
            animator.SetBool("In", true);
            await UniTask.Delay(SWITCH_ELAPSED);
            MajInstances.LightManager.SetAllLight(LoadingLightColor);
            await SceneManager.LoadSceneAsync(sceneName);
            await UniTask.DelayFrame(2);
            if(autoFadeOut)
            { 
                animator.SetBool("In", false);
                MajInstances.LightManager.SetAllLight(Color.white);
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
            SubImage.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            while (!taskToRun.IsCompleted)
                await UniTask.Yield();
            if (taskToRun.IsFaulted)
                MajDebug.LogException(taskToRun.Exception);
            await UniTask.Delay(SWITCH_ELAPSED);
            MajInstances.LightManager.SetAllLight(LoadingLightColor);
            await SceneManager.LoadSceneAsync(sceneName);
            await UniTask.DelayFrame(2);
            animator.SetBool("In", false);
            MajInstances.LightManager.SetAllLight(Color.white);
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
            while (!taskToRun.IsCompleted)
                await UniTask.Yield();
            if(taskToRun.IsFaulted)
                MajDebug.LogException(taskToRun.AsTask().Exception);
            await UniTask.Delay(SWITCH_ELAPSED);
            MajInstances.LightManager.SetAllLight(LoadingLightColor);
            await SceneManager.LoadSceneAsync(sceneName);
            await UniTask.DelayFrame(2);
            animator.SetBool("In", false);
            MajInstances.LightManager.SetAllLight(Color.white);
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
            while (taskToRun.Status is not (UniTaskStatus.Succeeded or UniTaskStatus.Faulted or UniTaskStatus.Canceled))
                await UniTask.Yield();
            if (taskToRun.Status is UniTaskStatus.Faulted)
                MajDebug.LogException(taskToRun.AsTask().Exception);
            await UniTask.Delay(SWITCH_ELAPSED);
            MajInstances.LightManager.SetAllLight(LoadingLightColor);
            await SceneManager.LoadSceneAsync(sceneName);
            await UniTask.DelayFrame(2);
            animator.SetBool("In", false);
            MajInstances.LightManager.SetAllLight(Color.white);
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
            while (!taskToRun.IsCompleted)
                await UniTask.Yield();
            if (taskToRun.IsFaulted)
                throw taskToRun.Exception;
            await UniTask.Delay(SWITCH_ELAPSED);
            MajInstances.LightManager.SetAllLight(LoadingLightColor);
            await SceneManager.LoadSceneAsync(sceneName);
            await UniTask.DelayFrame(2);
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
            while (!taskToRun.IsCompleted)
                await UniTask.Yield();
            if (taskToRun.IsFaulted)
                throw taskToRun.AsTask().Exception;
            await UniTask.Delay(SWITCH_ELAPSED);
            MajInstances.LightManager.SetAllLight(LoadingLightColor);
            await SceneManager.LoadSceneAsync(sceneName);
            await UniTask.DelayFrame(2);
            animator.SetBool("In", false);
            MajInstances.LightManager.SetAllLight(Color.white);
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
            while (taskToRun.Status is not (UniTaskStatus.Succeeded or UniTaskStatus.Faulted or UniTaskStatus.Canceled))
                await UniTask.Yield();
            await UniTask.Delay(SWITCH_ELAPSED);
            MajInstances.LightManager.SetAllLight(LoadingLightColor);
            await SceneManager.LoadSceneAsync(sceneName);
            await UniTask.DelayFrame(2);
            animator.SetBool("In", false);
            MajInstances.LightManager.SetAllLight(Color.white);
            switch (taskToRun.Status)
            {
                case UniTaskStatus.Succeeded:
                    return taskToRun.AsValueTask().Result;
                case UniTaskStatus.Faulted:
                    throw taskToRun.AsTask().Exception;
                default:
                    throw new TaskCanceledException();
            }
        }
        public async UniTask<T> SwitchSceneAfterTaskAsync<T>(string sceneName, UniTask<T> taskToRun)
        {
            return await SwitchSceneInternalAsync(sceneName, taskToRun);
        }
    }
}