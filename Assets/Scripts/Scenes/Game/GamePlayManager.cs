using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using MajdataPlay.Extensions;
using MajdataPlay.Attributes;
using MajSimai;
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
using MajdataPlay.Timer;
using MajdataPlay.Collections;
using MajdataPlay.Game.Types;
using Unity.VisualScripting.Antlr3.Runtime;
using Cysharp.Text;
using Unity.VisualScripting;
using MajdataPlay.List;

namespace MajdataPlay.Game
{
#nullable enable
    public class GamePlayManager : MonoBehaviour, INoteController
    {
        public float NoteSpeed { get; private set; } = 7f;
        public float TouchSpeed { get; private set; } = 7f;
        public bool IsClassicMode => _setting.Judge.Mode == JudgeMode.Classic;
        // Timeline
        /// <summary>
        /// The timing of the current Update<para>Unit: Second</para>
        /// </summary>
        public float ThisFrameSec => _thisFrameSec;
        /// <summary>
        /// The timing of the current FixedUpdate<para>Unit: Second</para>
        /// </summary>
        public float ThisFixedUpdateSec => _thisFixedUpdateSec;
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
        /// Current audio Total length
        /// </summary>
        public float AudioLength { get; private set; } = 0f;
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
        public bool IsAutoplay => AutoplayMode != AutoplayMode.Disable;
        public AutoplayMode AutoplayMode { get; private set; } = AutoplayMode.Disable;
        public JudgeGrade AutoplayGrade { get; private set; } =  JudgeGrade.Perfect;
        public JudgeStyleType JudgeStyle { get; private set; } = JudgeStyleType.DEFAULT;
        public float PlaybackSpeed 
        {
            get => _playbackSpeed;
            private set => _playbackSpeed = value;
        }
        public GamePlayStatus State { get; private set; } = GamePlayStatus.Start;
        // Data
        public bool IsPracticeMode => _gameInfo.IsPracticeMode;
        internal GameMode Mode => _gameInfo.Mode;
        public MaiScore? HistoryScore { get; private set; }
        public Material BreakMaterial { get; } = MajEnv.BreakMaterial;
        public Material DefaultMaterial { get; } = MajEnv.DefaultMaterial;
        public Material HoldShineMaterial { get; } = MajEnv.HoldShineMaterial;

        public GameObject AllPerfectAnimation;
        public GameObject FullComboAnimation;


        [SerializeField]
        Sprite _maskSpriteA;
        [SerializeField]
        Sprite _maskSpriteB;
        [SerializeField]
        Animator _bgInfoHeaderAnim;
        [SerializeField]
        GameSetting _setting = MajInstances.Setting;
        [SerializeField]
        GameObject _skipBtn;
        [SerializeField]
        SpriteMask _noteMask;
        [ReadOnlyField]
        [SerializeField]
        float _thisFrameSec = 0f;
        [ReadOnlyField]
        [SerializeField]
        float _thisFixedUpdateSec = 0f;
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
        bool _isTrackSkipAvailable = MajEnv.UserSetting.Game.TrackSkip;
        bool _isFastRetryAvailable = MajEnv.UserSetting.Game.FastRetry;
        float? _allNotesFinishedTiming = null;
        float _2367PressTime = 0;
        float _3456PressTime = 0;
        long _fileTimeAtStart = 0;

        readonly SceneSwitcher _sceneSwitcher = MajInstances.SceneSwitcher;

        Text _errText;
        MajTimer _timer = MajTimeline.CreateTimer();
        float _audioTrackStartAt = 0f;

        GameInfo _gameInfo = MajInstanceHelper<GameInfo>.Instance!;
        InputManager _ioManager = MajInstances.InputManager;

        SimaiFile _simaiFile;
        SimaiChart _chart;
        ISongDetail _songDetail;

        AudioSampleWrap? _audioSample = null;

        BGManager _bgManager;
        NoteLoader _noteLoader;
        NoteManager _noteManager;
        ObjectCounter _objectCounter;
        XxlbAnimationController _xxlbController;

