using System;
using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MajdataPlay.Buffers
{
    internal unsafe class NativeArrayMemoryManager<T> : MemoryManager<T> where T : struct
    {
        readonly int _length;
        readonly void* _unityNativePointer;
        readonly NativeArray<T> _nativeArray;
        readonly bool _isManaged;

        public NativeArrayMemoryManager(NativeArray<T> nativeArray, bool isManaged)
        {
            _unityNativePointer = NativeArrayUnsafeUtility.GetUnsafePtr(nativeArray);
            _length = nativeArray.Length;
            _nativeArray = nativeArray;
            _isManaged = isManaged;
        }

        public override Span<T> GetSpan()
        {
            return new Span<T>(_unityNativePointer, _length);
        }
        public override MemoryHandle Pin(int elementIndex = 0)
        {
            return new MemoryHandle(Unsafe.Add<T>(_unityNativePointer, elementIndex));
        }
        public override void Unpin()
        {

        }
        protected override void Dispose(bool disposing)
        {
            if(!_isManaged)
            {
                return;
            }
            _nativeArray.Dispose();
        }
    }
}
