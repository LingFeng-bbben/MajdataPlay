using MajdataPlay.Extensions;
using MajdataPlay.Types;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Collections
{
    public class SongCollection : IEnumerable<ISongDetail>, IReadOnlyCollection<ISongDetail>
    {
        public ISongDetail Current => _sorted[Index];
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
        public ChartStorageLocation Location { get; init; } = ChartStorageLocation.Local;
        public ChartStorageType Type { get; init; } = ChartStorageType.List;
        public bool IsOnline => Location == ChartStorageLocation.Online;
        public string Name { get; private set; }
        public bool IsSorted { get; private set; } = false;
        public int Count => _sorted.Length;
        public bool IsEmpty => _sorted.Length == 0;
        public DanInfo? DanInfo { get; init; }

        protected ISongDetail[] _sorted;
        protected ISongDetail[] _origin;
        public ISongDetail this[int index] => _sorted[index];
        public SongCollection(string name, in ISongDetail[] pArray)
        {
            _sorted = pArray;
            _origin = pArray;
            Name = name;
        }
        public SongCollection()
        {
            _origin = new ISongDetail[0];
            _sorted = _origin;
            Name = string.Empty;
        }
        public bool MoveNext()
        {
            if (Index >= Count - 1)
                return false;
            Index++;
            return true;
        }
        public void Move(int diff) => Index = (Index + diff).Clamp(0, Count - 1);
        public void SortAndFilter(SongOrder orderBy)
        {
            if(Type == ChartStorageType.Dan)
            {
                return;
            }
            IsSorted = true;
            var filtered = Filter(_origin, orderBy.Keyword);
            var sorted = Sort(filtered, orderBy.SortBy);

            var newIndex = sorted.FindIndex(x => x == Current);
            newIndex = newIndex is -1 ? 0 : newIndex;
            _index = newIndex;
            this._sorted = sorted;
        }
        public async Task SortAndFilterAsync(SongOrder orderBy)
        {
            await Task.Run(() => SortAndFilter(orderBy));
        }
        public void Reset()
        {
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
        public ISongDetail[] ToArray() => _origin;
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
                return origin;
            keyword = keyword.ToLower();
            var result = new Span<ISongDetail>(new ISongDetail[origin.Length]);
            var i = 0;
            foreach (var song in origin)
            {
                var isTitleMatch = song.Title.ToLower().Contains(keyword);
                var isArtistMatch = song.Artist.ToLower().Contains(keyword);
                var isDesMatch = song.Designers.Any(p => p == null ? false : p.ToLower().Contains(keyword));
                var isLevelMatch = song.Levels.Any(p => p == null ? false : p.ToLower() == keyword);

                var isMatch = isTitleMatch || isArtistMatch || isDesMatch || isLevelMatch;
                if (isMatch)
                    result[i++] = song;
            }
            return result.Slice(0, i).ToArray();
        }
        public static SongCollection Empty(string name) => new SongCollection(name, Array.Empty<ISongDetail>());
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
