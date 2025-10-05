using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Enc
{
    public static partial class BassEnc
    {
        /// <summary>
        /// ACM codec name to give priority for the formats it supports (e.g. 'l3codecp.acm').
        /// </summary>
        public static string ACMLoad
        {
            get => Marshal.PtrToStringAnsi(Bass.GetConfigPtr(Configuration.EncodeACMLoad));
            set
            {
                var ptr = Marshal.StringToHGlobalAnsi(value);

                Bass.Configure(Configuration.EncodeACMLoad, ptr);

                Marshal.FreeHGlobal(ptr);
            }
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_GetACMFormat(int handle, IntPtr form, int formlen, string title, int flags);

        /// <summary>
        /// Presents the user with a list of available ACM (Audio Compression Manager) codec output formats to choose from.
        /// </summary>
        /// <param name="Handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Format">Pointer to the format buffer.</param>
        /// <param name="FormatLength">Size of the format buffer. If this is 0, then a suggested format buffer length is returned (which is the maximum length of all installed codecs), without displaying the codec selector.</param>
        /// <param name="Title">Window title for the selector... <see langword="null" /> = "Choose the output format".</param>
        /// <param name="Flags">A combination of <see cref="ACMFormatFlags"/>.</param>
        /// <param name="Encoding">Can be used to restrict the choice to a particular format tag. This is required with <see cref="ACMFormatFlags.Suggest"/>, and is optional otherwise.</param>
        /// <returns>
        /// If successful, the user-selected codec format details are put in the provided buffer and the length of the format details is returned, else 0 is returned. 
        /// Use <see cref="Bass.LastError" /> to get the error code.
        /// If <paramref name="FormatLength"/> is 0, then the suggested format buffer size is returned.
        /// </returns>
        /// <remarks>
        /// This function presents the user with a list of available ACM codecs to choose from, given the sample format of the channel.
        /// The details of the chosen codec's output are returned in the Format buffer, which can then be used with <see cref="EncodeStartACM(int,IntPtr,EncodeFlags,EncodeProcedure,IntPtr)" /> or <see cref="EncodeStartACM(int,IntPtr,EncodeFlags,string)" /> to begin encoding.
        /// <para>
        /// The <paramref name="Format" /> buffer contents are actually a WAVEFORMATEX or ACMFORMAT structure.
        /// If writing the encoder output to a WAVE file, the Format buffer contents would be the format chunk ("fmt") of the file.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">There are no codecs available that will accept the channel's format.</exception>
        /// <exception cref="Errors.AcmCancel">The user pressed the "cancel" button.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int GetACMFormat(int Handle,
                                       IntPtr Format = default(IntPtr),
                                       int FormatLength = 0,
                                       string Title = null,
                                       ACMFormatFlags Flags = ACMFormatFlags.Default,
                                       WaveFormatTag Encoding = WaveFormatTag.Unknown)
        {
            var acMflags = BitHelper.MakeLong((short)(Flags | ACMFormatFlags.Unicode), (short)Encoding);

            return BASS_Encode_GetACMFormat(Handle, Format, FormatLength, Title, acMflags);
        }

        /// <summary>
        /// Sets up an encoder on a channel, using an ACM codec and sending the output to a user defined function.
        /// </summary>
        /// <param name="Handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Format">ACM codec output format (as returned by <see cref="GetACMFormat" />).</param>
        /// <param name="Flags">A combination of <see cref="EncodeFlags"/>.</param>
        /// <param name="Procedure">Callback function to receive the encoded data.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>The encoder handle is returned if the encoder is successfully started, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The ACM encoder allows installed ACM (Audio Compression Manager) codecs to be used for encoding.
        /// The codec used is determined by the contents of the Format parameter. 
        /// The <see cref="GetACMFormat" /> function can be used to initialize that.
        /// <para>
        /// Internally, the sending of sample data to the encoder is implemented via a DSP callback on the channel.
        /// That means when you play the channel (or call <see cref="Bass.ChannelGetData(int,IntPtr,int)" /> if it's a decoding channel), the sample data will be sent to the encoder at the same time. 
        /// The encoding is performed in the DSP callback.
        /// There isn't a separate process doing the encoding, as when using an external encoder via <see cref="EncodeStart(int,string,EncodeFlags,EncodeProcedure,IntPtr)" />.
        /// </para>
        /// <para>
        /// By default, the encoder DSP has a priority setting of -1000, which determines where in the DSP chain the encoding is performed.
        /// That can be changed using the <see cref="DSPPriority"/> config option.
        /// </para>
        /// <para>
        /// Besides the automatic DSP system, data can also be manually fed to the encoder via the <see cref="EncodeWrite(int,IntPtr,int)" /> function.
        /// Both methods can be used together, but in general, the "automatic" system ought be paused when using the "manual" system, by use of the <see cref="EncodeFlags.Pause"/> flag or the <see cref="EncodeSetPaused" /> function.
        /// </para>
        /// <para>
        /// When queued encoding is enabled via the <see cref="EncodeFlags.Queue"/> flag, the DSP system or <see cref="EncodeWrite(int,IntPtr,int)" /> call will just buffer the data, and the data will then be fed to the encoder by another thread.
        /// The buffer will grow as needed to hold the queued data, up to a limit specified by the <see cref="EncodeFlags.Queue"/> config option.
        /// If the limit is exceeded (or there is no free memory), data will be lost;
        /// <see cref="EncodeSetNotify" /> can be used to be notified of that occurrence.
        /// The amount of data that is currently queued, as well as the queue limit and how much data has been lost, is available from <see cref="EncodeGetCount" />.
        /// </para>
        /// <para>When done encoding, use <see cref="EncodeStop(int)" /> to close the encoder.</para>
        /// <para>
        /// Multiple encoders can be set on a channel.
        /// For simplicity, the encoder functions will accept either an encoder handle or a channel handle.
        /// When using a channel handle, the function is applied to all encoders that are set on that channel.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The codec specified in <paramref name="Format" /> couldn't be initialized.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_StartACM")]
        public static extern int EncodeStartACM(int Handle, IntPtr Format, EncodeFlags Flags, EncodeProcedure Procedure, IntPtr User = default(IntPtr));

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_StartACMFile(int handle, IntPtr form, EncodeFlags flags, string filename);

        /// <summary>
        /// Sets up an encoder on a channel, using an ACM codec and writing the output to a file.
        /// </summary>
        /// <param name="Handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Format">ACM codec output format (as returned by <see cref="GetACMFormat" />).</param>
        /// <param name="Flags">A combination of <see cref="EncodeFlags"/>.</param>
        /// <param name="FileName">The filename to write.</param>
        /// <returns>The encoder handle is returned if the encoder is successfully started, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The ACM encoder allows installed ACM (Audio Compression Manager) codecs to be used for encoding.
        /// The codec used is determined by the contents of the Format parameter. 
        /// The <see cref="GetACMFormat" /> function can be used to initialize that.
        /// <para>
        /// Internally, the sending of sample data to the encoder is implemented via a DSP callback on the channel.
        /// That means when you play the channel (or call <see cref="Bass.ChannelGetData(int,IntPtr,int)" /> if it's a decoding channel), the sample data will be sent to the encoder at the same time. 
        /// The encoding is performed in the DSP callback.
        /// There isn't a separate process doing the encoding, as when using an external encoder via <see cref="EncodeStart(int,string,EncodeFlags,EncodeProcedure,IntPtr)" />.
        /// </para>
        /// <para>
        /// By default, the encoder DSP has a priority setting of -1000, which determines where in the DSP chain the encoding is performed.
        /// That can be changed using the <see cref="DSPPriority"/> config option.
        /// </para>
        /// <para>
        /// Besides the automatic DSP system, data can also be manually fed to the encoder via the <see cref="EncodeWrite(int,IntPtr,int)" /> function.
        /// Both methods can be used together, but in general, the "automatic" system ought be paused when using the "manual" system, by use of the <see cref="EncodeFlags.Pause"/> flag or the <see cref="EncodeSetPaused" /> function.
        /// </para>
        /// <para>
        /// When queued encoding is enabled via the <see cref="EncodeFlags.Queue"/> flag, the DSP system or <see cref="EncodeWrite(int,IntPtr,int)" /> call will just buffer the data, and the data will then be fed to the encoder by another thread.
        /// The buffer will grow as needed to hold the queued data, up to a limit specified by the <see cref="EncodeFlags.Queue"/> config option.
        /// If the limit is exceeded (or there is no free memory), data will be lost;
        /// <see cref="EncodeSetNotify" /> can be used to be notified of that occurrence.
        /// The amount of data that is currently queued, as well as the queue limit and how much data has been lost, is available from <see cref="EncodeGetCount" />.
        /// </para>
        /// <para>When done encoding, use <see cref="EncodeStop(int)" /> to close the encoder.</para>
        /// <para>
        /// Multiple encoders can be set on a channel.
        /// For simplicity, the encoder functions will accept either an encoder handle or a channel handle.
        /// When using a channel handle, the function is applied to all encoders that are set on that channel.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The codec specified in <paramref name="Format" /> couldn't be initialized.</exception>
        /// <exception cref="Errors.Create">The file couldn't be created.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int EncodeStartACM(int Handle, IntPtr Format, EncodeFlags Flags, string FileName)
        {
            return BASS_Encode_StartACMFile(Handle, Format, Flags | EncodeFlags.Unicode, FileName);
        }
    }
}