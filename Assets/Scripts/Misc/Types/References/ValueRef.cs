using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable
#pragma warning disable CS8500
namespace MajdataPlay.References
{
    public unsafe readonly ref struct ValueRef<T> where T : unmanaged
    {
        public ref T Target
        {
            get => ref *_pointer;
        }
        readonly T* _pointer;

        public ValueRef(ref T obj)
        {
            _pointer = (T*)Unsafe.AsPointer(ref obj);
        }
        public ValueRef(T* pointer)
        {
            _pointer = pointer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyValueRef<T> AsReadOnly()
        {
            return new ReadOnlyValueRef<T>(this);
        }
        public T* AsPointer()
        {
            return _pointer;
        }
        public static implicit operator ReadOnlyValueRef<T>(ValueRef<T> @ref) => @ref.AsReadOnly();
    }
}
