using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable enable
namespace MajdataPlay.Buffers
{
    /// <summary>
    /// Represents an array rented from an <see cref="ArrayPool{T}"/> that will be
    /// returned to the pool when <see cref="Dispose"/> is called.
    /// </summary>
    /// <typeparam name="T">The element type of the array.</typeparam>
    /// <remarks>
    /// This type is a <see langword="struct"/>. Copying the struct will copy the
    /// reference to the same underlying array and pool. If multiple copies call
    /// <see cref="Dispose"/>, the array may be returned to the pool multiple times,
    /// which can lead to undefined behavior.
    ///
    /// To avoid this, ensure the instance is not copied and that
    /// <see cref="Dispose"/> is called only once.
    /// </remarks>
    public readonly struct ArrayOwner<T> : IEnumerable<T>, IDisposable
    {
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref this[index];
            }
        }
        /// <summary>
        /// The rented array instance.
        /// </summary>
        public readonly T[] Array;

        /// <summary>
        /// The length of the rented array.
        /// </summary>
        public readonly int Length;

        /// <summary>
        /// Indicates whether this instance represents an empty array.
        /// </summary>
        public readonly bool IsEmpty;

        public static readonly ArrayOwner<T> Empty = new ArrayOwner<T>();

        readonly ArrayPool<T> _pool;

        /// <summary>
        /// Initializes an empty <see cref="ArrayOwner{T}"/> instance.
        /// The underlying array will be <see cref="Array.Empty{T}"/>.
        /// </summary>
        public ArrayOwner()
        {
            Array = System.Array.Empty<T>();
            Length = 0;
            IsEmpty = true;
            _pool = ArrayPool<T>.Shared;
        }

        /// <summary>
        /// Initializes a new <see cref="ArrayOwner{T}"/> using an existing array
        /// rented from the specified <see cref="ArrayPool{T}"/>.
        /// </summary>
        /// <param name="array">The rented array.</param>
        /// <param name="pool">The pool that the array should be returned to.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="array"/> or <paramref name="pool"/> is <c>null</c>.
        /// </exception>
        public ArrayOwner(T[] array, ArrayPool<T> pool)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (pool is null)
            {
                throw new ArgumentNullException(nameof(pool));
            }

            Array = array;
            Length = array.Length;
            IsEmpty = array.Length == 0;
            _pool = pool;
        }
        /// <summary>
        /// Returns a reference to the element at the specified index without
        /// performing bounds checking.
        /// </summary>
        /// <param name="index">The zero-based index of the element.</param>
        /// <returns>A reference to the element at the specified index.</returns>
        /// <remarks>
        /// The caller is responsible for ensuring that the index is within bounds.
        /// Accessing an invalid index may lead to undefined behavior.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReadUnsafe(int index)
        {
            return ref Unsafe.Add(ref MemoryMarshal.GetReference(Array.AsSpan()), index);
        }

        /// <summary>
        /// Returns the rented array to the originating <see cref="ArrayPool{T}"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not return the array if it is the shared empty array.
            if (Array.Length == 0)
            {
                return;
            }

            _pool.Return(Array, true);
        }
        public ArrayOwnerEnumerator GetEnumerator()
        {
            return new ArrayOwnerEnumerator(this);
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            return Array.AsSpan();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start)
        {
            return Array.AsSpan(start);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length)
        {
            return Array.AsSpan(start, length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory()
        {
            return Array.AsMemory();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory(int start)
        {
            return Array.AsMemory(start);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory(int start, int length)
        {
            return Array.AsMemory(start, length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Span<T>(ArrayOwner<T> arrayOwner)
        {
            return arrayOwner.Array;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Memory<T>(ArrayOwner<T> arrayOwner)
        {
            return arrayOwner.Array;
        }

        public struct ArrayOwnerEnumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            int _index;
            T? _current;

            ArrayOwner<T> _array;

            public ref T Current
            {
                get
                {
                    return ref _array.ReadUnsafe(_index - 1);
                }
            }
            T IEnumerator<T>.Current
            {
                get
                {
                    return _current!;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return _current!;
                }
            }

            public ArrayOwnerEnumerator(ArrayOwner<T> array)
            {
                this._array = array;
                _index = 0;
                _current = default;
            }

            public void Dispose()
            {

            }
            public bool MoveNext()
            {
                if (_index < _array.Length)
                {
                    _current = _array.ReadUnsafe(_index);
                    _index++;
                    return true;
                }
                return false;
            }

            void IEnumerator.Reset()
            {
                _index = 0;
                _current = default!;
            }
        }
    }
}
