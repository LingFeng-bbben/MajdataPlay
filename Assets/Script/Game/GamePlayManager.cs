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
using MajdataPlay.Types.Attribute;

namespace MajdataPlay.Game
{
#nullable enable
    public class GamePlayManager : MonoBehaviour
    {
        public static GamePlayManager Instance { get; private set; }

        public float NoteSpeed { get; private set; } = 9f;
        public float TouchSpeed { get; private set; } = 7.5f;
        // Timeline
        /// <summary>
        /// Time provider
        /// </summary>
        public float TimeSource
        {
            get
            {
                if (MajInstances.GameManager.UseUnityTimer)
                    return Time.unscaledTime;

                GetSystemTimePreciseAsFileTime(out var filetime);
                filetime = filetime - _fileTimeAtStart;
                //print(filetime);
                return (float)(filetime / 10000000d);
            }
        }
        /// <summary>
        /// 当前逻辑帧的时刻<para>Unit: Second</para>
        /// </summary>
        public float ThisFrameSec => _thisFrameSec;
        public float FirstNoteAppearTiming
        {
            get => _firstNoteAppearTiming;
            set => _firstNoteAppearTiming = value;
        }
        public float AudioTime => _audioTime;
        public float AudioTimeNoOffset => _audioTimeNoOffset;
        public float AudioStartTime => _audioStartTime;
        // Control
        public bool IsStart => _audioSample?.IsPlaying ?? false;
        public float CurrentSpeed { get; set; } = 1f;
        public ComponentState State { get; private set; } = ComponentState.Idle;
        // Data
        public MaiScore? HistoryScore { get; private set; }
        public BreakShineParam BreakParam
        {
            get
            {
                return new BreakShineParam()
                {
                    Brightness = 0.95f + Math.Max(Mathf.Sin(GetFrame() * 0.20f) * 0.8f, 0),
                    Contrast = 1f + Math.Min(Mathf.Sin(GetFrame() * 0.2f) * -0.15f, 0)
                };
            }
        }



        [SerializeField]
        GameObject _loadingMask;
        [SerializeField]
        GameSetting _setting = MajInstances.Setting;
        [SerializeField]
        GameObject _skipBtn;

        [ReadOnlyField]
        [SerializeField]
        float _thisFrameSec = 0f;
        [ReadOnlyField]
        [SerializeField]
        float _firstNoteAppearTiming = 0f;
        [ReadOnlyField]
        [SerializeField]
        float _audioTime = -114514;
        [ReadOnlyField]
        [SerializeField]
        float _audioTimeNoOffset = -114514;
        [ReadOnlyField]
        [SerializeField]
        float _audioStartTime = -114514;

        long _fileTimeAtStart = 0;

        Image _loadingImage;
        Text _errText;
        TextMeshPro _loadingText;

        SimaiProcess _chart;
        SongDetail _songDetail;

        AudioSampleWrap? _audioSample = null;

        NoteLoader _noteLoader;
        ObjectCounter _objectCounter;

        CancellationTokenSource _allTaskTokenSource = new();
        List<AnwserSoundPoint> _anwserSoundList = new List<AnwserSoundPoint>();
        private void Awake()
        {
            Instance = this;
            MajInstanceHelper<GamePlayManager>.Instance = this;
            //print(MajInstances.GameManager.SelectedIndex);
            _songDetail = SongStorage.WorkingCollection.Current;
            HistoryScore = MajInstances.ScoreManager.GetScore(_songDetail, MajInstances.GameManager.SelectedDiff);
            GetSystemTimePreciseAsFileTime(out _fileTimeAtStart);
        }

        private void OnPauseButton(object sender, InputEventArgs e)
        {
            if (e.IsButton && e.IsClick && e.Type == SensorType.P1)
            {
                print("Pause!!");
                BackToList().Forget();
            }
        }

