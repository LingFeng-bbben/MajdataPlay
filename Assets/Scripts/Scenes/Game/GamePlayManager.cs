using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Utils;
using MajdataPlay.Extensions;
using MajSimai;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using MajdataPlay.Timer;
using Cysharp.Text;
using MajdataPlay.List;
using System.Text.Json;
using MajdataPlay.Editor;
using MajdataPlay.Game.Notes.Controllers;
using MajdataPlay.Recording;
using UnityEngine.Profiling;
using MajdataPlay.Numerics;
using MajdataPlay.Game.Notes;
using MychIO;

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
        ///  The first Note appear timing
        /// </summary>
        public float FirstNoteAppearTiming
        {
            get => _firstNoteAppearTiming;
            set => _firstNoteAppearTiming = value;
        }
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
        public bool IsStart { get; private set; } = false;
        public bool IsAutoplay => AutoplayMode != AutoplayMode.Disable;
        public AutoplayMode AutoplayMode
        {
            get => ModInfo.AutoPlay;
        }

        public JudgeGrade AutoplayGrade { get; private set; } =  JudgeGrade.Perfect;
        public GameModInfo ModInfo { get; private set; }
        public float PlaybackSpeed 
        {
            get => ModInfo.PlaybackSpeed;
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
        GameSetting _setting = MajInstances.Settings;
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
        float _audioTimeNoOffset = -114514;
        [ReadOnlyField]
        [SerializeField]
        float _audioStartTime = -114514;
        int _chartRotation = 0;

        bool _isTrackSkipAvailable = MajEnv.UserSettings.Game.TrackSkip;
        bool _isFastRetryAvailable = MajEnv.UserSettings.Game.FastRetry;
        float? _allNotesFinishedTiming = null;
        float _2367PressTime = 0;
        float _3456PressTime = 0;
        float _p1SkipTime = 0;

        readonly SceneSwitcher _sceneSwitcher = MajInstances.SceneSwitcher;

        Task _generateAnswerSFXTask = Task.CompletedTask;
        Text _errText;
        MajTimer _timer = MajTimeline.CreateTimer();
        float _audioTrackStartAt = 0f;

        GameInfo _gameInfo = Majdata<GameInfo>.Instance!;

        SimaiFile _simaiFile;
        SimaiChart _chart;
        ISongDetail _songDetail;

        AudioSampleWrap? _audioSample = null;

        BGManager _bgManager;
        NoteLoader _noteLoader;
        NoteManager _noteManager;
        NoteAudioManager _noteAudioManager;
        NotePoolManager _notePoolManager;
        NoteEffectPool _noteEffectPool;
        ObjectCounter _objectCounter;
        TimeDisplayer _timeDisplayer;
        RecorderStatusDisplayer _recorderStateDisplayer;

        readonly CancellationTokenSource _cts = new();

        #region GameLoading

        void Awake()
        {
            Majdata<GamePlayManager>.Instance = this;
            Majdata<INoteController>.Instance = this;
            Majdata<INoteTimeProvider>.Instance = this;
            if (_gameInfo is null || _gameInfo.Current is null)
            {
                throw new ArgumentNullException(nameof(_gameInfo));
            }
            //print(MajInstances.GameManager.SelectedIndex);
            _songDetail = _gameInfo.Current;
            HistoryScore = ScoreManager.GetScore(_songDetail, MajInstances.GameManager.SelectedDiff);
            _timer = MajTimeline.CreateTimer();
#if !UNITY_EDITOR
            if(_setting.Debug.HideCursorInGame)
            {
                Cursor.visible = false;
            }
#endif
            LoadGameMod();
            if (_gameInfo.IsDanMode)
            {
                LoadDanModSettings();
            }
            if (InputManager.IsTouchPanelConnected)
            {
                Destroy(GameObject.Find("EventSystem"));
            }
        }
        void Start()
        {
            _noteManager = Majdata<NoteManager>.Instance!;
            _bgManager = Majdata<BGManager>.Instance!;
            _objectCounter = Majdata<ObjectCounter>.Instance!;
            _noteAudioManager = Majdata<NoteAudioManager>.Instance!;
            _notePoolManager = Majdata<NotePoolManager>.Instance!;
            _noteEffectPool = Majdata<NoteEffectPool>.Instance!;
            _timeDisplayer = Majdata<TimeDisplayer>.Instance!;
            _noteLoader = Majdata<NoteLoader>.Instance!;
            _recorderStateDisplayer = Majdata<RecorderStatusDisplayer>.Instance!;

            _errText = GameObject.Find("ErrText").GetComponent<Text>();
            _chartRotation = _setting.Game.Rotation.Clamp(-7, 7);
            
            InitGame().Forget();
            return;
        }
        void LoadDanModSettings()
        {
            var danInfo = _gameInfo.DanInfo;
            var playbackSpeed = ModInfo.PlaybackSpeed;
            var isAllBreak = ModInfo.AllBreak;
            var isAllEx = ModInfo.AllEx;
            var isAllTouch = ModInfo.AllTouch;
            var isUseButtonRingForTouch = ModInfo.ButtonRingForTouch;
            var isSlideNoHead = ModInfo.SlideNoHead;
            var isSlideNoTrack = ModInfo.SlideNoTrack;
            var autoplayMode = ModInfo.AutoPlay;
            var judgeStyle = ModInfo.JudgeStyle;
            var subdivideSlideJudgeGrade = ModInfo.SubdivideSlideJudgeGrade;
            var noteMask = ModInfo.NoteMask;
            foreach (var (k,v) in danInfo!.Mods)
            {
                switch(k)
                {
                    case "PlaybackSpeed":
                        if (v.ValueKind is JsonValueKind.Number && 
                            v.TryGetSingle(out var playbackSpeed1) || (float.TryParse(v.ToString(), out playbackSpeed1)))
                        {
                            playbackSpeed = playbackSpeed1;
                        }
                        break;
                    case "AllBreak":
                        if(v.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        {
                            isAllBreak = v.GetBoolean();
                        }
                        else if(bool.TryParse(v.ToString(), out var allBreak))
                        {
                            isAllBreak = allBreak;
                        }
                        break;
                    case "AllEx":
                        if (v.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        {
                            isAllEx = v.GetBoolean();
                        }
                        else if (bool.TryParse(v.ToString(), out var allEx))
                        {
                            isAllEx = allEx;
                        }
                        break;
                    case "AllTouch":
                        if (v.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        {
                            isAllTouch = v.GetBoolean();
                        }
                        else if (bool.TryParse(v.ToString(), out var allTouch))
                        {
                            isAllEx = allTouch;
                        }
                        break;
                    case "ButtonRingForTouch":
                        if (v.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        {
                            isUseButtonRingForTouch = v.GetBoolean();
                        }
                        else if (bool.TryParse(v.ToString(), out var buttonRingSlide))
                        {
                            isUseButtonRingForTouch = buttonRingSlide;
                        }
                        break;
                    case "IsSlideNoHead":
                        if (v.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        {
                            isSlideNoHead = v.GetBoolean();
                        }
                        else if (bool.TryParse(v.ToString(), out var slideNoHead))
                        {
                            isSlideNoHead = slideNoHead;
                        }
                        break;
                    case "IsSlideNoTrack":
                        if (v.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        {
                            isSlideNoTrack = v.GetBoolean();
                        }
                        else if (bool.TryParse(v.ToString(), out var slideNoTrack))
                        {
                            isSlideNoTrack = slideNoTrack;
                        }
                        break;
                    case "AutoPlay":
                        if (v.ValueKind is JsonValueKind.Number && v.TryGetInt32(out var modeIndex))
                        {
                            autoplayMode = (AutoplayMode)modeIndex;
                        }
                        else if(Enum.TryParse<AutoplayMode>(v.ToString(), false, out var mode))
                        {
                            autoplayMode = mode;
                        }
                        break;
                    case "JudgeStyle":
                        if (v.ValueKind is JsonValueKind.Number && v.TryGetInt32(out var styleIndex))
                        {
                            judgeStyle = (JudgeStyleType)styleIndex;
                        }
                        else if (Enum.TryParse<JudgeStyleType>(v.ToString(), false, out var style))
                        {
                            judgeStyle = style;
                        }
                        break;
                    case "NoteMask":
                        noteMask = v.ToString();
                        break;
                    case "SubdivideSlideJudgeGrade":
                        if (v.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        {
                            subdivideSlideJudgeGrade = v.GetBoolean();
                        }
                        else if (bool.TryParse(v.ToString(), out var ssjg))
                        {
                            subdivideSlideJudgeGrade = ssjg;
                        }
                        break;
                }
            }
            ModInfo = new(ModInfo)
            {
                PlaybackSpeed = playbackSpeed,
                AllBreak = isAllBreak,
                AllEx = isAllEx,
                AllTouch = isAllTouch,
                ButtonRingForTouch = isUseButtonRingForTouch,
                SlideNoHead = isSlideNoHead,
                SlideNoTrack = isSlideNoTrack,
                AutoPlay = autoplayMode,
                JudgeStyle = judgeStyle,
                NoteMask = noteMask,
                SubdivideSlideJudgeGrade = subdivideSlideJudgeGrade
            };
        }
        void LoadGameMod()
        {
            var modsetting = MajInstances.GameManager.Setting.Mod;
            ModInfo = modsetting;
            //AutoplayParam = mod5.Value ?? 7;
        }
        /// <summary>
        /// Parse the chart and load it into memory, or dump it locally if the chart is online
        /// </summary>
        /// <returns></returns>
        async UniTaskVoid InitGame()
        {
            State = GamePlayStatus.Loading;
            try
            {
                if (_songDetail.IsOnline)
                {
                    var progress = new NetProgress();
                    _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading")}...");
                    _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Audio Track")}...");
                    var task1 = _songDetail.GetAudioTrackAsync(progress, token: _cts.Token).AsValueTask();
                    while (!task1.IsCompleted)
                    {
                        await UniTask.Yield();
                        _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Audio Track")}...\n{progress.Percent * 100:F2}%");
                    }
                    progress.Reset();
                    _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Maidata")}...");
                    var task2 = _songDetail.GetMaidataAsync(false, progress, token: _cts.Token).AsValueTask();
                    while (!task2.IsCompleted)
                    {
                        await UniTask.Yield();
                        _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Maidata")}...\n{progress.Percent * 100:F2}%");
                    }
                    progress.Reset();
                    _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Picture")}...");
                    var task3 = _songDetail.GetCoverAsync(false, progress, token: _cts.Token).AsValueTask();
                    while (!task3.IsCompleted)
                    {
                        await UniTask.Yield();
                        _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Picture")}...\n{progress.Percent * 100:F2}%");
                    }
                    progress.Reset();
                    _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Video")}...");
                    var task4 = _songDetail.GetVideoPathAsync(progress, token: _cts.Token).AsValueTask();
                    while (!task4.IsCompleted)
                    {
                        await UniTask.Yield();
                        _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Video")}...\n{progress.Percent * 100:F2}%");
                    }
                    _sceneSwitcher.SetLoadingText(string.Empty);
                }

                await LoadAudioTrack();
                await InitBackground();
                await ParseChart();
                await LoadNotes();
                await PrepareToPlay();
            }
            catch (EmptyChartException)
            {
                InputManager.ClearAllSubscriber();
                var s = Localization.GetLocalizedText("Empty Chart");
                //var ss = string.Format(Localization.GetLocalizedText("Return to {0} in {1} seconds"), "List", "1");
                MajInstances.SceneSwitcher.SetLoadingText($"{s}", Color.red);
                await UniTask.Delay(1000);
                BackToList().Forget();
            }
            catch (OBSRecorderException)
            {
                InputManager.ClearAllSubscriber();
                var s = Localization.GetLocalizedText("OBSError");
                MajInstances.SceneSwitcher.SetLoadingText($"{s}", Color.red);
                await UniTask.Delay(1000);
                BackToList().Forget();
            }
            catch(InvalidSimaiMarkupException syntaxE)
            {
                MajInstances.SceneSwitcher.SetLoadingText($"{"Invalid syntax".i18n()}\n(at L{syntaxE.Line}:C{syntaxE.Column}) \"{syntaxE.Content}\"\n{syntaxE.Message}", Color.red);
                MajDebug.LogError(syntaxE);
                return;
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
                    var playbackSpeed = PlaybackSpeed;
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
            AudioLength = (float)_audioSample.Length.TotalSeconds / MajInstances.Settings.Mod.PlaybackSpeed;
        }
        /// <summary>
        /// Parse the chart into memory
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TaskCanceledException"></exception>
        async UniTask ParseChart()
        {
            void ChartMirror(ref string chartContent)
            {
                var mirrorType = _setting.Game.Mirror;
                if (mirrorType is MirrorType.Off)
                    return;
                chartContent = SimaiMirror.NoteMirrorHandle(chartContent, mirrorType);
            }
            try
            {
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Deserialization")}...");

                _simaiFile = await _songDetail.GetMaidataAsync(true);
                var levelIndex = (int)_gameInfo.CurrentLevel;
                var maidata = _simaiFile.RawCharts[levelIndex];

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
                        var range = new Range<double>(timeRange.Start - _simaiFile.Offset, timeRange.End - _simaiFile.Offset);
                        _chart.Clamp(range);
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
                if (ModInfo.PlaybackSpeed != 1)
                {
                    _chart.Scale(PlaybackSpeed);
                }
                if (ModInfo.AllBreak)
                {
                    _chart.ConvertToBreak();
                }
                if (ModInfo.AllEx)
                {
                    _chart.ConvertToEx();
                }
                if (ModInfo.AllTouch)
                {
                    _chart.ConvertToTouch();
                }
                if (_chart.IsEmpty)
                {
                    throw new EmptyChartException();
                }

                GameObject.Find("ChartAnalyzer").GetComponent<ChartAnalyzer>().AnalyzeAndDrawGraphAsync(_chart, AudioLength).Forget();
                var simaiCmd = _simaiFile.Commands.FirstOrDefault(x => x.Prefix == "clock_count");
                var countnum = 4;
                if (!int.TryParse(simaiCmd?.Value ?? string.Empty, out countnum))
                {
                    countnum = 4;
                }
                _generateAnswerSFXTask = _noteAudioManager.GenerateAnswerSFX(_chart, IsPracticeMode, countnum);
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

            var dim = _setting.Game.BackgroundDim;
            if (dim < 1f)
            {
                var videoPath = await _songDetail.GetVideoPathAsync();
                if (!string.IsNullOrEmpty(videoPath))
                {
                    await _bgManager.SetBackgroundMovie(videoPath, await _songDetail.GetCoverAsync(false));
                }
                else
                {
                    _bgManager.SetBackgroundPic(await _songDetail.GetCoverAsync(false));
                }        
            }

            _bgManager.SetBackgroundDim(1.0f);
        }
        /// <summary>
        /// Parse and load notes into NotePool
        /// </summary>
        /// <returns></returns>
        async UniTask LoadNotes()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            var tapSpeed = Math.Abs(_setting.Game.TapSpeed);

            if(_setting.Game.TapSpeed < 0)
            {
                _noteLoader.NoteSpeed = -((float)(107.25 / (71.4184491 * Mathf.Pow(tapSpeed + 0.9975f, -0.985558604f))));
            }
            else
            {
                _noteLoader.NoteSpeed = ((float)(107.25 / (71.4184491 * Mathf.Pow(tapSpeed + 0.9975f, -0.985558604f))));
            }
            _noteLoader.TouchSpeed = _setting.Game.TouchSpeed;
            _noteLoader.ChartRotation = _chartRotation;

            //var loaderTask = noteLoader.LoadNotes(Chart);
            var loaderTask = _noteLoader.LoadNotesIntoPoolAsync(_chart).AsTask();

            while (!loaderTask.IsCompleted)
            {
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Loading Chart")}...\n{_noteLoader.Progress * 100:F2}%");
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
            await UniTask.Yield();
        }
        async UniTask PrepareToPlay()
        {
            if (_audioSample is null)
            {
                return;
            }
            switch (ModInfo.NoteMask)
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
            var token = _cts.Token;
            const float BG_FADE_IN_LENGTH_SEC = 0.25f;
            Time.timeScale = 1f;
            var firstClockTiming = _noteAudioManager.AnswerSFXTimings[0].Timing;
            float extraTime = 5f;
            if (firstClockTiming < 0f)
            {
                extraTime = (-(float)firstClockTiming) + 5f;
            }
            if (FirstNoteAppearTiming < 0f)
            {
                extraTime = MathF.Min(extraTime, (-FirstNoteAppearTiming + 5f));
            }
            _audioStartTime = (float)(_timer.ElapsedSecondsAsFloat + _audioSample.CurrentSec) + extraTime;
            _thisFrameSec = -extraTime;
            _thisFixedUpdateSec = _thisFrameSec;

            _noteManager.InitializeUpdater();
            while (!_generateAnswerSFXTask.IsCompleted)
            {
                await UniTask.Yield();
            }
            var allBackgroundTasks = ListManager.WaitForBackgroundTasksSuspendAsync().AsValueTask();
            while (!allBackgroundTasks.IsCompleted)
            {
                _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Waiting for all background tasks to suspend")}...");
                await UniTask.Yield();
            }
            
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            var wait4Recorder = RecordHelper.StartRecordAsync($"{_songDetail.Title}_{_songDetail.Designers[(int)_gameInfo.CurrentLevel]}");
            while (!wait4Recorder.IsCompleted)
            {
                _sceneSwitcher.SetLoadingText($"{"Waiting for recorder".i18n()}...");
                await UniTask.Yield();
            }
            if(wait4Recorder.IsFaulted)
            {
                throw wait4Recorder.Exception.GetBaseException();
            }
            _sceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Loading")}...");
            await UniTask.Delay(1000);
            MajInstances.SceneSwitcher.FadeOut();
            await UniTask.Delay(100); //wait the animation

            MajInstances.GameManager.DisableGC();

            State = GamePlayStatus.Running;
            IsStart = true;
            var startSec = _audioTrackStartAt * PlaybackSpeed;
            if (!IsPracticeMode)
            {
                var userSettingBGDim = _setting.Game.BackgroundDim;
                var dimDiff = 1 - userSettingBGDim;
                var isVideoStarted = false;
                while (_timer.ElapsedSecondsAsFloat - AudioStartTime < 0)
                {
                    var timeDiff = _timer.ElapsedSecondsAsFloat - AudioStartTime;
                    if (timeDiff > -0.1f && !isVideoStarted) 
                    {
                        _bgManager.PlayVideo(startSec, PlaybackSpeed);
                        isVideoStarted = true;
                    }
                    if(timeDiff > -BG_FADE_IN_LENGTH_SEC)
                    {
                        var dim = 1 - (((BG_FADE_IN_LENGTH_SEC + timeDiff) / BG_FADE_IN_LENGTH_SEC) * dimDiff);
                        _bgManager.SetBackgroundDim(dim);
                    }
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                    token.ThrowIfCancellationRequested();
                }
            }
            else
            {
                _bgManager.PlayVideo(startSec + 0.25f, PlaybackSpeed);
            }
            _bgManager.SetBackgroundDim(_setting.Game.BackgroundDim);
            _audioSample.Play();
            _audioSample.Volume = 0;
            _audioSample.CurrentSec = startSec;

            _audioStartTime = _timer.ElapsedSecondsAsFloat - _audioTrackStartAt;
            MajDebug.Log($"Chart playback speed: {PlaybackSpeed}x");
            _bgInfoHeaderAnim.SetTrigger("fadeIn");
            if(IsPracticeMode)
            {
                var elapsedSeconds = 0f;
                var originVol = _setting.Audio.Volume.BGM;
                
                BgHeaderFadeOut();
                try
                {
                    while (elapsedSeconds < 3)
                    {
                        token.ThrowIfCancellationRequested();
                        _audioSample.Volume = (elapsedSeconds / 3f) * originVol;
                        await UniTask.Yield();
                        elapsedSeconds += MajTimeline.DeltaTime;
                    }
                }
                catch(Exception e)
                {
                    MajDebug.LogException(e);
                }
                finally
                {
                    _audioSample.Volume = originVol;
                }
            }
            else
            {
                token.ThrowIfCancellationRequested();
                _audioSample.Volume = _setting.Audio.Volume.BGM;
                await UniTask.Delay(3000);
                token.ThrowIfCancellationRequested();
                BgHeaderFadeOut();
            }
        }
        void BgHeaderFadeOut()
        {
            if (_gameInfo.IsDanMode)
                return;
            switch (MajInstances.Settings.Game.BGInfo)
            {
                case BGInfoType.Achievement_101:
                case BGInfoType.Achievement_100:
                case BGInfoType.Achievement:
                case BGInfoType.AchievementClassical:
                case BGInfoType.AchievementClassical_100:
                case BGInfoType.S_Border:
                case BGInfoType.SS_Border:
                case BGInfoType.SSS_Border:
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

#endregion

        #region GameUpdate
        internal void OnPreUpdate()
        {
            Profiler.BeginSample("GamePlayManager.OnPreUpdate");
            AudioTimeUpdate();
            ComponentPreUpdate();
            Profiler.EndSample();
        }
        internal void OnUpdate()
        {
            Profiler.BeginSample("GamePlayManager.OnUpdate");
            NoteManagerUpdate();
            GameControlUpdate();
            FnKeyStateUpdate();
            Profiler.EndSample();
        }
        internal void OnLateUpdate()
        {
            Profiler.BeginSample("GamePlayManager.OnLateUpdate");
            switch (State)
            {
                case GamePlayStatus.WaitForEnd:
                case GamePlayStatus.Blocking:
                case GamePlayStatus.Running:
                    _noteAudioManager.OnLateUpdate();
                    _noteManager.OnLateUpdate();
                    _objectCounter.OnLateUpdate();
                    break;
            }
            _noteEffectPool.OnLateUpdate();
            _recorderStateDisplayer.OnLateUpdate();
            if(_bgManager.CurrentSec > _bgManager.MediaLength.TotalSeconds)
            {
                _bgManager.SetBackgroundDim(1.0f);
            }
            else
            {
                _bgManager.OnLateUpdate();
            }
            Profiler.EndSample();
        }
        void GameControlUpdate()
        {
            if (_audioSample is null)
            {
                return;
            }
            else if (State < GamePlayStatus.Running)
            {
                return;
            }
            else if (!_objectCounter.AllFinished)
            {
                return;
            }
            if (_allNotesFinishedTiming is null)
            {
                _allNotesFinishedTiming = _thisFrameSec;
                return;
            }
            else
            {
                if (_thisFrameSec - (float)_allNotesFinishedTiming < 0.1)
                {
                    return;
                }
            }
            var remainingTime = _thisFrameSec - (_audioSample.Length.TotalSeconds / PlaybackSpeed);
            _2367Timer = 0;
            _3456Timer = 0;
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
                                if (IsPracticeMode)
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
        void ComponentPreUpdate()
        {
            switch(State)
            {
                case GamePlayStatus.WaitForEnd:
                case GamePlayStatus.Blocking:
                case GamePlayStatus.Running:
                    _noteAudioManager.OnPreUpdate();
                    _noteManager.OnPreUpdate();
                    _notePoolManager.OnPreUpdate();
                    break;
            }
            _timeDisplayer.OnPreUpdate();
        }
        void NoteManagerUpdate()
        {
            switch (State)
            {
                case GamePlayStatus.WaitForEnd:
                case GamePlayStatus.Blocking:
                case GamePlayStatus.Running:
                    _noteManager.OnUpdate();
                    break;
            }
        }
        void FnKeyStateUpdate()
        {
            if (State != GamePlayStatus.Ended)
            {

                var _2367 = InputManager.CheckButtonStatus(ButtonZone.A2, SwitchStatus.On) &&
                            InputManager.CheckButtonStatus(ButtonZone.A3, SwitchStatus.On) &&
                            InputManager.CheckButtonStatus(ButtonZone.A6, SwitchStatus.On) &&
                            InputManager.CheckButtonStatus(ButtonZone.A7, SwitchStatus.On) &&
                            _isTrackSkipAvailable;
                var _3456 = InputManager.CheckButtonStatus(ButtonZone.A3, SwitchStatus.On) &&
                            InputManager.CheckButtonStatus(ButtonZone.A4, SwitchStatus.On) &&
                            InputManager.CheckButtonStatus(ButtonZone.A5, SwitchStatus.On) &&
                            InputManager.CheckButtonStatus(ButtonZone.A6, SwitchStatus.On) &&
                            _isFastRetryAvailable;
                var _p1Skip = InputManager.CheckButtonStatus(ButtonZone.P1, SwitchStatus.On);
                if (_p1Skip)
                {
                    _p1SkipTime += MajTimeline.DeltaTime;
                }
                else if (_2367)
                {
                    _2367PressTime += MajTimeline.DeltaTime;
                    _3456PressTime = 0;
                }
                else if (_3456)
                {
                    _3456PressTime += MajTimeline.DeltaTime;
                    _2367PressTime = 0;
                }
                else
                {
                    _3456PressTime = 0;
                    _2367PressTime = 0;
                    _p1SkipTime = 0;
                }

#if UNITY_ANDROID
                var p1timeout = 0.5f;
#else
                var p1timeout = 0f;
#endif
                if (_p1SkipTime > p1timeout)
                {
                    BackToList().Forget();
                }
                else if (_2367PressTime >= 0.5f && _isTrackSkipAvailable)
                {
                    BackToList().Forget();
                }
                else if (_3456PressTime >= 0.5f && _isFastRetryAvailable)
                {
                    FastRetry().Forget();
                }
            }
            else
            {
                _3456PressTime = 0;
                _2367PressTime = 0;
                _p1SkipTime = 0;
                return;
            }
        }
        void AudioTimeUpdate()
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
                    var realTimeDifferenceb = (float)_bgManager.CurrentSec - (_timer.ElapsedSecondsAsFloat - AudioStartTime) * PlaybackSpeed;

                    _thisFrameSec = timeOffset - chartOffset;
                    _audioTimeNoOffset = (float)_audioSample.CurrentSec;
                    _errText.text = ZString.Format("Delta\nAudio {0:F4}\nVideo {1:F4}", Math.Abs(realTimeDifference),Math.Abs(realTimeDifferenceb));

                    if (Math.Abs(realTimeDifference) > 0.01f && _thisFrameSec > 0 && MajInstances.Settings.Debug.TryFixAudioSync)
                    {
                        _audioSample.CurrentSec = _timer.ElapsedSecondsAsFloat - AudioStartTime;
                    }                  
                    break;
            }
        }

#endregion

        #region GameEnding

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
                    LedRing.SetAllLight(Color.yellow);
                    break;
                case ComboState.AP:
                    AllPerfectAnimation.SetActive(true);
                    MajInstances.AudioManager.PlaySFX("all_perfect.wav");
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    LedRing.SetAllLight(Color.red);
                    break;
                case ComboState.FCPlus:
                    FullComboAnimation.SetActive(true);
                    MajInstances.AudioManager.PlaySFX("full_combo_plus.wav");
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    LedRing.SetAllLight(Color.green);
                    break;
                case ComboState.FC:
                    FullComboAnimation.SetActive(true);
                    MajInstances.AudioManager.PlaySFX("full_combo.wav");
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    LedRing.SetAllLight(Color.green);
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
            var wait4Recorder = RecordHelper.StopRecordAsync();
            while (!wait4Recorder.IsCompleted)
            {
                _sceneSwitcher.SetLoadingText($"{"Waiting for recorder".i18n()}...");
                await UniTask.Yield();
            }
            MajInstances.SceneSwitcher.FadeIn();
            await UniTask.Delay(400);
            ClearAllResources();
            MajInstances.SceneSwitcher.SwitchScene("Game", false);
        }

        public void GameOver()
        {
            //TODO: Play GameOver Animation
            CalculateScore(playEffect:false);

            EndGame(targetScene: "TotalResult").Forget();
        }

        async UniTaskVoid BackToList()
        {
            State = GamePlayStatus.Ended;
            ClearAllResources();
            var sceneSwitcher = MajInstances.SceneSwitcher;
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            sceneSwitcher.FadeIn();
            var wait4Recorder = RecordHelper.StopRecordAsync();
            while (!wait4Recorder.IsCompleted)
            {
                _sceneSwitcher.SetLoadingText($"{"Waiting for recorder".i18n()}...");
                await UniTask.Yield();
            }
            _sceneSwitcher.SetLoadingText(string.Empty);
            sceneSwitcher.SwitchScene("List",false);
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

        #endregion

        #region Clean Up
        void DisposeAudioTrack()
        {
            if (_audioSample is not null)
            {
                _audioSample.Stop();
                _audioSample = null;
            }
        }
        void ClearAllResources()
        {
            StopAllCoroutines();

            _cts.Cancel();

            InputManager.ClearAllSubscriber();
            MajInstances.SceneSwitcher.SetLoadingText(string.Empty, Color.white);
            MajInstances.GameManager.EnableGC();
            Majdata<GamePlayManager>.Free();
            Majdata<INoteController>.Free();
            Majdata<INoteTimeProvider>.Free();
        }

        void OnDestroy()
        {
            try
            {
                MajDebug.Log("GPManagerDestroy");
                //we dont StopRecordAsync at here because we want the result screen as well
                DisposeAudioTrack();
                ClearAllResources();
            }
            finally
            {
                Cursor.visible = true;
            }
        }
        #endregion
    }
}
