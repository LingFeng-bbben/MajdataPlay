#nullable enable
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Scripting.LifecycleManagement;

namespace MajdataPlay
{
    [AutoStaticsCleanup]
    internal unsafe static partial class Majdata<T> where T : class
    {
        public static T? Instance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _instance;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var spin = new SpinWait();
                while (true)
                {
                    var currentState = Volatile.Read(ref _state);
                    if (currentState == 1)
                    {
                        throw new InvalidOperationException();
                    }

                    if (currentState == 0 && Interlocked.CompareExchange(ref _state, 2, 0) == 0)
                    {
                        _instance = value;
                        Volatile.Write(ref _state, 0);
                        return;
                    }
                    spin.SpinOnce();
                }
            }
        }

        public static bool IsNull
        {
            [MemberNotNullWhen(false, nameof(Instance), nameof(_instance))]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _instance is null;
        }
        public static bool IsSingleton
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Volatile.Read(ref _state) == 1;
        }

        private static int _state = 0;
        private static T? _instance = null;

        public static void SetAsSingleton(T instance)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            var spin = new SpinWait();
            while (true)
            {
                var currentState = Volatile.Read(ref _state);
                if (currentState == 1)
                {
                    throw new InvalidOperationException();
                }

                if (currentState == 0 && Interlocked.CompareExchange(ref _state, 1, 0) == 0)
                {
                    _instance = instance;
                    return;
                }
                spin.SpinOnce();
            }
        }

        /// <summary>
        /// Release the instance
        /// </summary>
        public static void Free()
        {
            var spin = new SpinWait();
            while (true)
            {
                var currentState = Volatile.Read(ref _state);
                if (currentState != 2 && Interlocked.CompareExchange(ref _state, 2, currentState) == currentState)
                {
                    _instance = null;
                    Volatile.Write(ref _state, 0);
                    return;
                }
                spin.SpinOnce();
            }
        }
    }
}
