using Cysharp.Threading.Tasks;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
#nullable enable
namespace MajdataPlay
{
    public static class UnityEngineObjectExtensions
    {
        public static async ValueTask<bool> IsNativeAliveAsync(this Object source)
        {
            try
            {
                await using (UniTask.ReturnToCurrentSynchronizationContext())
                {
                    await UniTask.SwitchToMainThread();
                    return source != null;
                }
            }
            catch(Exception e)
            {
                MajDebug.LogException(e);
                throw;
            }
        }
        public static bool IsNativeAlive(this Object source)
        {
            if(Thread.CurrentThread.ManagedThreadId != MajEnv.MainThread.ManagedThreadId)
            {
                var taskSource = new TaskCompletionSource<bool>();
                UniTask.Post(() =>
                {
                    taskSource.SetResult(source != null);
                });
                return taskSource.Task.Result;
            }
            else
            {
                return source != null;
            }
        }
    }
}