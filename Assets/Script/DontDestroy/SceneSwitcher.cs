using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay
{
    public partial class SceneSwitcher : MonoBehaviour
    {
        Animator animator;
        public Image SubImage;
        public Image MainImage;
        public static SceneSwitcher Instance;
        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
            DontDestroyOnLoad(this);
        }

        public void SwitchScene(int sceneIndex)
        {
            SwitchSceneInternal(sceneIndex).Forget();
        }
        public void SwitchScene(string sceneName)
        {
            SwitchSceneInternal(sceneName).Forget();
        }
        async UniTask SwitchSceneInternal(int sceneIndex)
        {
            SubImage.sprite = SkinManager.Instance.SelectedSkin.SubDisplay;
            MainImage.sprite = SkinManager.Instance.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            await UniTask.Delay(250);
            await SceneManager.LoadSceneAsync(sceneIndex);
            animator.SetBool("In", false);
        }
        async UniTaskVoid SwitchSceneInternal(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            await SwitchSceneInternal(scene.buildIndex);
        }
    }
    public partial class SceneSwitcher : MonoBehaviour
    {
        // Task
        async UniTask SwitchSceneInternalAsync(int sceneIndex, Task taskToRun)
        {
            SubImage.sprite = SkinManager.Instance.SelectedSkin.SubDisplay;
            MainImage.sprite = SkinManager.Instance.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            while (!taskToRun.IsCompleted)
                await UniTask.Yield();
            await SceneManager.LoadSceneAsync(sceneIndex);
            await UniTask.Delay(100);
            animator.SetBool("In", false);
        }
        public async UniTaskVoid SwitchSceneAfterTaskAsync(int sceneIndex, Task taskToRun)
        {
            await SwitchSceneInternalAsync(sceneIndex, taskToRun);
        }
        public async UniTaskVoid SwitchSceneAfterTaskAsync(string sceneName, Task taskToRun)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            await SwitchSceneInternalAsync(scene.buildIndex, taskToRun);
        }
        // ValueTasl
        async UniTask SwitchSceneInternalAsync(int sceneIndex, ValueTask taskToRun)
        {
            SubImage.sprite = SkinManager.Instance.SelectedSkin.SubDisplay;
            MainImage.sprite = SkinManager.Instance.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            while (!taskToRun.IsCompleted)
                await UniTask.Yield();
            await SceneManager.LoadSceneAsync(sceneIndex);
            await UniTask.Delay(100);
            animator.SetBool("In", false);
        }
        public async UniTaskVoid SwitchSceneAfterTaskAsync(int sceneIndex, ValueTask taskToRun)
        {
            await SwitchSceneInternalAsync(sceneIndex, taskToRun);
        }
        public async UniTaskVoid SwitchSceneAfterTaskAsync(string sceneName, ValueTask taskToRun)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            await SwitchSceneInternalAsync(scene.buildIndex, taskToRun);
        }
        // UniTask
        async UniTask SwitchSceneInternalAsync(int sceneIndex, UniTask taskToRun)
        {
            SubImage.sprite = SkinManager.Instance.SelectedSkin.SubDisplay;
            MainImage.sprite = SkinManager.Instance.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            while (taskToRun.Status is not (UniTaskStatus.Succeeded or UniTaskStatus.Faulted or UniTaskStatus.Canceled))
                await UniTask.Yield();
            await SceneManager.LoadSceneAsync(sceneIndex);
            await UniTask.Delay(100);
            animator.SetBool("In", false);
        }
        public async UniTaskVoid SwitchSceneAfterTaskAsync(int sceneIndex, UniTask taskToRun)
        {
            await SwitchSceneInternalAsync(sceneIndex, taskToRun);
        }
        public async UniTaskVoid SwitchSceneAfterTaskAsync(string sceneName, UniTask taskToRun)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            await SwitchSceneInternalAsync(scene.buildIndex, taskToRun);
        }


        // Task
        async UniTask<T> SwitchSceneInternalAsync<T>(int sceneIndex, Task<T> taskToRun)
        {
            SubImage.sprite = SkinManager.Instance.SelectedSkin.SubDisplay;
            MainImage.sprite = SkinManager.Instance.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            while (!taskToRun.IsCompleted)
                await UniTask.Yield();
            await SceneManager.LoadSceneAsync(sceneIndex);
            await UniTask.Delay(100);
            animator.SetBool("In", false);
            return taskToRun.Result;
        }
        public async UniTask<T> SwitchSceneAfterTaskAsync<T>(int sceneIndex, Task<T> taskToRun)
        {
            return await SwitchSceneInternalAsync(sceneIndex, taskToRun);
        }
        public async UniTask<T> SwitchSceneAfterTaskAsync<T>(string sceneName, Task<T> taskToRun)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            return await SwitchSceneInternalAsync(scene.buildIndex, taskToRun);
        }
        // ValueTasl
        async UniTask<T> SwitchSceneInternalAsync<T>(int sceneIndex, ValueTask<T> taskToRun)
        {
            SubImage.sprite = SkinManager.Instance.SelectedSkin.SubDisplay;
            MainImage.sprite = SkinManager.Instance.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            while (!taskToRun.IsCompleted)
                await UniTask.Yield();
            await SceneManager.LoadSceneAsync(sceneIndex);
            await UniTask.Delay(100);
            animator.SetBool("In", false);
            return taskToRun.Result;
        }
        public async UniTask<T> SwitchSceneAfterTaskAsync<T>(int sceneIndex, ValueTask<T> taskToRun)
        {
            return await SwitchSceneInternalAsync(sceneIndex, taskToRun);
        }
        public async UniTask<T> SwitchSceneAfterTaskAsync<T>(string sceneName, ValueTask<T> taskToRun)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            return await SwitchSceneInternalAsync(scene.buildIndex, taskToRun);
        }
        // UniTask
        async UniTask<T> SwitchSceneInternalAsync<T>(int sceneIndex, UniTask<T> taskToRun)
        {
            SubImage.sprite = SkinManager.Instance.SelectedSkin.SubDisplay;
            MainImage.sprite = SkinManager.Instance.SelectedSkin.LoadingSplash;
            animator.SetBool("In", true);
            while (taskToRun.Status is not (UniTaskStatus.Succeeded or UniTaskStatus.Faulted or UniTaskStatus.Canceled))
                await UniTask.Yield();
            await SceneManager.LoadSceneAsync(sceneIndex);
            await UniTask.Delay(100);
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
        public async UniTask<T> SwitchSceneAfterTaskAsync<T>(int sceneIndex, UniTask<T> taskToRun)
        {
            return await SwitchSceneInternalAsync(sceneIndex, taskToRun);
        }
        public async UniTask<T> SwitchSceneAfterTaskAsync<T>(string sceneName, UniTask<T> taskToRun)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            return await SwitchSceneInternalAsync(scene.buildIndex, taskToRun);
        }
    }
}