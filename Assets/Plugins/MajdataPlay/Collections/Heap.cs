using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#nullable enable
namespace MajdataPlay.Collections
{
    using Unsafe = System.Runtime.CompilerServices.Unsafe;
    public unsafe struct Heap<T> : IEnumerable<T>, ICloneable, IDisposable
    {
        public long Length
        {
            get
            {
                ThrowIfDisposed();
                return _length;
            }
        }
        public bool IsEmpty
        {
            get
            {
                ThrowIfDisposed();
                return _length == 0;
            }
        }
        public static Heap<T> Empty { get; } = new Heap<T>(0);
        public ref T this[long index]
        {
            get
            {
                ThrowIfDisposed();
                if (index >= Length || index < 0)
                    throw new IndexOutOfRangeException();
                return ref GetElement(index + _start);
            }
        }

        readonly bool _selfAllocation;
        readonly long _start;
        readonly long _length;
        readonly void* _pointer;
        readonly object? _object;

        bool _isDisposed;

        Heap(bool _)
        {
            _object = null;
            _isDisposed = false;
            _pointer = default;
            _length = 0;
            _start = 0;
            _selfAllocation = false;
        }
        public Heap(long length): this(true)
        {
            if (length == 0)
            {
                return;
            }
            else if(length < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            _pointer = (void*)Marshal.AllocHGlobal(new IntPtr(length * Unsafe.SizeOf<T>()));
            _length = length;
            _selfAllocation = true;
            
            for (int i = 0; i < length; i++)
            {
                var ptr = Unsafe.Add<T>(_pointer, i);
                ref var objRef = ref Unsafe.AsRef<T>(ptr);
                objRef = default(T);
            }
        }
        public Heap(void* pointer, long start, long length) : this(true)
        {
            if(start < 0 || length < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (pointer is null)
                throw new NullReferenceException();
            _pointer = pointer;
            _length = length;
            _start = start;
        }
        public Heap(void* pointer, long length) : this(pointer, 0, length)
        {

        }
        public Heap(T[] array, int start, int length) : this(true)
        {
            if (start < 0 || length < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            _object = array;
            _start = start;
            _length = length;
        }
        public Heap(T[] array, int length) : this(array, 0, length)
        {

        }
        public Heap(T[] array) : this(array, 0, array.Length)
        {

        }
        public Heap<T> Slice(long start)
        {
            if(start > _length || start < 0)
                throw new ArgumentOutOfRangeException();

            var array = _object as T[];
            if(array is null)
            {
                return new Heap<T>(_pointer, start, _length - start);
            }
            else
            {
                if(start > int.MaxValue)
                    throw new ArgumentOutOfRangeException();
                return new Heap<T>(array, (int)start, (int)(_length - start));
            }
        }
        public Heap<T> Slice(long start, long length)
        {

            if (start > _length || start < 0 || _length - start < length)
                throw new ArgumentOutOfRangeException();

            var array = _object as T[];
            if (array is null)
            {
                return new Heap<T>(_pointer, start, length);
            }
            else
            {
                if (start > int.MaxValue || length > int.MaxValue)
                    throw new ArgumentOutOfRangeException();
                return new Heap<T>(array, (int)start, (int)(length));
            }
        }
        public void CopyTo(Heap<T> dest)
        {
            if (dest.Length < _length)
                throw new ArgumentException("destination is shorter than the source Heap");
            for (int i = 0; i < _length; i++)
                dest[i] = this[i];
        }
        public object Clone()
        {
            if (IsEmpty)
                return Empty;
            var newHeap = new Heap<T>(_length);
            CopyTo(newHeap);
            return newHeap;
        }
        public void Dispose()
        {
            if (!_selfAllocation)
                return;
            ThrowIfDisposed();
            _isDisposed = true;
            if (!IsEmpty)
            {
                var array = _object as T[];
                if (array is not null)
                {
                    return;
                }
                else if (_pointer != default)
                {
                    Marshal.FreeHGlobal((IntPtr)_pointer);
                }
            }
        }
        void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(ToString());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ref T GetElement(long elementOffset)
        {
            var array = _object as T[];
            if(array is not null)
            {
                return ref array[elementOffset];
            }
            else
            {
                var ptr = AddOffset(elementOffset);

                return ref Unsafe.AsRef<T>(ptr);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void* AddOffset(long elementOffset)
        {
            var ptr = (byte*)_pointer;
            var elementSize = Unsafe.SizeOf<T>();

            return ptr + elementOffset * elementSize;
        }
        public static bool TryAlloc(long length, out Heap<T> heap)
        {
            try
            {
                heap = new Heap<T>(length);
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
