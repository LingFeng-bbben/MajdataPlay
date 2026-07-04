using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.Collections;
using MajdataPlay.Editor;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Scenes.Game;
using MajdataPlay.Scenes.List.Models;
using MajdataPlay.Settings.Runtime;
using MajdataPlay.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
#nullable enable
namespace MajdataPlay.Scenes.List
{
    public class CoverListManager : MonoBehaviour
    {
        public ISongDetail? SelectedSong { get; private set; } = null;
        public float PreloadCooldownTimer
        {
            get
            {
                return _preloadCooldownTimer;
            }
        }

        [SerializeField]
        [FormerlySerializedAs("songCoverDisplayerPrefab")]
        GameObject _songCoverDisplayerPrefab;

        [SerializeField]
        [FormerlySerializedAs("centerCoverDisplayer")]
        CoverBigDisplayer _centerCoverDisplayer;

        [SerializeField]
        [FormerlySerializedAs("songCoverListRoot")]
        GameObject _songCoverListRoot;

        [SerializeField]
        [FormerlySerializedAs("thumbnailListRoot")]
        GameObject _thumbnailListRoot;

        [SerializeField]
        [FormerlySerializedAs("progressDisplayer")]
        TextMeshProUGUI _progressDisplayer;

        [SerializeField]
        [ReadOnlyField]
        int _selectedDifficulty = 0;

        int _songCount = 0;

        // List cursor position and desired position
        int _listDesiredPos = 0;
        float _listCursorPos = 0;
        float _listCursorPosStepPerSec = 0f;

        float _preloadCooldownTimer = 0.5f;
        bool _isNeedPreload = false;
        bool _isEmptyCollection = true;

        ListManager _listManager;
        PreviewSoundPlayer _previewSoundPlayer;

        SongCollection _currentCollection = SongCollection.Empty("Empty");

        SongCoverDisplayer[] _songCoverDisplayers = Array.Empty<SongCoverDisplayer>();
        ThumbnailDisplayer[] _songThumbnailDisplayers = Array.Empty<ThumbnailDisplayer>();

        readonly RentedList<SongCoverBinding> _songCoverBindings = new();
        readonly RentedList<SongThumbnailBinding> _songThumbnailBindings = new();

        readonly Queue<SongCoverDisplayer> _idleSongCoverDisplayer = new();
        readonly Queue<ThumbnailDisplayer> _idleSongThumbnailDisplayer = new();

        readonly ListConfig _listConfig = MajEnv.RuntimeConfig?.List ?? new();

