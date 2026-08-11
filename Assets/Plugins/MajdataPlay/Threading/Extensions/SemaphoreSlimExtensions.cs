using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MajdataPlay.Threading;
public static class SemaphoreSlimExtensions
{
    public static SemaphoreSlimDisposable AsDisposable(this SemaphoreSlim slim)
    {
        return new SemaphoreSlimDisposable(slim);
    }
    public static SemaphoreSlimDisposable Lock(this SemaphoreSlim slim)
    {
        var @lock = new SemaphoreSlimDisposable(slim);
        @lock.Lock();
        return @lock;
    }
    public static async ValueTask<SemaphoreSlimDisposable> LockAsync(this SemaphoreSlim slim, CancellationToken token = default)
    {
        var @lock = new SemaphoreSlimDisposable(slim);
        await @lock.LockAsync(token);
        return @lock;
    }
    public readonly struct SemaphoreSlimDisposable : IDisposable
    {
        readonly SemaphoreSlim _lock;
        public SemaphoreSlimDisposable(SemaphoreSlim @lock)
        {
            if (@lock is null)
            {
                throw new ArgumentNullException(nameof(@lock));
            }
            _lock = @lock;
        }
        public void Lock()
        {
            _lock.Wait();
        }
        public Task LockAsync(CancellationToken token = default)
        {
            return _lock.WaitAsync(token);
        }
        public void Dispose()
        {
            _lock.Release();
        }
    }
}
