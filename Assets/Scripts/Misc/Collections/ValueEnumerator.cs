#nullable enable
namespace MajdataPlay.Collections
{
    public struct ValueEnumerator<T>
    {
        public T? Current { get; private set; }
        int _index;
        T[] _source;
        public ValueEnumerator(T[] source)
        {
            _index = 0;
            _source = source;
            Current = default;
        }
        public bool MoveNext()
        {
            if(_index >= _source.Length)
                return false;
            Current = _source[_index++];
            return true;
        }
    }
}