        private void Awake()
        {
            Majdata<CoverListManager>.Instance = this;
            _previewSoundPlayer = GetComponent<PreviewSoundPlayer>();

            var songCoverListRoot = _songCoverListRoot.transform;
            var coverDisplayerCount = songCoverListRoot.childCount;
            _songCoverDisplayers = new SongCoverDisplayer[coverDisplayerCount];
            for (var i = 0; i < coverDisplayerCount; i++)
            {
                var displayerTransform = songCoverListRoot.GetChild(i);
                var displayer = displayerTransform.GetComponent<SongCoverDisplayer>();
                if (displayer is null)
                {
                    throw new InvalidOperationException($"Child {i} of {_songCoverListRoot.name} does not have a SongCoverDisplayer component.");
                }
                _songCoverDisplayers[i] = displayer;
                _idleSongCoverDisplayer.Enqueue(displayer);
            }

            var thumbnailListRoot = _thumbnailListRoot.transform;
            var thumbnailDisplayerCount = thumbnailListRoot.childCount;
            _songThumbnailDisplayers = new ThumbnailDisplayer[thumbnailDisplayerCount];
            for (var i = 0; i < thumbnailDisplayerCount; i++)
            {
                var displayerTransform = thumbnailListRoot.GetChild(i);
                var displayer = displayerTransform.GetComponent<ThumbnailDisplayer>();
                if (displayer is null)
                {
                    throw new InvalidOperationException($"Child {i} of {_thumbnailListRoot.name} does not have a ThumbnailDisplayer component.");
                }
                _songThumbnailDisplayers[i] = displayer;
                _idleSongThumbnailDisplayer.Enqueue(displayer);
            }
        }
        void Start()
        {
            _listManager = Majdata<ListManager>.Instance!;
        }
        void OnDestroy()
        {
            Majdata<CoverListManager>.Free();
            
            _idleSongCoverDisplayer.Clear();
        }
        internal void SetCollection(SongCollection collection)
        {
            if(collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            if(collection == _currentCollection)
            {
                return;
            }
            Clear();
            if (!collection.IsEmpty)
            {
                collection.Index = 0;
                _currentCollection = collection;
                _isEmptyCollection = false;                
                
                _songCount = _currentCollection.Count;
                _listCursorPos = 0;
                for (var i = 0; i < _songCount; i++)
                {
                    var songDetail = _currentCollection[i];
                    var coverBinding = new SongCoverBinding()
                    {
                        SongDetail = songDetail
                    };
                    var thumbnailBinding = new SongThumbnailBinding()
                    {
                        SongDetail = songDetail
                    }; 
                    _songCoverBindings.Add(coverBinding);
                    _songThumbnailBindings.Add(thumbnailBinding);
                }
            }
            else
            {
                _isEmptyCollection = true;
            }
        }

        public void SlideList(int delta)
        {

        }
        public void SlideListToTop()
        {
            SlideList(int.MinValue / 2);
        }
        public void SlideListToTail()
        {
            SlideList(int.MaxValue / 2);
        }
        internal void OnUpdate()
        {
            UpdateDisplayerBinding();
            UpdateDisplayerPosition();
        }
        void Clear()
        {
            SelectedSong = null;
            _songCount = 0;
            _songCoverBindings.Clear();
            _songThumbnailBindings.Clear();
            for (var i = 0; i < _songCoverDisplayers.Length; i++)
            {
                var displayer = _songCoverDisplayers[i];
                displayer.gameObject.SetActive(false);
                _idleSongCoverDisplayer.Enqueue(displayer);
            }
            for (var i = 0; i < _songThumbnailDisplayers.Length; i++)
            {
                var displayer = _songThumbnailDisplayers[i];
                displayer.gameObject.SetActive(false);
                _idleSongThumbnailDisplayer.Enqueue(displayer);
            }
        }
        void UpdateDisplayerBinding()
        {
            if(_isEmptyCollection)
            {
                return;
            }
            var songCoverBindings = _songCoverBindings.AsSpan();
            var songThumbnailBindings = _songThumbnailBindings.AsSpan();
            var currentListCursorPos = (int)_listCursorPos;
            for (var i = 0; i < _songCount; i++)
            {
                ref var coverBinding = ref songCoverBindings[i];
                ref var thumbnailBinding = ref songThumbnailBindings[i];
                var absDistance = Math.Abs(i - currentListCursorPos);

                // Update song cover binding
                var coverDisplayer = coverBinding.Displayer;
                if (absDistance > 3)
                {
                    if(coverDisplayer is not null)
                    {
                        coverBinding.Displayer = null;
                        coverDisplayer.SetActive(false);
                        _idleSongCoverDisplayer.Enqueue(coverDisplayer);
                    }                    
                }
                else
                {
                    if (coverDisplayer is null)
                    {
                        if (_idleSongCoverDisplayer.TryDequeue(out coverDisplayer))
                        {
                            coverBinding.Displayer = coverDisplayer;
                            coverDisplayer.SetSongDetail(coverBinding.SongDetail);
                            coverDisplayer.SetActive(true);
                        }
                        else
                        {
                            MajDebug.LogWarning("No idle song cover displayer available.");
                        }                            
                    }                    
                }

                // Update thumbnail binding
                var thumbnailDisplayer = thumbnailBinding.Displayer;
                if (absDistance > 6)
                {
                    if (thumbnailDisplayer is not null)
                    {
                        thumbnailBinding.Displayer = null;
                        thumbnailDisplayer.SetActive(false);
                        _idleSongThumbnailDisplayer.Enqueue(thumbnailDisplayer);
                    }
                }
                else
                {
                    if (thumbnailDisplayer is null)
                    {
                        if (!_idleSongThumbnailDisplayer.TryDequeue(out thumbnailDisplayer))
                        {
                            thumbnailBinding.Displayer = thumbnailDisplayer;
                            thumbnailDisplayer.SetSongDetail(thumbnailBinding.SongDetail);
                            thumbnailDisplayer.SetActive(true);
                        }
                        else
                        {
                            MajDebug.LogWarning("No idle song thumbnail displayer available.");
                        }                            
                    }
                }
            }
        }
        void UpdateDisplayerPosition()
        {
            if (_listCursorPos == _listDesiredPos)
            {
                return;
            }
            else
            {
                _listCursorPos += _listCursorPosStepPerSec * MajTimeline.DeltaTime;
                _listCursorPos = Mathf.Min(_listCursorPos, _listDesiredPos);
            }
            var songCoverBindings = _songCoverBindings.AsSpan();
            var songThumbnailBindings = _songThumbnailBindings.AsSpan();
            for (var i = 0; i < _songCount; i++)
            {
                ref var coverBinding = ref songCoverBindings[i];
                ref var thumbnailBinding = ref songThumbnailBindings[i];
                var delta = i - _listCursorPos;

                // Update song cover position
                var coverDisplayer = coverBinding.Displayer;
                if (coverDisplayer is not null)
                {
                    coverDisplayer.RectTransform.anchoredPosition = GetCoverDisplayerPositionFromDelta(delta);
                }

                // Update thumbnail position
                var thumbnailDisplayer = thumbnailBinding.Displayer;
                if (thumbnailDisplayer is not null)
                {
                    thumbnailDisplayer.RectTransform.anchoredPosition = GetThumbnailDisplayerPositionFromDelta(delta);
                }
            }
        }

        internal void SetCursor(ISongDetail songDetail)
        {
            var pos = SongStorage.CollectionIndex;
            SongStorage.WorkingCollection.SetCursor(songDetail);
            _currentCollection.SetCursor(songDetail);
            _listConfig.SelectedSongIndex = SongStorage.WorkingCollection.Index;
            _listConfig.SelectedSongHash = songDetail.Hash;
        }
        //async Task AnalyzeAndUpdateBpmLedAsync(ISongDetail songDetail, ChartLevel level)
        //{
        //    await chartAnalyzer.AnalyzeAndDrawGraphAsync(songDetail, level);
        //    if (!IsChartList || !ReferenceEquals(_currentCollection.Current, songDetail) || _listConfig.SelectedDiff != level)
        //    {
        //        return;
        //    }

        //    var bpm = chartAnalyzer.LastAnalyzeBpm;
        //    if (chartAnalyzer.LastAnalyzeIsEmpty)
        //    {
        //        CabinetLed.SetButtonLight(Color.red, 3);
        //        CabinetLed.SetCabinetLight(1.0f);
        //        return;
        //    }
        //    CabinetLed.SetButtonLight(Color.green, 3);
        //    CabinetLed.SetCabinetLight(1.0f);
        //    while (IsChartList &&
        //           ReferenceEquals(_currentCollection.Current, songDetail) &&
        //           _listConfig.SelectedDiff == level &&
        //           _previewSoundPlayer.IsPreviewPending(songDetail) &&
        //           !_previewSoundPlayer.IsPreviewPlaying(songDetail))
        //    {
        //        await UniTask.Yield();
        //    }
        //    if (!IsChartList ||
        //        !ReferenceEquals(_currentCollection.Current, songDetail) ||
        //        _listConfig.SelectedDiff != level ||
        //        !_previewSoundPlayer.IsPreviewPlaying(songDetail))
        //    {
        //        return;
        //    }
        //    if (bpm <= 0f)
        //    {
        //        CabinetLed.SetButtonLight(Color.green, 3);
        //        CabinetLed.SetCabinetLight(1.0f);
        //        return;
        //    }

        //    var halfNoteMs = 120000f / bpm;
        //    CabinetLed.SetSineFunc(3, Color.green, (long)halfNoteMs);
        //    CabinetLed.SetCabinetLightSineFunc(1.0f, (long)(halfNoteMs * 2));
        //}
        //void RefreshSongCoverBindings()
        //{
        //    var bindings = _songDetailBindings.Span;
        //    for (int i = 0; i < bindings.Length; i++)
        //    {
        //        ref var binding = ref bindings[i];
        //        if (binding.Displayer is not null)
        //        {
        //            var cover = binding.Displayer;
        //            binding.Displayer = null;
        //            cover.gameObject.SetActive(false);
        //            _idleSongCoverDisplayer.Enqueue(cover);
        //        }
        //        binding.SongDetail = _currentCollection[i];
        //    }
        //}

        Vector2 GetCoverDisplayerPositionFromDelta(float delta)
        {
            const int X_POS_STEP = 169;
            const int X_POS_WITH_DELTA_1 = 264;

            var absDelta = Mathf.Abs(delta);
            if (delta == 0)
            {
                return Vector2.zero;
            }
            else if (absDelta.InRange(0, 1))
            {
                return new Vector2(X_POS_WITH_DELTA_1 * absDelta * Mathf.Sign(delta), 0);
            }
            else
            {
                var index = (int)absDelta;
                var posStartAt = X_POS_WITH_DELTA_1 * index;
                var middle = X_POS_STEP * Mathf.Floor(absDelta);

                return new Vector2((posStartAt + middle) * Mathf.Sign(delta), 0);
            }
        }
        Vector2 GetThumbnailDisplayerPositionFromDelta(float delta)
        {
            const int X_POS_STEP = 80;
            const int X_POS_WITH_DELTA_1 = 203;
            
            var absDelta = Mathf.Abs(delta);
            if (delta == 0)
            {
                return Vector2.zero;
            }
            else if (absDelta.InRange(0, 1))
            {
                return new Vector2(X_POS_WITH_DELTA_1 * absDelta * Mathf.Sign(delta), 0);
            }
            else
            {
                var index = (int)absDelta;
                var posStartAt = X_POS_WITH_DELTA_1 * index;
                var middle = X_POS_STEP * Mathf.Floor(absDelta);

                return new Vector2((posStartAt + middle) * Mathf.Sign(delta), 0);
            }
        }
        struct SongCoverBinding
        {
            public ISongDetail SongDetail { get; set; }
            public SongCoverDisplayer? Displayer { get; set; }
            public ValueTask? PreloadTask { get; set; }


            public SongCoverBinding(ISongDetail songDetail, SongCoverDisplayer? displayer)
            {
                SongDetail = songDetail;
                Displayer = displayer;
                PreloadTask = null;
            }
            public void PreloadAsync()
            {
                if(PreloadTask is ValueTask task)
                {
                    if(!task.IsCompleted || task.IsCompletedSuccessfully)
                    {
                        return;
                    }
                }
                var preloadTask = SongDetail.PreloadAsync();
                if(!preloadTask.IsCompleted)
                {
                    ListManager.AllBackgroundTasks.Add(preloadTask.AsTask());
                }
                PreloadTask = preloadTask;
            }
        }
        struct SongThumbnailBinding
        {
            public ISongDetail SongDetail { get; init; }
            public ThumbnailDisplayer? Displayer { get; set; }
            public ValueTask? PreloadTask { get; set; }

            public SongThumbnailBinding(SongDetail songDetail, ThumbnailDisplayer? displayer)
            {
                SongDetail = songDetail;
                Displayer = displayer;
            }
        }
    }
}
