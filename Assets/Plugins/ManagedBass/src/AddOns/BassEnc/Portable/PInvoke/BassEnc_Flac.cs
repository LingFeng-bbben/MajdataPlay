using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Enc
{
    /// <summary>
    /// BassEnc_Flac is an extension to the BassFlac add-on that allows BASS channels to be Flac encoded.
    /// </summary>
    public static class BassEnc_Flac
    {
#if __STATIC_LINKING__
        const string DllName = "__Internal";
#else
        const string DllName = "bassenc_flac";
#endif
                
        [DllImport(DllName)]
        static extern int BASS_Encode_FLAC_GetVersion();

        /// <summary>
        /// Gets the Version of BassEnc_Flac that is loaded.
        /// </summary>
        public static Version Version => Extensions.GetVersion(BASS_Encode_FLAC_GetVersion());

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_FLAC_Start(int Handle, string Options, EncodeFlags Flags, EncodeProcedureEx ProcedureEx, IntPtr User);

        /// <summary>
        /// Start FLAC Encoding to <see cref="EncodeProcedure"/>.
        /// </summary>
        public static int Start(int Handle, string Options, EncodeFlags Flags, EncodeProcedureEx ProcedureEx, IntPtr User)
        {
            return BASS_Encode_FLAC_Start(Handle, Options, Flags | EncodeFlags.Unicode, ProcedureEx, User);
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_FLAC_StartFile(int Handle, string Options, EncodeFlags Flags, string FileName);

        /// <summary>
        /// Start FLAC Encoding to File.
        /// </summary>
        public static int Start(int Handle, string Options, EncodeFlags Flags, string FileName)
        {
            return BASS_Encode_FLAC_StartFile(Handle, Options, Flags | EncodeFlags.Unicode, FileName);
        }
    }
}