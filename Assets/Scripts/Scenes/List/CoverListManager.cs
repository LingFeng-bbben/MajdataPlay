using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.Collections;
using MajdataPlay.Editor;
using MajdataPlay.IO;
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
        int _songListCursor = 0;
        float _songListDesiredPos = 0;

        float _preloadCooldownTimer = 0.5f;
        bool _isNeedPreload = false;
        bool _isEmptyCollection = true;

        ListManager _listManager;
        PreviewSoundPlayer _previewSoundPlayer;

        SongCollection _currentCollection = SongCollection.Empty("Empty");

        SongCoverDisplayer[] _songCoverDisplayers = Array.Empty<SongCoverDisplayer>();
        ThumbnailDisplayer[] _songThumbnailDisplayers = Array.Empty<ThumbnailDisplayer>();

        readonly RentedList<SongDetailBinding> _songDetailBindings = new();
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
                _songListCursor = 0;
                for (var i = 0; i < _songCount; i++)
                {
                    var songDetail = _currentCollection[i];
                    var coverBinding = new SongDetailBinding()
                    {
                        SongDetail = songDetail
                    };
                    var thumbnailBinding = new SongThumbnailBinding()
                    {
                        SongDetail = songDetail
                    }; 
                    _songDetailBindings.Add(coverBinding);
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
        void Update()
        {

        }
        void Clear()
        {
            SelectedSong = null;
            _songDetailBindings.Clear();
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
        //void SongCoverUpdate(Memory<SongDetailBinding> bindingsMemory)
        //{
        //    var bindings = bindingsMemory.Span;
        //    for (int i = 0; i < bindingsMemory.Length; i++)
        //    {
        //        var distance = i - listPosReal;
        //        var absDistance = Mathf.Abs(distance);
        //        ref var binding = ref bindings[i];
        //        SongCoverSmallDisplayer cover;

        //        if (absDistance > 5)
        //        {
        //            if(binding.Displayer is not null)
        //            {
        //                cover = binding.Displayer;
        //                binding.Displayer = null;
        //                cover.gameObject.SetActive(false);
        //                _idleSongCoverDisplayer.Enqueue(cover);
        //            }
        //            continue;
        //        }
        //        else
        //        {
        //            if(binding.Displayer is null)
        //            {
        //                if(_idleSongCoverDisplayer.TryDequeue(out cover))
        //                {
        //                    binding.Displayer = cover;
        //                    cover.gameObject.SetActive(true);
        //                    cover.SetSongDetail(binding.SongDetail);
        //                }
        //                else
        //                {
        //                    continue;
        //                }
        //            }
        //            else
        //            {
        //                cover = binding.Displayer;
        //            }
        //        }

        //        if (absDistance > 4)
        //        {
        //            cover.SetOpacity(-Mathf.Abs(distance) + 5);
        //        }
        //        else
        //        {
        //            cover.SetOpacity(1f);
        //        }

        //        cover.RectTransform.anchoredPosition = GetCoverPosition(radius, (distance * angle - 90) * Mathf.Deg2Rad);
        //    }
        //}
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
        
        struct SongDetailBinding
        {
            public ISongDetail SongDetail { get; set; }
            public SongCoverDisplayer? Displayer { get; set; }
            public ValueTask? PreloadTask { get; set; }


            public SongDetailBinding(ISongDetail songDetail, SongCoverDisplayer? displayer)
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
