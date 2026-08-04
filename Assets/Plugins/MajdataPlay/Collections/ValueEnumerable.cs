using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Collections
{
    public struct ValueEnumerable<T>
    {
        T[] _source;

        public ValueEnumerable(T[] source)
        {
            _source = source;
        }
        public ValueEnumerator<T> GetEnumerator() => new ValueEnumerator<T>(_source);
    }
}
