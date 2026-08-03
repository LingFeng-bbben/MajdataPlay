using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Collections
{
    internal class MyFavoriteSongCollection : SongCollection , ICollection<ISongDetail>
    {
        public bool IsReadOnly => false;

        readonly List<ISongDetail> _dataSet = new();
        readonly HashSet<string> _hashSet = new();

        public MyFavoriteSongCollection() : base("MyFavorites", Array.Empty<ISongDetail>())
        {
            Type = ChartStorageType.FavoriteList;
        }
        public MyFavoriteSongCollection(List<ISongDetail> dataSet, HashSet<string> hashSet) : base("MyFavorites", dataSet.ToArray())
        {
            Type = ChartStorageType.FavoriteList;
            _dataSet = dataSet;
            _hashSet = hashSet;
        }

        public void Add(ISongDetail item)
        {
            NormalizeIndex();
            var currentHash = IsEmpty ? null : Current.Hash;
            if (!_hashSet.Add(item.Hash))
            {
                return;
            }
            _dataSet.Add(item);
            Origin = _dataSet.ToArray();
            if(!IsSorted)
            {
                Sorted = Origin;
            }
            else
            {
                var sorted = new List<ISongDetail>(Sorted);
                sorted.Add(item);
                Sorted = sorted.ToArray();
            }
            NormalizeIndex();
            if (currentHash is not null)
            {
                SetCursor(currentHash);
            }
        }
        public void Clear()
        {
            _dataSet.Clear();
            _hashSet.Clear();
            Origin = Array.Empty<ISongDetail>();
            Sorted = Origin;
            NormalizeIndex();
        }
        public bool Contains(ISongDetail item)
        {
            return _hashSet.Contains(item.Hash);
        }
        public bool Contains(string hashBase64Str)
        {
            return _hashSet.Contains(hashBase64Str);
        }
        public void CopyTo(ISongDetail[] array, int arrayIndex)
        {
            _dataSet.CopyTo(array, arrayIndex);
        }
        public bool Remove(ISongDetail item)
        {
            return Remove(item.Hash);
        }
        public bool Remove(string hashBase64Str)
        {
            NormalizeIndex();
            var currentHash = IsEmpty ? null : Current.Hash;
            if (!_hashSet.Remove(hashBase64Str))
            {
                return false;
            }
            var index = _dataSet.FindIndex(x => x.Hash == hashBase64Str);
            if(index == -1)
            {
                throw new KeyNotFoundException();
            }
            _dataSet.RemoveAt(index);
            Origin = _dataSet.ToArray();
            if (!IsSorted)
            {
                Sorted = Origin;
            }
            else
            {
                Sorted = Sorted.Where(x => x.Hash != hashBase64Str).ToArray();
            }
            NormalizeIndex();
            if (currentHash is not null && currentHash != hashBase64Str)
            {
                SetCursor(currentHash);
            }
            return true;
        }
        public HashSet<string> ExportHashSet()
        {
            return _hashSet;
        }
    }
}
