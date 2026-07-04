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
using UnityEngine;
using UnityEngine.Serialization;
#nullable enable
namespace MajdataPlay.Scenes.List
{
    public class CoverListDisplayer : MonoBehaviour
    {
        const int POOL_SONG_COVER_CAPACITY = 8;
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

        readonly RentedList<SongDetailBinding> _songDetailBindings = new();
        readonly Queue<SongCoverDisplayer> _allocatedSongCoverDisplayer = new(POOL_SONG_COVER_CAPACITY);
        readonly Queue<SongCoverDisplayer> _idleSongCoverDisplayer = new(POOL_SONG_COVER_CAPACITY);

        readonly ListConfig _listConfig = MajEnv.RuntimeConfig?.List ?? new();

        private void Awake()
        {
            Majdata<CoverListDisplayer>.Instance = this;
            _previewSoundPlayer = GetComponent<PreviewSoundPlayer>();
            for (var i = 0; i < POOL_SONG_COVER_CAPACITY; i++)
            {
                var obj = Instantiate(_songCoverDisplayerPrefab, _songCoverListRoot.transform);
                var displayer = obj.GetComponent<SongCoverDisplayer>();
                displayer.gameObject.SetActive(false);
                _idleSongCoverDisplayer.Enqueue(displayer);
            }
        }
        void Start()
        {
            _listManager = Majdata<ListManager>.Instance!;
        }
        void OnDestroy()
        {
            Majdata<CoverListDisplayer>.Free();
            
            _idleSongCoverDisplayer.Clear();
        }
        internal void SetCollection(SongCollection collection)
        {
            if(collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            SelectedSong = null;
            if (!collection.IsEmpty)
            {
                collection.Index = 0;
                _currentCollection = collection;
                _isEmptyCollection = false;
                while (_allocatedSongCoverDisplayer.TryDequeue(out var displayer))
                {
                    displayer.gameObject.SetActive(false);
                    _idleSongCoverDisplayer.Enqueue(displayer);
                }
                _songDetailBindings.Clear();
                _songCount = _currentCollection.Count;
                _songListCursor = 0;
                for (var i = 0; i < _songCount; i++)
                {
                    var songDetail = _currentCollection[i];
                    var binding = GetSongDetailBinding(songDetail);
                    _songDetailBindings.Add(binding);
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
        Vector3 GetCoverPosition(float radius, float position)
        {
            return new Vector3(radius * Mathf.Sin(position), radius * Mathf.Cos(position));
        }
        SongDetailBinding GetSongDetailBinding(ISongDetail songDetail)
        {
            return new SongDetailBinding()
            {
                SongDetail = songDetail
            };
        }
        
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
    }
}
