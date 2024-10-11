using MajdataPlay.IO;
using MajdataPlay.Types;
using MajSimaiDecode;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Scripting;
using System.IO;
using MajdataPlay.Utils;
using PimDeWitte.UnityMainThreadDispatcher;

namespace MajdataPlay.Game
{
#nullable enable
    public class GamePlayManager : MonoBehaviour
    {
        /// <summary>
        /// 当前逻辑帧的时刻<para>Unit: Second</para>
        /// </summary>
        public float ThisFrameSec { get; private set; } = 0;
        public static GamePlayManager Instance { get; private set; }
        public float FirstNoteAppearTiming { get; set; } = 0f;
        public ComponentState State { get; private set; } = ComponentState.Idle;
        public MaiScore? HistoryScore { get; private set; }
        public (float, float) BreakParams => (0.95f + Math.Max(Mathf.Sin(GetFrame() * 0.20f) * 0.8f, 0), 1f + Math.Min(Mathf.Sin(GetFrame() * 0.2f) * -0.15f, 0));
        public float NoteSpeed { get; private set; } = 9f;
        public float TouchSpeed { get; private set; } = 7.5f;
        public float AudioTime { get; private set; } = -114514f;
        public float AudioTimeNoOffset { get; private set; } = -114514f;
        public bool IsStart => audioSample?.IsPlaying ?? false;
        public float CurrentSpeed { get; set; } = 1f;
        public float AudioStartTime { get; private set; } = -114514f;


        [SerializeField]
        GameObject loadingMask;
        [SerializeField]
        GameSetting gameSetting = GameManager.Instance.Setting;
        [SerializeField]
        GameObject skipBtn;
        NoteLoader noteLoader;
        Text errorText;
        SimaiProcess Chart;
        SongDetail song;
        AudioSampleWrap? audioSample = null;
        ObjectCounter objectCounter;
        CancellationTokenSource allTaskTokenSource = new();
        List<AnwserSoundPoint> AnwserSoundList = new List<AnwserSoundPoint>();

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long filetime);
        float timeSource
        {
            get
            {
                if (GameManager.Instance.UseUnityTimer)
                    return Time.unscaledTime;

                GetSystemTimePreciseAsFileTime(out var filetime);
                filetime = filetime - fileTimeAtStart;
                //print(filetime);
                return (float)(filetime / 10000000d);
            }
        }
        long fileTimeAtStart = 0;
        TextMeshPro loadingText;
        Image loadingImage;

        private void Awake()
        {
            Instance = this;
            //print(GameManager.Instance.SelectedIndex);
            song = GameManager.Instance.Collection.Current;
            HistoryScore = ScoreManager.Instance.GetScore(song, GameManager.Instance.SelectedDiff);
            GetSystemTimePreciseAsFileTime(out fileTimeAtStart);
        }

        private void OnPauseButton(object sender, InputEventArgs e)
        {
            if (e.IsButton && e.IsClick && e.Type == SensorType.P1)
            {
                print("Pause!!");
                BackToList();
            }
        }

        void Start()
        {
            objectCounter = FindObjectOfType<ObjectCounter>();
            State = ComponentState.Loading;
            loadingText = loadingMask.transform.GetChild(0).GetComponent<TextMeshPro>();
            loadingImage = loadingMask.GetComponent<Image>();
            InputManager.Instance.BindAnyArea(OnPauseButton);
            errorText = GameObject.Find("ErrText").GetComponent<Text>();
            DumpOnlineChart().Forget();
        }

