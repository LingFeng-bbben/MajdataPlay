using Cysharp.Threading.Tasks;
using MajdataPlay.Collections;
using MajdataPlay.Game;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public int desiredListPos = 0;
        public float listPosReal;
        public float turnSpeed;
        public float radius;
        public float offset;
        public float angle;

        public int selectedDifficulty = 0;

        private SongCollection[] dirs = SongStorage.Collections;
        private SongCollection songs = new SongCollection();

        ListManager _listManager;

        private void Awake()
        {
            MajInstanceHelper<CoverListDisplayer>.Instance = this;
        }
        void Start()
        {
            _listManager = MajInstanceHelper<ListManager>.Instance!;
        }
        void OnDestroy()
        {
            MajInstanceHelper<CoverListDisplayer>.Free();
        }

        public void SwitchToDirList()
        {
            foreach (var cover in _songCovers)
            {
                Destroy(cover.gameObject);
            }
            _songCovers.Clear();
            Mode = CoverListMode.Directory;
            desiredListPos = SongStorage.CollectionIndex;
            foreach (var dir in dirs)
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
            if (desiredListPos > _folderCovers.Count) desiredListPos = 0;
            listPosReal = desiredListPos;
            SlideListInternal(desiredListPos);
        }

        public void SwitchToSongList()
        {
            if (songs.Count == 0) return;
            if (songs.Type == ChartStorageType.Dan) return;
            foreach (var cover in _folderCovers)
            {
                Destroy(cover.gameObject);
            }
            _folderCovers.Clear();
            Mode = CoverListMode.Chart;
            desiredListPos = SongStorage.WorkingCollection.Index;
            foreach (var song in songs)
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
                var songinfo = songs[desiredListPos];
                var songScore = MajInstances.ScoreManager.GetScore(songinfo, MajInstances.GameManager.SelectedDiff);
                CoverBigDisplayer.SetMeta(songinfo.Title, songinfo.Artist, songinfo.Designers[selectedDifficulty], songinfo.Levels[selectedDifficulty]);
                CoverBigDisplayer.SetScore(songScore);
                chartAnalyzer.AnalyzeAndDrawGraphAsync(songinfo, (ChartLevel)selectedDifficulty).Forget();

                for (int i = 0; i < _songCovers.Count; i++)
                {
                    var text = songs[i].Levels[selectedDifficulty];
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
                    var collection = SongStorage.WorkingCollection;
                    collection.Move(delta);
                    desiredListPos = collection.Index;
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

        public void SlideListInternal(int pos)
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
                    songs = dirs[desiredListPos];
                    if(songs.Type == ChartStorageType.List)
                    {
                        CoverBigDisplayer.SetMeta(songs.Name, "Count:"+songs.Count, "", "");
                    }
                    else
                    {
                        CoverBigDisplayer.SetMeta(songs.DanInfo.Name, songs.DanInfo.Description, "", "");
                    }
                    
                    CoverBigDisplayer.SetScore(new MaiScore());
                    SongStorage.CollectionIndex = desiredListPos;
                    break;
                case CoverListMode.Chart:
                    var songinfo = songs[desiredListPos];
                    var songScore = MajInstances.ScoreManager.GetScore(songinfo, MajInstances.GameManager.SelectedDiff);
                    CoverBigDisplayer.SetSongDetail(songinfo);
                    CoverBigDisplayer.SetMeta(songinfo.Title, songinfo.Artist, songinfo.Designers[selectedDifficulty], songinfo.Levels[selectedDifficulty]);
                    CoverBigDisplayer.SetScore(songScore);
                    SubInfoDisplayer.RefreshContent(songinfo);
                    GetComponent<PreviewSoundPlayer>().PlayPreviewSound(songinfo);
                    chartAnalyzer.AnalyzeAndDrawGraphAsync(songinfo, (ChartLevel)selectedDifficulty).Forget();
                    SongStorage.WorkingCollection.Index = desiredListPos;
                    PreloadSongDetail();
                    break;
            }
        }
        void PreloadSongDetail()
        {
            for (int i = 0; i < songs.Count; i++)
            {
                var distance = i - listPosReal;
                if (Mathf.Abs(distance) <= 10)
                {
                    if (songs[i] is null)
                        continue;
                    var preloadTask = songs[i].PreloadAsync(_listManager.CancellationToken);
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
                        if (songs.Count > 0)
                        {
                            if (coveri >= songs.Count) coveri = 0;
                            CoverBigDisplayer.SetSongDetail(songs[coveri++]);
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