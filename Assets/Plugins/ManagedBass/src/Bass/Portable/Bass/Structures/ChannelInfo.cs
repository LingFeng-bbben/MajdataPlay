using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    /// <summary>
    /// Used with <see cref="Bass.ChannelGetInfo(int,out ChannelInfo)" /> to retrieve information on a channel.
    /// </summary>
    /// <remarks>
    /// A "channel" can be a playing sample (HCHANNEL), a sample stream (HSTREAM), a MOD music (HMUSIC), or a recording (HRECORD).
    /// <para>Each "Channel" function can be used with one or more of these channel types.</para>
    /// <para>
    /// The BassFlags.SoftwareMixing flag indicates whether or not the channel's sample data is being mixed into the final output by the hardware.
    /// It does not indicate (in the case of a stream or MOD music) whether the processing required to generate the sample data is being done by the hardware, this processing is always done in software.
    /// </para>
    /// <para>
    /// BASS supports 8/16/32-bit sample data, so if a WAV file, for example, uses another sample resolution, it'll have to be converted by BASS.
    /// The <see cref="OriginalResolution"/> member can be used to check what the resolution originally was.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct ChannelInfo
    {
        int freq;
        int chans;
        BassFlags flags;
        ChannelType ctype;
        int origres;
        int plugin;
        int sample;
        IntPtr filename;

        /// <summary>
        /// Default playback rate.
        /// </summary>
        public int Frequency => freq;

        /// <summary>
        /// Number of channels... 1=mono, 2=stereo, etc.
        /// </summary>
        public int Channels => chans;

        /// <summary>
        /// Sample/Stream/Music/Speaker flags.
        /// A combination of <see cref="BassFlags"/>.
        /// </summary>
        /// <remarks><b>Platform-specific</b>
        /// <para>
        /// On Linux/iOS/OSX, the <see cref="BassFlags.Unicode"/> flag may not be present even if it was used in the stream's creation, as BASS will have translated the filename to the native UTF-8 form.
        /// On Windows CE, the opposite is true: the <see cref="BassFlags.Unicode"/> flag may be present even if it was not used in the stream's creation, as BASS will have translated the filename to the native UTF-16 form.
        /// </para>
        /// </remarks>
        public BassFlags Flags => flags;

        /// <summary>
        /// The Type of Channel
        /// </summary>
        public ChannelType ChannelType => ctype;

        /// <summary>
        /// The plugin that is handling the channel... 0 = not using a plugin.
        /// <para>
        /// Note this is only available with streams created using the plugin system via the standard BASS stream creation functions, not those created by add-on functions.
        /// Information on the plugin can be retrieved via <see cref="Bass.PluginGetInfo" />.
        /// </para>
        /// </summary>
        public int Plugin => plugin;

        /// <summary>
        /// The sample that is playing on the channel. (HCHANNEL only)
        /// </summary>
        public int Sample => sample;

        /// <summary>
        /// The resolution which Bass uses for the stream.
        /// </summary>
        public Resolution Resolution
        {
            get
            {
                if (flags.HasFlag(BassFlags.Byte))
                    return Resolution.Byte;
                return flags.HasFlag(BassFlags.Float) ? Resolution.Float : Resolution.Short;
            }
        }

        /// <summary>
        /// The original resolution (bits per sample)... 0 = undefined.
        /// </summary>
        public int OriginalResolution => origres;

        /// <summary>
        /// The filename associated with the channel. (HSTREAM only)
        /// </summary>
        public string FileName => filename == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(filename);

        /// <summary>
        /// Is the channel a decoding channel?
        /// </summary>
        public bool IsDecodingChannel => flags.HasFlag(BassFlags.Decode);
    }
}
