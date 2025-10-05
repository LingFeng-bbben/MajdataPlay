using System;

namespace ManagedBass.Wasapi
{
    [Flags]
    enum WasapiDeviceInfoFlags
    {
        /// <summary>
        /// Unknown flags. 
        /// e.g. the WASAPI device is not present.
        /// </summary>
        Unknown,

        /// <summary>
        /// The WASAPI device is enabled (active).
        /// </summary>
        Enabled = 0x1,

        /// <summary>
        /// The WASAPI device is the default device.
        /// </summary>
        Default = 0x2,

        /// <summary>
        /// The WASAPI device is initialized.
        /// </summary>
        Initialized = 0x4,

        /// <summary>
        /// The WASAPI device is a loopback device.
        /// </summary>
        Loopback = 0x8,

        /// <summary>
        /// The WASAPI device is an Input (capture) device.
        /// </summary>
        Input = 0x10,

        /// <summary>
        /// The WASAPI device is unplugged.
        /// </summary>
        Unplugged = 0x20,

        /// <summary>
        /// The WASAPI device is disabled.
        /// </summary>
        Disabled = 0x40
    }
}