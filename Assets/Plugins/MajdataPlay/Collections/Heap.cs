using MajdataPlay.UnsafeKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#nullable enable
namespace MajdataPlay.Collections
{
    //public unsafe class Heap<T> : IEnumerable<T>, ICloneable, IDisposable where T: unmanaged
    //{
    //    public long Length
    //    {
    //        get
    //        {
    //            ThrowIfDisposed();
    //            return _length;
    //        }
    //    }
    //    public bool IsEmpty
    //    {
    //        get
    //        {
    //            ThrowIfDisposed();
    //            return _length == 0;
    //        }
    //    }
    //    public static Heap<T> Empty { get; } = new Heap<T>(0);
    //    public ref T this[long index]
    //    {
    //        get
    //        {
    //            ThrowIfDisposed();
    //            if (index >= _length || index < 0)
    //            {
    //                throw new IndexOutOfRangeException();
    //            }
    //            return ref UnsafeHelper.GetElement(_pointer, index + _startAt);
    //        }
    //    }

    //    readonly bool _leaveFree;
    //    readonly long _startAt;
    //    readonly long _length;
    //    readonly T* _pointer;

    //    bool _isDisposed;

    //    Heap()
    //    {
    //        _isDisposed = false;
    //        _pointer = default;
    //        _length = 0;
    //        _startAt = 0;
    //        _leaveFree = false;
    //    }
    //    public Heap(long length): this()
    //    {
    //        if (length == 0)
    //        {
    //            return;
    //        }
    //        else if(length < 0)
    //        {
    //            throw new ArgumentOutOfRangeException();
    //        }
    //        _pointer = UnsafeHelper.Alloc<T>(length);
    //        _length = length;
    //        _leaveFree = true;
            
    //        for (int i = 0; i < length; i++)
    //        {
    //            var ptr = Unsafe.Add<T>(_pointer, i);
    //            ref var objRef = ref Unsafe.AsRef<T>(ptr);
    //            objRef = default(T);
    //        }
    //    }
    //    public Heap(void* pointer, long length, bool leaveFree) : this(pointer, 0, length, leaveFree)
    //    {

    //    }
    //    public Heap(void* pointer, long start, long length, bool leaveFree) : this()
    //    {
    //        if(start < 0 || length < 0)
    //        {
    //            throw new ArgumentOutOfRangeException();
    //        }
    //        if (pointer is null)
    //        {
    //            throw new NullReferenceException();
    //        }
    //        _pointer = (T*)pointer;
    //        _length = length;
    //        _startAt = start;
    //        _leaveFree = leaveFree;
    //    }
        
    //    public Heap<T> Slice(long start)
    //    {
    //        if(start > _length || start < 0)
    //        {
    //            throw new ArgumentOutOfRangeException();
    //        }

    //        return new Heap<T>(_pointer, start, _length - start, false);
    //    }
    //    public Heap<T> Slice(long start, long length)
    //    {

    //        if (start > _length || start < 0 || _length - start < length)
    //        {
    //            throw new ArgumentOutOfRangeException();
    //        }

    //        return new Heap<T>(_pointer, start, length, false);
    //    }
    //    public void CopyTo(Heap<T> dest)
    //    {
    //        if (dest.Length < _length)
    //        {
    //            throw new ArgumentException("destination is shorter than the source Heap");
    //        }
    //        for (int i = 0; i < _length; i++)
    //        {
    //            dest[i] = this[i];
    //        }
    //    }
    //    public object Clone()
    //    {
    //        if (IsEmpty)
    //        {
    //            return Empty;
    //        }
    //        var newHeap = new Heap<T>(_length);
    //        CopyTo(newHeap);
    //        return newHeap;
    //    }
    //    public void Dispose()
    //    {
    //        if (!_leaveFree)
    //        {
    //            return;
    //        }
    //        ThrowIfDisposed();
    //        _isDisposed = true;
    //        if (!IsEmpty)
    //        {
    //            if (_pointer != default)
    //            {
    //                Marshal.FreeHGlobal((IntPtr)_pointer);
    //            }
    //        }
    //    }
    //    void ThrowIfDisposed()
    //    {
    //        if (_isDisposed)
    //        {
    //            throw new ObjectDisposedException(ToString());
    //        }
    //    }

    //    public static bool TryAlloc(long length, out Heap<T> heap)
    //    {
    //        try
    //        {
    //            heap = new Heap<T>(length);
    //            return true;
    //        }
    //        catch
    //        {
    //            heap = Empty;
    //            return false;
    //        }
    //    }
    //    public IEnumerator<T> GetEnumerator() => new Enumerator(this);
    //    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
    //    struct Enumerator : IEnumerator<T>
    //    {
    //        Heap<T> _heap;
    //        public T Current { get; private set; }
    //        object IEnumerator.Current { get => Current; }
    //        int index;
    //        public Enumerator(in Heap<T> heap)
    //        {
    //            this._heap = heap;
    //            Current = default;
    //            index = 0;
    //        }
    //        public bool MoveNext()
    //        {
    //            if (index >= _heap.Length)
    //            {
    //                return false;
    //            }
    //            Current = _heap[index++];
    //            return true;
    //        }
    //        public void Reset() => index = 0;
    //        public void Dispose() { }
    //    }
    //}
}
