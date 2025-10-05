using System;

namespace ManagedBass
{
    /// <summary>
    /// Device Info Flags
    /// </summary>
    [Flags]
    public enum DeviceInfoFlags
    {
        /// <summary>
        /// The device is not enabled and not initialized.
        /// </summary>
        None,

        /// <summary>
        /// The device is enabled.
        /// It will not be possible to initialize the device if this flag is not present.
        /// </summary>
        Enabled = 0x1,

        /// <summary>
        /// The device is the system default.
        /// </summary>
        Default = 0x2,

        /// <summary>
        /// The device is initialized, ie. <see cref="Bass.Init"/> or <see cref="Bass.RecordInit"/> has been called.
        /// </summary>
        Initialized = 0x4,

        /// <summary>
        /// The device is a Loopback device.
        /// </summary>
        Loopback = 0x8,

        /// <summary>
        /// Bitmask to identify the device Type.
        /// </summary>
        TypeMask = -16777216
    }
}
