#nullable enable
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
using System.Runtime.CompilerServices;

namespace MajdataPlay
{
    internal unsafe static class Majdata<T>
    {
        /// <summary>
        /// Get or set a globally unique instance
        /// </summary>
        public static ref T? Instance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref _instance;
            }
        }
        public static bool IsNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _instance is null;
            }
        }

        static T? _instance = default;

        /// <summary>
        /// Release the instance
        /// </summary>
        public static void Free()
        {
            _instance = default;
        }
    }
}
