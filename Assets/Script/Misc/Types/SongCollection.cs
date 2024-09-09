using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MajdataPlay.Types
{
    public class SongCollection : IEnumerable<SongDetail>
    {
        public string Name { get; private set; }
        public int Count => songs.Length;


        private SongDetail[] songs;
        public SongCollection(string name,in SongDetail[] pArray)
        {
            songs = pArray;
            Name = name;
        }

        public SongCollection()
        {
            songs = new SongDetail[0];
            Name = null;
        }
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
    }
}
