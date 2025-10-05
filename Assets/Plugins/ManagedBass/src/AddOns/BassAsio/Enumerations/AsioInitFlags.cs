using System;

namespace ManagedBass.Asio
{
    /// <summary>
    /// Initialization flags to be used with <see cref="BassAsio.Init" />
    /// </summary>
    [Flags]
    public enum AsioInitFlags
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Host driver in dedicated thread
        /// </summary>
        Thread,

        /// <summary>
        /// Order joined channels by when they were joined
        /// </summary>
        JoinOrder
    }
}