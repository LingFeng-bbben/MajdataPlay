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
    public class SongCollection : IEnumerable<SongDetail>
    {
        public SongDetail Current => sorted[Index];
        public int Index
        {
            get => _index;
            set
            {
                if (IsEmpty)
                    throw new ArgumentOutOfRangeException("this collection is empty");
                _index = value.Clamp(0, origin.Length - 1);
            }
        }
        public string Name { get; private set; }
        public bool IsSorted { get; private set; } = false;
        public int Count => sorted.Length;
        public bool IsEmpty => sorted.Length == 0;

        SongDetail[] sorted;
        SongDetail[] origin;
        public SongDetail this[int index] => sorted[index];
        public SongCollection(string name, in SongDetail[] pArray)
        {
            sorted = pArray;
            origin = pArray;
            Name = name;
        }
        public SongCollection()
        {
            origin = new SongDetail[0];
            sorted = origin;
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
            IsSorted = true;
            var filtered = Filter(origin, orderBy.Keyword);
            var sorted = Sort(filtered, orderBy.SortBy);

            var newIndex = sorted.FindIndex(x => x == Current);
            newIndex = newIndex is -1 ? 0 : newIndex;
            _index = newIndex;
            this.sorted = sorted;
        }
        public async Task SortAndFilterAsync(SongOrder orderBy)
        {
            await Task.Run(() => SortAndFilter(orderBy));
        }
        public void Reset()
        {
            IsSorted = false;
            if (sorted.Length != 0)
            {
                var newIndex = origin.FindIndex(x => x == Current);
                newIndex = newIndex is -1 ? 0 : newIndex;
                _index = newIndex;
            }
            else
            {
                _index = 0;
            }
            sorted = origin;
        }
        static SongDetail[] Sort(SongDetail[] origin, SortType sortType)
        {
            if (origin.IsEmpty())
                return origin;
            IEnumerable<SongDetail> result = sortType switch
            {
                SortType.ByTime => origin.OrderByDescending(o => o.AddTime),
                SortType.ByDiff => origin.OrderByDescending(o => o.Levels[4]),
                SortType.ByDes => origin.OrderBy(o => o.Designers[4]),
                SortType.ByTitle => origin.OrderBy(o => o.Title),
                _ => origin
            };
            return result.ToArray();
        }
        static SongDetail[] Filter(SongDetail[] origin, string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return origin;
            keyword = keyword.ToLower();
            var result = new Span<SongDetail>(new SongDetail[origin.Length]);
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
        public static SongCollection Empty(string name) => new SongCollection(name, Array.Empty<SongDetail>());
        public IEnumerator<SongDetail> GetEnumerator() => new Enumerator(sorted);

        // Implementation for the GetEnumerator method.
        IEnumerator IEnumerable.GetEnumerator() => origin.GetEnumerator();
        struct Enumerator : IEnumerator<SongDetail>
        {
            SongDetail[] songs;
            public SongDetail Current { get; private set; }
            object IEnumerator.Current { get => Current; }
            int index;
            public Enumerator(in SongDetail[] songs)
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
