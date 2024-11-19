using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable
#pragma warning disable CS8500
namespace MajdataPlay.References
{
    public unsafe readonly struct Ref<T> : IDisposable
    {
        public ref T? Target
        {
            get => ref *_pointer;
        }

        readonly T* _pointer;
        readonly IntPtr _handle;
        public Ref(ref T obj)
        {
            var handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
            handle.AddrOfPinnedObject();

            _pointer = (T*)Unsafe.AsPointer(ref obj);
            _handle = GCHandle.ToIntPtr(handle);
        }
        public void Dispose()
        {
            if (_handle == IntPtr.Zero)
                return;
            var handle = GCHandle.FromIntPtr(_handle);
            handle.Free();
            _handle = IntPtr.Zero;
            _pointer = null;
        }
    }
}
