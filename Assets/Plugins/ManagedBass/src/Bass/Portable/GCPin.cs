using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    /// <summary>
    /// Wrapper for pinned allocation using <see cref="GCHandle"/>.
    /// </summary>
    public class GCPin : IDisposable
    {
        GCHandle _gcHandle;

        /// <summary>
        /// Creates a new instance of <see cref="GCPin"/>.
        /// </summary>
        public GCPin(object Item)
        {
            _gcHandle = GCHandle.Alloc(Item, GCHandleType.Pinned);
        }

        /// <summary>
        /// Create Stream Helper method.
        /// </summary>
        /// <param name="Function">The Stream Creation function.</param>
        /// <param name="Memory">The <see cref="object"/> to pin and pass to <paramref name="Function"/>.</param>
        /// <returns>The Handle returned by <paramref name="Function"/>.</returns>
        public static int CreateStreamHelper(Func<IntPtr, int> Function, object Memory)
        {
            var GCPin = GCHandle.Alloc(Memory, GCHandleType.Pinned);

            var Handle = Function(GCPin.AddrOfPinnedObject());

            if (Handle == 0)
                GCPin.Free();
            else Bass.ChannelSetSync(Handle, SyncFlags.Free, 0, (a, b, c, d) => GCPin.Free());

            return Handle;
        }

        /// <summary>
        /// <see cref="IntPtr"/> to pinned resource.
        /// </summary>
        public IntPtr Pointer => _gcHandle.AddrOfPinnedObject();

        /// <summary>
        /// Frees the pinned resource.
        /// </summary>
        public void Dispose() => _gcHandle.Free();
    }
}
