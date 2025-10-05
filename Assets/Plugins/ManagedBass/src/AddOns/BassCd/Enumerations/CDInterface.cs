namespace ManagedBass.Cd
{
    /// <summary>
    /// The interface to use to access CD drives (used with <see cref="BassCd.SetInterface" />).
    /// </summary>
    public enum CDInterface
    {
        /// <summary>
        /// Automatically detect an available interface.
        /// The interfaces are checked in the order that they are listed here.
        /// For example, if both SPTI and ASPI are available, SPTI will be used.
        /// </summary>
        Auto,

        /// <summary>
        /// SCSI Pass-Through Interface.
        /// This is only available on NT-based Windows, not Windows 9x, and generally only to administrator user accounts, not limited/restricted user accounts.
        /// </summary>
        SPTI,

        /// <summary>
        /// Advanced SCSI Programming Interface.
        /// This is the only interface available on Windows 9x, and can also be installed on NT-based Windows.
        /// </summary>
        ASPI,

        /// <summary>
        /// Windows I/O. Like SPTI, this is only available on NT-based Windows, but it is also available to limited/restricted user accounts.
        /// Some features are not available via this interface, notably sub-channel data reading and read speed control (except on Vista or newer).
        /// Door status detection is also affected.
        /// </summary>
        WIO,

        /// <summary>
        /// Linux interface.
        /// </summary>
        Linux
    }
}