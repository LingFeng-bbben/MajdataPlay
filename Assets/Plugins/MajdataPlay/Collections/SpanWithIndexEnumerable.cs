using System;
#nullable enable
namespace MajdataPlay.Collections
{
    public ref struct SpanWithIndexEnumerable<T>
    {
        ReadOnlySpan<T> _source;
        public SpanWithIndexEnumerable(ReadOnlySpan<T> source)
        {
            _source = source;
        }
        public SpanWithIndexEnumerator<T> GetEnumerator() => new SpanWithIndexEnumerator<T>(_source);
    }
}