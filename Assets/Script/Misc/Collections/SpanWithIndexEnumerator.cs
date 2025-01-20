using System;
#nullable enable
namespace MajdataPlay.Collections
{
    public ref struct SpanWithIndexEnumerator<T>
    {
        int _index;
        ReadOnlySpan<T> _source;
        ReadOnlySpan<T>.Enumerator _enumerator;
        public (int, T) Current => (_index, _enumerator.Current);
        public SpanWithIndexEnumerator(ReadOnlySpan<T> source)
        {
            _source = source;
            _index = -1;
            _enumerator = source.GetEnumerator();
        }
        public bool MoveNext()
        {
            _index++;
            return _enumerator.MoveNext();
        }
    }
}