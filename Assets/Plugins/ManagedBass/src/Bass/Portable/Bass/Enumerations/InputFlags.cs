using System;

namespace ManagedBass
{
    /// <summary>
    /// <see cref="Bass.RecordSetInput" /> flags.
    /// </summary>
    [Flags]
    public enum InputFlags
    {
        /// <summary>
        /// Don't change any setting. 
        /// Use this flag, if you only want to set the volume.
        /// </summary>
        None,

        /// <summary>
        /// Disable the Input.
        /// This flag can't be used when the device supports only one Input at a time.
        /// </summary>
        Off = 0x10000,

        /// <summary>
        /// Enable the Input.
        /// If the device only allows one Input at a time, then any previously enabled Input will be disabled by this.
        /// </summary>
        On = 0x20000
    }
}
