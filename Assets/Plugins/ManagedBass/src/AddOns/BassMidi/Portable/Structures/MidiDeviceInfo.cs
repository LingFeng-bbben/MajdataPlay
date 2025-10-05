using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Midi
{
    /// <summary>
    /// Used with <see cref="BassMidi.InGetDeviceInfo(int, out MidiDeviceInfo)" /> to retrieve information on a MIDI input device.
    /// </summary>
    /// <remarks>
    /// <para>
    /// On Windows, <see cref="ID"/> consists of a manufacturer identifier in the LOWORD and a product identifier in the HIWORD.
    /// This will not uniquely identify a particular device, ie. multiple devices may have the same value..
    /// On OSX, <see cref="ID"/> is the device's "kMIDIPropertyUniqueID" property value, which is unique to the device.
    /// On Linux, id contains the device's ALSA client ID in the LOWORD and port ID in the HIWORD.</para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct MidiDeviceInfo
    {
        IntPtr name;
        int id;
        DeviceInfoFlags flags;

        /// <summary>
        /// An identification number.
        /// </summary>
        /// <remarks>
        /// On Windows, <see cref="ID"/> consists of a manufacturer identifier in the LOWORD and a product identifier in the HIWORD.
        /// This will not uniquely identify a particular device, ie. multiple devices may have the same value..
        /// On OSX, <see cref="ID"/> is the device's "kMIDIPropertyUniqueID" property value, which is unique to the device.
        /// On Linux, id contains the device's ALSA client ID in the LOWORD and port ID in the HIWORD.
        /// </remarks>
        public int ID => id;

        /// <summary>
        /// The name/description of the device.
        /// </summary>
        public string Name => name == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(name);

        /// <summary>
        /// Gets whether the device is enabled.
        /// </summary>
        public bool IsEnabled => flags.HasFlag(DeviceInfoFlags.Enabled);

        /// <summary>
        /// Gets whether the device is already initialized.
        /// </summary>
        public bool IsInitialized => flags.HasFlag(DeviceInfoFlags.Initialized);

        /// <summary>
		/// Returns the Name of the Device.
		/// </summary>
		public override string ToString() => Name;
    }
}