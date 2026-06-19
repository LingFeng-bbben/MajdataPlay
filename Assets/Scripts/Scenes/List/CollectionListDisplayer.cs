using MajdataPlay.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Scenes.List
{
    public class CollectionListDisplayer: MonoBehaviour
    {
        public SongCollection SelectedCollection
        {
            get
            {
                return _currentCollection;
            }
        }

        SongCollection _currentCollection = SongCollection.Empty("Empty");
        ReadOnlyMemory<SongCollection> _collections = ReadOnlyMemory<SongCollection>.Empty;

        SongCollection[] _easySortedCollections = Array.Empty<SongCollection>();
        SongCollection[] _basicSortedCollections = Array.Empty<SongCollection>();
        SongCollection[] _advanceSortedCollections = Array.Empty<SongCollection>();
        SongCollection[] _expertSortedCollections = Array.Empty<SongCollection>();
        SongCollection[] _masterSortedCollections = Array.Empty<SongCollection>();
        SongCollection[] _reMasterSortedCollections = Array.Empty<SongCollection>();
        SongCollection[] _utageSortedCollections = Array.Empty<SongCollection>();

        void Awake()
        {
            Majdata<CollectionListDisplayer>.Instance = this;
        }
        internal void Init()
        {
            InitCollectionStorage();
        }
        void UpdateCurrentSongCollection()
        {
            var pos = SongStorage.CollectionIndex;
            switch (ChartLevel.Expert)
            {
                case ChartLevel.Easy:
                    _currentCollection = _easySortedCollections[pos];
                    break;
                case ChartLevel.Basic:
                    _currentCollection = _basicSortedCollections[pos];
                    break;
                case ChartLevel.Advance:
                    _currentCollection = _advanceSortedCollections[pos];
                    break;
                case ChartLevel.Expert:
                    _currentCollection = _expertSortedCollections[pos];
                    break;
                case ChartLevel.Master:
                    _currentCollection = _masterSortedCollections[pos];
                    break;
                case ChartLevel.ReMaster:
                    _currentCollection = _reMasterSortedCollections[pos];
                    break;
                case ChartLevel.UTAGE:
                    _currentCollection = _utageSortedCollections[pos];
                    break;
                default:
                    throw new ArgumentOutOfRangeException("sb");
            }
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
            var collection = _collections.Span[SongStorage.CollectionIndex];

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

        SongCollectionBinding GetSongCollectionBinding(SongCollection songCollection)
        {
            return new SongCollectionBinding()
            {
                Collection = songCollection
            };
        }
        struct SongCollectionBinding
        {
            public SongCollection Collection { get; set; }
            public FolderCoverSmallDisplayer? Displayer { get; set; }

            public SongCollectionBinding(SongCollection collection, FolderCoverSmallDisplayer? displayer)
            {
                Collection = collection;
                Displayer = displayer;
            }
        }
    }
}
