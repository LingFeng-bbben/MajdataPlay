using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Buffers
{
    internal struct RentedArray<T> : IMemoryOwner<T>, IDisposable
    {
        const int MAX_ARRAY_LENGTH = 256 * 1024 * 1024; // 256MB
        const int MAX_ARRAY_PER_BUCKET = 100;

        public T[] Array
        {
            get
            {
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
                return _length;
            }
        }

        bool _isDisposed;
        readonly T[] _array;
        readonly int _length;
        readonly ArrayPool<T> _arrayPool;
        
        readonly static ArrayPool<T> _sharedArrayPool = ArrayPool<T>.Create(MAX_ARRAY_LENGTH, MAX_ARRAY_PER_BUCKET);
        //public RentedArray()
        //{
        //    _arrayPool = _sharedArrayPool;
        //    _array = Array.Empty<T>();
        //}
        public RentedArray(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), $"Length must be between 1 and {MAX_ARRAY_LENGTH}.");
            }
            _isDisposed = false;
            _arrayPool = _sharedArrayPool;
            _array = _arrayPool.Rent(length);
            _length = _array.Length;
        }
        public RentedArray(T[] rentedArray, ArrayPool<T> arrayPool)
        {
            if (rentedArray is null)
            {
                throw new ArgumentNullException(nameof(rentedArray));
            }
            else if(arrayPool is null)
            {
                throw new ArgumentNullException(nameof(arrayPool));
            }
            _isDisposed = false;
            _arrayPool = arrayPool;
            _array = rentedArray;
            _length = rentedArray.Length;
        }
        public void Dispose() 
        {
            if(_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            _arrayPool.Return(_array, true);
        }
        public static RentedArray<T> Rent(int arraySize)
        {
            return new RentedArray<T>(arraySize);
        }
        void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(RentedArray<T>), "This rented array has been disposed.");
            }
        }
    }
}
