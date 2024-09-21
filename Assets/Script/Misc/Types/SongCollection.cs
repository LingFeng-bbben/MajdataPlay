using MajdataPlay.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Types
{
    public class SongCollection : IEnumerable<SongDetail>
    {
        public SongDetail Current => songs[Index];
        public int Index
        {
            get => _index;
            set
            {
                if(IsEmpty)
                    throw new ArgumentOutOfRangeException("this collection is empty");
                _index = value.Clamp(0, songs.Length - 1);
            }
        }
        public string Name { get; private set; }
        public int Count => songs.Length;
        public bool IsEmpty => songs.Length == 0;

        SongDetail[] songs;
        public SongDetail this[int index] => songs[index];
        public SongCollection(string name,in SongDetail[] pArray)
        {
            songs = pArray;
            Name = name;
        }
        public SongCollection()
        {
            songs = new SongDetail[0];
            Name = string.Empty;
        }
        public bool MoveNext()
        {
            if(Index >= Count - 1)
                return false;
            Index++;
            return true;
        }
        public void Move(int diff) => Index = (Index + diff).Clamp(0, Count - 1);
        public static SongCollection Empty(string name) => new SongCollection(name, Array.Empty<SongDetail>());
        public IEnumerator<SongDetail> GetEnumerator() => new Enumerator(songs);

        // Implementation for the GetEnumerator method.
        IEnumerator IEnumerable.GetEnumerator() => songs.GetEnumerator();
        struct Enumerator: IEnumerator<SongDetail>
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
                if(index >= songs.Length)
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
