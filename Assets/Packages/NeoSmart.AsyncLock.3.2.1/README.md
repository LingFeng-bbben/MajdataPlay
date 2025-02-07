## AsyncLock: An async/await-friendly lock

AsyncLock is an async/await-friendly lock implementation for .NET Standard, making writing code like the snippet below (mostly) possible:

```csharp
lock (_lockObject)
{
    await DoSomething();
}
```
Unlike most other so-called "async locks" for C#, AsyncLock is actually designed to support the programming paradigm `lock` encourages, not just the technical elements. You can read more about the pitfalls with other so-called asynchronous locks and the difficulties of creating a reentrance-safe implementation [here](https://neosmart.net/blog/2017/asynclock-an-asyncawait-friendly-locking-library-for-c-and-net/).

With `AsyncLock`, you don't have to worry about which thread is running what code in order to determine whether or not your locks will have any effect or if they'll be bypassed completely, you just write code the way you normally would and you'll find AsyncLock to correctly marshal access to protected code segments.

### Using AsyncLock

There are only three functions to familiarize yourself with: the `AsyncLock()` constructor and the two locking variants `Lock()`/`LockAsync()` .

`AsyncLock()` creates a new asynchronous lock. A separate AsyncLock should be used for each "critical operation" you will be performing. (Or you can use a global lock just like some people still insist on using global mutexes and semaphores. We won't judge too harshly.)

Everywhere you would normally use `lock (_lockObject)` you will now use one of

* `using (_lock.Lock())` or
* `using (await _lock.LockAsync())`

That's all there is to it!

### Async-friendly locking by design

Much like the`SemaphoreSlim` class, `AsyncLock` offers two different "wait" options, a blocking `Lock()` call and the asynchronous `LockAsync()` call. The utmost scare should be taken to never call `LockAsync()` without an `await` before it, for obvious reasons.

Upon using `LockAsync()`, `AsyncLock` will attempt to obtain exclusive access to the lock. Should that not be possible in the current state, it will cede its execution slot and return to the caller, allowing the system to marshal resources efficiently as needed without blocking until the lock becomes available. Once the lock is available, the `AsyncLock()` call will resume, transferring execution to the protected section of the code.

### AsyncLock usage example

```csharp
private class AsyncLockTest
{
    var _lock = new AsyncLock();

    void Test()
    {
        // The code below will be run immediately (likely in a new thread)
        Task.Run(async () =>
             {
                 // A first call to LockAsync() will obtain the lock without blocking
                 using (await _lock.LockAsync())
                 {
                     // A second call to LockAsync() will be recognized as being
                     // reentrant and permitted to go through without blocking.
                     using (await _lock.LockAsync())
                     {
                         // We now exclusively hold the lock for 1 minute
                         await Task.Delay(TimeSpan.FromMinutes(1));
                     }
                 }
             }).Wait(TimeSpan.FromSeconds(30));

        // This call to obtain the lock is made synchronously from the main thread.
        // It will, however, block until the asynchronous code which obtained the lock
        // above finishes.
        using (_lock.Lock())
        {
            // Now we have obtained exclusive access.
            // <Safely perform non-thread-safe operation safely here>
        }
    }
}
```
