using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using MajdataPlay.Extensions;
using MajdataPlay.Types.Attribute;
using MajSimaiDecode;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace MajdataPlay.Game
{
#nullable enable
    public class GamePlayManager : MonoBehaviour
    {
        public float NoteSpeed { get; private set; } = 7f;
        public float TouchSpeed { get; private set; } = 7f;
        public bool IsClassicMode => _setting.Judge.Mode == JudgeMode.Classic;
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
        /// The timing of the current FixedUpdate<para>Unit: Second</para>
        /// </summary>
        public float ThisFrameSec => _thisFrameSec;
        /// <summary>
        ///  The first Note appear timing
        /// </summary>
        public float FirstNoteAppearTiming
        {
            get => _firstNoteAppearTiming;
            set => _firstNoteAppearTiming = value;
        }
        /// <summary>
        /// Current audio playback time
        /// </summary>
        public float AudioTime => _audioTime;
        /// <summary>
        /// Current audio playback time without offset correction
        /// </summary>
        public float AudioTimeNoOffset => _audioTimeNoOffset;
        /// <summary>
        /// The timing of audio starting to play
        /// </summary>
        public float AudioStartTime => _audioStartTime;
        // Control
        public bool IsStart => _audioSample?.IsPlaying ?? false;
        public bool IsAutoplay { get; private set; } = false;
        public float AutoplayParam { get; private set; } = 7;
        public JudgeStyleType JudgeStyle { get; private set; } = JudgeStyleType.DEFAULT;
        public float PlaybackSpeed 
        {
            get => _playbackSpeed;
            private set => _playbackSpeed = value;
        }
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

        public GameObject AllPerfectAnimation;
        public GameObject FullComboAnimation;

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
        float _playbackSpeed = 1f;
        int _chartRotation = 0;
        bool _isAllBreak = false;
        bool _isAllEx = false;
        bool _isAllTouch = false;
        long _fileTimeAtStart = 0;

        Text _errText;

        HttpTransporter _httpDownloader = new();
        SimaiProcess _chart;
        SongDetail _songDetail;

        AudioSampleWrap? _audioSample = null;

        BGManager _bgManager;
        NoteLoader _noteLoader;
        ObjectCounter _objectCounter;
        XxlbAnimationController _xxlbController;

        CancellationTokenSource _allTaskTokenSource = new();
        List<AnwserSoundPoint> _anwserSoundList = new List<AnwserSoundPoint>();
        void Awake()
        {
            MajInstanceHelper<GamePlayManager>.Instance = this;
            //print(MajInstances.GameManager.SelectedIndex);
            _songDetail = SongStorage.WorkingCollection.Current;
            HistoryScore = MajInstances.ScoreManager.GetScore(_songDetail, MajInstances.GameManager.SelectedDiff);
            GetSystemTimePreciseAsFileTime(out _fileTimeAtStart);
        }
        void OnPauseButton(object sender, InputEventArgs e)
        {
            if (e.IsButton && e.IsClick && e.Type == SensorType.P1)
            {
                print("Pause!!");
                BackToList().Forget();
            }
        }
        void Start()
        {
            State = ComponentState.Loading;

            _bgManager = MajInstanceHelper<BGManager>.Instance!;
            _objectCounter = MajInstanceHelper<ObjectCounter>.Instance!;
            _xxlbController = MajInstanceHelper<XxlbAnimationController>.Instance!;

            _errText = GameObject.Find("ErrText").GetComponent<Text>();
            _chartRotation = _setting.Game.Rotation.Clamp(-7, 7);
            MajInstances.InputManager.BindAnyArea(OnPauseButton);
            LoadGameMod();
            LoadChart().Forget();
        }
        void LoadGameMod()
        {
            GameModHelper.Refresh();
            var mod = GameModHelper.GetGameMod(Types.Mods.ModType.TIME_SCALE);
            var mod2 = GameModHelper.GetGameMod(Types.Mods.ModType.ALL_BREAK);
            var mod3 = GameModHelper.GetGameMod(Types.Mods.ModType.ALL_EX);
            var mod4 = GameModHelper.GetGameMod(Types.Mods.ModType.ALL_TOUCH);
            var mod5 = GameModHelper.GetGameMod(Types.Mods.ModType.AUTOPLAY);
            var mod6 = GameModHelper.GetGameMod(Types.Mods.ModType.JUDGE_STYLE);

            if (mod is not null && mod.Active && mod.Value > 0)
                PlaybackSpeed = mod.Value ?? 0;
            if (mod2 is not null)
                _isAllBreak = mod2.Active;
            if (mod3 is not null)
                _isAllEx = mod3.Active;
            if (mod4 is not null)
                _isAllTouch = mod4.Active;
            if (mod5 is not null)
            {
                IsAutoplay = mod5.Active;
                AutoplayParam = mod5.Value ?? 7;
            }
            if (mod6 is not null && mod6.Active)
                JudgeStyle = (JudgeStyleType)(mod6.Value ?? 0);
        }
        /// <summary>
        /// Parse the chart and load it into memory, or dump it locally if the chart is online
        /// </summary>
        /// <returns></returns>
        async UniTaskVoid LoadChart()
        {
            try
            {
                if (_songDetail.isOnline)
                    await DumpOnlineChart();
                await UniTask.WhenAll(LoadAudioTrack(), ParseChart());
            }
            catch(HttpTransmitException httpEx)
            {
                State = ComponentState.Failed;
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Failed to download chart")}", Color.red);
                Debug.LogError(httpEx);
                return;
            }
            catch(InvalidAudioTrackException audioEx)
            {
                State = ComponentState.Failed;
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Failed to load chart")}\n{audioEx.Message}", Color.red);
                Debug.LogError(audioEx);
                return;
            }
            catch(OperationCanceledException canceledEx)
            {
                Debug.LogWarning(canceledEx);
                return;
            }
            catch(Exception e)
            {
                State = ComponentState.Failed;
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Unknown error")}\n{e.Message}", Color.red);
                Debug.LogError(e);
                return;
            }

            PrepareToPlay().Forget();
        }
        /// <summary>
        /// Dump online chart to local
        /// </summary>
        /// <returns></returns>
        async UniTask DumpOnlineChart()
        {
            var chartFolder = Path.Combine(GameManager.ChartPath, $"MajnetPlayed/{_songDetail.Hash}");
            Directory.CreateDirectory(chartFolder);
            var dirInfo = new DirectoryInfo(chartFolder);
            var trackPath = Path.Combine(chartFolder, "track.mp3");
            var chartPath = Path.Combine(chartFolder, "maidata.txt");
            var bgPath = Path.Combine(chartFolder, "bg.png");
            var videoPath = Path.Combine(chartFolder, "bg.mp4");
            var trackUri = _songDetail.TrackPath;
            var chartUri = _songDetail.MaidataPath;
            var bgUri = _songDetail.BGPath;
            var videoUri = _songDetail.VideoPath;

            if (trackUri is null or "")
                throw new AudioTrackNotFoundException(trackPath);
            if (chartUri is null or "")
                throw new ChartNotFoundException(_songDetail);
            
            MajInstances.LightManager.SetAllLight(Color.blue);
            MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading")}...");
            if (!File.Exists(trackPath))
            {
                await DownloadFile(trackUri, trackPath, r =>
                {
                    MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Audio Track")}...");
                });
            }
            if (!File.Exists(chartPath))
            {
                await DownloadFile(chartUri, chartPath, r =>
                {
                    MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Maidata")}...");
                });
            }
            SongDetail song;
            if (bgUri is null or "")
            {
                song = await SongDetail.ParseAsync(dirInfo.GetFiles());
                song.Hash = _songDetail.Hash;
                _songDetail = song;
                return; 
            }
            if (!File.Exists(bgPath))
            {
                await DownloadFile(bgUri, bgPath, r =>
                {
                    MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Picture")}...");
                });
            }
            if (!File.Exists(videoPath) && videoUri is not null)
            {
                try
                {
                    await DownloadFile(videoUri, videoPath, r =>
                    {
                        MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Video")}...");
                    });
                }
                catch
                {
                    Debug.Log("No video for this song");
                    File.Delete(videoPath);
                    videoPath = "";
                }
            }
            song = await SongDetail.ParseAsync(dirInfo.GetFiles());
            song.Hash = _songDetail.Hash;
            song.OnlineId = _songDetail.OnlineId;
            song.ApiEndpoint = _songDetail.ApiEndpoint;
            _songDetail = song;
        }
        /*async UniTask<GetResult> DownloadFile(string uri,string savePath,Action<IHttpProgressReporter> onProgressChanged,int buffersize = 128*1024)
        {
            var dlInfo = GetRequest.Create(uri, savePath);
            var reporter = dlInfo.ProgressReporter;
            var task = _httpDownloader.GetAsync(dlInfo,buffersize);

            while(!task.IsCompleted)
            {
                onProgressChanged(reporter!);
                await UniTask.Yield();
            }
            onProgressChanged(reporter!);
            await UniTask.Yield();
            return task.Result;
        }*/
        /*async UniTask DownloadString(string uri, string savePath)
        {
            var task = HttpTransporter.ShareClient.GetStringAsync(uri);

            while (!task.IsCompleted)
            {
                await UniTask.Yield();
            }
            File.WriteAllText(savePath, task.Result);
            return;
        }*/
        /*async UniTask DownloadFile(string uri, string savePath, Action<float> progressCallback)
        {
            UnityWebRequest trackreq = UnityWebRequest.Get(uri);
            trackreq.downloadHandler = new DownloadHandlerFile(savePath);
            var result = trackreq.SendWebRequest();
            while (!result.isDone)
            {
                progressCallback.Invoke(trackreq.downloadProgress);
                await UniTask.Yield();
            }
            if (trackreq.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error downloading file: " + trackreq.error);
                throw new Exception("Download file failed");
            }
        }*/
        async UniTask DownloadFile(string uri, string savePath, Action<float> progressCallback)
        {
            var task = HttpTransporter.ShareClient.GetByteArrayAsync(uri);
            float fakeprogress = 0f;
            while (!task.IsCompleted)
            {
                progressCallback.Invoke(fakeprogress);
                fakeprogress += 0.001f;
                if(fakeprogress >0.99f) fakeprogress = 0.99f;
                await UniTask.Yield();
            }
            if (task.IsCanceled)
            {
                throw new Exception("Download failed");
            }
            File.WriteAllBytes(savePath, task.Result);
            return;
        }
        async UniTask LoadAudioTrack()
        {
            var trackPath = _songDetail.TrackPath ?? string.Empty;
            if(!File.Exists(trackPath))
                throw new AudioTrackNotFoundException(trackPath);
            _audioSample = await MajInstances.AudioManager.LoadMusicAsync(trackPath,true);
            await UniTask.Yield();
            if (_audioSample is null)
                throw new InvalidAudioTrackException("Failed to decode audio track", trackPath);
            _audioSample.SetVolume(_setting.Audio.Volume.BGM);
            _audioSample.Speed = PlaybackSpeed;
            MajInstances.LightManager.SetAllLight(Color.white);
        }
        /// <summary>
        /// Parse the chart into memory
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TaskCanceledException"></exception>
        async UniTask ParseChart()
        {
            var maidata = _songDetail.LoadInnerMaidata((int)MajInstances.GameManager.SelectedDiff);
            MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Deserialization")}...");
            if (string.IsNullOrEmpty(maidata))
            {
                BackToList().Forget();
                throw new TaskCanceledException("Empty chart");
            }
            ChartMirror(ref maidata);
            _chart = new SimaiProcess(maidata);
            if (_chart.notelist.Count == 0)
            {
                BackToList().Forget();
                throw new TaskCanceledException("Empty chart");
            }
            if(PlaybackSpeed != 1)
                _chart.Scale(PlaybackSpeed);
            if(_isAllBreak)
                _chart.ConvertToBreak();
            if (_isAllEx)
                _chart.ConvertToEx();
            if(_isAllTouch)
                _chart.ConvertToTouch();
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
        /// Load the background picture and set brightness
        /// </summary>
        /// <returns></returns>
        async UniTask InitBackground()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            var BGManager = GameObject.Find("Background").GetComponent<BGManager>();
            if (!string.IsNullOrEmpty(_songDetail.VideoPath))
                BGManager.SetBackgroundMovie(_songDetail.VideoPath,PlaybackSpeed);
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
        /// Parse and load notes into NotePool
        /// </summary>
        /// <returns></returns>
        async UniTask LoadNotes()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            var tapSpeed = Math.Abs(_setting.Game.TapSpeed);

            _noteLoader = GameObject.Find("NoteLoader").GetComponent<NoteLoader>();
            if(_setting.Game.TapSpeed < 0)
                _noteLoader.NoteSpeed = -((float)(107.25 / (71.4184491 * Mathf.Pow(tapSpeed + 0.9975f, -0.985558604f))));
            else
                _noteLoader.NoteSpeed = ((float)(107.25 / (71.4184491 * Mathf.Pow(tapSpeed + 0.9975f, -0.985558604f))));
            _noteLoader.TouchSpeed = _setting.Game.TouchSpeed;
            _noteLoader.ChartRotation = _chartRotation;

            //var loaderTask = noteLoader.LoadNotes(Chart);
            var loaderTask = _noteLoader.LoadNotesIntoPool(_chart);
            var timer = 1f;

            while (_noteLoader.State < NoteLoaderStatus.Finished)
            {
                if (_noteLoader.State == NoteLoaderStatus.Error)
                {
                    var e = loaderTask.AsTask().Exception;
                    MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Failed to load chart")}\n{e.Message}%",Color.red);
                    Debug.LogError(e);
                    StopAllCoroutines();
                    throw e;
                }
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Loading Chart")}...\n{_noteLoader.Process * 100:F2}%");
                await UniTask.Yield();
            }
            MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Loading Chart")}...\n100.00%");

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

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            MajInstances.SceneSwitcher.FadeOut();

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

            State = ComponentState.Running;

            while (TimeSource - AudioStartTime < 0)
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            _audioSample.Play();
            _audioSample.CurrentSec = 0;
            _audioStartTime = TimeSource;
            Debug.Log($"Chart playback speed: {PlaybackSpeed}x");
        }

        void OnDestroy()
        {
            print("GPManagerDestroy");
            DisposeAudioTrack();
            _audioSample = null;
            State = ComponentState.Finished;
            _allTaskTokenSource.Cancel();
            MajInstances.GameManager.EnableGC();
            MajInstanceHelper<GamePlayManager>.Free();
        }
        void Update()
        {
            UpdateAudioTime();
            if (_audioSample is null)
                return;

            else if (!_objectCounter.AllFinished)
                return;

            if (State == ComponentState.Running)
            {
                State = ComponentState.Calculate;

                var acc = _objectCounter.CalculateFinalResult();
                print("GameResult: " + acc);
                GameManager.LastGameResult = _objectCounter.GetPlayRecord(_songDetail, MajInstances.GameManager.SelectedDiff);
                var judgerecord = ((GameResult)GameManager.LastGameResult).JudgeRecord;
                var unpackedinfo = JudgeDetail.UnpackJudgeRecord(judgerecord.TotalJudgeInfo);
                var breakunpackedinfo = JudgeDetail.UnpackJudgeRecord(judgerecord[ScoreNoteType.Break]);
                if (unpackedinfo.IsAllPerfect && breakunpackedinfo.IsTheoretical)
                {
                    //AP+
                    AllPerfectAnimation.SetActive(true);
                    MajInstances.AudioManager.PlaySFX("all_perfect_plus.wav");
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    EndGame(5000).Forget();
                    return;
                }
                else if (unpackedinfo.IsAllPerfect)
                {
                    //AP
                    AllPerfectAnimation.SetActive(true);
                    MajInstances.AudioManager.PlaySFX("all_perfect.wav");
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    EndGame(5000).Forget();
                    return;
                }
                else if (unpackedinfo.IsFullComboPlus)
                {
                    //FC+
                    FullComboAnimation.SetActive(true);
                    MajInstances.AudioManager.PlaySFX("full_combo_plus.wav");
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    EndGame(5000).Forget();
                    return;
                }
                else if (unpackedinfo.IsFullCombo)
                {
                    //FC
                    FullComboAnimation.SetActive(true);
                    MajInstances.AudioManager.PlaySFX("full_combo.wav");
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    EndGame(5000).Forget();
                    return;
                }
            }
            if (State == ComponentState.Calculate)
            {
                var remainingTime = AudioTime - (_audioSample.Length.TotalSeconds / PlaybackSpeed);
                if (remainingTime < -6)
                    _skipBtn.SetActive(true);
                else if (remainingTime >= 0)
                {
                    _skipBtn.SetActive(false);
                    EndGame(2000).Forget();
                }
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
            else if (AudioStartTime == -114514f)
                return;
            if (State == ComponentState.Running || State == ComponentState.Calculate)
            {
                //Do not use this!!!! This have connection with sample batch size
                //AudioTime = (float)audioSample.GetCurrentTime();
                var chartOffset = ((float)_songDetail.First + _setting.Judge.AudioOffset) / PlaybackSpeed;
                var timeOffset = TimeSource - AudioStartTime;
                _audioTime = timeOffset - chartOffset;
                _audioTimeNoOffset = timeOffset;

                var realTimeDifference = (float)_audioSample.CurrentSec - (TimeSource - AudioStartTime);
                if (Math.Abs(realTimeDifference) > 0.02f && AudioTime > 0 && MajInstances.Setting.Debug.TryFixAudioSync)
                {
                    _errText.text = "修正音频哟\n" + realTimeDifference;
                    _audioStartTime -= realTimeDifference * 0.8f;
                }
            }
        }
        async void StartToPlayAnswer()
        {
            int i = 0;
            await Task.Run(() =>
            {
                var isUnityFMOD = MajInstances.Setting.Audio.Backend == SoundBackendType.Unity;
                while (!_allTaskTokenSource.IsCancellationRequested)
                {
                    
                    try
                    {
                        if (i >= _anwserSoundList.Count)
                            return;

                        var noteToPlay = _anwserSoundList[i].time;
                        var delta = AudioTime - noteToPlay;

                        if (delta > 0)
                        {
                            if (_anwserSoundList[i].isClock)
                            {
#if UNITY_EDITOR
                                if (isUnityFMOD)
                                    GameManager.ExecutionQueue.Enqueue(() => MajInstances.AudioManager.PlaySFX("answer_clock.wav"));
                                else
                                    MajInstances.AudioManager.PlaySFX("answer_clock.wav");
#else
                                MajInstances.AudioManager.PlaySFX("answer_clock.wav");
#endif
                                GameManager.ExecutionQueue.Enqueue(() => _xxlbController.Stepping());
                            }
                            else
                            {
#if UNITY_EDITOR
                                if (isUnityFMOD)
                                    GameManager.ExecutionQueue.Enqueue(() => MajInstances.AudioManager.PlaySFX("answer.wav"));
                                else
                                    MajInstances.AudioManager.PlaySFX("answer.wav");
#else
                                MajInstances.AudioManager.PlaySFX("answer.wav");
#endif
                            }
                            _anwserSoundList[i].isPlayed = true;
                            i++;
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogException(e);
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
        public async UniTaskVoid EndGame(int delayMiliseconds = 100)
        {
            State = ComponentState.Finished;
            MajInstances.InputManager.ClearAllSubscriber();
            _bgManager.CancelTimeRef();

            await UniTask.Delay(delayMiliseconds);

            MajInstances.GameManager.EnableGC();
            
            DisposeAudioTrack();

            MajInstances.InputManager.UnbindAnyArea(OnPauseButton);
            await UniTask.DelayFrame(5);
            MajInstances.SceneSwitcher.SwitchScene("Result");
        }
        void ChartMirror(ref string chartContent)
        {
            var mirrorType = _setting.Game.Mirror;
            if (mirrorType is MirrorType.Off)
                return;
            chartContent = SimaiMirror.NoteMirrorHandle(chartContent, mirrorType);
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