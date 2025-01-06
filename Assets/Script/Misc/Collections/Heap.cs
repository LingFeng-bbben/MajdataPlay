using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#nullable enable
namespace MajdataPlay.Collections
{
    public unsafe class Heap<T> : IEnumerable<T>, ICloneable, IDisposable where T : unmanaged
    {
        public int Length => (int)_length;
        public long LongLength => _length;
        public bool IsEmpty => _length == 0;
        public static Heap<T> Empty { get; } = new Heap<T>(0);
        public T this[long index]
        {
            get
            {
                if (index >= LongLength || index < 0)
                    throw new IndexOutOfRangeException();
                return _pointer[index];
            }
            set => _pointer[index] = value;

        }

        readonly bool _need2Free = true;
        readonly long _length;
        readonly T* _pointer;
        readonly GCHandle? _ptrHandle = null;

        bool _isDisposed = false;


        public Heap(long length)
        {
            _pointer = (T*)Marshal.AllocHGlobal(new IntPtr(length));
            _length = length;
            for (int i = 0; i < length; i++)
                _pointer[i] = default;
        }
        public Heap(T[] array) : this(array, 0, array.Length)
        {

        }
        public Heap(T[] array, int start, int length)
        {
            //var pointer = Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(array));
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            var pointer = handle.AddrOfPinnedObject();
            _pointer = (T*)Unsafe.Add<T>(pointer.ToPointer(), start);
            _length = length;
            _need2Free = false;
        }
        public Heap(T* pointer, long length)
        {
            if (pointer is null)
                throw new NullReferenceException();
            _pointer = pointer;
            _length = length;
        }
        public Heap(void* pointer, long length) : this((T*)pointer, length)
        {

        }
        public Heap(IntPtr pointer, long length) : this((T*)pointer, length)
        {

        }
        ~Heap()
        {
            if (!_isDisposed)
                Dispose();
        }
        public void CopyTo(Heap<T> dest)
        {
            if (dest.LongLength < _length)
                throw new ArgumentException("destination is shorter than the source Heap");
            for (int i = 0; i < _length; i++)
                dest[i] = _pointer[i];
        }
        public object Clone()
        {
            if (IsEmpty)
                return Empty;
            var ptr = (T*)Marshal.AllocHGlobal(new IntPtr(_length));
            var newHeap = new Heap<T>(ptr, _length);
            for (int i = 0; i < _length; i++)
                newHeap[i] = _pointer[i];
            return newHeap;
        }
        public void Dispose()
        {
            ThrowIfDisposed(_isDisposed);
            _isDisposed = true;
            if (!IsEmpty)
            {
                if (_need2Free)
                    Marshal.FreeHGlobal((IntPtr)_pointer);
                else if (_ptrHandle is GCHandle handle)
                    handle.Free();
            }
        }
        void ThrowIfDisposed(bool condition)
        {
            if (condition)
                throw new ObjectDisposedException(ToString());
        }
        public static bool TryAlloc(long length, out Heap<T> heap)
        {
            try
            {
                var ptr = (T*)Marshal.AllocHGlobal(new IntPtr(length));
                heap = new Heap<T>(ptr, length);
                return true;
            }
            catch
            {
                heap = Empty;
                return false;
            }
        }
        public IEnumerator<T> GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
        struct Enumerator : IEnumerator<T>
        {
            Heap<T> _heap;
            public T Current { get; private set; }
            object IEnumerator.Current { get => Current; }
            int index;
            public Enumerator(in Heap<T> heap)
            {
                this._heap = heap;
                Current = default;
                index = 0;
            }
            public bool MoveNext()
            {
                if (index >= _heap.Length)
                    return false;
                Current = _heap[index++];
                return true;
            }
            public void Reset() => index = 0;
            public void Dispose() { }
        }
    }
}
