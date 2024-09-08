using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongCollection : IEnumerable
{
    private SongDetail[] songs;
    public SongCollection(SongDetail[] pArray)
    {
        songs = new SongDetail[pArray.Length];

        for (int i = 0; i < pArray.Length; i++)
        {
            songs[i] = pArray[i];
        }
    }

    // Implementation for the GetEnumerator method.
    IEnumerator IEnumerable.GetEnumerator()
    {
        return songs.GetEnumerator();
    }
}