        void Start()
        {
            _objectCounter = FindObjectOfType<ObjectCounter>();
            State = ComponentState.Loading;
            _loadingText = _loadingMask.transform.GetChild(0).GetComponent<TextMeshPro>();
            _loadingImage = _loadingMask.GetComponent<Image>();
            MajInstances.InputManager.BindAnyArea(OnPauseButton);
            _errText = GameObject.Find("ErrText").GetComponent<Text>();
            DumpOnlineChart().Forget();
        }

        async UniTask DumpOnlineChart()
        {
            if (_songDetail.isOnline)
            {
                LightManager.Instance.SetAllLight(Color.red);
                _loadingText.text = $"{Localization.GetLocalizedText("Downloading")}...";
                var dumpTask = _songDetail.DumpToLocal();
                while (!dumpTask.IsCompleted)
                {
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                }
                _songDetail = dumpTask.Result;
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
                            _loadingText.text = $"{Localization.GetLocalizedText("Failed to load chart")}\n{audioE.Message}";
                            _loadingText.color = Color.red;
                            Debug.LogError(audioE);
                            return;
                        case TaskCanceledException:
                            return;
                        default:
                            State = ComponentState.Failed;
                            _errText.text = "加载note时出错了哟\n" + e.Message;
                            Debug.LogError(e);
                            return;
                    }
                }
            }

