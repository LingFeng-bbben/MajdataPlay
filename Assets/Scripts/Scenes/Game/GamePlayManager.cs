using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MajdataPlay.Editor;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Numerics;
using MajdataPlay.Recording;
using MajdataPlay.Scenes.Game.Notes;
using MajdataPlay.Scenes.Game.Notes.Controllers;
using MajdataPlay.Scenes.List;
using MajdataPlay.Settings;
using MajdataPlay.Settings.Runtime;
using MajdataPlay.Timer;
using MajdataPlay.Utils;
using MajSimai;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace MajdataPlay.Scenes.Game
{
#nullable enable
    public class GamePlayManager : MonoBehaviour, INoteController
    {
        public float NoteSpeed { get; private set; } = 7f;
        public float TouchSpeed { get; private set; } = 7f;
        public bool IsClassicMode
        {
            get
            {
                return (_setting?.Judge.Mode ?? JudgeModeOption.Modern) == JudgeModeOption.Classic;
            }
        }
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
        /// The timing of audio starting to play
        /// </summary>
        public float AudioStartTime => _audioStartTime;
        // Control
        public bool IsStart { get; private set; } = false;
        public bool IsAutoplay => AutoplayMode != AutoplayModeOption.Disable;
        public AutoplayModeOption AutoplayMode
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
        float _audioStartTime = -114514;
        int _chartRotation = 0;

        bool _isTrackSkipAvailable = MajEnv.Settings?.Game.TrackSkip ?? false;
        bool _isFastRetryAvailable = MajEnv.Settings?.Game.FastRetry ?? false;
        float? _allNotesFinishedTiming = null;
        float _2367PressTime = 0;
        float _3456PressTime = 0;
        float _p1SkipTime = 0;

        // Offset
        float _chartOffset = 0f;
        /// <summary>
        /// Setting - Judge - AudioOffset
        /// </summary>
        float _audioTimeOffsetSec = 0f;
        float _displayOffsetSec = 0f;

        Task _generateAnswerSFXTask = Task.CompletedTask;
        TextMeshProUGUI _errText;
        MajTimer _timer = MajTimeline.CreateTimer();
        float _audioTrackStartAt = 0f;

        GameInfo _gameInfo = Majdata<GameInfo>.Instance!;

        SimaiFile _simaiFile;
        SimaiChart _chart;
        ChartSetting _chartSetting;
        ISongDetail _songDetail;

        float _trackVolume = 1f;

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
        readonly ListConfig _listConfig = MajEnv.RuntimeConfig?.List ?? new();
        readonly SceneSwitcher _sceneSwitcher = MajInstances.SceneSwitcher;

        readonly static Utf16PreparedFormat<float, float> ERROR_TEXT_FORMAT = ZString.PrepareUtf16<float, float>("Delta\nAudio {0:F4}\nVideo {1:F4}");

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
            HistoryScore = ScoreManager.GetScore(_songDetail, _listConfig.SelectedDiff);
            _timer = MajTimeline.CreateTimer();
            _chartSetting = ChartSettingStorage.GetSetting(_songDetail);
            if(_setting.Debug.OffsetUnit == OffsetUnitOption.Second)
            {
                _audioTimeOffsetSec = _setting.Judge.AudioOffset;
                _audioTimeOffsetSec += _chartSetting.AudioOffset;
                _displayOffsetSec = _setting.Debug.DisplayOffset;
            }
            else
            {
                _audioTimeOffsetSec = _setting.Judge.AudioOffset * MajEnv.FRAME_LENGTH_SEC;
                _audioTimeOffsetSec += _chartSetting.AudioOffset * MajEnv.FRAME_LENGTH_SEC;
                _displayOffsetSec = _setting.Debug.DisplayOffset * MajEnv.FRAME_LENGTH_SEC;
            }
            _trackVolume = (MajEnv.Settings.Audio.Volume.Track + _chartSetting.TrackVolumeOffset).Clamp(0, 2);
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
            InputManager.UseOuterTouchAsSensor = true;
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

            _errText = GameObject.Find("ErrText").GetComponent<TextMeshProUGUI>();
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
                switch (k)
                {
                    case "PlaybackSpeed":
                        {
                            if (v.Type == JTokenType.Float || v.Type == JTokenType.Integer)
                            {
                                playbackSpeed = v.ToObject<float>();
                            }
                            else if (float.TryParse(v.ToString(), out var playbackSpeed1))
                            {
                                playbackSpeed = playbackSpeed1;
                            }
                        }
                        break;
                    case "AllBreak":
                    case "AllEx":
                    case "AllTouch":
                    case "ButtonRingForTouch":
                    case "IsSlideNoHead":
                    case "IsSlideNoTrack":
                    case "SubdivideSlideJudgeGrade":
                        {
                            if (v.Type == JTokenType.Boolean)
                            {
                                bool value = v.ToObject<bool>();
                                switch (k)
                                {
                                    case "AllBreak": isAllBreak = value; break;
                                    case "AllEx": isAllEx = value; break;
                                    case "AllTouch": isAllTouch = value; break;
                                    case "ButtonRingForTouch": isUseButtonRingForTouch = value; break;
                                    case "IsSlideNoHead": isSlideNoHead = value; break;
                                    case "IsSlideNoTrack": isSlideNoTrack = value; break;
                                    case "SubdivideSlideJudgeGrade": subdivideSlideJudgeGrade = value; break;
                                }
                            }
                            else if (bool.TryParse(v.ToString(), out var boolValue))
                            {
                                switch (k)
                                {
                                    case "AllBreak": isAllBreak = boolValue; break;
                                    case "AllEx": isAllEx = boolValue; break;
                                    case "AllTouch": isAllTouch = boolValue; break;
                                    case "ButtonRingForTouch": isUseButtonRingForTouch = boolValue; break;
                                    case "IsSlideNoHead": isSlideNoHead = boolValue; break;
                                    case "IsSlideNoTrack": isSlideNoTrack = boolValue; break;
                                    case "SubdivideSlideJudgeGrade": subdivideSlideJudgeGrade = boolValue; break;
                                }
                            }
                        }
                        break;
                    case "AutoPlay":
                        {
                            if (v.Type == JTokenType.Integer)
                            {
                                autoplayMode = (AutoplayModeOption)v.ToObject<int>();
                            }
                            else if (Enum.TryParse<AutoplayModeOption>(v.ToString(), out var autoplayMode1))
                            {
                                autoplayMode = autoplayMode1;
                            }
                        }
                        break;
                    case "JudgeStyle":
                        {
                            if (v.Type == JTokenType.Integer)
                            {
                                judgeStyle = (JudgeStyleOption)v.ToObject<int>();
                            }
                            else if (Enum.TryParse<JudgeStyleOption>(v.ToString(), out var judgeStyle1))
                            {
                                judgeStyle = judgeStyle1;
                            }
                        }
                        break;
                    case "NoteMask":
                        noteMask = v.ToString();
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
            var token = _cts.Token;
            try
            {
                if (_songDetail.IsOnline)
                {
                    var progress = new NetProgress();
                    var lastPercent = 0f;
                    _sceneSwitcher.SetLoadingText($"{"Downloading".i18n()}...");
                    _sceneSwitcher.SetLoadingText($"{"Downloading Audio Track".i18n()}...");
                    var task1 = _songDetail.GetAudioTrackAsync(progress, token: _cts.Token);
                    while (!task1.IsCompleted)
                    {
                        await UniTask.Yield(cancellationToken: token);
                        var percent = progress.Percent.Clamp(0, 1);
                        LedRingLoadingUpdate(percent, lastPercent);
                        lastPercent = percent;
                        _sceneSwitcher.SetLoadingText($"{"Downloading Audio Track".i18n()}...\n{percent * 100:F2}%");
                    }
                    lastPercent = 0;
                    progress.Reset();
                    token.ThrowIfCancellationRequested();
                    _sceneSwitcher.SetLoadingText($"{"Downloading Maidata".i18n()}...");
                    var task2 = _songDetail.GetMaidataAsync(false, progress, token: _cts.Token);
                    while (!task2.IsCompleted)
                    {
                        await UniTask.Yield(cancellationToken: token);
                        var percent = progress.Percent.Clamp(0, 1);
                        LedRingLoadingUpdate(percent, lastPercent);
                        lastPercent = percent;
                        _sceneSwitcher.SetLoadingText($"{"Downloading Maidata".i18n()}...\n{percent * 100:F2}%");
                    }
                    lastPercent = 0;
                    progress.Reset();
                    token.ThrowIfCancellationRequested();
                    _sceneSwitcher.SetLoadingText($"{"Downloading Picture".i18n()}...");
                    var task3 = _songDetail.GetCoverAsync(false, progress, token: _cts.Token);
                    while (!task3.IsCompleted)
                    {
                        await UniTask.Yield(cancellationToken: token);
                        var percent = progress.Percent.Clamp(0, 1);
                        LedRingLoadingUpdate(percent, lastPercent);
                        lastPercent = percent;
                        _sceneSwitcher.SetLoadingText($"{"Downloading Picture".i18n()}...\n{percent * 100:F2}%");
                    }
                    lastPercent = 0;
                    progress.Reset();
                    token.ThrowIfCancellationRequested();
                    _sceneSwitcher.SetLoadingText($"{"Downloading Video".i18n()}...");
                    var task4 = _songDetail.GetVideoPathAsync(progress, token: _cts.Token);
                    while (!task4.IsCompleted)
                    {
                        await UniTask.Yield(cancellationToken: token);
                        var percent = progress.Percent.Clamp(0, 1);
                        LedRingLoadingUpdate(percent, lastPercent);
                        lastPercent = percent;
                        _sceneSwitcher.SetLoadingText($"{"Downloading Video".i18n()}...\n{percent * 100:F2}%");
                    }
                    _sceneSwitcher.SetLoadingText(string.Empty);
                }

                await LoadAudioTrack();
                token.ThrowIfCancellationRequested();
                await InitBackground();
                token.ThrowIfCancellationRequested();
                await ParseChart();
                token.ThrowIfCancellationRequested();
                await LoadNotes();
                token.ThrowIfCancellationRequested();
                await PrepareToPlay();
            }
            catch (EmptyChartException)
            {
                await UniTask.SwitchToMainThread();
                InputManager.ClearAllSubscriber();
                var s = Localization.GetLocalizedText("Empty Chart");
                //var ss = string.Format(Localization.GetLocalizedText("Return to {0} in {1} seconds"), "List", "1");
                MajInstances.SceneSwitcher.SetLoadingText($"{s}", Color.red);
                await UniTask.Delay(1000);
                ReturnTo().Forget();
            }
            catch (OBSRecorderException)
            {
                await UniTask.SwitchToMainThread();
                InputManager.ClearAllSubscriber();
                var s = Localization.GetLocalizedText("OBSError");
                MajInstances.SceneSwitcher.SetLoadingText($"{s}", Color.red);
                await UniTask.Delay(1000);
                ReturnTo().Forget();
            }
            catch(InvalidSimaiMarkupException syntaxE)
            {
                await UniTask.SwitchToMainThread();
                MajInstances.SceneSwitcher.SetLoadingText($"{"Invalid syntax".i18n()}\n(at L{syntaxE.Line}:C{syntaxE.Column}) \"{syntaxE.Content}\"\n{syntaxE.Message}", Color.red);
                MajDebug.LogError(syntaxE);
                return;
            }
            catch(HttpException httpEx)
            {
                await UniTask.SwitchToMainThread();
                MajInstances.SceneSwitcher.SetLoadingText("Failed to download chart".i18n(), Color.red);
                MajDebug.LogError(httpEx);
                return;
            }
            catch(InvalidAudioTrackException audioEx)
            {
                await UniTask.SwitchToMainThread();
                MajInstances.SceneSwitcher.SetLoadingText($"{"Failed to load chart".i18n()}\n{audioEx.Message}", Color.red);
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
                await UniTask.SwitchToMainThread();
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Unknown error")}\n{e.Message}", Color.red);
                MajDebug.LogError(e);
                throw;
            }
        }
        void LedRingLoadingUpdate(float percent,float lastPercent)
        {
            var progress = (int)(8 * percent);
            var lastProgress = (int)(8 * lastPercent);
            if (progress == lastProgress)
            {
                return;
            }
            LedRing.SetAllLight(Color.black);
            if (progress == 0)
            {
                LedRing.SetSineFunc(0, Color.green, 1000);
                return;
            }
            else if(progress == 8)
            {
                LedRing.SetAllLight(Color.green);
                return;
            }
            for (var i = 0; i < progress; i++)
            {
                LedRing.SetButtonLight(Color.green, i);
            }
            LedRing.SetSineFunc(progress, Color.green, 1000);
        }


        async UniTask LoadAudioTrack()
        {
            var audioSample = await _songDetail.GetAudioTrackAsync();
            if(audioSample is null || audioSample.IsEmpty)
            {
                throw new InvalidAudioTrackException("Failed to decode audio track", string.Empty);
            }
            _audioSample = audioSample;
            _audioSample.SetVolume(_trackVolume);
            _audioSample.Speed = PlaybackSpeed;
            _audioSample.IsLoop = false;
            _audioSample.CurrentSec = 0;
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
                if (mirrorType is MirrorOption.Off)
                    return;
                chartContent = SimaiMirror.NoteMirrorHandle(chartContent, mirrorType);
            }
            MajInstances.SceneSwitcher.SetLoadingText($"{"Deserialization".i18n()}...");

            _simaiFile = await _songDetail.GetMaidataAsync(true);
            _chartOffset = _simaiFile.Offset;
            var levelIndex = (int)_gameInfo.CurrentLevel;
            var maidata = _simaiFile.Charts[levelIndex].Fumen;

            if (string.IsNullOrEmpty(maidata))
            {
                throw new EmptyChartException();
            }

            ChartMirror(ref maidata);
            _chart = await SimaiParser.ParseChartAsync(_songDetail.Levels[levelIndex], _songDetail.Designers[levelIndex], maidata);

            if (IsPracticeMode)
            {
                if (_gameInfo.TimeRange is Range<double> timeRange)
                {
                    var range = new Range<double>(timeRange.Start - _simaiFile.Offset, timeRange.End - _simaiFile.Offset);
                    _chart = _chart.Clamp(range);
                }
                //else if (_gameInfo.ComboRange is Range<long> comboRange)
                //{
                //    _chart = _chart.Clamp(comboRange);
                //    if (_chart.NoteTimings.Length != 0)
                //    {
                //        var startAt = _chart.NoteTimings[0].Timing;
                //        startAt = Math.Max(startAt - 3, 0);

                //        _audioTrackStartAt = (float)startAt;
                //    }
                //}
            }
            _chart = _chart.AddOffset(_chartOffset + _audioTimeOffsetSec);
            if (ModInfo.PlaybackSpeed != 1)
            {
                _chart = _chart.Scale(PlaybackSpeed);
            }
            if (ModInfo.AllBreak)
            {
                _chart = _chart.ConvertToBreak();
            }
            if (ModInfo.AllEx)
            {
                _chart = _chart.ConvertToEx();
            }
            if (ModInfo.AllTouch)
            {
                _chart = _chart.ConvertToTouch();
            }
            if (_chart.IsEmpty)
            {
                throw new EmptyChartException();
            }
            _chart = _chart.AddOffset(-_displayOffsetSec);
            await UniTask.SwitchToMainThread();
            GameObject.Find("ChartAnalyzer").GetComponent<ChartAnalyzer>().AnalyzeAndDrawGraphAsync(_chart, AudioLength).Forget();
            await UniTask.SwitchToThreadPool();
            var simaiCmd = _simaiFile.Commands.FirstOrDefault(x => x.Prefix == "clock_count");
            var countnum = 4;
            if (!int.TryParse(simaiCmd.Value, out countnum))
            {
                countnum = 4;
            }
            _generateAnswerSFXTask = _noteAudioManager.GenerateAnswerSFX(_chart, IsPracticeMode, countnum);
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
                    var cover = await _songDetail.GetCoverAsync(false);
                    await UniTask.SwitchToMainThread();
                    await _bgManager.SetMovieAsync(videoPath, cover);
                }
                else
                {
                    var cover = await _songDetail.GetCoverAsync(false);
                    await UniTask.SwitchToMainThread();
                    _bgManager.SetBackgroundPic(cover);
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
            var loaderTask = _noteLoader.LoadNotesIntoPoolAsync(_chart, _cts.Token);
            var lastPercent = 0f;
            while (!loaderTask.Status.IsCompleted())
            {
                var percent = (float)_noteLoader.Progress.Clamp(0, 1);
                MajInstances.SceneSwitcher.SetLoadingText($"{"Loading Chart".i18n()}...\n{_noteLoader.Progress * 100:F2}%");
                LedRingLoadingUpdate(percent, lastPercent);
                lastPercent = percent;
                await UniTask.Yield();
            }
            if(loaderTask.Status.IsCanceled())
            {
                LedRing.SetAllLight(Color.white);
                return;
            }
            else if(loaderTask.Status.IsFaulted())
            {
                var task = loaderTask.AsTask();
                var e = task.Exception.InnerException;

                MajInstances.SceneSwitcher.SetLoadingText($"{"Failed to load chart".i18n()}\n{e.Message}%", Color.red);
                LedRing.SetAllLight(Color.red);
                MajDebug.LogException(task.Exception);
                StopAllCoroutines();
                throw e;
            }
            MajInstances.SceneSwitcher.SetLoadingText($"{"Loading Chart".i18n()}...\n100.00%");

            _noteEffectPool.Init();
            await UniTask.Yield();
        }
        async UniTask PrepareToPlay()
        {
            if (_audioSample is null)
            {
                return;
            }
            LedRing.SetAllLight(Color.white);
            await UniTask.SwitchToMainThread();
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
            var firstClockTiming = _noteAudioManager.FirstClockTiming;
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

            await _noteManager.InitAsync();
            while (!_generateAnswerSFXTask.IsCompleted)
            {
                token.ThrowIfCancellationRequested();
                await UniTask.Yield();
            }
            var allBackgroundTasks = ListManager.WaitForBackgroundTaskSuspendAsync();
            while (!allBackgroundTasks.IsCompleted)
            {
                token.ThrowIfCancellationRequested();
                await UniTask.SwitchToMainThread();
                _sceneSwitcher.SetLoadingText($"{"Waiting for all background tasks to suspend".i18n()}...");
                await UniTask.Yield();
            }
            token.ThrowIfCancellationRequested();
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            var wait4Recorder = RecordHelper.StartRecordAsync($"{_songDetail.Title}_{_songDetail.Designers[(int)_gameInfo.CurrentLevel]}");
            while (!wait4Recorder.IsCompleted)
            {
                token.ThrowIfCancellationRequested();
                await UniTask.SwitchToMainThread();
                _sceneSwitcher.SetLoadingText($"{"Waiting for recorder".i18n()}...");
                await UniTask.Yield();
            }
            token.ThrowIfCancellationRequested();
            if (wait4Recorder.IsFaulted)
            {
                throw wait4Recorder.Exception.GetBaseException();
            }
            await UniTask.SwitchToMainThread();
            _sceneSwitcher.SetLoadingText($"{"Loading".i18n()}...");
            MajInstances.GameManager.DisableGC();

            await UniTask.Delay(1000, cancellationToken: token);
            MajInstances.SceneSwitcher.FadeOut();
            await UniTask.Delay(100, cancellationToken: token); //wait the animation

            State = GamePlayStatus.Running;
            IsStart = true;
            var startSec = _audioTrackStartAt * PlaybackSpeed;
            if (!IsPracticeMode)
            {
                var userSettingBGDim = _setting.Game.BackgroundDim;
                var dimDiff = 1 - userSettingBGDim;
                var isVideoStarted = false;
                while (_timer.ElapsedSecondsAsFloat - _audioStartTime < 0)
                {
                    var timeDiff = _timer.ElapsedSecondsAsFloat - _audioStartTime;
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
            MajDebug.LogInfo($"Chart playback speed: {PlaybackSpeed}x");
            _bgInfoHeaderAnim.SetTrigger("fadeIn");
            if(IsPracticeMode)
            {
                var elapsedSeconds = 0f;
                var originVol = _trackVolume;
                
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
                _audioSample.Volume = _trackVolume;
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
                case BGInfoOption.Achievement_101:
                case BGInfoOption.Achievement_100:
                case BGInfoOption.Achievement:
                case BGInfoOption.AchievementClassical:
                case BGInfoOption.AchievementClassical_100:
                case BGInfoOption.S_Border:
                case BGInfoOption.SS_Border:
                case BGInfoOption.SSS_Border:
                case BGInfoOption.MyBest:
                case BGInfoOption.DXScore:
                    _bgInfoHeaderAnim.SetTrigger("fadeOut");
                    break;
                case BGInfoOption.CPCombo:
                case BGInfoOption.PCombo:
                case BGInfoOption.Combo:
                case BGInfoOption.DXScoreRank:
                case BGInfoOption.Diff:
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
            Profiler.BeginSample("GamePlayManager.AudioTimeUpdate");
            AudioTimeUpdate();
            Profiler.EndSample();
            ComponentPreUpdate();
            Profiler.EndSample();
        }
        internal void OnUpdate()
        {
            Profiler.BeginSample("GamePlayManager.OnUpdate");
            NoteManagerUpdate();
            Profiler.BeginSample("GamePlayManager.GameControlUpdate");
            GameControlUpdate();
            Profiler.EndSample();
            Profiler.BeginSample("GamePlayManager.FnKeyStateUpdate");
            FnKeyStateUpdate();
            Profiler.EndSample();
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
            Profiler.BeginSample("TimeDisplayer.OnPreUpdate");
            _timeDisplayer.OnPreUpdate();
            Profiler.EndSample();
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

#if UNITY_ANDROID || UNITY_IOS
                var p1timeout = 0.5f;
#else
                var p1timeout = 0f;
#endif
                if (_p1SkipTime > p1timeout)
                {
                    if(IsPracticeMode)
                    {
                        var info = new GameInfo(GameMode.Practice, _gameInfo.Charts, _gameInfo.Levels, 114514);
                        info.TimeRange = _gameInfo.TimeRange;
                        Majdata<GameInfo>.Instance = info;
                        ReturnTo("Practice").Forget();
                    }
                    else
                    {
                        ReturnTo().Forget();
                    }
                }
                else if (_2367PressTime >= 0.5f && _isTrackSkipAvailable)
                {
                    if (IsPracticeMode)
                    {
                        var info = new GameInfo(GameMode.Practice, _gameInfo.Charts, _gameInfo.Levels, 114514);
                        info.TimeRange = _gameInfo.TimeRange;
                        Majdata<GameInfo>.Instance = info;
                        ReturnTo("Practice").Forget();
                    }
                    else
                    {
                        ReturnTo().Forget();
                    }
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
            {
                return;
            }
            else if (AudioStartTime == -114514f)
            {
                return;
            }

            switch (State)
            {
                case GamePlayStatus.Running:
                case GamePlayStatus.Blocking:
                case GamePlayStatus.WaitForEnd:
                    {
                        //Do not use this!!!! This have connection with sample batch size
                        //AudioTime = (float)audioSample.GetCurrentTime();
                        var elapsedSeconds = _timer.ElapsedSecondsAsFloat;
                        var playbackSpeed = PlaybackSpeed;
                        var timeOffset = elapsedSeconds - _audioStartTime;
                        var realTimeDifference = (float)_audioSample.CurrentSec - (elapsedSeconds - _audioStartTime) * playbackSpeed;
                        var realTimeDifferenceb = (float)_bgManager.CurrentSec - (elapsedSeconds - _audioStartTime) * playbackSpeed;

                        _thisFrameSec = timeOffset;
#if UNITY_ANDROID || UNITY_IOS
                        if(realTimeDifference < 0 && !_objectCounter.AllFinished && _audioSample.IsPlaying)
                        {
                            _thisFrameSec += realTimeDifference;
                        }
#endif
                        var sb = ZString.CreateStringBuilder(true);
                        try
                        {
                            ERROR_TEXT_FORMAT.FormatTo(ref sb, Math.Abs(realTimeDifference), Math.Abs(realTimeDifferenceb));
                            var a = sb.AsArraySegment();
                            _errText.SetCharArray(a.Array, a.Offset, a.Count);
                        }
                        finally
                        {
                            sb.Dispose();
                        }
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
            var result = _objectCounter.GetPlayRecord(_songDetail, _listConfig.SelectedDiff);
            _gameInfo.RecordResult(result);

            if (!playEffect)
            {
                return result;
            }
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
                    LedRing.SetAllLightSineFunc(Color.yellow, 2000);
                    break;
                case ComboState.AP:
                    AllPerfectAnimation.SetActive(true);
                    MajInstances.AudioManager.PlaySFX("all_perfect.wav");
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    LedRing.SetAllLightSineFunc(Color.red, 2000);
                    break;
                case ComboState.FCPlus:
                    FullComboAnimation.SetActive(true);
                    MajInstances.AudioManager.PlaySFX("full_combo_plus.wav");
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    LedRing.SetAllLightSineFunc(Color.green, 2000);
                    break;
                case ComboState.FC:
                    FullComboAnimation.SetActive(true);
                    MajInstances.AudioManager.PlaySFX("full_combo.wav");
                    MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
                    LedRing.SetAllLightSineFunc(Color.green, 2000);
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
            var originVol = _trackVolume;
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

        async UniTaskVoid ReturnTo(string sceneName = "List")
        {
            State = GamePlayStatus.Ended;
            var sceneSwitcher = MajInstances.SceneSwitcher;
            await sceneSwitcher.FadeInAsync();
            await UniTask.Delay(500);
            _audioSample?.Stop();
            ClearAllResources();
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            var wait4Recorder = RecordHelper.StopRecordAsync();
            while (!wait4Recorder.IsCompleted)
            {
                _sceneSwitcher.SetLoadingText($"{"Waiting for recorder".i18n()}...");
                await UniTask.Yield();
            }
            _sceneSwitcher.SetLoadingText(string.Empty);
            sceneSwitcher.SwitchScene(sceneName, false);
        }
        public async UniTaskVoid EndGame(int delayMiliseconds = 100,string targetScene = "Result")
        {
            if (State == GamePlayStatus.Ended)
                return;
            State = GamePlayStatus.Ended;

            await UniTask.Delay(delayMiliseconds);
            await MajInstances.SceneSwitcher.FadeInAsync();
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
                MajDebug.LogInfo("GPManagerDestroy");
                //we dont StopRecordAsync at here because we want the result screen as well
                DisposeAudioTrack();
                ClearAllResources();
            }
            finally
            {
                Cursor.visible = true;
                InputManager.UseOuterTouchAsSensor = false;
            }
        }
        #endregion
    }
}
