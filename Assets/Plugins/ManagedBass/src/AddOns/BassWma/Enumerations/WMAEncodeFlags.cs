using System;

namespace ManagedBass.Wma
{
    /// <summary>
    /// WMA encoding flags.
    /// </summary>
    [Flags]
    public enum WMAEncodeFlags
    {
        /// <summary>
		/// File is a Unicode (16-bit characters) filename.
		/// </summary>
		Unicode = -2147483648,

		/// <summary>
		/// Default encoding, no tags.
		/// </summary>
		Default = 0,

		/// <summary>
		/// GetRates: get available CBR quality settings.
		/// </summary>
		RatesCBR = 0,

		/// <summary>
		/// 8 bit sample data.
		/// </summary>
		Byte = 0x1,

		/// <summary>
		/// 32-bit floating-point sample data.
		/// </summary>
		Float = 0x100,

		/// <summary>
		/// Standard WMA encoding.
		/// </summary>
		Standard = 0x2000,

		/// <summary>
		/// WMA Professional encoding.
		/// </summary>
		Professional = 0x4000,

		/// <summary>
		///  Enable 24-bit encoding.
		/// </summary>
		Encode24Bit = 0x8000,

        /// <summary>
        /// Save uncompressed PCM data.
        /// </summary>
        /// <remarks>
        /// When the sample data is floating-point (<see cref="Float"/> flag is used), the <see cref="Encode24Bit"/> flag is considered;
        /// 24-bit data is written with it and 16-bit without.
        /// The "bitrate" parameter is ignored, except that it should be non-0.
        /// </remarks>
        PCM = 0x10000,

		/// <summary>
		/// GetRates: get available VBR quality settings.
		/// </summary>
		RatesVBR = 0x10000,

		/// <summary>
		/// Enable the specification of tags mid-stream (after encoding has begun).
		/// </summary>
		Script = 0x20000,

		/// <summary>
		/// Queue data to feed encoder asynchronously
		/// </summary>
		Queue = 0x40000,

        /// <summary>
        /// Use a BASS channel as source (provided in Frequency parameter).
        /// Channels parameter is ignored.
        /// If the BASSenc add-on is loaded, then the BassEnc.DSPPriority setting is used to determine where in the channel's DSP chain the encoding is performed, otherwise priority -1000 is used.
        /// </summary>
        Source = 0x80000
    }
}