        List<AnwserSoundPoint> _anwserSoundList = new List<AnwserSoundPoint>();
        readonly CancellationTokenSource _cts = new();
        void Awake()
        {
            MajInstanceHelper<GamePlayManager>.Instance = this;
            MajInstanceHelper<INoteController>.Instance = this;
            if (_gameInfo is null || _gameInfo.Current is null)
                throw new ArgumentNullException(nameof(_gameInfo));
            //print(MajInstances.GameManager.SelectedIndex);
            _songDetail = _gameInfo.Current;
            HistoryScore = MajInstances.ScoreManager.GetScore(_songDetail, MajInstances.GameManager.SelectedDiff);
            _timer = MajTimeline.CreateTimer();
#if !UNITY_EDITOR
            Cursor.visible = false;
#endif
        }
        void OnPauseButton(object sender, InputEventArgs e)
        {
            if (e.IsButton && e.IsDown && e.Type == SensorArea.P1)
            {
                print("Pause!!");
                BackToList().Forget();
            }
        }
        void Start()
        {
            _noteManager = MajInstanceHelper<NoteManager>.Instance!;
            _bgManager = MajInstanceHelper<BGManager>.Instance!;
            _objectCounter = MajInstanceHelper<ObjectCounter>.Instance!;
            _xxlbController = MajInstanceHelper<XxlbAnimationController>.Instance!;

            _errText = GameObject.Find("ErrText").GetComponent<Text>();
            _chartRotation = _setting.Game.Rotation.Clamp(-7, 7);
            MajInstances.InputManager.BindAnyArea(OnPauseButton);
            LoadGameMod();
            InitGame().Forget();
        }
        void LoadGameMod()
        {
            var modsetting = MajInstances.GameManager.Setting.Mod;
            PlaybackSpeed = modsetting.PlaybackSpeed;
            _isAllBreak = modsetting.AllBreak;
            _isAllEx = modsetting.AllEx;
            _isAllTouch = modsetting.AllTouch;
            AutoplayMode = modsetting.AutoPlay;
            //AutoplayParam = mod5.Value ?? 7;
            JudgeStyle = modsetting.JudgeStyle;
            switch(modsetting.NoteMask)
            {
                case "Inner":
                    _noteMask.gameObject.SetActive(true);
                    _noteMask.sprite = _maskSpriteB;
                    break;
                case "Outer":
                    _noteMask.gameObject.SetActive(true);
                    _noteMask.sprite = _maskSpriteA;
                    break;
                case "Disable":
                    _noteMask.gameObject.SetActive(false); 
                    break;
            }
        }
        /// <summary>
        /// Parse the chart and load it into memory, or dump it locally if the chart is online
        /// </summary>
        /// <returns></returns>
        async UniTaskVoid InitGame()
        {
            var inputManager = MajInstances.InputManager;
            State = GamePlayStatus.Loading;
            try
            {
                if(_songDetail.IsOnline)
                {
                    MajInstances.LightManager.SetAllLight(Color.blue);
                    _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading")}...");
                    _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Audio Track")}...");
                    var task1 = _songDetail.GetAudioTrackAsync().AsValueTask();
                    while(!task1.IsCompleted)
                        await UniTask.Yield();
                    _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Maidata")}...");
                    var task2 = _songDetail.GetMaidataAsync().AsValueTask();
                    while (!task2.IsCompleted)
                        await UniTask.Yield();
                    _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Picture")}...");
                    var task3 = _songDetail.GetCoverAsync(false).AsValueTask();
                    while (!task3.IsCompleted)
                        await UniTask.Yield();
                    _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Video")}...");
                    var task4 = _songDetail.GetVideoPathAsync().AsValueTask();
                    while (!task4.IsCompleted)
                        await UniTask.Yield();
                    _sceneSwitcher.SetLoadingText(string.Empty);
                }
                await LoadAudioTrack();
                await InitBackground();
                await ParseChart();
                await LoadNotes();
                await PrepareToPlay();
            }
            catch(EmptyChartException)
            {
                inputManager.ClearAllSubscriber();
                var s = Localization.GetLocalizedText("Empty Chart");
                var ss = string.Format(Localization.GetLocalizedText("Return to {0} in {1} seconds"), "List", "5");
                MajInstances.SceneSwitcher.SetLoadingText($"{s}, {ss}", Color.red);
                var waitTime = 0f;
                do
                {
                    await UniTask.Yield();
                    waitTime += Time.deltaTime;
                    if(MajInstances.InputManager.CheckButtonStatus(SensorArea.P1, SensorStatus.On))
                    {
                        break;
                    }

                } while (waitTime <= 5);
                
                BackToList().Forget();
            }
            catch(HttpTransmitException httpEx)
            {
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Failed to download chart")}", Color.red);
                MajDebug.LogError(httpEx);
                return;
            }
            catch(InvalidAudioTrackException audioEx)
            {
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Failed to load chart")}\n{audioEx.Message}", Color.red);
                MajDebug.LogError(audioEx);
                return;
            }
            catch(TaskCanceledException e)
            {
                MajDebug.LogWarning(e);
                return;
            }
            catch(OperationCanceledException canceledEx)
            {
                MajDebug.LogWarning(canceledEx);
                return;
            }
            catch(Exception e)
            {
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Unknown error")}\n{e.Message}", Color.red);
                MajDebug.LogError(e);
                throw;
            }
        }
        
        async UniTask LoadAudioTrack()
        {
            var audioSample = await _songDetail.GetAudioTrackAsync();
            if(audioSample is null || audioSample.IsEmpty)
                throw new InvalidAudioTrackException("Failed to decode audio track", string.Empty);
            _audioSample = audioSample;
            _audioSample.SetVolume(_setting.Audio.Volume.BGM);
            _audioSample.Speed = PlaybackSpeed;
            if(IsPracticeMode)
            {
                if(_gameInfo.TimeRange is Range<double> timeRange)
                {
                    var playbackSpeed = _playbackSpeed;
                    var startAt = timeRange.Start;
                    var endAt = timeRange.End;
                    startAt = Math.Max(startAt - 3, 0) / playbackSpeed;
                    endAt = Math.Min(endAt, _audioSample.Length.TotalSeconds) / playbackSpeed;

                    if(startAt >= endAt)
                    {
                        //throw a exception
                    }

                    _audioTrackStartAt = (float)startAt;
                }
            }
            AudioLength = (float)_audioSample.Length.TotalSeconds / MajInstances.Setting.Mod.PlaybackSpeed;
            MajInstances.LightManager.SetAllLight(Color.white);
        }
        /// <summary>
        /// Parse the chart into memory
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TaskCanceledException"></exception>
        async UniTask ParseChart()
        {
            try
            {
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Deserialization")}...");

                _simaiFile = await _songDetail.GetMaidataAsync(true);
                var levelIndex = (int)_gameInfo.CurrentLevel;
                var maidata = _simaiFile.Fumens[levelIndex];

                if (string.IsNullOrEmpty(maidata))
                {
                    throw new EmptyChartException();
                }

                ChartMirror(ref maidata);
                var simaiParser = SimaiParser.Shared;
                _chart = await simaiParser.ParseChartAsync(_songDetail.Levels[levelIndex], _songDetail.Designers[levelIndex], maidata);

                if (IsPracticeMode)
                {
                    if (_gameInfo.TimeRange is Range<double> timeRange)
                    {
                        _chart.Clamp(timeRange);
                    }
                    else if (_gameInfo.ComboRange is Range<long> comboRange)
                    {
                        _chart.Clamp(comboRange);
                        if (_chart.NoteTimings.Length != 0)
                        {
                            var startAt = _chart.NoteTimings[0].Timing;
                            startAt = Math.Max(startAt - 3, 0);

                            _audioTrackStartAt = (float)startAt;
                        }
                    }
                }
                if (PlaybackSpeed != 1)
                    _chart.Scale(PlaybackSpeed);
                if (_isAllBreak)
                    _chart.ConvertToBreak();
                if (_isAllEx)
                    _chart.ConvertToEx();
                if (_isAllTouch)
                    _chart.ConvertToTouch();
                if (_chart.IsEmpty)
                {
                    throw new EmptyChartException();
                }

                GameObject.Find("ChartAnalyzer").GetComponent<ChartAnalyzer>().AnalyzeAndDrawGraphAsync(_chart, AudioLength).Forget();
                await Task.Run(() => 
                {
                    //Generate ClockSounds
                    var simaiCmd = _simaiFile.Commands.Where(x => x.Prefix == "clock_count")
                                                      .FirstOrDefault();
                    var countnum = 4;
                    var firstBpm = _chart.NoteTimings.FirstOrDefault().Bpm;
                    var interval = 60 / firstBpm;

                    if (!int.TryParse(simaiCmd?.Value ?? string.Empty, out countnum))
                    {
                        countnum = 4;
                    }
                    if (!IsPracticeMode)
                    {
                        if (_chart.NoteTimings.Any(o => o.Timing < countnum * interval))
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
                    }

                    //Generate AnwserSounds
                    foreach (var timingPoint in _chart.NoteTimings)
                    {
                        if (timingPoint.Notes.All(o => o.IsSlideNoHead)) continue;

                        _anwserSoundList.Add(new AnwserSoundPoint()
                        {
                            time = timingPoint.Timing,
                            isClock = false,
                            isPlayed = false
                        });
                        var holds = timingPoint.Notes.FindAll(o => o.Type == SimaiNoteType.Hold || o.Type == SimaiNoteType.TouchHold);
                        if (holds.Length == 0)
                            continue;
                        foreach (var hold in holds)
                        {
                            var newtime = timingPoint.Timing + hold.HoldTime;
                            if (!_chart.NoteTimings.Any(o => Math.Abs(o.Timing - newtime) < 0.001) &&
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
            finally
            {
                await UniTask.Yield();
            }
        }

        /// <summary>
        /// Load the background picture and set brightness
        /// </summary>
        /// <returns></returns>
        async UniTask InitBackground()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            var BGManager = GameObject.Find("Background").GetComponent<BGManager>();
            var dim = _setting.Game.BackgroundDim;
            if (dim < 1f)
            {
                var videoPath = await _songDetail.GetVideoPathAsync();
                if (!string.IsNullOrEmpty(videoPath))
                {
                    BGManager.SetBackgroundMovie(videoPath, PlaybackSpeed);
                }
                else
                {
                    BGManager.SetBackgroundPic(await _songDetail.GetCoverAsync(false));
                }
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
            var loaderTask = _noteLoader.LoadNotesIntoPool(_chart).AsTask();

            while (!loaderTask.IsCompleted)
            {
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Loading Chart")}...\n{_noteLoader.Process * 100:F2}%");
                await UniTask.Yield();
            }
            if(loaderTask.IsFaulted)
            {
                var e = loaderTask.Exception.InnerException;

                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Failed to load chart")}\n{e.Message}%", Color.red);
                MajDebug.LogException(loaderTask.Exception);
                StopAllCoroutines();
                throw e;
            }
            MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Loading Chart")}...\n100.00%");
            await UniTask.Delay(1000);
        }
        async UniTask PrepareToPlay()
        {
            if (_audioSample is null)
                return;
            _audioTime = -5f;

            Time.timeScale = 1f;
            var firstClockTiming = _anwserSoundList[0].time;
            float extraTime = 5f;
            if (firstClockTiming < -5f)
                extraTime += (-(float)firstClockTiming - 5f) + 2f;
            if (FirstNoteAppearTiming != 0)
                extraTime += -(FirstNoteAppearTiming + 4f);
            _audioStartTime = (float)(_timer.ElapsedSecondsAsFloat + _audioSample.CurrentSec) + extraTime;
            _thisFrameSec = -extraTime;
            _thisFixedUpdateSec = _thisFrameSec;

            StartToPlayAnswer();
            _noteManager.InitializeUpdater();

            var allBackguardTasks = ListManager.WaitForBackgroundTasksSuspendAsync().AsValueTask();
            while(!allBackguardTasks.IsCompleted)
            {
                _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Waiting for all background tasks to suspend")}...");
                await UniTask.Yield();
            }
            _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Loading")}...");
            await UniTask.Delay(1000);
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            MajInstances.SceneSwitcher.FadeOut();

            MajInstances.GameManager.DisableGC();

            State = GamePlayStatus.Running;

            if (!IsPracticeMode)
            {
                while (_timer.ElapsedSecondsAsFloat - AudioStartTime < 0)
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            }
            _audioSample.Volume = 0;
            _audioSample.Play();
            _audioSample.CurrentSec = _audioTrackStartAt * _playbackSpeed;
            _audioStartTime = _timer.ElapsedSecondsAsFloat - _audioTrackStartAt;
            MajDebug.Log($"Chart playback speed: {PlaybackSpeed}x");
            _bgInfoHeaderAnim.SetTrigger("fadeIn");
            if(IsPracticeMode)
            {
                var elapsedSeconds = 0f;
                var originVol = _setting.Audio.Volume.BGM;
                
                BgHeaderFadeOut();
                while (elapsedSeconds < 3)
                {
                    _audioSample.Volume = (elapsedSeconds / 3f) * originVol;
                    await UniTask.Yield();
                    elapsedSeconds += Time.deltaTime;
                }
                _audioSample.Volume = originVol;
            }
            else
            {
                _audioSample.Volume = _setting.Audio.Volume.BGM;
                await UniTask.Delay(3000);
                BgHeaderFadeOut();
            }
        }
        void BgHeaderFadeOut()
        {
            if (_gameInfo.IsDanMode)
                return;
            switch (MajInstances.Setting.Game.BGInfo)
            {
                case BGInfoType.Achievement_101:
                case BGInfoType.Achievement_100:
                case BGInfoType.Achievement:
                case BGInfoType.AchievementClassical:
                case BGInfoType.AchievementClassical_100:
                case BGInfoType.S_Board:
                case BGInfoType.SS_Board:
                case BGInfoType.SSS_Board:
                case BGInfoType.MyBest:
                case BGInfoType.DXScore:
                    _bgInfoHeaderAnim.SetTrigger("fadeOut");
                    break;
                case BGInfoType.CPCombo:
                case BGInfoType.PCombo:
                case BGInfoType.Combo:
                case BGInfoType.DXScoreRank:
                case BGInfoType.Diff:
                    break;
                default:
                    return;
            }
        }
        
        internal void OnUpdate()
        {
            UpdateAudioTime();
            UpdateFnKeyState();
            if (_audioSample is null)
                return;
            else if (State < GamePlayStatus.Running)
                return;
            else if (!_objectCounter.AllFinished)
                return;
            if(_allNotesFinishedTiming is null)
            {
                _allNotesFinishedTiming = _audioTime;
                return;
            }
            else
            {
                if (_audioTime - (float)_allNotesFinishedTiming < 0.1)
                    return;
            }
            var remainingTime = _audioTime - (_audioSample.Length.TotalSeconds / PlaybackSpeed);
            _2367PressTime = 0;
            _3456PressTime = 0;
            switch (State)
            {
                case GamePlayStatus.Running:
                    {
                        var result = CalculateScore();
                        
                        switch (result.ComboState)
                        {
                            case ComboState.APPlus:
                            case ComboState.AP:
                            case ComboState.FCPlus:
                            case ComboState.FC:
                                if(IsPracticeMode)
                                {
                                    NextRound4Practice(2000).Forget();
                                }
                                else
                                {
                                    EndGame(5000).Forget();
                                }
                                return;
                        }
                        if (remainingTime < -7 && !IsPracticeMode)
                        {
                            _skipBtn.SetActive(true);
                        }
                        State = GamePlayStatus.WaitForEnd;
                    }
                    break;
                case GamePlayStatus.WaitForEnd:
                    {
                        if (IsPracticeMode)
                        {
                            NextRound4Practice().Forget();
                            return;
                        }
                        else if (remainingTime >= 0)
                        {
                            _skipBtn.SetActive(false);
                            EndGame(2000).Forget();
                        }
                    }
                    break;
            }
        }
        void UpdateFnKeyState()
        {
            if (!_isFastRetryAvailable && !_isTrackSkipAvailable)
                return;
            else if (IsPracticeMode)
                return;
            else if (_audioTime < 5f)
                return;
            switch(State)
            {
                case GamePlayStatus.Running:
                    var _2367 = _ioManager.CheckButtonStatus(SensorArea.A2, SensorStatus.On) &&
                                _ioManager.CheckButtonStatus(SensorArea.A3, SensorStatus.On) &&
                                _ioManager.CheckButtonStatus(SensorArea.A6, SensorStatus.On) &&
                                _ioManager.CheckButtonStatus(SensorArea.A7, SensorStatus.On);
                    var _3456 = _ioManager.CheckButtonStatus(SensorArea.A3, SensorStatus.On) &&
                                _ioManager.CheckButtonStatus(SensorArea.A4, SensorStatus.On) &&
                                _ioManager.CheckButtonStatus(SensorArea.A5, SensorStatus.On) &&
                                _ioManager.CheckButtonStatus(SensorArea.A6, SensorStatus.On);
                    if(_2367)
                    {
                        _2367PressTime += Time.deltaTime;
                        _3456PressTime = 0;
                    }
                    else if(_3456)
                    {
                        _3456PressTime += Time.deltaTime;
                        _2367PressTime = 0;
                    }
                    else
                    {
                        _3456PressTime = 0;
                        _2367PressTime = 0;
                    }
                    break;
                default:
                    _3456PressTime = 0;
                    _2367PressTime = 0;
                    return;
            }
            if(_2367PressTime >= 1f && _isTrackSkipAvailable)
            {
                CalculateScore();
                EndGame().Forget();
            }
            else if(_3456PressTime >= 1f && _isFastRetryAvailable)
            {
                FastRetry().Forget();
            }
        }
        internal void OnFixedUpdate()
        {
            if (_audioSample is null)
                return;
            else if (AudioStartTime == -114514f)
                return;

            switch (State)
            {
                case GamePlayStatus.Running:
                case GamePlayStatus.Blocking:
                case GamePlayStatus.WaitForEnd:
                    var chartOffset = (_simaiFile.Offset + _setting.Judge.AudioOffset) / PlaybackSpeed;
                    var timeOffset = _timer.ElapsedSecondsAsFloat - AudioStartTime;
                    _thisFixedUpdateSec = timeOffset - chartOffset;
                    break;
            }
        }
        void UpdateAudioTime()
        {
            if (_audioSample is null)
                return;
            else if (AudioStartTime == -114514f)
                return;

            switch (State)
            {
                case GamePlayStatus.Running:
                case GamePlayStatus.Blocking:
                case GamePlayStatus.WaitForEnd:
                    //Do not use this!!!! This have connection with sample batch size
                    //AudioTime = (float)audioSample.GetCurrentTime();
                    var chartOffset = (_simaiFile.Offset + _setting.Judge.AudioOffset) / PlaybackSpeed;
                    var timeOffset = _timer.ElapsedSecondsAsFloat - AudioStartTime;
                    var realTimeDifference = (float)_audioSample.CurrentSec - (_timer.ElapsedSecondsAsFloat - AudioStartTime) * PlaybackSpeed;

                    _audioTime = timeOffset - chartOffset;
                    _thisFrameSec = _audioTime;
                    _audioTimeNoOffset = timeOffset;
                    _errText.text = ZString.Format("Diff{0:F4}", Math.Abs(realTimeDifference));

                    if (Math.Abs(realTimeDifference) > 0.01f && _audioTime > 0 && MajInstances.Setting.Debug.TryFixAudioSync)
                    {
                        _audioSample.CurrentSec = _timer.ElapsedSecondsAsFloat - AudioStartTime;
                    }
                    break;
            }
        }
        private GameResult CalculateScore(bool playEffect = true)
        {
            var acc = _objectCounter.CalculateFinalResult();
            print("GameResult: " + acc);
            var result = _objectCounter.GetPlayRecord(_songDetail, MajInstances.GameManager.SelectedDiff);
            _gameInfo.RecordResult(result);

            if (!playEffect) 
                return result;
            PlayComboEffect(result);

            return result;
        }
        void PlayComboEffect(GameResult result)
        {
            switch(result.ComboState)
            {
                case ComboState.APPlus:
                    AllPerfectAnimation.SetActive(true);
                    MajInstances.AudioManager.PlaySFX("all_perfect_plus.wav");
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    break;
                case ComboState.AP:
                    AllPerfectAnimation.SetActive(true);
                    MajInstances.AudioManager.PlaySFX("all_perfect.wav");
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    break;
                case ComboState.FCPlus:
                    FullComboAnimation.SetActive(true);
                    MajInstances.AudioManager.PlaySFX("full_combo_plus.wav");
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    break;
                case ComboState.FC:
                    FullComboAnimation.SetActive(true);
                    MajInstances.AudioManager.PlaySFX("full_combo.wav");
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    break;
            }
        }
        async UniTaskVoid NextRound4Practice(int delayMiliseconds = 100)
        {
            if (State == GamePlayStatus.Ended)
                return;

            State = GamePlayStatus.Ended;
            
            await UniTask.Delay(delayMiliseconds);
            ClearAllResources();
            var remainingSeconds = 1f;
            var originVol = _setting.Audio.Volume.BGM;
            _audioSample!.Volume = 0;
            while (remainingSeconds > 0)
            {
                _audioSample.Volume = (remainingSeconds / 1f) * originVol;

                await UniTask.Yield();
                remainingSeconds -= Time.deltaTime;
            }
            _audioSample.Volume = 0;
            _audioSample.Pause();

            await UniTask.Delay(200);
            if(_gameInfo.NextRound())
            {
                MajInstances.SceneSwitcher.SwitchScene("Game",false);
            }
            else
            {
                MajInstances.SceneSwitcher.SwitchScene("Result");
            }
        }
        async UniTaskVoid FastRetry()
        {
            if (State == GamePlayStatus.Ended)
                return;
            State = GamePlayStatus.Ended;
            MajInstances.SceneSwitcher.FadeIn();
            await UniTask.Delay(400);
            MajInstances.SceneSwitcher.SwitchScene("Game", false);
            ClearAllResources();
        }
        
        async void StartToPlayAnswer()
        {
            await Task.Run(async () => 
            {
                int i = 0;
                var token = _cts.Token;
                var isUnityFMOD = MajInstances.Setting.Audio.Backend == SoundBackendType.Unity;

                while (true)
                {
                    token.ThrowIfCancellationRequested();
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
                                    MajEnv.ExecutionQueue.Enqueue(() => MajInstances.AudioManager.PlaySFX("answer_clock.wav"));
                                else
                                    MajInstances.AudioManager.PlaySFX("answer_clock.wav");
#else
                                MajInstances.AudioManager.PlaySFX("answer_clock.wav");
#endif
                                MajEnv.ExecutionQueue.Enqueue(() => _xxlbController.Stepping());
                            }
                            else
                            {
#if UNITY_EDITOR
                                if (isUnityFMOD)
                                    MajEnv.ExecutionQueue.Enqueue(() => MajInstances.AudioManager.PlaySFX("answer.wav"));
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
                    catch (Exception e)
                    {
                        MajDebug.LogException(e);
                    }
                    finally
                    {
                        await Task.Delay(1);
                    }
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
                _audioSample.Stop();
                _audioSample = null;
            }
        }
        public void GameOver()
        {
            //TODO: Play GameOver Animation
            CalculateScore(playEffect:false);

            EndGame(targetScene: "TotalResult").Forget();
        }
        void ClearAllResources()
        {
            StopAllCoroutines();

            _cts.Cancel();

            if(!_bgManager.IsUnityNull())
                _bgManager.CancelTimeRef();

            MajInstances.InputManager.ClearAllSubscriber();
            MajInstances.SceneSwitcher.SetLoadingText(string.Empty, Color.white);
            MajInstances.GameManager.EnableGC();
            MajInstanceHelper<GamePlayManager>.Free();
            MajInstanceHelper<INoteController>.Free();
        }
        async UniTaskVoid BackToList()
        {
            ClearAllResources();

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            MajInstances.SceneSwitcher.SwitchScene("List");
        }
        public async UniTaskVoid EndGame(int delayMiliseconds = 100,string targetScene = "Result")
        {
            if (State == GamePlayStatus.Ended)
                return;
            State = GamePlayStatus.Ended;

            await UniTask.Delay(delayMiliseconds);
            MajInstances.SceneSwitcher.FadeIn();
            await UniTask.Delay(400);
            ClearAllResources();
            await UniTask.DelayFrame(5);
            
            MajInstances.SceneSwitcher.SwitchScene(targetScene);
        }
        void ChartMirror(ref string chartContent)
        {
            var mirrorType = _setting.Game.Mirror;
            if (mirrorType is MirrorType.Off)
                return;
            chartContent = SimaiMirror.NoteMirrorHandle(chartContent, mirrorType);
        }
        void OnDestroy()
        {
            try
            {
                MajDebug.Log("GPManagerDestroy");

                DisposeAudioTrack();
                ClearAllResources();
            }
            finally
            {
                Cursor.visible = true;
            }
        }
        class AnwserSoundPoint
        {
            public double time;
            public bool isClock;
            public bool isPlayed;
        }
    }
}
