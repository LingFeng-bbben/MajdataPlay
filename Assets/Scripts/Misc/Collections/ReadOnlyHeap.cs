using System;
using System.Collections;
using System.Collections.Generic;

namespace MajdataPlay.Collections
{
    public unsafe struct ReadOnlyHeap<T>: IEnumerable<T>, ICloneable, IDisposable
    {
        public long Length
        {
            get
            {
                return _heap.Length;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return _heap.IsEmpty;
            }
        }
        public static Heap<T> Empty { get; } = new Heap<T>(0);
        public ref readonly T this[long index]
        {
            get
            {
                return ref _heap[index];
            }
        }
        readonly Heap<T> _heap;

        ReadOnlyHeap(Heap<T> heap)
        {
            _heap = heap;
        }
        
        public ReadOnlyHeap<T> Slice(long start)
        {
            return _heap.Slice(start);
        }
        public ReadOnlyHeap<T> Slice(long start, long length)
        {
            return _heap.Slice(start, length);
        }
        public void CopyTo(Heap<T> dest)
        {
            _heap.CopyTo(dest);
        }
        public void Dispose()
        {
            _heap.Dispose();
        }
        public object Clone()
        {
            return (ReadOnlyHeap<T>)_heap.Clone();
        }
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_heap).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_heap).GetEnumerator();

        public static implicit operator ReadOnlyHeap<T>(Heap<T> heap)
        {
            return new ReadOnlyHeap<T>(heap);
        }
    }
}
