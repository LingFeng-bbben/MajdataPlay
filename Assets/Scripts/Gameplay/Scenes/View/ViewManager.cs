﻿using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Game;
using MajdataPlay.Game.Notes.Controllers;
using MajdataPlay.IO;
using MajdataPlay.Timer;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using MajdataPlay.View.Types;
using MajSimai;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;
#nullable enable
namespace MajdataPlay.View
{
    internal class ViewManager: MajComponent, INoteController
    {
        public static ViewSummary Summary
        {
            get
            {
                return new()
                {
                    State = _state,
                    ErrMsg = _errMsg,
                    Timeline = _thisFrameSec,
                };
            }
        }
        public float AudioLength { get; private set; } = 0f;
        public bool IsStart 
        { 
            get
            {
                switch(_state)
                {
                    case ViewStatus.Playing:
                        return true;
                    default:
                        return false;
                }
            }
        }
        public bool IsAutoplay => AutoplayMode != AutoplayMode.Disable;
        public AutoplayMode AutoplayMode => MajEnv.UserSetting.Mod.AutoPlay;
        public JudgeGrade AutoplayGrade { get; private set; } = JudgeGrade.Perfect;
        public JudgeStyleType JudgeStyle => MajEnv.UserSetting.Mod.JudgeStyle;
        public Material BreakMaterial { get; } = MajEnv.BreakMaterial;
        public Material DefaultMaterial { get; } = MajEnv.DefaultMaterial;
        public Material HoldShineMaterial { get; } = MajEnv.HoldShineMaterial;
        public float ThisFrameSec => _thisFrameSec;
        public float ThisFixedUpdateSec => _thisFrameSec;
        public float AudioTimeNoOffset => _audioTimeNoOffset;
        public float Offset { get; set; } = 0f;

        public GameObject LoadingIndicator;

        float _timerStartAt = 0f;

        static ViewStatus _state = ViewStatus.Idle;
        static string _errMsg = string.Empty;
        static float _thisFrameSec = 0f;
        static float _audioTimeNoOffset = 0f;

        float _playbackSpeed = 1f;

        readonly SimaiParser SIMAI_PARSER = SimaiParser.Shared;
        readonly string CACHE_PATH = Path.Combine(MajEnv.CachePath, "View");

        // Assets
        static AudioSampleWrap? _audioSample = null;
        static Sprite? _bgCover = null;
        static string? _videoPath = null;
        //WsServer _httpServer;
        GameSetting _setting = MajInstances.Setting;
        NoteLoader _noteLoader;
        NoteManager _noteManager;
        NoteAudioManager _noteAudioManager;
        NotePoolManager _notePoolManager;
        BGManager _bgManager;
        TimeDisplayer _timeDisplayer;
        ChartAnalyzer _chartAnalyzer;

        SimaiChart? _chart;
        

        static MajTimer _timer = MajTimeline.CreateTimer();
        
        protected override void Awake()
        {
            base.Awake();
            if(!Directory.Exists(CACHE_PATH))
            {
                Directory.CreateDirectory(CACHE_PATH);
            }
            Majdata<ViewManager>.Instance = this;
            Majdata<INoteController>.Instance = this;
            Majdata<INoteTimeProvider>.Instance = this;
            //PlayerSettings.resizableWindow = true;
            //Screen.SetResolution(1920, 1080, false);
        }
        void Start()
        {
            _bgManager = Majdata<BGManager>.Instance!;
            //_httpServer = Majdata<WsServer>.Instance!;
            _noteLoader = Majdata<NoteLoader>.Instance!;
            _noteManager = Majdata<NoteManager>.Instance!;
            _noteAudioManager = Majdata<NoteAudioManager>.Instance!;
            _notePoolManager = Majdata<NotePoolManager>.Instance!;
            _timeDisplayer = Majdata<TimeDisplayer>.Instance!;
            _chartAnalyzer = Majdata<ChartAnalyzer>.Instance!;

            if (!string.IsNullOrEmpty(_videoPath))
            {
                _bgManager.SetBackgroundMovie(_videoPath, _bgCover).AsTask().Wait();
            }
            else if (_bgCover is not null)
            {
                _bgManager.DisableVideo();
                _bgManager.SetBackgroundPic(_bgCover);
            }
        }
        void Update()
        {
            switch (_state)
            {
                case ViewStatus.Playing:
                    
                    var elasped = _timer.UnscaledElapsedSecondsAsFloat;
                    _thisFrameSec = (elasped - _timerStartAt - Offset / _playbackSpeed) * _playbackSpeed;
                    if (_audioSample != null)
                    {
                        _audioTimeNoOffset = (float)_audioSample.CurrentSec * _playbackSpeed;
                        if (!_audioSample.IsPlaying)
                            StopAsync().Forget();
                    }

                    _timeDisplayer.OnPreUpdate();
                    _noteAudioManager.OnPreUpdate();
                    _noteManager.OnPreUpdate();
                    _notePoolManager.OnPreUpdate();
                    
                    _noteManager.OnUpdate();
                    break;
            }
        }
        void LateUpdate()
        {
            switch (_state)
            {
                case ViewStatus.Playing:
                    _noteAudioManager.OnLateUpdate();
                    _noteManager.OnLateUpdate();
                    Majdata<ObjectCounter>.Instance?.OnLateUpdate();
                    break;
            }
        }

