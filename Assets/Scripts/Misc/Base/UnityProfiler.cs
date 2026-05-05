using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Profiling;

namespace MajdataPlay
{
    public readonly ref struct UnityProfiler
    {
        public required string Name { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("ENABLE_PROFILER")]
        public void Dispose()
        {
            Profiler.EndSample();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityProfiler Create(string name)
        {
            Profiler.BeginSample(name);
            return new UnityProfiler()
            {
                Name = name,
            };
        }
    }
}