            PrepareToPlay().Forget();
        }
        async UniTask LoadAudioTrack()
        {
            var trackPath = _songDetail.TrackPath ?? string.Empty;
            if(!File.Exists(trackPath))
                throw new InvalidAudioTrackException("Audio track not found", trackPath);
            _audioSample = await MajInstances.AudioManager.LoadMusicAsync(trackPath);
            await UniTask.Yield();
            if (_audioSample is null)
                throw new InvalidAudioTrackException("Failed to decode audio track", trackPath);
            _audioSample.SetVolume(_setting.Audio.Volume.BGM);
            LightManager.Instance.SetAllLight(Color.white);
        }
        async UniTask LoadChart()
        {
            var maidata = _songDetail.LoadInnerMaidata((int)MajInstances.GameManager.SelectedDiff);
            _loadingText.text = $"{Localization.GetLocalizedText("Deserialization")}...";
            if (string.IsNullOrEmpty(maidata))
            {
                BackToList();
                throw new TaskCanceledException("Empty chart");
            }
            _chart = new SimaiProcess(maidata);
            if (_chart.notelist.Count == 0)
            {
                BackToList();
                throw new TaskCanceledException("Empty chart");
            }

            await Task.Run(() =>
            {
                //Generate ClockSounds
                var countnum = _songDetail.ClockCount == null ? 4 : _songDetail.ClockCount;
                var firstBpm = _chart.notelist.FirstOrDefault().currentBpm;
                var interval = 60 / firstBpm;
                if (_chart.notelist.Any(o => o.time < countnum * interval))
                {
                    //if there is something in first measure, we add clock before the bgm
                    for (int i = 0; i < countnum; i++)
                    {
                        _anwserSoundList.Add(new AnwserSoundPoint()
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
                        _anwserSoundList.Add(new AnwserSoundPoint()
                        {
                            time = i * interval,
                            isClock = true,
                            isPlayed = false
                        });
                    }
                }


                //Generate AnwserSounds
                foreach (var timingPoint in _chart.notelist)
                {
                    if (timingPoint.noteList.All(o => o.isSlideNoHead)) continue;

                    _anwserSoundList.Add(new AnwserSoundPoint()
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
                        if (!_chart.notelist.Any(o => Math.Abs(o.time - newtime) < 0.001) &&
                            !_anwserSoundList.Any(o => Math.Abs(o.time - newtime) < 0.001)
                            )
                            _anwserSoundList.Add(new AnwserSoundPoint()
                            {
                                time = newtime,
                                isClock = false,
                                isPlayed = false
                            });
                    }
                }
                _anwserSoundList = _anwserSoundList.OrderBy(o => o.time).ToList();
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
            if (!string.IsNullOrEmpty(_songDetail.VideoPath))
                BGManager.SetBackgroundMovie(_songDetail.VideoPath);
            else
            {
                var task = _songDetail.GetSpriteAsync();
                while (!task.IsCompleted)
                {
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                }
                BGManager.SetBackgroundPic(task.Result);
            }


            BGManager.SetBackgroundDim(_setting.Game.BackgroundDim);
        }
        /// <summary>
        /// 初始化NoteLoader与实例化Note对象
        /// </summary>
        /// <returns></returns>
        async UniTask LoadNotes()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            _noteLoader = GameObject.Find("NoteLoader").GetComponent<NoteLoader>();
            _noteLoader.noteSpeed = (float)(107.25 / (71.4184491 * Mathf.Pow(_setting.Game.TapSpeed + 0.9975f, -0.985558604f)));
            _noteLoader.touchSpeed = _setting.Game.TouchSpeed;

            //var loaderTask = noteLoader.LoadNotes(Chart);
            var loaderTask = _noteLoader.LoadNotesIntoPool(_chart);
            var timer = 1f;

            while (_noteLoader.State < NoteLoaderStatus.Finished)
            {
                if (_noteLoader.State == NoteLoaderStatus.Error)
                {
                    var e = loaderTask.AsTask().Exception;
                    _errText.text = "加载note时出错了哟\n" + e.Message;
                    _loadingText.text = $"{Localization.GetLocalizedText("Failed to load chart")}\n{e.Message}%";
                    Debug.LogError(e);
                    StopAllCoroutines();
                    throw e;
                }
                _loadingText.text = $"{Localization.GetLocalizedText("Loading Chart")}...\n{_noteLoader.Process * 100:F2}%";
                await UniTask.Yield();
            }
            _loadingText.text = $"{Localization.GetLocalizedText("Loading Chart")}...\n100.00%";

            while (timer > 0)
            {
                await UniTask.Yield();
                timer -= Time.deltaTime;
                var textColor = Color.white;
                var maskColor = Color.black;
                textColor.a = timer / 1f;
                maskColor.a = timer / 1f * 0.75f;
                _loadingImage.color = maskColor;
                _loadingText.color = textColor;
            }

            _loadingMask.SetActive(false);
            _loadingText.gameObject.SetActive(false);
        }
        async UniTaskVoid PrepareToPlay()
        {
            if (_audioSample is null)
                return;
            _audioTime = -5f;

            await InitBackground();
            var noteLoaderTask = LoadNotes().AsTask();

            while (!noteLoaderTask.IsCompleted)
            {
                if (noteLoaderTask.IsFaulted)
                    throw noteLoaderTask.Exception;
                await UniTask.Yield();
            }

            MajInstances.GameManager.DisableGC();
            Time.timeScale = 1f;
            var firstClockTiming = _anwserSoundList[0].time;
            float extraTime = 5f;
            if (firstClockTiming < -5f)
                extraTime += (-(float)firstClockTiming - 5f) + 2f;
            if (FirstNoteAppearTiming != 0)
                extraTime += -(FirstNoteAppearTiming + 4f);
            _audioStartTime = TimeSource + (float)_audioSample.CurrentSec + extraTime;
            StartToPlayAnswer();
            _audioSample.Play();
            _audioSample.Pause();

            State = ComponentState.Running;

            while (TimeSource - AudioStartTime < 0)
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            _audioSample.Play();
            _audioStartTime = TimeSource;

        }

        void OnDestroy()
        {
            print("GPManagerDestroy");
            DisposeAudioTrack();
            _audioSample = null;
            State = ComponentState.Finished;
            _allTaskTokenSource.Cancel();
            MajInstances.GameManager.EnableGC();
        }
        // Update is called once per frame
        void Update()
        {
            UpdateAudioTime();
            if (_audioSample is null)
                return;
            else if (!_objectCounter.AllFinished)
                return;
            else if (State != ComponentState.Running)
                return;

            var remainingTime = AudioTime - _audioSample.Length.TotalSeconds;
            if(remainingTime < -6)
                _skipBtn.SetActive(true);
            else if(remainingTime >= 0)
            {
                _skipBtn.SetActive(false);
                EndGame().Forget();
            }
        }
        void FixedUpdate()
        {
            _thisFrameSec = _audioTime;
        }
        void UpdateAudioTime()
        {
            if (_audioSample is null)
                return;
            else if (State != ComponentState.Running)
                return;
            else if (AudioStartTime == -114514f)
                return;
            //Do not use this!!!! This have connection with sample batch size
            //AudioTime = (float)audioSample.GetCurrentTime();
            var chartOffset = (float)_songDetail.First + _setting.Judge.AudioOffset;
            _audioTime = TimeSource - AudioStartTime - chartOffset;
            _audioTimeNoOffset = TimeSource - AudioStartTime;

            var realTimeDifference = (float)_audioSample.CurrentSec - (TimeSource - AudioStartTime);
            if (!_audioSample.IsPlaying)
                return;
            if (Math.Abs(realTimeDifference) > 0.04f && AudioTime > 0)
            {
                _errText.text = "音频错位了哟\n" + realTimeDifference;
            }
            else if (Math.Abs(realTimeDifference) > 0.02f && AudioTime > 0 && MajInstances.Setting.Debug.TryFixAudioSync)
            {
                _errText.text = "修正音频哟\n" + realTimeDifference;
                _audioStartTime -= realTimeDifference * 0.8f;
            }
        }
        async void StartToPlayAnswer()
        {
            int i = 0;
            await Task.Run(() =>
            {
                while (!_allTaskTokenSource.IsCancellationRequested)
                {
                    if (i >= _anwserSoundList.Count)
                        return;

                    var noteToPlay = _anwserSoundList[i].time;
                    var delta = AudioTime - noteToPlay;

                    if (delta > 0)
                    {
                        if (_anwserSoundList[i].isClock)
                        {
                            MajInstances.AudioManager.PlaySFX(SFXSampleType.CLOCK);
                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            {
                                XxlbAnimationController.instance.Stepping();
                            });
                        }
                        else
                            MajInstances.AudioManager.PlaySFX(SFXSampleType.ANSWER);
                        _anwserSoundList[i].isPlayed = true;
                        i++;
                    }
                    //await Task.Delay(1);
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
            if (_audioSample is not null)
            {
                _audioSample.Pause();
                _audioSample.Dispose();
                _audioSample = null;
            }
        }
        async UniTaskVoid BackToList()
        {
            MajInstances.InputManager.UnbindAnyArea(OnPauseButton);
            MajInstances.GameManager.EnableGC();
            StopAllCoroutines();
            DisposeAudioTrack();

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            MajInstances.SceneSwitcher.SwitchScene("List");

        }
        public async UniTaskVoid EndGame()
        {
            var acc = _objectCounter.CalculateFinalResult();
            print("GameResult: " + acc);
            GameManager.LastGameResult = _objectCounter.GetPlayRecord(_songDetail, MajInstances.GameManager.SelectedDiff);
            MajInstances.GameManager.EnableGC();
            BGManager.Instance.CancelTimeRef();
            State = ComponentState.Finished;
            DisposeAudioTrack();

            MajInstances.InputManager.UnbindAnyArea(OnPauseButton);
            await UniTask.DelayFrame(5);
            MajInstances.SceneSwitcher.SwitchSceneAfterTaskAsync("Result", UniTask.Delay(1000)).Forget();
        }
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long filetime);
        class AnwserSoundPoint
        {
            public double time;
            public bool isClock;
            public bool isPlayed;
        }
    }
}