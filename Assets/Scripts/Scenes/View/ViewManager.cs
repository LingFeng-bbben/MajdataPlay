using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Game;
using MajdataPlay.IO;
using MajdataPlay.Timer;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using MajdataPlay.View.Types;
using MajSimai;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        public bool IsStart { get; private set; }
        public bool IsAutoplay => AutoplayMode != AutoplayMode.Disable;
        public AutoplayMode AutoplayMode { get; private set; } = AutoplayMode.Enable;
        public JudgeGrade AutoplayGrade { get; private set; } = JudgeGrade.Perfect;
        public JudgeStyleType JudgeStyle { get; private set; } = JudgeStyleType.DEFAULT;
        public Material BreakMaterial { get; } = MajEnv.BreakMaterial;
        public Material DefaultMaterial { get; } = MajEnv.DefaultMaterial;
        public Material HoldShineMaterial { get; } = MajEnv.HoldShineMaterial;
        public float ThisFrameSec => _thisFrameSec;
        public float ThisFixedUpdateSec => _thisFrameSec;

        float _timerStartAt = 0f;

        static ViewStatus _state = ViewStatus.Idle;
        static string _errMsg = string.Empty;
        static float _thisFrameSec = 0f;

        readonly SimaiParser SIMAI_PARSER = SimaiParser.Shared;
        readonly string CACHE_PATH = Path.Combine(MajEnv.CachePath, "View");
        string? _videoPath = null;

        HttpServer _httpServer;
        NoteLoader _noteLoader;
        BGManager _bgManager;

        SimaiChart? _chart;
        Sprite _bgCover = MajEnv.EmptySongCover;
        AudioSampleWrap? _audioSample = null;

        static MajTimer _timer = MajTimeline.CreateTimer();
        
        protected override void Awake()
        {
            base.Awake();
            if(!Directory.Exists(CACHE_PATH))
            {
                Directory.CreateDirectory(CACHE_PATH);
            }
            MajInstanceHelper<ViewManager>.Instance = this;
            MajInstanceHelper<INoteController>.Instance = this;
            Screen.SetResolution(1920, 1080, false);
        }
        void Start()
        {
            _bgManager = MajInstanceHelper<BGManager>.Instance!;
            _httpServer = MajInstanceHelper<HttpServer>.Instance!;
            _noteLoader = MajInstanceHelper<NoteLoader>.Instance!;
        }
        void Update()
        {
            switch(_state)
            {
                case ViewStatus.Playing:
                    var elasped = _timer.UnscaledElapsedSecondsAsFloat;
                    _thisFrameSec += elasped - _timerStartAt;
                    _timerStartAt = elasped;
                    break;
            }
        }
        internal async UniTask<bool> Play(EditorRequest request)
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
                _state = ViewStatus.Busy;
                await UniTask.Yield();
                _timerStartAt = _timer.UnscaledElapsedSecondsAsFloat;
                _state = ViewStatus.Playing;
                _thisFrameSec = (float)_audioSample!.CurrentSec;
                _audioSample!.Play();
                if (_state == ViewStatus.Paused)
                    return true;
                return true;
            }
            catch
            {
                _state = ViewStatus.Error;
                throw;
            }
        }
        internal async UniTask<bool> Pause()
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
                _state = ViewStatus.Busy;
                await UniTask.Yield();
                _audioSample!.Pause();
                _thisFrameSec = (float)_audioSample.CurrentSec;
                _state = ViewStatus.Paused;
                return true;
            }
            catch
            {
                _state = ViewStatus.Error;
                throw;
            }
        }
        internal async UniTask<bool> Stop()
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
                _state = ViewStatus.Busy;
                await UniTask.Yield();
                _audioSample!.Stop();
                _thisFrameSec = 0;
                return true;
            }
            catch
            {
                _state = ViewStatus.Error;
                throw;
            }
        }
        internal async UniTask<bool> Reset()
        {
            switch(_state)
            {
                case ViewStatus.Idle:
                    return false;
            }
            try
            {
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
        internal async Task LoadAssests(byte[] audioTrack, byte[] bg, byte[]? pv)
        {
            _state = ViewStatus.Busy;
            try
            {
                var audioTrackPath = Path.Combine(CACHE_PATH, "audioTrack.track");
                var videoPath = Path.Combine(CACHE_PATH, "bg.mp4");

                await File.WriteAllBytesAsync(audioTrackPath, audioTrack);

                var sample = await MajInstances.AudioManager.LoadMusicAsync(audioTrackPath);
                var cover = await SpriteLoader.LoadAsync(bg);

                if (pv is null || pv.Length == 0)
                {
                    _videoPath = string.Empty;
                }
                else
                {
                    await File.WriteAllBytesAsync(videoPath, pv);
                    _videoPath = videoPath;
                }
                _audioSample = sample;
                _bgCover = cover;
                _state = ViewStatus.Loaded;
            }
            catch (Exception ex)
            {
                _errMsg = ex.ToString();
                _state = ViewStatus.Error;
                throw;
            }
        }
        internal async Task ParseAndLoadChartAsync(double startAt)
        {
            _state = ViewStatus.Busy;
            try
            {
                _chart = await SIMAI_PARSER.ParseChartAsync(string.Empty, string.Empty, string.Empty);
                var range = new Range<double>(startAt, double.MaxValue);
                _chart.Clamp(range);
                await _noteLoader.LoadNotesIntoPool(_chart);
                if(_videoPath is null)
                {
                    _bgManager.SetBackgroundPic(_bgCover);
                }

                _state = ViewStatus.Ready;
            }
            catch (Exception ex)
            {
                _errMsg = ex.ToString();
                _state = ViewStatus.Error;
                throw;
            }
        }
        void OnDestroy()
        {
            MajInstanceHelper<ViewManager>.Free();
            MajInstanceHelper<INoteController>.Free();
        }
    }
}
