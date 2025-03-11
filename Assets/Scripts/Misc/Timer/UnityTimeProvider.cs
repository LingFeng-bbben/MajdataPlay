using Cysharp.Threading.Tasks;
using MajdataPlay.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Timer
{
    public class UnityTimeProvider: ITimeProvider
    {
        public BuiltInTimeProvider Type { get; } = BuiltInTimeProvider.Unity;
        public long Ticks { get; private set; } = 0;

        DateTime _startAt = DateTime.Now;
        CancellationTokenSource _cts = new();

        public UnityTimeProvider() 
        {
            Update().Forget();
        }
        ~UnityTimeProvider()
        {
            _cts.Cancel();
        }
        async UniTaskVoid Update()
        {
            var token = _cts.Token;
            await Task.Delay(5000);
            await UniTask.Yield();
            while (true)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    var now = _startAt.AddSeconds(Time.unscaledTimeAsDouble);
                    Ticks = (now - _startAt).Ticks;
                    await UniTask.Yield(PlayerLoopTiming.LastPreUpdate);
                }
                catch(Exception e)
                {
                    MajDebug.LogException(e);
                }
                
            }
        }
    }
}