        void FixedUpdate()
        {
            switch (_state)
            {
                case ViewStatus.Busy:
                    if(!LoadingIndicator.activeSelf)
                        LoadingIndicator.SetActive(true);
                    break;
                default:
                    if (LoadingIndicator.activeSelf)
                        LoadingIndicator.SetActive(false);
                    break;
            }
        }
        internal async UniTask<bool> PlayAsync()
        {
            return await PlayAsync(_playbackSpeed);
        }
        internal async UniTask<bool> PlayAsync(float speed)
        {
            switch(_state)
            {
                case ViewStatus.Ready:
                case ViewStatus.Paused:
                    break;
                default:
                    return false;
            }
            try
            {
                while (_state is ViewStatus.Busy)
                    await UniTask.Yield();
                _state = ViewStatus.Busy;
                _playbackSpeed = speed;
                await UniTask.SwitchToMainThread();
                await UniTask.Yield();
                _timerStartAt = _timer.UnscaledElapsedSecondsAsFloat - (float)_audioSample!.CurrentSec;
                _state = ViewStatus.Playing;
                _audioTimeNoOffset = (float)_audioSample!.CurrentSec;
                _audioSample.Speed = speed;
                _audioSample!.Play();
                _bgManager.PlayVideo(_audioTimeNoOffset, speed);
                await UniTask.SwitchToThreadPool();
                return true;
            }
            catch (Exception ex)
            {
                MajDebug.LogException(ex);
                _state = ViewStatus.Error;
                throw;
            }
        }
        internal async UniTask<bool> PauseAsync()
        {
            switch (_state)
            {
                case ViewStatus.Playing:
                    break;
                default:
                    return false;
            }
            try
            {
                while (_state is ViewStatus.Busy)
                    await UniTask.Yield();
                _state = ViewStatus.Busy;
                await UniTask.Yield();
                _audioSample!.Pause();
                _bgManager.PauseVideo();
                _thisFrameSec = (float)_audioSample.CurrentSec;
                _state = ViewStatus.Paused;
                return true;
            }
            catch(Exception ex) 
            {
                    MajDebug.LogException(ex);
                    _state = ViewStatus.Error;
                throw;
            }
        }
        internal async UniTask<bool> StopAsync()
        {
            switch (_state)
            {
                case ViewStatus.Playing:
                case ViewStatus.Paused:
                    break;
                default:
                    return false;
            }
            try
            {
                while (_state is ViewStatus.Busy)
                    await UniTask.Yield();
                _state = ViewStatus.Busy;
                await UniTask.Yield();
                _audioSample!.Stop();
                _thisFrameSec = 0;
                ClearAll();
                _bgManager.StopVideo();
                _bgManager.SetBackgroundPic(null);
                _state = ViewStatus.Loaded;
                return true;
            }
            catch(Exception ex)
            {
                MajDebug.LogException(ex);
                _state = ViewStatus.Error;
                throw;
            }
        }
        void ClearAll()
        {
            _noteLoader.Clear();
            _noteManager.Clear();
            _notePoolManager.Clear();
            _noteAudioManager.Clear();
            Majdata<NoteEffectPool>.Instance?.Reset();
            Majdata<ObjectCounter>.Instance?.Clear();
            Majdata<MultTouchHandler>.Instance?.Clear();
        }
        internal async UniTask<bool> ResetAsync()
        {
            switch(_state)
            {
                case ViewStatus.Idle:
                    return false;
            }
            try
            {
                while(_state is ViewStatus.Busy)
                    await UniTask.Yield();
                _state = ViewStatus.Busy;
                await UniTask.Yield();
                _thisFrameSec = 0;
                if (_audioSample is not null)
                    _audioSample.Dispose();
                _bgManager.CancelTimeRef();
                await SceneManager.LoadSceneAsync("View");
                return true;
            }
            finally
            {
                _state = ViewStatus.Idle;
            }
        }
        internal async Task LoadAssests(string audioPath, string bgPath, string? pvPath)
        {
            while (_state is ViewStatus.Busy)
                await UniTask.Yield();
            _state = ViewStatus.Busy;
            try
            {
                if(_audioSample is not null) _audioSample.Dispose();
                var sample = await AudioManager.LoadMusicAsync(audioPath, true);
                if (File.Exists(bgPath))
                {
                    var cover = await SpriteLoader.LoadAsync(bgPath);
                    _bgCover = cover;
                }
                else
                {
                    _bgCover = null;
                }
                _videoPath = pvPath;
                _audioSample = sample;
                _state = ViewStatus.Loaded;
            }
            catch (Exception ex)
            {
                MajDebug.LogException(ex);
                _errMsg = ex.ToString();
                _state = ViewStatus.Error;
                throw;
            }
        }
        /*internal async Task LoadAssests(byte[] audio, byte[] bg, byte[]? pv)
        {
            while (_state is ViewStatus.Busy)
                await UniTask.Yield();
            _state = ViewStatus.Busy;
            try
            {
                var sampleCachePath = Path.Combine(CACHE_PATH, "track.majplay");
                var videoCachePath = Path.Combine(CACHE_PATH, "video.majplay");
                await File.WriteAllBytesAsync(sampleCachePath, audio);

                var sample = await MajInstances.AudioManager.LoadMusicAsync(sampleCachePath, true);
                if (pv is null || pv.Length == 0)
                {
                    var cover = await SpriteLoader.LoadAsync(bg);
                    _bgCover = cover;
                    _videoPath = string.Empty;
                }
                else
                {
                    await File.WriteAllBytesAsync(videoCachePath, pv);
                    _videoPath = videoCachePath;
                }
                _audioSample = sample;

                _state = ViewStatus.Loaded;
            }
            catch (Exception ex)
            {
                MajDebug.LogException(ex);
                _errMsg = ex.ToString();
                _state = ViewStatus.Error;
                throw;
            }
        }*/
        internal async Task ParseAndLoadChartAsync(double startAt, string fumen)
        {
            while (_state is ViewStatus.Busy)
                await UniTask.Yield();
            _state = ViewStatus.Busy;
            try
            {
                _chart = await SIMAI_PARSER.ParseChartAsync(string.Empty, string.Empty, fumen);
                AudioLength = (float)_audioSample!.Length.TotalSeconds;
                await _chartAnalyzer.AnalyzeAndDrawGraphAsync(_chart, (float)_audioSample.Length.TotalSeconds);
                var range = new Range<double>(startAt-Offset, double.MaxValue);
                _chart.Clamp(range);
                await UniTask.SwitchToMainThread();

                var tapSpeed = Math.Abs(_setting.Game.TapSpeed);

                if (_setting.Game.TapSpeed < 0)
                    _noteLoader.NoteSpeed = -((float)(107.25 / (71.4184491 * Mathf.Pow(tapSpeed + 0.9975f, -0.985558604f))));
                else
                    _noteLoader.NoteSpeed = ((float)(107.25 / (71.4184491 * Mathf.Pow(tapSpeed + 0.9975f, -0.985558604f))));
                _noteLoader.TouchSpeed = _setting.Game.TouchSpeed;

                await _noteLoader.LoadNotesIntoPoolAsync(_chart);
                _noteManager.InitializeUpdater();
                if (_videoPath.IsNullOrEmpty())
                {
                    _bgManager.DisableVideo();
                    _bgManager.SetBackgroundPic(_bgCover);
                }
                else
                {
                    await _bgManager.SetBackgroundMovie(_videoPath, _bgCover);
                }
                _audioSample!.CurrentSec = startAt;
                await _noteAudioManager.GenerateAnswerSFX(_chart, false, 0);
                await UniTask.SwitchToThreadPool();
                _state = ViewStatus.Ready;
            }
            catch (Exception ex)
            {
                MajDebug.LogException(ex);
                _errMsg = ex.ToString();
                _state = ViewStatus.Error;
                throw;
            }
        }
        void OnDestroy()
        {
            Majdata<ViewManager>.Free();
            Majdata<INoteController>.Free();
            Majdata<INoteTimeProvider>.Free();
        }
    }
}
