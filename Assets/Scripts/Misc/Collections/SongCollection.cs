using MajdataPlay.Extensions;
using MajdataPlay.Numerics;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Collections
{
    public class SongCollection : IEnumerable<ISongDetail>, IReadOnlyCollection<ISongDetail>
    {
        public Guid Id { get; init; } = Guid.Empty;
        public string Path { get; init; }
        public ISongDetail Current
        {
            get
            {
                return _sorted[Index];
            }
        }
        public int Index
        {
            get => _index;
            set
            {
                if (IsEmpty)
                    throw new ArgumentOutOfRangeException("this collection is empty");
                _index = value.Clamp(0, _origin.Length - 1);
            }
        }
        public ISongDetail this[int index]
        {
            get
            {
                return _sorted[index];
            }
        }
        public ChartStorageLocation Location { get; init; } = ChartStorageLocation.Local;
        public ChartStorageType Type { get; init; } = ChartStorageType.List;
        public bool IsOnline => Location == ChartStorageLocation.Online;
        public string Name { get; private set; }
        public bool IsSorted { get; private set; } = false;
        public int Count
        {
            get
            {
                return _sorted.Length;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return _sorted.Length == 0;
            }
        }
        public bool IsVirtual { get; init; }
        public DanInfo? DanInfo { get; init; }

        protected ISongDetail[] _sorted;
        protected ISongDetail[] _origin;

        readonly static Guid COLLECTION_ALL_GUID = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
        readonly static Guid COLLECTION_MY_FAVORITE_GUID = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2);

        public SongCollection(string name, ISongDetail[] pArray): this(null, name, pArray)
        {

        }
        public SongCollection(string? dirPath, string name, ISongDetail[] pArray)
        {
            if (pArray.Length == 0)
            {
                _sorted = Array.Empty<ISongDetail>();
                _origin = Array.Empty<ISongDetail>();
            }
            else
            {
                var array = new ISongDetail[pArray.Length];
                Array.Copy(pArray, array, pArray.Length);
                _sorted = array;
                _origin = array;
            }
            if(string.IsNullOrEmpty(dirPath))
            {
                IsVirtual = true;
                Path = string.Empty;
                
                switch(name)
                {
                    case "All":
                        Id = COLLECTION_ALL_GUID;
                        break;
                    case "MyFavorites":
                        Id = COLLECTION_MY_FAVORITE_GUID;
                        break;
                    default:
                        //throw new ArgumentOutOfRangeException("Unexpected virtual collection name", nameof(name));
                        Id = Guid.Empty;
                        break;
                }
            }
            else
            {
                if(!Directory.Exists(dirPath))
                {
                    throw new ArgumentException($"Directory '{dirPath}' is not exist.");
                }
                IsVirtual = false;
                Path = dirPath!;
                var flagDirPath = System.IO.Path.Combine(dirPath!, ".MajdataPlay");
                var flagFilePath = System.IO.Path.Combine(flagDirPath, "id");

                if (!Directory.Exists(flagDirPath))
                {
                    var info = Directory.CreateDirectory(flagDirPath);
                    info.Attributes |= FileAttributes.Hidden;
                }
                if (File.Exists(flagFilePath))
                {
                    var guidStr = File.ReadAllText(flagFilePath);
                    if(Guid.TryParse(guidStr, out var guid))
                    {
                        Id = guid;
                    }
                    else
                    {
                        guid = Guid.NewGuid();
                        File.WriteAllText(flagFilePath, guid.ToString());
                        Id = guid;
                    }
                }
                else
                {
                    var guid = Guid.NewGuid();
                    Id = guid;
                    File.WriteAllText(flagFilePath, guid.ToString());
                }
            }
            Name = name;
        }
        public SongCollection()
        {
            _origin = Array.Empty<ISongDetail>();
            _sorted = _origin;
            Path = string.Empty;
            Name = string.Empty;
            Id = Guid.Empty;
            IsVirtual = true;
        }
        public bool MoveNext()
        {
            if (Index >= Count - 1)
                return false;
            Index++;
            return true;
        }
        public void Move(int diff)
        {
            Index = (Index + diff).Clamp(0, Count - 1);
        }
        public void SortAndFilter(SongOrder orderBy)
        {
            if(Type == ChartStorageType.Dan || IsEmpty)
            {
                return;
            }
            IsSorted = true;
            var filtered = Filter(_origin, orderBy.Keyword);
            var sorted = Sort(filtered, orderBy.SortBy);

            SetCursor(Current, sorted);
            this._sorted = sorted;
        }
        public async Task SortAndFilterAsync(SongOrder orderBy)
        {
            await Task.Run(() => SortAndFilter(orderBy));
        }
        public void Reset()
        {
            if (!IsSorted)
            {
                return;
            }
            IsSorted = false;
            if (_sorted.Length != 0)
            {
                var newIndex = _origin.FindIndex(x => x == Current);
                newIndex = newIndex is -1 ? 0 : newIndex;
                _index = newIndex;
            }
            else
            {
                _index = 0;
            }
            _sorted = _origin;
        }
        public void SetCursor(ISongDetail target)
        {
            SetCursor(target, _sorted);
        }
        void SetCursor(ISongDetail target, ISongDetail[] dataSet)
        {
            if(IsEmpty)
            {
                return;
            }
            var newIndex = dataSet.FindIndex(x => x.Hash == target.Hash);
            newIndex = newIndex is -1 ? 0 : newIndex;
            _index = newIndex;
        }
        public ISongDetail[] ToArray()
        {
            if(_origin.Length == 0)
            {
                return Array.Empty<ISongDetail>();
            }
            var array = new ISongDetail[_origin.Length];
            Array.Copy(_origin, array, _origin.Length);

            return array;
        }
        static ISongDetail[] Sort(ISongDetail[] origin, SortType sortType)
        {
            if (origin.IsEmpty())
                return origin;
            IEnumerable<ISongDetail> result = sortType switch
            {
                SortType.ByTime => origin.OrderByDescending(o => o.Timestamp),
                SortType.ByDiff => origin.OrderByDescending(o => o.Levels[4]),
                SortType.ByDes => origin.OrderBy(o => o.Designers[4]),
                SortType.ByTitle => origin.OrderBy(o => o.Title),
                _ => origin
            };
            return result.ToArray();
        }
        static ISongDetail[] Filter(ISongDetail[] origin, string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return origin;
            }
            keyword = keyword.ToLower();
            var result = new Span<ISongDetail>(new ISongDetail[origin.Length]);
            var i = 0;
            foreach (var song in origin)
            {
                var isTitleMatch = song.Title.ToLower().Contains(keyword);
                var isArtistMatch = song.Artist.ToLower().Contains(keyword);
                var isDesMatch = song.Designers.Any(p => p == null ? false : p.ToLower().Contains(keyword));
                var isLevelMatch = song.Levels.Any(p => p == null ? false : p.ToLower() == keyword);
                var isTagDesMatch = song.Description.ToLower().Contains(keyword);

                var isMatch = isTitleMatch || isArtistMatch || isDesMatch || isLevelMatch || isTagDesMatch;
                if (isMatch)
                {
                    result[i++] = song;
                }
            }
            return result.Slice(0, i).ToArray();
        }
        public static SongCollection Empty(string name) => new SongCollection(name, Array.Empty<ISongDetail>());
        public static SongCollection Empty(string path, string name) => new SongCollection(path, name, Array.Empty<ISongDetail>());
        public IEnumerator<ISongDetail> GetEnumerator() => new Enumerator(_sorted);

        // Implementation for the GetEnumerator method.
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        struct Enumerator : IEnumerator<ISongDetail>
        {
            ISongDetail[] songs;
            public ISongDetail Current { get; private set; }
            object IEnumerator.Current { get => Current; }
            int index;
            public Enumerator(in ISongDetail[] songs)
            {
                this.songs = songs;
                Current = default;
                index = 0;
            }
            public bool MoveNext()
            {
                if (index >= songs.Length)
                    return false;
                Current = songs[index++];
                return true;
            }
            public void Reset() => index = 0;
            public void Dispose() { }
        }
        int _index = 0;
    }
}
