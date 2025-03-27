using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
#nullable enable
#pragma warning disable CS8500
namespace MajdataPlay.References
{
    /// <summary>
    /// Managed references
    /// <para>When allocing, always try to transfer the object to the heap and pin it</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe struct Ref<T> : IDisposable
    {
        /// <summary>
        /// Reference to object instance
        /// </summary>
        public ref T Target
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<T>(_pointer);
        }

        void* _pointer;
        IntPtr _handle;
        public Ref(ref T obj)
        {
            var handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
            handle.AddrOfPinnedObject();

            _pointer = Unsafe.AsPointer(ref obj);
            _handle = GCHandle.ToIntPtr(handle);
        }
        /// <summary>
        /// Releases the handle of the object instance
        /// <para>After disposal, if you try to get a reference to the object from <see cref="Ref{T}"/> instance, the behavior is undefined</para>
        /// </summary>
        public void Dispose()
        {
            if (_handle == IntPtr.Zero)
                return;
            var handle = GCHandle.FromIntPtr(_handle);
            handle.Free();
            _handle = IntPtr.Zero;
            _pointer = null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyRef<T> AsReadOnly()
        {
            return new ReadOnlyRef<T>(this);
        }
        public void* AsPointer()
        {
            return _pointer;
        }
        public static implicit operator ReadOnlyRef<T>(Ref<T> @ref) => @ref.AsReadOnly();
    }
}
