using System.Runtime.InteropServices;

namespace ManagedBass
{
    /// <summary>
    /// Used with <see cref="Bass.RecordGetInfo" /> to retrieve information on the current recording device.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="SupportedFormats"/> member does not represent all the formats supported by the device, just the "standard" ones.</para>
    /// <para>If there is no DirectSound driver for the device (ie. it's being emulated), then the driver member will contain something like "WaveIn" instead of a filename.</para>
    /// <para><b>Platform-specific</b></para>
    /// <para>
    /// The <see cref="IsCertified"/> and <see cref="SupportsDirectSound"/> members are only used on Windows.
    /// The <see cref="SupportedFormats"/> member is only used on Windows/OSX/iOS, and only for the device's channel count in the case of OSX and iOS.
    /// On Windows, it does not necessarily represent all of the formats supported by the device, just the "standard" ones.
    /// <see cref="Frequency"/> is also only available on Windows/OSX/iOS, but not on Windows prior to Vista.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct RecordInfo
    {
        RecordInfoFlags flags;
        int formats;
        int inputs;
        bool singlein;
        int freq;

        /// <summary>
        /// The standard wave formats supported by the device (default is <see cref="RecordFormatFlags.WF4S16"/>).
        /// </summary>
        public RecordFormatFlags SupportedFormats => (RecordFormatFlags)(formats & 0xffffff);

        /// <summary>
        /// The number of Input sources available to the device
        /// </summary>
        public int Inputs => inputs;

        /// <summary>
        /// <see langword="true" /> = only one Input may be active at a time
        /// </summary>
        public bool SingleInput => singlein;

        /// <summary>
        /// The device's current Input sample rate. This is only available on Windows Vista and OSX.
        /// </summary>
        public int Frequency => freq;

        /// <summary>
        /// Gets the available channel count for a recording Input.
        /// </summary>
        public int Channels => formats >> 24;

        /// <summary>
        /// The device driver has been certified by Microsoft. Always true for WDM drivers.
        /// </summary>
        public bool IsCertified => flags.HasFlag(RecordInfoFlags.Certified);

        /// <summary>
        /// The device's drivers has DirectSound support
        /// </summary>
        public bool SupportsDirectSound => flags.HasFlag(RecordInfoFlags.EmulatedDrivers);
    }
}
