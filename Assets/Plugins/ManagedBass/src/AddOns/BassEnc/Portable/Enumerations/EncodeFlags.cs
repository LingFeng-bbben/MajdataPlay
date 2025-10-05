using System;

namespace ManagedBass.Enc
{
    /// <summary>
    /// BassEnc Start flags for use with <see cref="BassEnc.EncodeStart(int,string,EncodeFlags,EncodeProcedure,IntPtr)" />.
    /// </summary>
    [Flags]
    public enum EncodeFlags
    {
        /// <summary>
        /// Cmdline is Unicode (16-bit characters).
        /// </summary>
        Unicode = -2147483648,

        /// <summary>
        /// Default option, incl. wave header, little-endian and no FP conversion.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Do NOT send a WAV header to the encoder.
        /// </summary>
        NoHeader = 0x1,

        /// <summary>
        /// Convert floating-point sample data to 8-bit integer.
        /// </summary>
        ConvertFloatTo8BitInt = 0x2,

        /// <summary>
        /// Convert floating-point sample data to 16-bit integer.
        /// </summary>
        ConvertFloatTo16BitInt = 0x4,

        /// <summary>
        /// Convert floating-point sample data to 24-bit integer.
        /// </summary>
        ConvertFloatTo24Bit = 0x6,

        /// <summary>
        /// Convert floating-point sample data to 32-bit integer.
        /// </summary>
        ConvertFloatTo32Bit = 0x8,

        /// <summary>
        /// Convert floating-point sample data back to channel's format.
        /// </summary>
        ConvertFloatAuto = 14,

        /// <summary>
        /// Big-Endian sample data.
        /// </summary>
        BigEndian = 0x10,

        /// <summary>
        /// Start the encoder paused.
        /// </summary>
        Pause = 0x20,

        /// <summary>
        /// Write PCM sample data (no encoder).
        /// </summary>
        PCM = 0x40,

        /// <summary>
        /// Write RF64 WAV header (no encoder).
        /// </summary>
        RF64 = 0x80,

        /// <summary>
        /// Convert to mono (if not already).
        /// </summary>
        Mono = 0x100,
        
        /// <summary>
        /// Queue data to feed encoder asynchronously.
        /// The queue Buffer will grow as needed to fit the data, but its size can be limited by the Queue config option (0 = no limit);
        /// The default is 10000ms.
        /// If the queue reaches the size limit and data is lost, the <see cref="EncodeNotifyStatus.QueueFull"/> notification will be triggered.
        /// </summary>
        Queue = 0x200,
        
        /// <summary>
        /// Send the sample format information to the encoder in WAVEFORMATEXTENSIBLE form instead of WAVEFORMATEX form.
        /// This flag is ignored if the <see cref="NoHeader"/> flag is used.
        /// </summary>
        WaveFormatExtensible = 0x400,
        
        /// <summary>
        /// Don't limit the data rate (to real-time speed) when sending to a Shoutcast or Icecast server.
        /// With this option you might disable the rate limiting during casting (as it'll be limited by the playback rate anyway if the source channel is being played).
        /// </summary>
        UnlimitedCastDataRate = 0x1000,
        
        /// <summary>
        /// Limit data rate to real-time.
        /// Limit the data rate to real-time speed, by introducing a delay when the rate is too high.
        /// With BASS 2.4.6 or above, this flag is ignored when the encoder is fed in a playback Buffer update cycle (including <see cref="Bass.Update"/> and <see cref="Bass.ChannelUpdate"/> calls),
        /// to avoid possibly causing playback Buffer underruns.
        /// Except for in those instances, this flag is applied automatically when the encoder is feeding a Shoutcast or Icecast server.
        /// </summary>
        Limit = 0x2000,

        /// <summary>
        /// Send an AIFF header to the encoder instead of a WAVE header.
        /// </summary>
        AIFF = 0x4000,

        /// <summary>
        /// Apply dither when converting floating-point sample data to integer.
        /// </summary>
        Dither = 0x8000,

        /// <summary>
        /// Free the encoder when the channel is freed.
        /// </summary>
        AutoFree = 0x40000
    }
}