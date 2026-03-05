using Cysharp.Threading.Tasks;
using MajdataPlay.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Timer
{
    public sealed class UnityTimeProvider : ITimeProvider
    {
        public BuiltInTimeProvider Type { get; } = BuiltInTimeProvider.Unity;
        public long Ticks { get; private set; } = 0;

        public void OnPreUpdate()
        {
            Ticks = TimeSpan.FromSeconds(Time.unscaledTimeAsDouble).Ticks;
        }
    }
}
