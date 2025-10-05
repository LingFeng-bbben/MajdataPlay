using System;

namespace ManagedBass.Wasapi
{
    /// <summary>
	/// The BassWasapi Volume curve to use with <see cref="BassWasapi.GetVolume" /> and <see cref="BassWasapi.SetVolume" />.
	/// </summary>
	[Flags]
    public enum WasapiVolumeTypes
    {
        /// <summary>
        /// Use the device volume.
        /// </summary>
        Device = 0,

        /// <summary>
        /// Logarithmic curve.
        /// </summary>
        LogaritmicCurve = 0,

        /// <summary>
        /// Linear curve.
        /// </summary>
        LinearCurve = 0x1,

        /// <summary>
        /// Windows' hybrid curve.
        /// </summary>
        WindowsHybridCurve = 0x2,

        /// <summary>
        /// Use the session volume.
        /// </summary>
        Session = 0x8
    }
}