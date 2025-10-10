using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.IL2CPP.CompilerServices;
#nullable enable
namespace MajdataPlay.Buffers;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
internal class RentedList<T> : IList<T>, ICollection<T>, IReadOnlyList<T>, IDisposable
{
    
    struct RentedArray : IMemoryOwner<T>, IDisposable
    {
        public T[] Array
        {
            get
            {
                ThrowIfDisposed();
                return _array;
            }
        }
        public Memory<T> Memory
        {
            get
            {
                ThrowIfDisposed();
                return _array;
            }
        }
        public int Length
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

        bool _isDisposed = false;
        T[] _array = System.Array.Empty<T>();
        int _length = 0;
        ArrayPool<T> _arrayPool = _sharedArrayPool;

        
        public RentedArray()
        {
            
        }
        public void Dispose()
        {
            if (_isDisposed || _array.Length == 0)
            {
                return;
            }
            _isDisposed = true;
            _arrayPool.Return(_array, true);
        }
        public static RentedArray Rent(int arraySize)
        {
            if (arraySize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arraySize), "Array size must be greater than zero.");
            }
            var array = _sharedArrayPool.Rent(arraySize);
            var rentedArray = new RentedArray
            {
                _array = array,
                _length = arraySize,
                _isDisposed = false,
            };
            return rentedArray;
        }
        void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(RentedArray), "This rented array has been disposed.");
            }
        }
    }
    public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
    {
        int _index;
        uint _version;
        T _current;

        RentedList<T> _list;

        public T Current
        {
            get
            {
                ThrowIfDisposed();
                return _current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                ThrowIfDisposed();
                if (_index == 0 || _index == _list._size + 1)
                {
                    throw new InvalidOperationException();
                }

                return Current;
            }
        }

        internal Enumerator(RentedList<T> list)
        {
            this._list = list;
            _index = 0;
            _version = list._version;
            _current = default!;
        }

        public void Dispose()
        {

        }
        public bool MoveNext()
        {
            ThrowIfDisposed();
            if (_version != _list._version)
            {
                throw new InvalidOperationException("Enumeration failed version check.");
            }
            if (_index < _list._size)
            {
                _current = _list._array[_index];
                _index++;
                return true;
            }

            return false;
        }

        void IEnumerator.Reset()
        {
            ThrowIfDisposed();
            if (_version != _list._version)
            {
                throw new InvalidOperationException("Enumeration failed version check.");
            }

            _index = 0;
            _current = default!;
        }
        void ThrowIfDisposed()
        {
            if (_list._isDisposed)
            {
                throw new ObjectDisposedException(nameof(RentedList<T>), "This rented array has been disposed.");
            }
        }
    }
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return _size;
        }
    }
    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return _rentedArray.Length;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            ThrowIfDisposed();
            if (value < _size)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Capacity cannot be less than the current size.");
            }
            if (value <= _rentedArray.Length && value > (_rentedArray.Length >> 1))
            {
                return; // No need to resize
            }
            var newRentedArray = RentedArray.Rent(value);
            if (_size > 0)
            {
                Array.Copy(_rentedArray.Array, newRentedArray.Array, _size);
            }
            _rentedArray.Dispose();
            _rentedArray = newRentedArray;
            _array = _rentedArray.Array;
        }
    }
    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            if ((uint)index >= (uint)_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            return _array[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            ThrowIfDisposed();
            if ((uint)index >= (uint)_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            _array[index] = value;
            _version++;
        }
    }
    bool ICollection<T>.IsReadOnly
    {
        get
        {
            return false;
        }
    }

    int _size = 0;
    uint _version = 0;
    T[] _array;
    bool _isDisposed = false;
    RentedArray _rentedArray;

    readonly static ArrayPool<T> _sharedArrayPool = Pool<T>.ArrayPool;
    ~RentedList()
    {
        Dispose();
    }
    public RentedList()
    {
        //List
        _rentedArray = RentedArray.Rent(0);
        _array = _rentedArray.Array;
    }
    public RentedList(IEnumerable<T> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items), "Items cannot be null.");
        }
        _rentedArray = RentedArray.Rent(0);
        _array = _rentedArray.Array;
        AddRange(items);
    }
    public RentedList(int capacity)
    {
        _rentedArray = RentedArray.Rent(capacity);
        _array = _rentedArray.Array;
    }
    public void Add(T item)
    {
        ThrowIfDisposed();
        if (_size >= _array.Length)
        {
            if (_array.Length != 0)
            {
                Capacity = _array.Length << 1;
            }
            else
            {
                Capacity = 1;
            }
        }
        _array[_size++] = item;
        _version++;
    }
    public void AddRange(IEnumerable<T> items)
    {
        ThrowIfDisposed();
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items), "Items cannot be null.");
        }
        foreach (var item in items)
        {
            Add(item);
        }
    }
    public void AddRange(ReadOnlySpan<T> items)
    {
        ThrowIfDisposed();
        for (var i = 0; i < items.Length; i++)
        {
            Add(items[i]);
        }
    }
    public void Insert(int index, T item)
    {
        ThrowIfDisposed();
        if ((uint)index > (uint)_size)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }
        if (_size >= _array.Length)
        {
            Capacity = _array.Length << 1;
        }
        if (index < _size)
        {
            Array.Copy(_array, index, _array, index + 1, _size - index);
        }
        _array[index] = item;
        _size++;
        _version++;
    }
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public void Clear()
    {
        ThrowIfDisposed();
        if (_size == 0)
        {
            return; // Nothing to clear
        }
        _size = 0;
        _version++;
        Array.Clear(_array, 0, _size);
    }
    public int IndexOf(T item)
    {
        ThrowIfDisposed();
        for (var i = 0; i < _size; i++)
        {
            var current = _array[i];
            if (EqualityComparer<T>.Default.Equals(current, item)) 
            {
                return i;
            }
        }
        return -1;
    }
    public bool Remove(T item)
    {
        ThrowIfDisposed();
        var index = IndexOf(item);
        if (index < 0)
        {
            return false;
        }
        RemoveAt(index);
        _version++;
        return true;
    }
    public void RemoveAt(int index)
    {
        ThrowIfDisposed();
        if ((uint)index >= (uint)_size)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }
        _size--;
        if(index != _size - 1)
        {
            Array.Copy(_array, index + 1, _array, index, _size - index);
        }

        _array[_size] = default!;
        _version++;
    }
    public bool Contains(T item)
    {
        ThrowIfDisposed();
        for (var i = 0; i < _size; i++)
        {
            var current = _array[i];
            if (EqualityComparer<T>.Default.Equals(current, item)) 
            {
                return true;
            }
        }
        return false;
    }
    public void CopyTo(T[] array, int arrayIndex)
    {
        ThrowIfDisposed();
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array), "Array cannot be null.");
        }
        if (arrayIndex < 0 || arrayIndex + _size > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index is out of range.");
        }
        else if(_size == 0)
        {
            return;
        }
        Array.Copy(_array, 0, array, arrayIndex, _size);
    }
    public void CopyTo(Span<T> span)
    {
        ThrowIfDisposed();
        if (span.Length < _size)
        {
            throw new ArgumentException("Span is too small to copy the elements.");
        }
        else if(_size == 0)
        {
            return;
        }
        var array = _array.AsSpan(0, _size);
        array.CopyTo(span);
    }
    public void Dispose()
    {
        if(_isDisposed)
        {
            return;
        }
        _isDisposed = true;
        _rentedArray.Dispose();
    }
    public T[] ToArray()
    {
        ThrowIfDisposed();
        if(_size == 0)
        {
            return Array.Empty<T>();
        }
        var array = new T[_size];
        Array.Copy(_array, array, _size);

        return array;
    }
    void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(RentedList<T>), "This rented array has been disposed.");
        }
    }
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

