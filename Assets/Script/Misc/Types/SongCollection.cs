using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MajdataPlay.Types
{
    public class SongCollection : IEnumerable<SongDetail>
    {
        public int Count => songs.Length;


        private SongDetail[] songs;
        public SongCollection(SongDetail[] pArray)
        {
            songs = new SongDetail[pArray.Length];

            for (int i = 0; i < pArray.Length; i++)
            {
                songs[i] = pArray[i];
            }
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
