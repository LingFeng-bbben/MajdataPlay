#nullable enable
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MajdataPlay
{
    internal unsafe static class Majdata<T>
    {
        /// <summary>
        /// Get or set a globally unique instance
        /// </summary>
        public static ref T? Instance
        {
            get => ref System.Runtime.CompilerServices.Unsafe.AsRef<T?>(_instancePtr);
        }
        public static void* Pointer
        {
            get => _instancePtr;
        }
        public static bool IsNull => _instance is null;

        static T? _instance = default;
        static void* _instancePtr = default;
        static void* _handlePtr = default;

        static Majdata()
        {
            var handle = GCHandle.Alloc(_instance, GCHandleType.Pinned);
            _instancePtr = System.Runtime.CompilerServices.Unsafe.AsPointer(ref _instance);
            _handlePtr = (void*)GCHandle.ToIntPtr(handle);
        }

        /// <summary>
        /// Release the instance
        /// </summary>
        public static void Free()
        {
            _instance = default;
        }
    }
}
