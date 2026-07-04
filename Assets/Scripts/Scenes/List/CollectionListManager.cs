using MajdataPlay.Collections;
using MajdataPlay.Editor;
using MajdataPlay.Scenes.List.Models;
using MajdataPlay.Settings.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Scenes.List
{
    public class CollectionListManager: MonoBehaviour
    {
        const int DISPLAYER_ANIM_DURATION_MS = 250;

        public SongCollection SelectedCollection
        {
            get
            {
                return _currentCollection;
            }
        }

        [SerializeField]
        [FormerlySerializedAs("centerCoverDisplayer")]
        CoverBigDisplayer _centerCoverDisplayer;

        [SerializeField]
        [FormerlySerializedAs("coverListDisplayer")]
        CoverListManager _coverListDisplayer;

        [SerializeField]
        [FormerlySerializedAs("collectionListRoot")]
        GameObject _collectionListRoot;

        [SerializeField]
        [FormerlySerializedAs("nameDisplayer")]
        TextMeshProUGUI _nameDisplayer;

        [SerializeField]
        [FormerlySerializedAs("iconDisplayer")]
        Image _iconDisplayer;

        [SerializeField]
        [ReadOnlyField]
        int _selectedDifficulty = 0;

        int _listDesiredPos = 0;
        float _listCursorPos = 0;
        float _displayerAnimStepPerSec = 0f;

        SongCollection _currentCollection = SongCollection.Empty("Empty");
        SongCollection[] _collections = Array.Empty<SongCollection>();

        CollectionDisplayer[] _collectionDisplayers = Array.Empty<CollectionDisplayer>();
        SongCollectionBinding[] _collectionBindings = Array.Empty<SongCollectionBinding>();

        SongCollection[] _easySortedCollections = Array.Empty<SongCollection>();
        SongCollection[] _basicSortedCollections = Array.Empty<SongCollection>();
        SongCollection[] _advanceSortedCollections = Array.Empty<SongCollection>();
        SongCollection[] _expertSortedCollections = Array.Empty<SongCollection>();
        SongCollection[] _masterSortedCollections = Array.Empty<SongCollection>();
        SongCollection[] _reMasterSortedCollections = Array.Empty<SongCollection>();
        SongCollection[] _utageSortedCollections = Array.Empty<SongCollection>();

        readonly Queue<CollectionDisplayer> _idleCollectionDisplayers = new();
        readonly ListConfig _listConfig = MajEnv.RuntimeConfig?.List ?? new();

        void Awake()
        {
            Majdata<CollectionListManager>.Instance = this;
            var collectionListRoot = _collectionListRoot.transform;
            var displayerCount = collectionListRoot.childCount;
            _collectionDisplayers = new CollectionDisplayer[displayerCount];
            for (int i = 0; i < displayerCount; i++)
            {
                var child = collectionListRoot.GetChild(i);
                var displayer = child.GetComponent<CollectionDisplayer>();
                if (displayer is null)
                {
                    throw new InvalidOperationException($"Child {child.name} does not have a CollectionDisplayer component.");
                }
                _collectionDisplayers[i] = displayer;
                _idleCollectionDisplayers.Enqueue(displayer);
            }
        }
        internal void Init()
        {
            InitCollectionStorage();
            InitCollectionBinding();
        }
        internal void SlideList(int delta)
        {
            var lastDesiredPos = _listDesiredPos;
            _listDesiredPos += delta;
            if (_listDesiredPos < 0)
            {
                _listDesiredPos = _collections.Length - 1;
            }
            else if (_listDesiredPos >= _collections.Length)
            {
                _listDesiredPos = 0;
            }
            UpdateSelectedSongCollection();
            if(lastDesiredPos != _listDesiredPos)
            {
                _displayerAnimStepPerSec = Mathf.Abs(_listDesiredPos - _listCursorPos) / (DISPLAYER_ANIM_DURATION_MS / 1000f);
            }
        }
        public void SlideDifficulty(int delta)
        {
            _selectedDifficulty += delta;
            SlideToDifficulty(_selectedDifficulty);
        }
        public void SlideToDifficulty(int pos)
        {
            _selectedDifficulty = pos;
            if (_selectedDifficulty > 6)
            {
                _selectedDifficulty = 0;
            }
            if (_selectedDifficulty < 0)
            {
                _selectedDifficulty = 6;
            }
            var chartLevel = (ChartLevel)_selectedDifficulty;
            _listConfig.SelectedDiff = chartLevel;
            UpdateSelectedSongCollection();

            _centerCoverDisplayer.SetDifficulty(_selectedDifficulty);
            _coverListDisplayer.SetCollection(_currentCollection);
        }

        internal void OnUpdate()
        {
            UpdateSongCollectionBinding();
            UpdateDisplayerAnim();
        }
        
        internal void NextCollection()
        {
            SlideList(1);
        }
        internal void PreviousCollection()
        {
            SlideList(-1);
        }

        void UpdateSelectedSongCollection()
        {
            var oldSelected = _currentCollection;
            switch (_listConfig.SelectedDiff)
            {
                case ChartLevel.Easy:
                    _currentCollection = _easySortedCollections[_listDesiredPos];
                    break;
                case ChartLevel.Basic:
                    _currentCollection = _basicSortedCollections[_listDesiredPos];
                    break;
                case ChartLevel.Advance:
                    _currentCollection = _advanceSortedCollections[_listDesiredPos];
                    break;
                case ChartLevel.Expert:
                    _currentCollection = _expertSortedCollections[_listDesiredPos];
                    break;
                case ChartLevel.Master:
                    _currentCollection = _masterSortedCollections[_listDesiredPos];
                    break;
                case ChartLevel.ReMaster:
                    _currentCollection = _reMasterSortedCollections[_listDesiredPos];
                    break;
                case ChartLevel.UTAGE:
                    _currentCollection = _utageSortedCollections[_listDesiredPos];
                    break;
                default:
                    throw new ArgumentOutOfRangeException("sb");
            }
            if (_currentCollection != oldSelected)
            {
                var oldSelectedSong = default(ISongDetail);
                if (oldSelected.Count != 0)
                {
                    oldSelectedSong = oldSelected.Current;
                }
                _coverListDisplayer.SetCollection(_currentCollection);
                if (oldSelectedSong is not null)
                {
                    _coverListDisplayer.SetCursor(oldSelectedSong);
                }
            }
        }
        void UpdateSongCollectionBinding()
        {
            var currentListCursorPos = (int)_listCursorPos;
            for (var i = 0; i < _collectionBindings.Length; i++)
            {
                ref var binding = ref _collectionBindings[i];
                var absDistance = Math.Abs(i - currentListCursorPos);
                var displayer = binding.Displayer;

                if (absDistance > 3)
                {                    
                    if (displayer is null)
                    {
                        continue;
                    }
                    binding.Displayer = null;
                    displayer.SetActive(false);
                    _idleCollectionDisplayers.Enqueue(displayer);
                }
                else
                {
                    if(displayer is not null)
                    {
                        continue;
                    }
                    if (!_idleCollectionDisplayers.TryDequeue(out displayer))
                    {
                        Debug.LogWarning("No idle collection displayer available.");
                        continue;
                    }
                    binding.Displayer = displayer;
                    displayer.SetCollection(binding.Collection);
                    displayer.SetActive(true);
                }
            }
        }
        void UpdateDisplayerAnim()
        {
            if(_listCursorPos == _listDesiredPos)
            {
                return;
            }
            else
            {
                _listCursorPos += _displayerAnimStepPerSec * MajTimeline.DeltaTime;
                _listCursorPos = Mathf.Min(_listCursorPos, _listDesiredPos);
            }
            for (var i = 0; i < _collectionBindings.Length; i++)
            {
                ref var binding = ref _collectionBindings[i];
                var displayer = binding.Displayer;
                if (displayer is null)
                {
                    continue;
                }
                var delta = i - _listCursorPos;
                displayer.RectTransform.anchoredPosition = GetDisplayerPositionFromDelta(delta);
            }
        }
        Vector2 GetDisplayerPositionFromDelta(float delta)
        {
            const int X_POS_WITH_DELTA_1 = 240;
            const int X_POS_WITH_DELTA_2 = 390;
            const int X_POS_WITH_DELTA_3 = 540;
            const int X_POS_WITH_DELTA_4 = 690;
            var absDelta = Mathf.Abs(delta);

            var x = absDelta switch
            {
                <= 1f => 240f * absDelta,
                <= 2f => Mathf.Lerp(X_POS_WITH_DELTA_1, X_POS_WITH_DELTA_2, absDelta - 1f),
                <= 3f => Mathf.Lerp(X_POS_WITH_DELTA_2, X_POS_WITH_DELTA_3, absDelta - 2f),
                <= 4f => Mathf.Lerp(X_POS_WITH_DELTA_3, X_POS_WITH_DELTA_4, absDelta - 3f),
                _ => X_POS_WITH_DELTA_4
            };

            return new Vector2(Mathf.Sign(delta) * x, 0);
        }
        void InitCollectionStorage()
        {
            var collections = SongStorage.Collections;
            var newCollections = new SongCollection[collections.Length];

            Parallel.For(0, collections.Length, i =>
            {
                var collection = collections[i];
                if (collection.Type == ChartStorageType.FavoriteList)
                {
                    newCollections[i] = collection;
                }
                else if (collection is OnlineSongCollection onlineCollection)
                {
                    newCollections[i] = new OnlineSongCollection(onlineCollection.Source, onlineCollection.Name, onlineCollection.ToArray())
                    {
                        DanInfo = collection.DanInfo,
                        Type = collection.Type,
                    };
                }
                else
                {
                    newCollections[i] = new SongCollection(collection.Name, collection.ToArray())
                    {
                        DanInfo = collection.DanInfo,
                        Type = collection.Type,
                    };
                }

                newCollections[i].Reset();
                if (!collection.IsEmpty)
                {
                    newCollections[i].SetCursor(collection.Current);
                }
                newCollections[i].SortAndFilter(SongStorage.OrderBy);
            });
            _collections = newCollections;
            var collection = _collections[SongStorage.CollectionIndex];

            if (SongStorage.OrderBy.SortBy == SortType.ByRank)
            {
                _easySortedCollections = new SongCollection[collections.Length];
                _basicSortedCollections = new SongCollection[collections.Length];
                _advanceSortedCollections = new SongCollection[collections.Length];
                _expertSortedCollections = new SongCollection[collections.Length];
                _masterSortedCollections = new SongCollection[collections.Length];
                _reMasterSortedCollections = new SongCollection[collections.Length];
                _utageSortedCollections = new SongCollection[collections.Length];

                Parallel.For(0, newCollections.Length, i =>
                {
                    var originCollection = newCollections[i];
                    var songs = originCollection.ToArray();
                    var songAndScores = songs.Select(x => (x, ScoreManager.GetSongScores(x))).ToArray();
                    var sortedEasy = Array.Empty<ISongDetail>();
                    var sortedBasic = Array.Empty<ISongDetail>();
                    var sortedAdvance = Array.Empty<ISongDetail>();
                    var sortedExpert = Array.Empty<ISongDetail>();
                    var sortedMaster = Array.Empty<ISongDetail>();
                    var sortedReMaster = Array.Empty<ISongDetail>();
                    var sortedUTAGE = Array.Empty<ISongDetail>();
                    if (MajEnv.Settings.Judge.Mode == Settings.JudgeModeOption.Classic)
                    {
                        sortedEasy = songAndScores.OrderByDescending(x => x.Item2.Easy.Acc.Classic)
                                                  .Select(x => x.x)
                                                  .ToArray();
                        sortedBasic = songAndScores.OrderByDescending(x => x.Item2.Basic.Acc.Classic)
                                                   .Select(x => x.x)
                                                   .ToArray();
                        sortedAdvance = songAndScores.OrderByDescending(x => x.Item2.Advance.Acc.Classic)
                                                     .Select(x => x.x)
                                                     .ToArray();
                        sortedExpert = songAndScores.OrderByDescending(x => x.Item2.Expert.Acc.Classic)
                                                     .Select(x => x.x)
                                                     .ToArray();
                        sortedMaster = songAndScores.OrderByDescending(x => x.Item2.Master.Acc.Classic)
                                                     .Select(x => x.x)
                                                     .ToArray();
                        sortedReMaster = songAndScores.OrderByDescending(x => x.Item2.ReMaster.Acc.Classic)
                                                      .Select(x => x.x)
                                                      .ToArray();
                        sortedUTAGE = songAndScores.OrderByDescending(x => x.Item2.UTAGE.Acc.Classic)
                                                   .Select(x => x.x)
                                                   .ToArray();
                    }
                    else
                    {
                        sortedEasy = songAndScores.OrderByDescending(x => x.Item2.Easy.Acc.DX)
                                                  .Select(x => x.x)
                                                  .ToArray();
                        sortedBasic = songAndScores.OrderByDescending(x => x.Item2.Basic.Acc.DX)
                                                   .Select(x => x.x)
                                                   .ToArray();
                        sortedAdvance = songAndScores.OrderByDescending(x => x.Item2.Advance.Acc.DX)
                                                     .Select(x => x.x)
                                                     .ToArray();
                        sortedExpert = songAndScores.OrderByDescending(x => x.Item2.Expert.Acc.DX)
                                                    .Select(x => x.x)
                                                    .ToArray();
                        sortedMaster = songAndScores.OrderByDescending(x => x.Item2.Master.Acc.DX)
                                                    .Select(x => x.x)
                                                    .ToArray();
                        sortedReMaster = songAndScores.OrderByDescending(x => x.Item2.ReMaster.Acc.DX)
                                                      .Select(x => x.x)
                                                      .ToArray();
                        sortedUTAGE = songAndScores.OrderByDescending(x => x.Item2.UTAGE.Acc.DX)
                                                       .Select(x => x.x)
                                                       .ToArray();
                    }
                    _easySortedCollections[i] = new SongCollection(originCollection.Name, sortedEasy);
                    _basicSortedCollections[i] = new SongCollection(originCollection.Name, sortedBasic);
                    _advanceSortedCollections[i] = new SongCollection(originCollection.Name, sortedAdvance);
                    _expertSortedCollections[i] = new SongCollection(originCollection.Name, sortedExpert);
                    _masterSortedCollections[i] = new SongCollection(originCollection.Name, sortedMaster);
                    _reMasterSortedCollections[i] = new SongCollection(originCollection.Name, sortedReMaster);
                    _utageSortedCollections[i] = new SongCollection(originCollection.Name, sortedUTAGE);

                    //_easySortedCollections[i].SortAndFilter(SongStorage.OrderBy);
                    //_basicSortedCollections[i].SortAndFilter(SongStorage.OrderBy);
                    //_advanceSortedCollections[i].SortAndFilter(SongStorage.OrderBy);
                    //_expertSortedCollections[i].SortAndFilter(SongStorage.OrderBy);
                    //_masterSortedCollections[i].SortAndFilter(SongStorage.OrderBy);
                    //_reMasterSortedCollections[i].SortAndFilter(SongStorage.OrderBy);
                    //_utageSortedCollections[i].SortAndFilter(SongStorage.OrderBy);
                });
            }
            else
            {
                _easySortedCollections = newCollections;
                _basicSortedCollections = newCollections;
                _advanceSortedCollections = newCollections;
                _expertSortedCollections = newCollections;
                _masterSortedCollections = newCollections;
                _reMasterSortedCollections = newCollections;
                _utageSortedCollections = newCollections;
            }
        }
        void InitCollectionBinding()
        {
            _collectionBindings = new SongCollectionBinding[_collections.Length];
            for (int i = 0; i < _collections.Length; i++)
            {
                var collection = _collections[i];
                var binding = new SongCollectionBinding()
                {
                    Collection = collection
                };
                _collectionBindings[i] = binding;
            }
        }

        struct SongCollectionBinding
        {
            public SongCollection Collection { get; init; }
            public CollectionDisplayer? Displayer { get; set; }

            public SongCollectionBinding(SongCollection collection, CollectionDisplayer? displayer)
            {
                Collection = collection;
                Displayer = displayer;
            }
        }
    }
}