        async UniTask DumpOnlineChart()
        {
            if (song.isOnline)
            {
                LightManager.Instance.SetAllLight(Color.red);
                loadingText.text = $"{Localization.GetLocalizedText("Downloading")}...";
                var dumpTask = song.DumpToLocal();
                while (!dumpTask.IsCompleted)
                {
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                }
                song = dumpTask.Result;
            }
            var loadingTask =  UniTask.WhenAll(LoadAudioTrack(), LoadChart());
            var task = loadingTask.AsTask();
            while (!task.IsCompleted)
                await UniTask.Yield();
            if(task.IsFaulted)
            {
                foreach (var e in task.Exception.InnerExceptions)
                {
                    switch(e)
                    {
                        case InvalidAudioTrackException audioE:
                            State = ComponentState.Failed;
                            loadingText.text = $"{Localization.GetLocalizedText("Failed to load chart")}\n{audioE.Message}";
                            loadingText.color = Color.red;
                            Debug.LogError(audioE);
                            return;
                        case TaskCanceledException:
                            return;
                        default:
                            State = ComponentState.Failed;
                            errorText.text = "加载note时出错了哟\n" + e.Message;
                            Debug.LogError(e);
                            return;
                    }
                }
            }

            DelayPlay().Forget();
        }
        async UniTask LoadAudioTrack()
        {
            var trackPath = song.TrackPath ?? string.Empty;
            if(!File.Exists(trackPath))
                throw new InvalidAudioTrackException("Audio track not found", trackPath);
            audioSample = await AudioManager.Instance.LoadMusicAsync(trackPath);
            await UniTask.Yield();
            if (audioSample is null)
                throw new InvalidAudioTrackException("Failed to decode audio track", trackPath);
            audioSample.SetVolume(gameSetting.Audio.Volume.BGM);
            LightManager.Instance.SetAllLight(Color.white);
        }
        async UniTask LoadChart()
        {
            var maidata = song.LoadInnerMaidata((int)GameManager.Instance.SelectedDiff);
            loadingText.text = $"{Localization.GetLocalizedText("Deserialization")}...";
            if (string.IsNullOrEmpty(maidata))
            {
                BackToList();
                throw new TaskCanceledException("Empty chart");
            }
            Chart = new SimaiProcess(maidata);
            if (Chart.notelist.Count == 0)
            {
                BackToList();
                throw new TaskCanceledException("Empty chart");
            }

            await Task.Run(() =>
            {
                //Generate ClockSounds
                var countnum = song.ClockCount == null ? 4 : song.ClockCount;
                var firstBpm = Chart.notelist.FirstOrDefault().currentBpm;
                var interval = 60 / firstBpm;
                if (Chart.notelist.Any(o => o.time < countnum * interval))
                {
                    //if there is something in first measure, we add clock before the bgm
                    for (int i = 0; i < countnum; i++)
                    {
                        AnwserSoundList.Add(new AnwserSoundPoint()
                        {
                            time = -(i + 1) * interval,
                            isClock = true,
                            isPlayed = false
                        });
                    }
                }
                else
                {
                    //if nothing there, we can add it with bgm
                    for (int i = 0; i < countnum; i++)
                    {
                        AnwserSoundList.Add(new AnwserSoundPoint()
                        {
                            time = i * interval,
                            isClock = true,
                            isPlayed = false
                        });
                    }
                }


                //Generate AnwserSounds
                foreach (var timingPoint in Chart.notelist)
                {
                    if (timingPoint.noteList.All(o => o.isSlideNoHead)) continue;

                    AnwserSoundList.Add(new AnwserSoundPoint()
                    {
                        time = timingPoint.time,
                        isClock = false,
                        isPlayed = false
                    });
                    var holds = timingPoint.noteList.FindAll(o => o.noteType == SimaiNoteType.Hold || o.noteType == SimaiNoteType.TouchHold);
                    if (holds.Count == 0) continue;
                    foreach (var hold in holds)
                    {
                        var newtime = timingPoint.time + hold.holdTime;
                        if (!Chart.notelist.Any(o => Math.Abs(o.time - newtime) < 0.001) &&
                            !AnwserSoundList.Any(o => Math.Abs(o.time - newtime) < 0.001)
                            )
                            AnwserSoundList.Add(new AnwserSoundPoint()
                            {
                                time = newtime,
                                isClock = false,
                                isPlayed = false
                            });
                    }
                }
                AnwserSoundList = AnwserSoundList.OrderBy(o => o.time).ToList();
            });
        }

