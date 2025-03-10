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
    public partial class SceneSwitcher : MonoBehaviour
    {
        public static MajScenes CurrentScene { get; private set; } = MajScenes.Init;
        public delegate void SceneSwitchEventHandler();
        public static event SceneSwitchEventHandler? OnSceneChanged;

        Animator animator;
        public Image SubImage;
        public Image MainImage;
        public TMP_Text loadingText;

        InputManager _inputManager;

        readonly string[] SCENE_NAMES = Enum.GetNames(typeof(MajScenes));

        const int SWITCH_ELAPSED = 400;
        private void Awake()
        {
            MajInstances.SceneSwitcher = this;
            var currentScene = SceneManager.GetActiveScene();
            var index = Array.FindIndex(SCENE_NAMES, x => x == currentScene.name);
            if(index != -1)
            {
                CurrentScene = Enum.Parse<MajScenes>(SCENE_NAMES[index]);
            }
            OnSceneChanged += CurrentSceneUpdate;
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

        // Start is called before the first frame update
        void Start()
        {
            _inputManager = MajInstances.InputManager;
            animator = GetComponent<Animator>();
            loadingText.gameObject.SetActive(false);
            DontDestroyOnLoad(this);
        }

        public void SwitchScene(string sceneName, bool autoFadeOut = true)
        {
            SwitchSceneInternal(sceneName,autoFadeOut).Forget();
        }

        public void FadeOut()
        {
            animator.SetBool("In", false);
            loadingText.gameObject.SetActive(false);
        }
        public void FadeIn()
        {
            animator.SetBool("In", true);
            loadingText.text = string.Empty;
            loadingText.gameObject.SetActive(true);
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
            _inputManager.ClearAllSubscriber();
            SubImage.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            loadingText.text = "";
            loadingText.gameObject.SetActive(true);
            animator.SetBool("In", true);
            await UniTask.Delay(SWITCH_ELAPSED);
            await SceneManager.LoadSceneAsync(sceneName);
            OnSceneChanged?.Invoke();
            if(autoFadeOut)
            { 
                animator.SetBool("In", false);
                loadingText.gameObject.SetActive(false);
            }
        }
    }
    public partial class SceneSwitcher : MonoBehaviour
    {
        // Task
        async UniTask SwitchSceneInternalAsync(string sceneName, Task taskToRun)
        {
            _inputManager.ClearAllSubscriber();
            SubImage.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            while (!taskToRun.IsCompleted)
                await UniTask.Yield();
            if (taskToRun.IsFaulted)
                MajDebug.LogException(taskToRun.Exception);
            await UniTask.Delay(SWITCH_ELAPSED);
            await SceneManager.LoadSceneAsync(sceneName);
            OnSceneChanged?.Invoke();
            animator.SetBool("In", false);
        }
        public async UniTaskVoid SwitchSceneAfterTaskAsync(string sceneName, Task taskToRun)
        {
            await SwitchSceneInternalAsync(sceneName, taskToRun);
        }
        // ValueTasl
        async UniTask SwitchSceneInternalAsync(string sceneName, ValueTask taskToRun)
        {
            _inputManager.ClearAllSubscriber();
            SubImage.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            while (!taskToRun.IsCompleted)
                await UniTask.Yield();
            if(taskToRun.IsFaulted)
                MajDebug.LogException(taskToRun.AsTask().Exception);
            await UniTask.Delay(SWITCH_ELAPSED);
            await SceneManager.LoadSceneAsync(sceneName);
            OnSceneChanged?.Invoke();
            animator.SetBool("In", false);
        }
        public async UniTaskVoid SwitchSceneAfterTaskAsync(string sceneName, ValueTask taskToRun)
        {
            await SwitchSceneInternalAsync(sceneName, taskToRun);
        }
        // UniTask
        async UniTask SwitchSceneInternalAsync(string sceneName, UniTask taskToRun)
        {
            _inputManager.ClearAllSubscriber();
            SubImage.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            while (taskToRun.Status is not (UniTaskStatus.Succeeded or UniTaskStatus.Faulted or UniTaskStatus.Canceled))
                await UniTask.Yield();
            if (taskToRun.Status is UniTaskStatus.Faulted)
                MajDebug.LogException(taskToRun.AsTask().Exception);
            await UniTask.Delay(SWITCH_ELAPSED);
            await SceneManager.LoadSceneAsync(sceneName);
            OnSceneChanged?.Invoke();
            animator.SetBool("In", false);
        }
        public async UniTaskVoid SwitchSceneAfterTaskAsync(string sceneName, UniTask taskToRun)
        {
            await SwitchSceneInternalAsync(sceneName, taskToRun);
        }


        // Task
        async UniTask<T> SwitchSceneInternalAsync<T>(string sceneName, Task<T> taskToRun)
        {
            _inputManager.ClearAllSubscriber();
            SubImage.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            while (!taskToRun.IsCompleted)
                await UniTask.Yield();
            if (taskToRun.IsFaulted)
                throw taskToRun.Exception;
            await UniTask.Delay(SWITCH_ELAPSED);
            await SceneManager.LoadSceneAsync(sceneName);
            OnSceneChanged?.Invoke();
            return taskToRun.Result;
        }
        public async UniTask<T> SwitchSceneAfterTaskAsync<T>(string sceneName, Task<T> taskToRun)
        {
            return await SwitchSceneInternalAsync(sceneName, taskToRun);
        }
        // ValueTasl
        async UniTask<T> SwitchSceneInternalAsync<T>(string sceneName, ValueTask<T> taskToRun)
        {
            _inputManager.ClearAllSubscriber();
            SubImage.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            while (!taskToRun.IsCompleted)
                await UniTask.Yield();
            if (taskToRun.IsFaulted)
                throw taskToRun.AsTask().Exception;
            await UniTask.Delay(SWITCH_ELAPSED);
            await SceneManager.LoadSceneAsync(sceneName);
            OnSceneChanged?.Invoke();
            animator.SetBool("In", false);
            return taskToRun.Result;
        }
        public async UniTask<T> SwitchSceneAfterTaskAsync<T>(string sceneName, ValueTask<T> taskToRun)
        {
            return await SwitchSceneInternalAsync(sceneName, taskToRun);
        }
        // UniTask
        async UniTask<T> SwitchSceneInternalAsync<T>(string sceneName, UniTask<T> taskToRun)
        {
            _inputManager.ClearAllSubscriber();
            SubImage.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            //MainImage.sprite = MajInstances.SkinManager.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            while (taskToRun.Status is not (UniTaskStatus.Succeeded or UniTaskStatus.Faulted or UniTaskStatus.Canceled))
                await UniTask.Yield();
            await UniTask.Delay(SWITCH_ELAPSED);
            await SceneManager.LoadSceneAsync(sceneName);
            OnSceneChanged?.Invoke();
            animator.SetBool("In", false);
            switch(taskToRun.Status)
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