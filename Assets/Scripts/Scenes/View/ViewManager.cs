using Cysharp.Threading.Tasks;
using MajdataPlay.Game;
using MajdataPlay.IO;
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
                    Timeline = _timeline,
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
        public float ThisFrameSec { get; private set; }
        public float ThisFixedUpdateSec { get; private set; }


        static ViewStatus _state = ViewStatus.Idle;
        static string _errMsg = string.Empty;
        static float _timeline = 0f;

        readonly SimaiParser SIMAI_PARSER = SimaiParser.Shared;
        readonly string CACHE_PATH = Path.Combine(MajEnv.CachePath, "View");
        string? _videoPath = null;

        HttpServer _httpServer;
        NoteLoader _noteLoader;

        SimaiChart? _chart;
        Sprite _bgCover = MajEnv.EmptySongCover;
        AudioSampleWrap? _audioSample = null;
        
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
            _httpServer = MajInstanceHelper<HttpServer>.Instance!;
            _noteLoader = MajInstanceHelper<NoteLoader>.Instance!;
        }
        internal void PlaybackRequestHandle(EditorRequest request)
        {

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
        internal async Task ParseAndLoadChartAsync()
        {
            _state = ViewStatus.Busy;
            try
            {
                _chart = await SIMAI_PARSER.ParseChartAsync(string.Empty, string.Empty, string.Empty);
                await _noteLoader.LoadNotesIntoPool(_chart);

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
