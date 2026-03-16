using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#nullable enable
namespace MajdataPlay.Buffers
{
    public class RentedList<T> : IList<T>, ICollection<T>, IReadOnlyList<T>, IDisposable
    {
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
                if(value == _rentedArray.Length)
                {
                    return;
                }
                var newRentedArray = new ArrayOwner<T>(_sharedPool.Rent(value), _sharedPool);
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
        ArrayOwner<T> _rentedArray;

        readonly static ArrayPool<T> _sharedPool = Pool<T>.ArrayPool;
        ~RentedList()
        {
            Dispose();
        }
        public RentedList()
        {
            //List
            _rentedArray = ArrayOwner<T>.Empty;
            _array = _rentedArray.Array;
        }
        public RentedList(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items), "Items cannot be null.");
            }
            _rentedArray = ArrayOwner<T>.Empty;
            _array = _rentedArray.Array;
            AddRange(items);
        }
        public RentedList(int capacity)
        {
            _rentedArray = new ArrayOwner<T>(_sharedPool.Rent(capacity), _sharedPool);
            _array = _rentedArray.Array;
        }
        public void Add(T item)
        {
            ThrowIfDisposed();
            EnsureCapacity(_size + 1);
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
            if ((uint)index >= (uint)_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            EnsureCapacity(_size + 1);
            if (index < _size - 1)
            {
                Array.Copy(_array, index, _array, index + 1, _size - index - 1);
            }
            _array[index] = item;
            _size++;
            _version++;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadUnsafe(int index)
        {
            return Unsafe.Add(ref MemoryMarshal.GetReference(_array.AsSpan()), index);
        }
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
            if (index != _size - 1)
            {
                Array.Copy(_array, index + 1, _array, index, _size - index - 1);
            }
            _size--;
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
            else if (_size == 0)
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
            else if (_size == 0)
            {
                return;
            }
            var array = _array.AsSpan(0, _size);
            array.CopyTo(span);
        }
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            _rentedArray.Dispose();
        }
        public T[] ToArray()
        {
            ThrowIfDisposed();
            if (_size == 0)
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
        void EnsureCapacity(int minCapacity)
        {
            if (_array.Length < minCapacity)
            {
                var newCapacity = ((_array.Length == 0) ? 16 : (_array.Length * 2));

                if (newCapacity < minCapacity)
                {
                    newCapacity = minCapacity;
                }
                Capacity = newCapacity;
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
}