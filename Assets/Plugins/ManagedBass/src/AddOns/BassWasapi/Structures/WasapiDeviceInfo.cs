using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Wasapi
{
    /// <summary>
	/// Used with <see cref="BassWasapi.GetDeviceInfo(int, out WasapiDeviceInfo)" /> to retrieve information on a Wasapi device.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
    public struct WasapiDeviceInfo
    {
        IntPtr name;
        IntPtr id;
        WasapiDeviceType type;
        WasapiDeviceInfoFlags flags;
        float minperiod;
        float defperiod;
        int mixfreq;
        int mixchans;

        /// <summary>
        /// The Type of the devices.
        /// </summary>
        public WasapiDeviceType Type => type;

        /// <summary>
        /// The minimum update period (in seconds) of the device.
        /// </summary>
        public double MinimumUpdatePeriod => minperiod;

        /// <summary>
        /// The default update period (in seconds) of the device.
        /// </summary>
        public double DefaultUpdatePeriod => defperiod;

        /// <summary>
        /// The shared-mode format mixers sample rate.
        /// </summary>
        public int MixFrequency => mixfreq;

        /// <summary>
        /// The shared-mode format mixers number of channels.
        /// </summary>
        public int MixChannels => mixchans;

        /// <summary>
        /// The description of the device.
        /// </summary>
        public string Name => name == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(name);

        /// <summary>
        /// The ID of the driver being used.
        /// </summary>
        public string ID => id == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(id);

        /// <summary>
        /// Gets whether the device is the system default device.
        /// </summary>
        public bool IsDefault => flags.HasFlag(WasapiDeviceInfoFlags.Default);

        /// <summary>
        /// Gets whether the device is enabled.
        /// </summary>
        public bool IsEnabled => flags.HasFlag(WasapiDeviceInfoFlags.Enabled);

        /// <summary>
        /// Gets whether the device is input device.
        /// </summary>
        public bool IsInput => flags.HasFlag(WasapiDeviceInfoFlags.Input);

        /// <summary>
        /// Gets whether the device is a loopback device (output capture).
        /// </summary>
        public bool IsLoopback => flags.HasFlag(WasapiDeviceInfoFlags.Loopback);

        /// <summary>
        /// Gets whether the device is initialized (using <see cref="BassWasapi.Init"/>).
        /// </summary>
        public bool IsInitialized => flags.HasFlag(WasapiDeviceInfoFlags.Initialized);

        /// <summary>
        /// Gets whether the device is unplugged.
        /// </summary>
        public bool IsUnplugged => flags.HasFlag(WasapiDeviceInfoFlags.Unplugged);

        /// <summary>
        /// Gets whether the device is disabled.
        /// </summary>
        public bool IsDisabled => flags.HasFlag(WasapiDeviceInfoFlags.Disabled);

        /// <summary>
        /// Returns the <see cref="Name"/> of the device.
        /// </summary>
        public override string ToString() => Name;
    }
}