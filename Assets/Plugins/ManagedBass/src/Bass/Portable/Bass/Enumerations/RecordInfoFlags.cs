using System;

namespace ManagedBass
{
    /// <summary>
    /// Flags to be used with <see cref="RecordInfo" />
    /// </summary>
    [Flags]
    enum RecordInfoFlags
    {
        /// <summary>
        /// None of the flags is set
        /// </summary>
        None,

        /// <summary>
        /// The device's drivers do NOT have DirectSound support, so it is being emulated.
        /// Updated drivers should be installed.
        /// </summary>
        EmulatedDrivers = 0x20,

        /// <summary>
        /// The device driver has been certified by Microsoft.
        /// This flag is always set on WDM drivers.
        /// </summary>
        Certified = 0x40
    }
}
