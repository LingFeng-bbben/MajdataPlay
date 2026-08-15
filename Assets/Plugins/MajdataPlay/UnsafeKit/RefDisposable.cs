using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MajdataPlay.UnsafeKit
{
    public readonly ref struct RefDisposable
    {
        public ref struct Disposable<T> where T : IDisposable
        {
            private Span<T> _instance;
            public readonly bool SetNullWhenDispose;

            internal Disposable(ref T instance, bool setNullWhenDispose)
            {
                _instance = MemoryMarshal.CreateSpan(ref instance, 1);
                SetNullWhenDispose = setNullWhenDispose;
            }

            public void Dispose()
            {
                _instance[0]?.Dispose();
                if (SetNullWhenDispose)
                {
                    _instance[0] = default;
                }
            }
        }
        public static Disposable<T> From<T>(ref T instance, bool setNullWhenDispose = false) where T : IDisposable
        {
            return new(ref instance, setNullWhenDispose);
        }
    }
}
