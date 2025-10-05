using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Enc
{
    /// <summary>
    /// BassEnc_Ogg is an extension to the BassEnc add-on that allows BASS channels to be Ogg Vorbis encoded, with support for OGGENC options.
    /// </summary>
    public static class BassEnc_Ogg
    {
#if __STATIC_LINKING__
        const string DllName = "__Internal";
#else
        const string DllName = "bassenc_ogg";
#endif
        
        [DllImport(DllName)]
        static extern int BASS_Encode_OGG_GetVersion();

        /// <summary>
        /// Gets the Version of BassEnc_Ogg that is loaded.
        /// </summary>
        public static Version Version => Extensions.GetVersion(BASS_Encode_OGG_GetVersion());

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_OGG_Start(int Handle, string Options, EncodeFlags Flags, EncodeProcedure Procedure, IntPtr User);

        /// <summary>
        /// Start Ogg Encoding to <see cref="EncodeProcedure"/>.
        /// </summary>
        /// <param name="Handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Options">
        /// Encoder options... null = use defaults.
        /// The following OGGENC style options are supported: -b / --bitrate, -m / --min-bitrate, -M / --max-bitrate, -q / --quality, -s / --serial, -t / --title, -a / --artist, -G / --genre, -d / --date, -l / --album, -N / --tracknum, -c / --comment.
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
        /// Ogg Vorbis encoding involves extensive floating-point operations, so it is not supported on platforms/architectures that do not have an FPU, eg. older ARM platforms/architectures.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid</exception>
        /// <exception cref="Errors.SampleFormat">The channel's sample format is not supported by the encoder.</exception>
        /// <exception cref="Errors.NotAvailable">This function is not available on platforms/architectures without an FPU.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem! </exception>
        public static int Start(int Handle, string Options, EncodeFlags Flags, EncodeProcedure Procedure, IntPtr User)
        {
            return BASS_Encode_OGG_Start(Handle, Options, Flags | EncodeFlags.Unicode, Procedure, User);
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_OGG_StartFile(int Handle, string Options, EncodeFlags Flags, string FileName);

        /// <summary>
        /// Start Ogg Encoding to File.
        /// </summary>
        /// <param name="Handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Options">
        /// Encoder options... null = use defaults.
        /// The following OGGENC style options are supported: -b / --bitrate, -m / --min-bitrate, -M / --max-bitrate, -q / --quality, -s / --serial, -t / --title, -a / --artist, -G / --genre, -d / --date, -l / --album, -N / --tracknum, -c / --comment.
        /// Anything else that is included will be ignored. 
        /// </param>
        /// <param name="Flags">A combination of <see cref="EncodeFlags"/>.</param>
        /// <param name="FileName">Output filename... null = no output file.</param>
        /// <returns>The encoder handle is returned if the encoder is successfully started, else 0 is returned. Use <see cref="Bass.LastError"/> to get the error code</returns>
        /// <remarks>
        /// <see cref="BassEnc.EncodeStart(int,string,EncodeFlags,EncoderProcedure,IntPtr)"/> is used internally to apply the encoder to the source channel, so the remarks in its documentation also apply to this function. 
        /// 
        /// <b>Platform-specific</b>
        /// Ogg Vorbis encoding involves extensive floating-point operations, so it is not supported on platforms/architectures that do not have an FPU, eg. older ARM platforms/architectures.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid</exception>
        /// <exception cref="Errors.SampleFormat">The channel's sample format is not supported by the encoder.</exception>
        /// <exception cref="Errors.Create">The file could not be created.</exception>
        /// <exception cref="Errors.NotAvailable">This function is not available on platforms/architectures without an FPU.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem! </exception>
        public static int Start(int Handle, string Options, EncodeFlags Flags, string FileName)
        {
            return BASS_Encode_OGG_StartFile(Handle, Options, Flags | EncodeFlags.Unicode, FileName);
        }
    }
}