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
            if (!_hashSet.Add(item.Hash))
            {
                return;
            }
            _dataSet.Add(item);
            _origin = _dataSet.ToArray();
            if(!IsSorted)
            {
                _sorted = _origin;
            }
            else
            {
                var sorted = new List<ISongDetail>(_sorted);
                sorted.Add(item);
                _sorted = sorted.ToArray();
            }
        }
        public void Clear()
        {
            _dataSet.Clear();
            _hashSet.Clear();
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
            if(!_hashSet.Remove(item.Hash))
            {
                return false;
            }
            _dataSet.Remove(item);
            _origin = _dataSet.ToArray();
            if (!IsSorted)
            {
                _sorted = _origin;
            }
            else if (_sorted.Any(x => x.Hash == item.Hash))
            {
                var sorted = new List<ISongDetail>(_sorted);
                sorted.Remove(item);
                _sorted = sorted.ToArray();
            }
            return true;
        }
        public bool Remove(string hashBase64Str)
        {
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
            _origin = _dataSet.ToArray();
            if (!IsSorted)
            {
                _sorted = _origin;
            }
            return true;
        }
        public HashSet<string> ExportHashSet()
        {
            return _hashSet;
        }
    }
}