        /// <summary>
        /// 背景加载
        /// </summary>
        /// <returns></returns>
        async UniTask InitBackground()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            var BGManager = GameObject.Find("Background").GetComponent<BGManager>();
            if (!string.IsNullOrEmpty(song.VideoPath))
                BGManager.SetBackgroundMovie(song.VideoPath);
            else
            {
                var task = song.GetSpriteAsync();
                while (!task.IsCompleted)
                {
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                }
                BGManager.SetBackgroundPic(task.Result);
            }


            BGManager.SetBackgroundDim(gameSetting.Game.BackgroundDim);
        }
        /// <summary>
        /// 初始化NoteLoader与实例化Note对象
        /// </summary>
        /// <returns></returns>
        async UniTask LoadNotes()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            noteLoader = GameObject.Find("NoteLoader").GetComponent<NoteLoader>();
            noteLoader.noteSpeed = (float)(107.25 / (71.4184491 * Mathf.Pow(gameSetting.Game.TapSpeed + 0.9975f, -0.985558604f)));
            noteLoader.touchSpeed = gameSetting.Game.TouchSpeed;

            //var loaderTask = noteLoader.LoadNotes(Chart);
            var loaderTask = noteLoader.LoadNotesIntoPool(Chart);
            var timer = 1f;

            while (noteLoader.State < NoteLoaderStatus.Finished)
            {
                if (noteLoader.State == NoteLoaderStatus.Error)
                {
                    var e = loaderTask.AsTask().Exception;
                    errorText.text = "加载note时出错了哟\n" + e.Message;
                    loadingText.text = $"{Localization.GetLocalizedText("Failed to load chart")}\n{e.Message}%";
                    Debug.LogError(e);
                    StopAllCoroutines();
                    throw e;
                }
                loadingText.text = $"{Localization.GetLocalizedText("Loading Chart")}...\n{noteLoader.Process * 100:F2}%";
                await UniTask.Yield();
            }
            loadingText.text = $"{Localization.GetLocalizedText("Loading Chart")}...\n100.00%";

            while (timer > 0)
            {
                await UniTask.Yield();
                timer -= Time.deltaTime;
                var textColor = Color.white;
                var maskColor = Color.black;
                textColor.a = timer / 1f;
                maskColor.a = timer / 1f * 0.75f;
                loadingImage.color = maskColor;
                loadingText.color = textColor;
            }

            loadingMask.SetActive(false);
            loadingText.gameObject.SetActive(false);
        }
        async UniTaskVoid DelayPlay()
        {
            if (audioSample is null)
                return;
            AudioTime = -5f;

            await InitBackground();
            var noteLoaderTask = LoadNotes().AsTask();

            while (!noteLoaderTask.IsCompleted)
            {
                if (noteLoaderTask.IsFaulted)
                    throw noteLoaderTask.Exception;
                await UniTask.Yield();
            }

            GameManager.Instance.DisableGC();
            Time.timeScale = 1f;
            var firstClockTiming = AnwserSoundList[0].time;
            float extraTime = 5f;
            if (firstClockTiming < -5f)
                extraTime += (-(float)firstClockTiming - 5f) + 2f;
            if (FirstNoteAppearTiming != 0)
                extraTime += -(FirstNoteAppearTiming + 4f);
            AudioStartTime = timeSource + (float)audioSample.CurrentSec + extraTime;
            StartToPlayAnswer();
            audioSample.Play();
            audioSample.Pause();

            State = ComponentState.Running;

            while (timeSource - AudioStartTime < 0)
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            audioSample.Play();
            AudioStartTime = timeSource;

        }

        private void OnDestroy()
        {
            print("GPManagerDestroy");
            DisposeAudioTrack();
            audioSample = null;
            State = ComponentState.Finished;
            allTaskTokenSource.Cancel();
            GameManager.Instance.EnableGC();
        }
        // Update is called once per frame
        void Update()
        {
            UpdateAudioTime();
            if (audioSample is null)
                return;
            else if (!objectCounter.AllFinished)
                return;
            else if (State != ComponentState.Running)
                return;

            var remainingTime = AudioTime - audioSample.Length.TotalSeconds;
            if(remainingTime < -6)
                skipBtn.SetActive(true);
            else if(remainingTime >= 0)
            {
                skipBtn.SetActive(false);
                EndGame();
            }
        }
        void FixedUpdate()
        {
            ThisFrameSec = AudioTime;
        }
        void UpdateAudioTime()
        {
            if (audioSample is null)
                return;
            else if (State != ComponentState.Running)
                return;
            else if (AudioStartTime == -114514f)
                return;
            //Do not use this!!!! This have connection with sample batch size
            //AudioTime = (float)audioSample.GetCurrentTime();
            var chartOffset = (float)song.First + gameSetting.Judge.AudioOffset;
            AudioTime = timeSource - AudioStartTime - chartOffset;
            AudioTimeNoOffset = timeSource - AudioStartTime;

            var realTimeDifference = (float)audioSample.CurrentSec - (timeSource - AudioStartTime);
            if (!audioSample.IsPlaying)
                return;
            if (Math.Abs(realTimeDifference) > 0.04f && AudioTime > 0)
            {
                errorText.text = "音频错位了哟\n" + realTimeDifference;
            }
            else if (Math.Abs(realTimeDifference) > 0.02f && AudioTime > 0 && GameManager.Instance.Setting.Debug.TryFixAudioSync)
            {
                errorText.text = "修正音频哟\n" + realTimeDifference;
                AudioStartTime -= realTimeDifference * 0.8f;
            }
        }
        async void StartToPlayAnswer()
        {
            int i = 0;
            await Task.Run(async () =>
            {
                while (!allTaskTokenSource.IsCancellationRequested)
                {
                    if (i >= AnwserSoundList.Count)
                        return;

                    var noteToPlay = AnwserSoundList[i].time;
                    var delta = AudioTime - noteToPlay;

                    if (delta > 0)
                    {
                        if (AnwserSoundList[i].isClock)
                        {
                            AudioManager.Instance.PlaySFX(SFXSampleType.CLOCK);
                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            {
                                XxlbAnimationController.instance.Stepping();
                            });
                        }
                        else
                            AudioManager.Instance.PlaySFX(SFXSampleType.ANSWER);
                        AnwserSoundList[i].isPlayed = true;
                        i++;
                    }
                    await Task.Delay(1);
                }
            });
        }
        public float GetFrame()
        {
            var _audioTime = AudioTime * 1000;

            return _audioTime / 16.6667f;
        }
        void DisposeAudioTrack()
        {
            if (audioSample is not null)
            {
                audioSample.Pause();
                audioSample.Dispose();
                audioSample = null;
            }
        }
        public void BackToList()
        {
            InputManager.Instance.UnbindAnyArea(OnPauseButton);
            GameManager.Instance.EnableGC();
            StopAllCoroutines();
            DisposeAudioTrack();
            //AudioManager.Instance.UnLoadMusic();
            
            DelayBackToList().Forget();

        }
        async UniTaskVoid DelayBackToList()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            GameObject.Find("Notes").GetComponent<NoteManager>().DestroyAllNotes();
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            SceneSwitcher.Instance.SwitchScene("List");
        }
        public void EndGame()
        {
            var acc = objectCounter.CalculateFinalResult();
            print("GameResult: " + acc);
            GameManager.LastGameResult = objectCounter.GetPlayRecord(song, GameManager.Instance.SelectedDiff);
            GameManager.Instance.EnableGC();
            DelayEndGame().Forget();
            BGManager.Instance.CancelTimeRef();
            State = ComponentState.Finished;
        }
        async UniTaskVoid DelayEndGame()
        {
            await UniTask.Delay(2000);
            DisposeAudioTrack();
            InputManager.Instance.UnbindAnyArea(OnPauseButton);
            SceneSwitcher.Instance.SwitchScene("Result");
        }
        class AnwserSoundPoint
        {
            public double time;
            public bool isClock;
            public bool isPlayed;
        }
    }
}