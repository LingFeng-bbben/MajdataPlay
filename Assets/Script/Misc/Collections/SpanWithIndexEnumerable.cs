using System;
#nullable enable
namespace MajdataPlay.Collections
{
    public ref struct SpanWithIndexEnumerable<T>
    {
        Span<T> _source;
        public SpanWithIndexEnumerable(Span<T> source)
        {
            _source = source;
        }
        public SpanWithIndexEnumerator<T> GetEnumerator() => new SpanWithIndexEnumerator<T>(_source);
    }
}