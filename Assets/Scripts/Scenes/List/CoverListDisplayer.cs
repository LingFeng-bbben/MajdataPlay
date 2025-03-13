using Cysharp.Threading.Tasks;
using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Game;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay.List
{
    public class CoverListDisplayer : MonoBehaviour
    {
        public bool IsDirList => Mode == CoverListMode.Directory;
        public bool IsChartList => Mode == CoverListMode.Chart;
        public CoverListMode Mode { get; set; } = CoverListMode.Directory;

        List<FolderCoverSmallDisplayer> _folderCovers = new List<FolderCoverSmallDisplayer>();
        List<SongCoverSmallDisplayer> _songCovers = new List<SongCoverSmallDisplayer>();
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

        public int selectedDifficulty = 0;

        //SongCollection[] dirs = SongStorage.Collections;
        Task _sortAndFindTask = Task.CompletedTask;

        ListManager _listManager;

        ReadOnlyMemory<SongCollection> _collections = ReadOnlyMemory<SongCollection>.Empty;
        SongCollection _currentCollection = SongCollection.Empty("Empty");

        private void Awake()
        {
            Majdata<CoverListDisplayer>.Instance = this;
            _sortAndFindTask = Task.Run(() =>
            {
                var collections = SongStorage.Collections;
                var newCollections = new SongCollection[collections.Length];
                for (var i = 0; i < collections.Length; i++)
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
                    newCollections[i].SortAndFilter(SongStorage.OrderBy);
                }
                _collections = newCollections;
                _currentCollection = _collections.Span[SongStorage.CollectionIndex];
            });
        }
        void Start()
        {
            _listManager = Majdata<ListManager>.Instance!;
        }
        void OnDestroy()
        {
            Majdata<CoverListDisplayer>.Free();
        }
        public async UniTask SwitchToDirListAsync()
        {
            while(!_sortAndFindTask.IsCompleted)
                await UniTask.Yield();
            SwitchToDirListInternal();
        }
        public async UniTask SwitchToSongListAsync()
        {
            while (!_sortAndFindTask.IsCompleted)
                await UniTask.Yield();
            SwitchToSongListInternal();
        }
        void SwitchToDirListInternal()
        {
            foreach (var cover in _songCovers)
            {
                Destroy(cover.gameObject);
            }
            _songCovers.Clear();
            SubInfoDisplayer.Hide();
            FavoriteAdder.Hide();
            Mode = CoverListMode.Directory;
            desiredListPos = SongStorage.CollectionIndex;
            foreach (var dir in _collections.Span)
            {
                var prefab = DirSmallPrefab;
                if (dir.Type == ChartStorageType.Dan)
                {
                    prefab = DanSmallPrefab;
                }
                var obj = Instantiate(prefab, transform);
                var coversmall = obj.GetComponent<FolderCoverSmallDisplayer>();
                //coversmall.SetCover(song.SongCover);
                coversmall.SetCollection(dir);
                _folderCovers.Add(coversmall);
                coversmall.gameObject.SetActive(false);
                coversmall.IsOnline = dir.IsOnline;
            }
            if (desiredListPos > _folderCovers.Count) 
                desiredListPos = 0;
            listPosReal = desiredListPos;
            SlideListInternal(desiredListPos);
        }

        void SwitchToSongListInternal()
        {
            if (_currentCollection.Count == 0) return;
            if (_currentCollection.Type == ChartStorageType.Dan) return;
            foreach (var cover in _folderCovers)
            {
                Destroy(cover.gameObject);
            }
            _folderCovers.Clear();
            Mode = CoverListMode.Chart;
            desiredListPos = SongStorage.WorkingCollection.Index;
            foreach (var song in _currentCollection)
            {
                var obj = Instantiate(CoverSmallPrefab, transform);
                var coversmall = obj.GetComponent<SongCoverSmallDisplayer>();
                coversmall.SetOpacity(0f);
                coversmall.SetSongDetail(song);
                coversmall.SetLevelText(song.Levels[selectedDifficulty]);
                _songCovers.Add(coversmall);
                coversmall.gameObject.SetActive(false);
            }
            if (desiredListPos > _songCovers.Count) desiredListPos = 0;
            listPosReal = desiredListPos;
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
            MajInstances.GameManager.SelectedDiff = (ChartLevel)selectedDifficulty;
            CoverBigDisplayer.SetDifficulty(selectedDifficulty);
            if (IsChartList)
            {
                var songinfo = _currentCollection[desiredListPos];
                var songScore = MajInstances.ScoreManager.GetScore(songinfo, MajInstances.GameManager.SelectedDiff);
                CoverBigDisplayer.SetMeta(songinfo.Title, songinfo.Artist, songinfo.Designers[selectedDifficulty], songinfo.Levels[selectedDifficulty]);
                CoverBigDisplayer.SetScore(songScore);
                chartAnalyzer.AnalyzeAndDrawGraphAsync(songinfo, (ChartLevel)selectedDifficulty).Forget();
                FavoriteAdder.SetSong(songinfo);
                for (int i = 0; i < _songCovers.Count; i++)
                {
                    var text = _currentCollection[i].Levels[selectedDifficulty];
                    if (string.IsNullOrEmpty(text)) 
                        text = "-";
                    _songCovers[i].SetLevelText(text);
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
                    break;
                case CoverListMode.Chart:
                    var collection = _currentCollection;
                    collection.Move(delta);
                    //desiredListPos = collection.Index;
                    desiredListPos += delta;
                    break;
            }
            SlideListInternal(desiredListPos);
        }
        public void RefreshList()
        {
            var collection = SongStorage.WorkingCollection;
            desiredListPos = collection.Index;
            SlideListInternal(desiredListPos);
        }

        void SlideListInternal(int pos)
        {
            MajInstances.AudioManager.PlaySFX("tap_perfect.wav");
            var coverCount = IsDirList ? _folderCovers.Count : _songCovers.Count;

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
                            CoverBigDisplayer.SetMeta(_currentCollection.DanInfo.Name, _currentCollection.DanInfo.Description, "", ""); ;
                            break;
                    }
                    CoverBigDisplayer.SetScore(new MaiScore());
                    SongStorage.CollectionIndex = desiredListPos;
                    break;
                case CoverListMode.Chart:
                    var songinfo = _currentCollection[desiredListPos];
                    var songScore = MajInstances.ScoreManager.GetScore(songinfo, MajInstances.GameManager.SelectedDiff);
                    CoverBigDisplayer.SetSongDetail(songinfo);
                    CoverBigDisplayer.SetMeta(songinfo.Title, songinfo.Artist, songinfo.Designers[selectedDifficulty], songinfo.Levels[selectedDifficulty]);
                    CoverBigDisplayer.SetScore(songScore);
                    SubInfoDisplayer.RefreshContent(songinfo);
                    GetComponent<PreviewSoundPlayer>().PlayPreviewSound(songinfo);
                    chartAnalyzer.AnalyzeAndDrawGraphAsync(songinfo, (ChartLevel)selectedDifficulty).Forget();
                    FavoriteAdder.SetSong(songinfo);
                    _currentCollection.Index = desiredListPos;
                    SongStorage.WorkingCollection.Index = desiredListPos;
                    PreloadSongDetail();
                    break;
            }
        }
        void PreloadSongDetail()
        {
            for (int i = 0; i < _currentCollection.Count; i++)
            {
                var distance = i - listPosReal;
                if (Mathf.Abs(distance) <= 10)
                {
                    if (_currentCollection[i] is null)
                        continue;
                    var preloadTask = _currentCollection[i].PreloadAsync(_listManager.CancellationToken);
                    if(!preloadTask.AsValueTask().IsCompleted)
                    {
                        ListManager.AllBackguardTasks.Add(preloadTask);
                    }
                }
            }
        }
        private void FixedUpdate()
        {
            var delta = (desiredListPos - listPosReal) * turnSpeed;
            listPosReal += Mathf.Clamp(delta, -1f, 1f);
            if (Mathf.Abs(desiredListPos - listPosReal) < 0.01f) 
                listPosReal = desiredListPos;
            
            switch(Mode)
            {
                case CoverListMode.Chart:
                    CoverListUpdate(_songCovers);
                    break;
                case CoverListMode.Directory:
                    CoverListUpdate(_folderCovers);
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
        void CoverListUpdate<T>(List<T> covers) where T : CoverSmallDisplayer
        {
            for (int i = 0; i < covers.Count; i++)
            {
                var distance = i - listPosReal;
                var cover = covers[i];
                if (Mathf.Abs(distance) > 5)
                {
                    if (cover.gameObject.activeSelf)
                        cover.gameObject.SetActive(false);
                    continue;
                }
                if (!cover.gameObject.activeSelf)
                    cover.gameObject.SetActive(true);
                cover.RectTransform.anchoredPosition = GetCoverPosition(radius, (distance * angle - 90) * Mathf.Deg2Rad);
                if(IsChartList && cover is SongCoverSmallDisplayer songCover)
                {
                    if (Mathf.Abs(distance) > 4)
                    {
                        songCover.SetOpacity(-Mathf.Abs(distance) + 5);
                    }
                    else
                    {
                        songCover.SetOpacity(1f);
                    }
                }
            }
        }
        private int coveri = 0;

        Vector3 GetCoverPosition(float radius, float position)
        {
            return new Vector3(radius * Mathf.Sin(position), radius * Mathf.Cos(position));
        }
    }
}