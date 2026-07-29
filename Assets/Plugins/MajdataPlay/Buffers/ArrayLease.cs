using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MajdataPlay.Buffers
{
    public struct ArrayLease<T> : IDisposable
    {
        public T[] Array
        {
            get
            {
                ThrowIfDispose();
                return _array;
            }
        }
        public Span<T> Span
        {
            get
            {
                ThrowIfDispose();
                return _array;
            }
        }
        public Memory<T> Memory
        {
            get
            {
                ThrowIfDispose();
                return _array;
            }
        }
        public int Length
        {
            get => _length;
        }

        int _isDisposed;

        readonly T[] _array;
        readonly int _length;
        readonly bool _clearArrayWhenReturn;
        readonly ArrayPool<T> _pool;

        public ArrayLease(T[] array, ArrayPool<T> pool, bool clearArrayWhenReturn)
        {
            _array = array;
            _pool = pool;
            _clearArrayWhenReturn = clearArrayWhenReturn;
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
            {
                return;
            }
            _pool.Return(_array, _clearArrayWhenReturn);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        void ThrowIfDispose()
        {
            if(_isDisposed == 0)
            {
                return;
            }
            throw new ObjectDisposedException(nameof(ArrayLease<T>));
        }
        public static ArrayLease<T> Rent(int minimumLength, bool clearArray = true, bool clearArrayWhenReturn = false)
        {
            var array = Pool<T>.RentArray(minimumLength, clearArray);

            return new ArrayLease<T>(array, Pool<T>.ArrayPool, clearArrayWhenReturn);
        }

        public static implicit operator Span<T>(ArrayLease<T> lease)
        {
            lease.ThrowIfDispose();
            return lease._array;
        }
        public static implicit operator Memory<T>(ArrayLease<T> lease)
        {
            lease.ThrowIfDispose();
            return lease._array;
        }
        public static implicit operator T[](ArrayLease<T> lease)
        {
            lease.ThrowIfDispose();
            return lease._array;
        }
    }
}
