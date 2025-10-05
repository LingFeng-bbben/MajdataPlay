using System;
using System.Collections.Concurrent;
using System.Linq;

namespace ManagedBass
{
    /// <summary>
    /// Holds References to Channel Items like <see cref="SyncProcedure"/> and <see cref="FileProcedures"/>.
    /// </summary>
    public static class ChannelReferences
    {
        static readonly ConcurrentDictionary<Tuple<int, int>, object> Procedures = new ConcurrentDictionary<Tuple<int, int>, object>();
        static readonly SyncProcedure Freeproc = Callback;

        /// <summary>
        /// Adds a Reference.
        /// </summary>
        public static void Add(int Handle, int SpecificHandle, object proc)
        {
            if (!CrossPlatformHelper.IsDynamicCodeSupported)
                return;

            if (proc == null)
                return;

            if (proc.Equals(Freeproc))
                return;

            var key = Tuple.Create(Handle, SpecificHandle);

            var contains = Procedures.ContainsKey(key);
            
            if (Freeproc != null && Procedures.All(pair => pair.Key.Item1 != Handle))
                Bass.ChannelSetSync(Handle, SyncFlags.Free, 0, Freeproc);

            if (contains)
                Procedures[key] = proc;
            else Procedures.TryAdd(key, proc);
        }

        /// <summary>
        /// Removes a Reference.
        /// </summary>
        public static void Remove(int Handle, int SpecialHandle)
        {
            if (!CrossPlatformHelper.IsDynamicCodeSupported)
                return;

            var key = Tuple.Create(Handle, SpecialHandle);
            Procedures.TryRemove(key, out object unused);
        }

        static void Callback(int Handle, int Channel, int Data, IntPtr User)
        {
            // ToArray is necessary because the object iterated on should not be modified.
            var toRemove = Procedures.Where(Pair => Pair.Key.Item1 == Channel).Select(Pair => Pair.Key).ToArray();
            
            foreach (var key in toRemove)
                Procedures.TryRemove(key, out object unused);
        }
    }
}
