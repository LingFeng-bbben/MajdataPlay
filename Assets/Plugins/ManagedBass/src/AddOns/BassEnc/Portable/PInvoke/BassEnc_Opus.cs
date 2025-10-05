using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Enc
{
    /// <summary>
    /// BassEnc_Opus is an extension to the BassEnc add-on that allows BASS channels to be Opus encoded, with support for OPUSENC options.
    /// </summary>
    public static class BassEnc_Opus
    {
#if __STATIC_LINKING__
        const string DllName = "__Internal";
#else
        const string DllName = "bassenc_opus";
#endif
                
        [DllImport(DllName)]
        static extern int BASS_Encode_OPUS_GetVersion();

        /// <summary>
        /// Gets the Version of BassEnc_Opus that is loaded.
        /// </summary>
        public static Version Version => Extensions.GetVersion(BASS_Encode_OPUS_GetVersion());

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_OPUS_Start(int Handle, string Options, EncodeFlags Flags, EncodeProcedure Procedure, IntPtr User);

        /// <summary>
        /// Start Opus Encoding to <see cref="EncodeProcedure"/>.
        /// </summary>
        /// <param name="Handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Options">
        /// Encoder options... null = use defaults.
        /// The following OPUSENC style options are supported: --bitrate, --vbr, --cvbr, --hard-cbr, --comp / --complexity, --framesize, --expect-loss, --max-delay, --serial, --comment, --artist, --title, --album, --date, --genre, --padding.
        /// Anything else that is included will be ignored.
        /// </param>
        /// <param name="Flags">A combination of <see cref="EncodeFlags"/>.</param>
        /// <param name="Procedure">Optional callback function to receive the encoded data... null = no callback.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>The encoder handle is returned if the encoder is successfully started, else 0 is returned. Use <see cref="Bass.LastError"/> to get the error code</returns>
        /// <remarks>
        /// <see cref="BassEnc.EncodeStart(int,string,EncodeFlags,EncoderProcedure,IntPtr)"/> is used internally to apply the encoder to the source channel, so the remarks in its documentation also apply to this function. 
        /// 
        /// <b>Platform-specific</b>
        /// On Windows and Linux, an SSE supporting CPU is required for sample rates other than 48000/24000/16000/12000/8000 Hz.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid</exception>
        /// <exception cref="Errors.SampleFormat">The channel's sample format is not supported by the encoder.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem! </exception>
        public static int Start(int Handle, string Options, EncodeFlags Flags, EncodeProcedure Procedure, IntPtr User)
        {
            return BASS_Encode_OPUS_Start(Handle, Options, Flags | EncodeFlags.Unicode, Procedure, User);
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_OPUS_StartFile(int Handle, string Options, EncodeFlags Flags, string FileName);

        /// <summary>
        /// Start Opus Encoding to File.
        /// </summary>
        /// <param name="Handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Options">
        /// Encoder options... null = use defaults.
        /// The following OPUSENC style options are supported: --bitrate, --vbr, --cvbr, --hard-cbr, --comp / --complexity, --framesize, --expect-loss, --max-delay, --serial, --comment, --artist, --title, --album, --date, --genre, --padding.
        /// Anything else that is included will be ignored.
        /// </param>
        /// <param name="Flags">A combination of <see cref="EncodeFlags"/>.</param>
        /// <param name="FileName">Output filename... null = no output file.</param>
        /// <returns>The encoder handle is returned if the encoder is successfully started, else 0 is returned. Use <see cref="Bass.LastError"/> to get the error code</returns>
        /// <remarks>
        /// <see cref="BassEnc.EncodeStart(int,string,EncodeFlags,EncoderProcedure,IntPtr)"/> is used internally to apply the encoder to the source channel, so the remarks in its documentation also apply to this function. 
        /// 
        /// <b>Platform-specific</b>
        /// On Windows and Linux, an SSE supporting CPU is required for sample rates other than 48000/24000/16000/12000/8000 Hz.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid</exception>
        /// <exception cref="Errors.SampleFormat">The channel's sample format is not supported by the encoder.</exception>
        /// <exception cref="Errors.Create">The file could not be created.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem! </exception>
        public static int Start(int Handle, string Options, EncodeFlags Flags, string FileName)
        {
            return BASS_Encode_OPUS_StartFile(Handle, Options, Flags | EncodeFlags.Unicode, FileName);
        }
    }
}