using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.Collections;
using MajdataPlay.Scenes.Game;
using MajdataPlay.Settings.Runtime;
using MajdataPlay.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.List
{
    public class CoverListDisplayer : MonoBehaviour
    {
        public bool IsDirList => Mode == CoverListMode.Directory;
        public bool IsChartList => Mode == CoverListMode.Chart;
        public CoverListMode Mode { get; set; } = CoverListMode.Directory;
        public ISongDetail SelectedSong
        {
            get
            {
                return _currentCollection.Current;
            }
        }
        public SongCollection SelectedCollection
        {
            get
            {
                return _currentCollection;
            }
        }
        public float PreloadCooldownTimer
        {
            get
            {
                return _preloadCooldownTimer;
            }
        }

        public string soundEffectName;
        public GameObject CoverSmallPrefab;
        public GameObject DirSmallPrefab;
        public GameObject DanSmallPrefab;
        public CoverBigDisplayer CoverBigDisplayer;
        public SubInfoDisplayer SubInfoDisplayer;
        public ChartAnalyzer chartAnalyzer;
        public FavoriteAdder FavoriteAdder;

        public int desiredListPos = 0;
        public float listPosReal;
        public float turnSpeed;
        public float radius;
        public float offset;
        public float angle;
        public bool DisableAnimation = false;

        public int selectedDifficulty = 0;

        private int coveri = 0;
        //SongCollection[] dirs = SongStorage.Collections;

        float _preloadCooldownTimer = 0.5f;
        bool _isNeedPreload = false;

        ListManager _listManager;
        PreviewSoundPlayer _previewSoundPlayer;

        Memory<SongDetailBinding> _songDetailBindings = Memory<SongDetailBinding>.Empty;
        Memory<SongCollectionBinding> _songCollectionBindings = Memory<SongCollectionBinding>.Empty;

        ReadOnlyMemory<SongCollection> _collections = ReadOnlyMemory<SongCollection>.Empty;
        SongCollection _currentCollection = SongCollection.Empty("Empty");

        ReadOnlyMemory<SongCoverSmallDisplayer> _allocatedSongCoverDisplayer = ReadOnlyMemory<SongCoverSmallDisplayer>.Empty;
        ReadOnlyMemory<FolderCoverSmallDisplayer> _allocatedFolderCoverDisplayer = ReadOnlyMemory<FolderCoverSmallDisplayer>.Empty;
        ReadOnlyMemory<FolderCoverSmallDisplayer> _allocatedDanCoverDisplayer = ReadOnlyMemory<FolderCoverSmallDisplayer>.Empty;

        SongDetailBinding[]? _rentSongDetailBindings = null;
        SongCollectionBinding[]? _rentSongCollectionBindings = null;

        static readonly Queue<SongCoverSmallDisplayer> _idleSongCoverDisplayer = new(16);
        static readonly Queue<FolderCoverSmallDisplayer> _idleFolderCoverDisplayer = new(16);
        static readonly Queue<FolderCoverSmallDisplayer> _idleDanCoverDisplayer = new(16);
        readonly ListConfig _listConfig = MajEnv.RuntimeConfig?.List ?? new();

        private void Awake()
        {
            Majdata<CoverListDisplayer>.Instance = this;
            _previewSoundPlayer = GetComponent<PreviewSoundPlayer>();
            var collections = SongStorage.Collections;
            var newCollections = new SongCollection[collections.Length];

            Parallel.For(0, collections.Length, i =>
            {
                var collection = collections[i];
                if (collection.Type == ChartStorageType.FavoriteList)
                {
                    newCollections[i] = collection;
                }
                else
                {
                    newCollections[i] = new SongCollection(collection.Name, collection.ToArray())
                    {
                        DanInfo = collection.DanInfo,
                        Type = collection.Type,
                        Location = collection.Location,
                    };
                }

                newCollections[i].Reset();
                if(!collection.IsEmpty)
                {
                    newCollections[i].SetCursor(collection.Current);
                }
                newCollections[i].SortAndFilter(SongStorage.OrderBy);
            });
            _collections = newCollections;
            _currentCollection = _collections.Span[SongStorage.CollectionIndex];

            var allocatedSongCoverDisplayer = new SongCoverSmallDisplayer[16];
            var allocatedFolderCoverDisplayer = new FolderCoverSmallDisplayer[16];
            var allocatedDanCoverDisplayer = new FolderCoverSmallDisplayer[16];
            
            for (var i = 0; i < 16; i++)
            {
                var obj = Instantiate(CoverSmallPrefab, transform);
                obj.SetActive(false);
                var coversmall = obj.GetComponent<SongCoverSmallDisplayer>();
                _idleSongCoverDisplayer.Enqueue(coversmall);
                allocatedSongCoverDisplayer[i] = coversmall;
            }
            for (var i = 0; i < 16; i++)
            {
                var obj = Instantiate(DirSmallPrefab, transform);
                obj.SetActive(false);
                var coversmall = obj.GetComponent<FolderCoverSmallDisplayer>();
                _idleFolderCoverDisplayer.Enqueue(coversmall);
                allocatedFolderCoverDisplayer[i] = coversmall;
            }
            for (var i = 0; i < 16; i++)
            {
                var obj = Instantiate(DanSmallPrefab, transform);
                obj.SetActive(false);
                var coversmall = obj.GetComponent<FolderCoverSmallDisplayer>();
                _idleDanCoverDisplayer.Enqueue(coversmall);
                allocatedDanCoverDisplayer[i] = coversmall;
            }

            _allocatedSongCoverDisplayer = allocatedSongCoverDisplayer;
            _allocatedFolderCoverDisplayer = allocatedFolderCoverDisplayer;
            _allocatedDanCoverDisplayer = allocatedDanCoverDisplayer;
        }
        void Start()
        {
            _listManager = Majdata<ListManager>.Instance!;
        }
        void OnDestroy()
        {
            Majdata<CoverListDisplayer>.Free();
            var collections = SongStorage.Collections;
            var thisCollections = _collections.Span;
            for (var i = 0; i < collections.Length; i++)
            {
                if(thisCollections[i].IsEmpty)
                {
                    continue;
                }
                collections[i].SetCursor(thisCollections[i].Current);
            }
            if(_rentSongDetailBindings is not null)
            {
                Pool<SongDetailBinding>.ReturnArray(_rentSongDetailBindings);
                _rentSongDetailBindings = null;
            }
            if(_rentSongCollectionBindings is not null)
            {
                Pool<SongCollectionBinding>.ReturnArray(_rentSongCollectionBindings);
                _rentSongCollectionBindings = null;
            }
            _idleDanCoverDisplayer.Clear();
            _idleFolderCoverDisplayer.Clear();
            _idleSongCoverDisplayer.Clear();
        }
        public void SwitchToDirList()
        {
            SwitchToDirListInternal();
        }
        public void SwitchToSongList()
        {
            SwitchToSongListInternal();
        }
        void SwitchToDirListInternal()
        {
            for (var i = 0; i < _songDetailBindings.Length; i++)
            {
                var songDetailbindings = _songDetailBindings.Span;
                ref var binding = ref songDetailbindings[i];
                if (binding.Displayer is not null)
                {
                    _idleSongCoverDisplayer.Enqueue(binding.Displayer);
                    binding.Displayer = null;
                }
            }
            foreach (var displayer in _allocatedSongCoverDisplayer.Span)
            {
                displayer.gameObject.SetActive(false);
            }
            _songDetailBindings = Memory<SongDetailBinding>.Empty;
            SubInfoDisplayer.Hide();
            FavoriteAdder.Hide();
            Mode = CoverListMode.Directory;
            desiredListPos = SongStorage.CollectionIndex;

            if (_rentSongDetailBindings is not null)
            {
                Pool<SongDetailBinding>.ReturnArray(_rentSongDetailBindings);
                _rentSongDetailBindings = null;
            }
            if (_rentSongCollectionBindings is not null)
            {
                Pool<SongCollectionBinding>.ReturnArray(_rentSongCollectionBindings);
                _rentSongCollectionBindings = null;
            }

            _rentSongCollectionBindings = Pool<SongCollectionBinding>.RentArray(_collections.Length);
            var rentBindings = _rentSongCollectionBindings;
            var bindings = rentBindings.AsSpan(0, _collections.Length);
            var collections = _collections.Span;
            for (var i = 0; i < bindings.Length; i++)
            {
                bindings[i] = GetSongCollectionBinding(collections[i]);
            }

            _songCollectionBindings = rentBindings.AsMemory(0, _collections.Length);
            if (desiredListPos > _songCollectionBindings.Length)
            {
                desiredListPos = 0;
            }
            listPosReal = desiredListPos;
            SlideListInternal(desiredListPos);
        }

        void SwitchToSongListInternal()
        {
            if (_currentCollection.Count == 0)
            {
                return;
            }
            if (_currentCollection.Type == ChartStorageType.Dan)
            {
                return;
            }

            for (var i = 0; i < _songCollectionBindings.Length; i++)
            {
                var collectionbindings = _songCollectionBindings.Span;
                ref var binding = ref collectionbindings[i];
                if(binding.Displayer is not null)
                {
                    binding.Displayer.gameObject.SetActive(false);
                    if (binding.Collection.Type == ChartStorageType.Dan)
                    {
                        _idleDanCoverDisplayer.Enqueue(binding.Displayer);
                    }
                    else
                    {
                        _idleFolderCoverDisplayer.Enqueue(binding.Displayer);
                    }
                    binding.Displayer = null;
                }
            }
            foreach (var displayer in _allocatedDanCoverDisplayer.Span)
            {
                displayer.gameObject.SetActive(false);
            }
            foreach (var displayer in _allocatedFolderCoverDisplayer.Span)
            {
                displayer.gameObject.SetActive(false);
            }
            _songCollectionBindings = Memory<SongCollectionBinding>.Empty;

            Mode = CoverListMode.Chart;
            desiredListPos = _currentCollection.Index;

            if (_rentSongDetailBindings is not null)
            {
                Pool<SongDetailBinding>.ReturnArray(_rentSongDetailBindings);
                _rentSongDetailBindings = null;
            }
            if (_rentSongCollectionBindings is not null)
            {
                Pool<SongCollectionBinding>.ReturnArray(_rentSongCollectionBindings);
                _rentSongCollectionBindings = null;
            }

            _rentSongDetailBindings = Pool<SongDetailBinding>.RentArray(_currentCollection.Count);
            var rentBindings = _rentSongDetailBindings;
            var bindings = rentBindings.AsSpan(0, _currentCollection.Count);
            for (var i = 0; i < bindings.Length; i++)
            {
                bindings[i] = GetSongDetailBinding(_currentCollection[i]);
            }
            _songDetailBindings = rentBindings.AsMemory(0, _currentCollection.Count);

            if (desiredListPos > _songDetailBindings.Length)
            {
                desiredListPos = 0;
            }
            listPosReal = desiredListPos;
            _preloadCooldownTimer = 0f;
            SlideListInternal(desiredListPos);
        }


        public void SlideDifficulty(int delta)
        {
            selectedDifficulty += delta;
            SlideToDifficulty(selectedDifficulty);
        }

        public void SlideToDifficulty(int pos)
        {
            selectedDifficulty = pos;
            if (selectedDifficulty > 6)
            {
                selectedDifficulty = 0;
            }
            if (selectedDifficulty < 0)
            {
                selectedDifficulty = 6;
            }
            _listConfig.SelectedDiff = (ChartLevel)selectedDifficulty;
            CoverBigDisplayer.SetDifficulty(selectedDifficulty);
            if (IsChartList)
            {
                var songinfo = _currentCollection[desiredListPos];
                var songScore = ScoreManager.GetScore(songinfo, _listConfig.SelectedDiff);
                CoverBigDisplayer.SetMeta(songinfo.Title, songinfo.Artist, songinfo.Designers[selectedDifficulty], songinfo.Levels[selectedDifficulty]);
                CoverBigDisplayer.SetScore(songScore);
                chartAnalyzer.AnalyzeAndDrawGraphAsync(songinfo, (ChartLevel)selectedDifficulty).Forget();
                FavoriteAdder.SetSong(songinfo);
                var allocatedSongCoverDisplayer = _allocatedSongCoverDisplayer.Span;
                for (int i = 0; i < allocatedSongCoverDisplayer.Length; i++)
                {
                    allocatedSongCoverDisplayer[i].SetLevel(selectedDifficulty);
                }
            }
        }

        public void SlideList(int delta)
        {
            switch(Mode)
            {
                case CoverListMode.Directory:
                    SongStorage.CollectionIndex += delta;
                    desiredListPos = SongStorage.CollectionIndex;
                    _listConfig.SelectedDir = SongStorage.CollectionIndex;
                    _listConfig.SelectedDirGuid = SongStorage.WorkingCollection.Id;
                    break;
                case CoverListMode.Chart:
                    var collection = _currentCollection;
                    collection.Move(delta);
                    var originPos = desiredListPos;
                    //desiredListPos = collection.Index;
                    desiredListPos += delta;
                    if(originPos != desiredListPos)
                    {
                        _isNeedPreload = true;
                        _preloadCooldownTimer = 0.5f;
                    }
                    _listConfig.SelectedSongIndex = collection.Index;
                    _listConfig.SelectedSongHash = collection.Current.Hash;
                    break;
            }
            SlideListInternal(desiredListPos);
        }
        public void SlideListToTop()
        {
            SlideList(int.MinValue / 2);
        }
        public void SlideListToTail()
        {
            SlideList(int.MaxValue / 2);
        }
        public void RandomSelect()
        {
            switch (Mode)
            {
                case CoverListMode.Directory:
                    var length = SongStorage.Collections.Length;
                    var rand = UnityEngine.Random.Range(0, length);
                    SlideListInternal(rand);
                    break;
                case CoverListMode.Chart:
                    var length1 = _currentCollection.Count;
                    var rand1 = UnityEngine.Random.Range(0, length1);
                    _currentCollection.Index = rand1;
                    SlideListInternal(rand1);
                    break;
            }
        }
        void SlideListInternal(int pos)
        {
            MajInstances.AudioManager.PlaySFX("tap_perfect.wav");
            var coverCount = IsDirList ? _songCollectionBindings.Length : _songDetailBindings.Length;

            desiredListPos = pos;
            if (desiredListPos >= coverCount)
            {
                desiredListPos = coverCount - 1;
            }
            if (desiredListPos <= 0)
            {
                desiredListPos = 0;
            }
            switch(Mode)
            {
                case CoverListMode.Directory:
                    _currentCollection = _collections.Span[desiredListPos];
                    SongStorage.CollectionIndex = desiredListPos;
                    switch(_currentCollection.Type)
                    {
                        case ChartStorageType.List:
                        case ChartStorageType.PlayList:
                        case ChartStorageType.FavoriteList:
                            CoverBigDisplayer.SetMeta(_currentCollection.Name, "Count:" + _currentCollection.Count, "", "");
                            break;
                        case ChartStorageType.Dan:
                            CoverBigDisplayer.SetMeta(_currentCollection.DanInfo!.Name, _currentCollection.DanInfo.Description, "", ""); ;
                            break;
                    }
                    CoverBigDisplayer.SetScore(new MaiScore());
                    SongStorage.CollectionIndex = desiredListPos;
                    break;
                case CoverListMode.Chart:
                    var songinfo = _songDetailBindings.Span[desiredListPos].SongDetail;
                    var songScore = ScoreManager.GetScore(songinfo, _listConfig.SelectedDiff);
                    CoverBigDisplayer.SetSongDetail(songinfo);
                    CoverBigDisplayer.SetMeta(songinfo.Title, songinfo.Artist, songinfo.Designers[selectedDifficulty], songinfo.Levels[selectedDifficulty]);
                    CoverBigDisplayer.SetScore(songScore);
                    SubInfoDisplayer.RefreshContent(songinfo);
                    _previewSoundPlayer.PlayPreviewSound(songinfo);
                    chartAnalyzer.AnalyzeAndDrawGraphAsync(songinfo, (ChartLevel)selectedDifficulty).Forget();
                    FavoriteAdder.SetSong(songinfo);
                    _currentCollection.Index = desiredListPos;
                    SongStorage.WorkingCollection.Index = desiredListPos;
                    break;
            }
        }
        void Update()
        {
            if(!_isNeedPreload)
            {
                return;
            }
            else if(_preloadCooldownTimer > 0f)
            {
                _preloadCooldownTimer -= MajTimeline.DeltaTime;
                return;
            }
            var bindings = _songDetailBindings.Span;
            for (int i = 0; i < bindings.Length; i++)
            {
                var distance = i - listPosReal;
                if (Mathf.Abs(distance) <= 10)
                {
                    var binding = bindings[i];
                    binding.PreloadAsync();
                }
            }
            _isNeedPreload = false;
        }
        void FixedUpdate()
        {
            var delta = (desiredListPos - listPosReal) ;
            listPosReal += Mathf.Clamp(delta* turnSpeed, -1f, 1f);
            if (Mathf.Abs(delta) < 0.01f || DisableAnimation || Mathf.Abs(delta) >3)
            {
                listPosReal = desiredListPos;
            }
            
            switch(Mode)
            {
                case CoverListMode.Chart:
                    SongCoverUpdate(_songDetailBindings);
                    break;
                case CoverListMode.Directory:
                    FolderCoverUpdate(_songCollectionBindings);
                    if(Time.frameCount % 50 == 0)
                    {
                        if (_currentCollection.Count > 0)
                        {
                            if (coveri >= _currentCollection.Count) coveri = 0;
                            CoverBigDisplayer.SetSongDetail(_currentCollection[coveri++]);
                        }
                        else
                        {
                            CoverBigDisplayer.SetNoCover();
                        }
                    }
                    break;
            }
        }
        void SongCoverUpdate(Memory<SongDetailBinding> bindingsMemory)
        {
            var bindings = bindingsMemory.Span;
            for (int i = 0; i < bindingsMemory.Length; i++)
            {
                var distance = i - listPosReal;
                var absDistance = Mathf.Abs(distance);
                ref var binding = ref bindings[i];
                SongCoverSmallDisplayer cover;

                if (absDistance > 5)
                {
                    if(binding.Displayer is not null)
                    {
                        cover = binding.Displayer;
                        binding.Displayer = null;
                        cover.gameObject.SetActive(false);
                        _idleSongCoverDisplayer.Enqueue(cover);
                    }
                    continue;
                }
                else
                {
                    if(binding.Displayer is null)
                    {
                        if(_idleSongCoverDisplayer.TryDequeue(out cover))
                        {
                            binding.Displayer = cover;
                            cover.gameObject.SetActive(true);
                            cover.SetSongDetail(binding.SongDetail);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        cover = binding.Displayer;
                    }
                }

                if (absDistance > 4)
                {
                    cover.SetOpacity(-Mathf.Abs(distance) + 5);
                }
                else
                {
                    cover.SetOpacity(1f);
                }

                cover.RectTransform.anchoredPosition = GetCoverPosition(radius, (distance * angle - 90) * Mathf.Deg2Rad);
            }
        }
        void FolderCoverUpdate(Memory<SongCollectionBinding> bindingsMemory)
        {
            var bindings = bindingsMemory.Span;
            for (int i = 0; i < bindingsMemory.Length; i++)
            {
                var distance = i - listPosReal;
                var absDistance = Mathf.Abs(distance);
                ref var binding = ref bindings[i];
                FolderCoverSmallDisplayer cover;

                if (absDistance > 5)
                {
                    if (binding.Displayer is not null)
                    {
                        cover = binding.Displayer;
                        binding.Displayer = null;
                        cover.gameObject.SetActive(false);
                        if(binding.Collection.Type == ChartStorageType.Dan)
                        {
                            _idleDanCoverDisplayer.Enqueue(cover);
                        }
                        else
                        {
                            _idleFolderCoverDisplayer.Enqueue(cover);
                        }
                    }
                    continue;
                }
                else
                {
                    if (binding.Displayer is null)
                    {
                        if (binding.Collection.Type == ChartStorageType.Dan)
                        {
                            if (!_idleFolderCoverDisplayer.TryDequeue(out cover))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (!_idleFolderCoverDisplayer.TryDequeue(out cover))
                            {
                                continue;
                            }
                        }
                        binding.Displayer = cover;
                        cover.gameObject.SetActive(true);
                        cover.SetCollection(binding.Collection);
                    }
                    else
                    {
                        cover = binding.Displayer;
                    }
                }
                cover.RectTransform.anchoredPosition = GetCoverPosition(radius, (distance * angle - 90) * Mathf.Deg2Rad);
            }
        }
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
        SongCollectionBinding GetSongCollectionBinding(SongCollection songCollection)
        {
            return new SongCollectionBinding()
            {
                Collection = songCollection
            };
        }
        struct SongDetailBinding
        {
            public ISongDetail SongDetail { get; set; }
            public SongCoverSmallDisplayer? Displayer { get; set; }
            public ValueTask? PreloadTask { get; set; }


            public SongDetailBinding(ISongDetail songDetail, SongCoverSmallDisplayer? displayer)
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
        struct SongCollectionBinding
        {
            public SongCollection Collection { get; set; }
            public FolderCoverSmallDisplayer? Displayer {  get; set; }

            public SongCollectionBinding(SongCollection collection, FolderCoverSmallDisplayer? displayer)
            {
                Collection = collection;
                Displayer = displayer;
            }
        }
    }
}