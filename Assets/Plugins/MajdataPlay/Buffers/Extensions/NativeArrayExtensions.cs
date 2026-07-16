using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

namespace MajdataPlay.Buffers
{
    public static class NativeArrayExtensions
    {
        public static Memory<T> AsMemory<T>(this NativeArray<T> buffer) where T : struct
        {
            var manager = new NativeArrayMemoryManager<T>(buffer, false);

            return manager.Memory;
        }
        public static IMemoryOwner<T> AsMemoryOwner<T>(this NativeArray<T> buffer) where T : struct
        {
            var manager = new NativeArrayMemoryManager<T>(buffer, true);

            return manager;
        }
    }
}
