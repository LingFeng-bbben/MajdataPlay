using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Profiling;

namespace MajdataPlay
{
    public readonly ref struct UnityProfiler
    {
        public required string Name { get; init; }

        public void Dispose()
        {
            Profiler.EndSample();
        }
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