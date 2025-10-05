using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ManagedBass.Enc
{

    /// <summary>
    /// BassEnc_Acc is an extension to the BassEnc add-on that allows BASS channels to be Acc encoded, with support for AACENC options.
    /// </summary>
    public static class BassEnc_Acc
    {
#if __STATIC_LINKING__
        const string DllName = "__Internal";
#else
    const string DllName = "bassenc_aac";
#endif

        [DllImport(DllName)]
        static extern int BASS_Encode_AAC_GetVersion();

        /// <summary>
        /// Gets the Version of BassEnc_Aac that is loaded.
        /// </summary>
        public static Version Version => Extensions.GetVersion(BASS_Encode_AAC_GetVersion());

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_AAC_Start(int Handle, string Options, EncodeFlags Flags, EncodeProcedure Procedure, IntPtr User);

        /// <summary>
        /// Start Aac Encoding to <see cref="EncodeProcedure"/>.
        /// </summary>
        /// <param name="Handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Options">
        /// Encoder options... null = use defaults.
        /// The following AACENC style options are supported: --object-type --bitrate, --vbr.
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
            return BASS_Encode_AAC_Start(Handle, Options, Flags | EncodeFlags.Unicode, Procedure, User);
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_AAC_StartFile(int Handle, string Options, EncodeFlags Flags, string FileName);

        /// <summary>
        /// Start AAC Encoding to File.
        /// </summary>
        /// <param name="Handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Options">
        /// Encoder options... null = use defaults.
        /// The following AACENC style options are supported: --object-type --bitrate, --vbr.
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
#if __IOS__
            var ftype = CharsToInt("adts");
            var atypeInt = GetArgValueInt(Options, "--object-type");
            var atype = CharsToInt(
                atypeInt == 5 ? "aach" :
                atypeInt == 23 ? "aacl" :
                atypeInt == 29 ? "aacp" :
                atypeInt == 39 ? "aace" : "aac ");
            var bitrate = GetArgValueInt(Options, "--bitrate");
            return BassEnc.EncodeStartCA(Handle, ftype, atype, Flags, bitrate ?? 0, FileName);
#else
            return BASS_Encode_AAC_StartFile(Handle, Options, Flags | EncodeFlags.Unicode, FileName);
#endif
        }

        /// <summary>
        /// Extracts the next int value that follows specified argument string in the command. 
        /// </summary>
        /// <param name="Cmd">The command-line to parse.</param>
        /// <param name="Arg">The argument string to get the value for.</param>
        /// <returns>The int value that follows the argument string, or null if not found or invalid.</returns>
        private static int? GetArgValueInt(string Cmd, string Arg)
        {
            Arg += " ";
            var posStart = Cmd.IndexOf(Arg, StringComparison.Ordinal);
            if (posStart > -1)
            {
                posStart += Arg.Length;
                var posEnd = Cmd.IndexOf(' ', posStart);
                if (posEnd == -1)
                {
                    posEnd = Cmd.Length - 1;
                }
                var value = Cmd.Substring(posStart, posEnd - posStart + 1);
                return int.TryParse(value, out var valueInt) ? (int?)valueInt : null;
            }
            return null;
        }

        /// <summary>
        /// Converts 4 chars into an Int32 value.
        /// </summary>
        /// <param name="chars">The 4 chars to convert.</param>
        /// <returns>An Int32 value.</returns>
        private static int CharsToInt(string chars) => BitConverter.ToInt32(Encoding.ASCII.GetBytes(chars), 0);
    }
}