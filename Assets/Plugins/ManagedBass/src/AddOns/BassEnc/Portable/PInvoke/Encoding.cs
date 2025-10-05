using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Enc
{
    public static partial class BassEnc
    {
        /// <summary>
        /// Sends a RIFF chunk to an encoder.
        /// </summary>
        /// <param name="Handle">The encoder Handle... a HENCODE.</param>
        /// <param name="ID">The 4 character chunk id (e.g. 'bext').</param>
        /// <param name="Buffer">The buffer containing the chunk data (without the id).</param>
        /// <param name="Length">The number of bytes in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// BassEnc writes the minimum chunks required of a WAV file: "fmt" and "data", and "ds64" and "fact" when appropriate.
        /// This function can be used to add other chunks. 
        /// For example, a BWF "bext" chunk or "INFO" tags.
        /// <para>
        /// Chunks can only be added prior to sample data being sent to the encoder.
        /// The <see cref="EncodeFlags.Pause"/> flag can be used when starting the encoder to ensure that no sample data is sent before additional chunks have been set.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">No RIFF headers/chunks are being sent to the encoder (due to the <see cref="EncodeFlags.NoHeader"/> flag being in effect), or sample data encoding has started.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_AddChunk")]
        public static extern bool EncodeAddChunk(int Handle, string ID, IntPtr Buffer, int Length);

        /// <summary>
        /// Sends a RIFF chunk to an encoder.
        /// </summary>
        /// <param name="Handle">The encoder Handle... a HENCODE.</param>
        /// <param name="ID">The 4 character chunk id (e.g. 'bext').</param>
        /// <param name="Buffer">The buffer containing the chunk data (without the id).</param>
        /// <param name="Length">The number of bytes in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// BassEnc writes the minimum chunks required of a WAV file: "fmt" and "data", and "ds64" and "fact" when appropriate.
        /// This function can be used to add other chunks. 
        /// For example, a BWF "bext" chunk or "INFO" tags.
        /// <para>
        /// Chunks can only be added prior to sample data being sent to the encoder.
        /// The <see cref="EncodeFlags.Pause"/> flag can be used when starting the encoder to ensure that no sample data is sent before additional chunks have been set.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">No RIFF headers/chunks are being sent to the encoder (due to the <see cref="EncodeFlags.NoHeader"/> flag being in effect), or sample data encoding has started.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_AddChunk")]
        public static extern bool EncodeAddChunk(int Handle, string ID, byte[] Buffer, int Length);
        
        /// <summary>
        /// Retrieves the channel that an encoder is set on.
        /// </summary>
        /// <param name="Handle">The encoder to get the channel from.</param>
        /// <returns>If successful, the encoder's channel Handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_GetChannel")]
        public static extern int EncodeGetChannel(int Handle);

        /// <summary>
        /// Retrieves the amount of data queued, sent to or received from an encoder, or sent to a cast server.
        /// </summary>
        /// <param name="Handle">The encoder Handle.</param>
        /// <param name="Count">The count to retrieve (see <see cref="EncodeCount"/>).</param>
        /// <returns>If successful, the requested count (in bytes) is returned, else -1 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// The queue counts are based on the channel's sample format (floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled),
        /// while the <see cref="EncodeCount.In"/> count is based on the sample format used by the encoder,
        /// which could be different if one of the Floating-point conversion flags is active or the encoder is using an ACM codec (which take 16-bit data).
        /// </para>
        /// <para>
        /// When the encoder output is being sent to a cast server, the <see cref="EncodeCount.Cast"/> count will match the <see cref="EncodeCount.Out"/> count,
        /// unless there have been problems (eg. network timeout) that have caused data to be dropped.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The encoder does not have a queue.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Count" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_GetCount")]
        public static extern long EncodeGetCount(int Handle, EncodeCount Count);

		/// <summary>
		/// Checks if an encoder is running on a channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <returns>The return value is one of <see cref="PlaybackState"/> values.</returns>
		/// <remarks>
		/// <para>When checking if there's an encoder running on a channel, and there are multiple encoders on the channel, <see cref="PlaybackState.Playing"/> will be returned if any of them are active.</para>
		/// <para>If an encoder stops running prematurely, <see cref="EncodeStop(int)" /> should still be called to release resources that were allocated for the encoding.</para>
		/// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_Encode_IsActive")]
        public static extern PlaybackState EncodeIsActive(int Handle);
        
		/// <summary>
		/// Moves an encoder (or all encoders on a channel) to another channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <param name="Channel">The channel to move the encoder(s) to... a HSTREAM, HMUSIC, or HRECORD.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
        /// The new channel must have the same sample format (rate, channels, resolution) as the old channel, as that is what the encoder is expecting. 
		/// A channel's sample format is available via <see cref="Bass.ChannelGetInfo(int, out ChannelInfo)" />.
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> or <paramref name="Channel" /> is not valid.</exception>
        /// <exception cref="Errors.SampleFormat">The new channel's sample format is not the same as the old channel's.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_SetChannel")]
        public static extern bool EncodeSetChannel(int Handle, int Channel);

        /// <summary>
        /// Sets a callback function on an encoder (or all encoders on a channel) to receive notifications about its status.
        /// </summary>
        /// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Procedure">Callback function to receive the notifications... <see langword="null" /> = no callback.</param>
        /// <param name="User">User instance data to Password to the callback function.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// When setting a notification callback on a channel, it only applies to the encoders that are currently set on the channel.
        /// Subsequent encoders will not automatically have the notification callback set on them, this function will have to be called again to set them up.
        /// </para>
        /// <para>
        /// An encoder can only have one notification callback set.
        /// Subsequent calls of this function can be used to change the callback function, or disable notifications (<paramref name="Procedure"/> = <see langword="null" />).
        /// </para>
        /// <para>
        /// The status of an encoder and its cast connection (if it has one) is checked when data is sent to the encoder or server, and by <see cref="EncodeIsActive" />.
        /// That means an encoder's death will not be detected automatically, and so no notification given, while no data is being encoded.
        /// </para>
        /// <para>If the encoder is already dead when setting up a notification callback, the callback will be triggered immediately.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_SetNotify")]
        public static extern bool EncodeSetNotify(int Handle, EncodeNotifyProcedure Procedure, IntPtr User = default(IntPtr));
        
		/// <summary>
		/// Pauses or resumes encoding on a channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <param name="Paused">Paused?</param>
		/// <returns>If no encoder has been started on the channel, <see langword="false" /> is returned, otherwise <see langword="true" /> is returned.</returns>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// When an encoder is paused, no sample data will be sent to the encoder "automatically".
        /// Data can still be sent to the encoder "manually" though, via the <see cref="EncodeWrite(int, IntPtr, int)" /> function.
        /// </para>
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.s</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_SetPaused")]
        public static extern bool EncodeSetPaused(int Handle, bool Paused = true);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_Start(int handle, string cmdline, EncodeFlags flags, EncodeProcedure proc, IntPtr user);

        /// <summary>
        /// Starts encoding on a channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="CommandLine">The encoder command-line, including the executable filename and any options. Or the output filename if the <see cref="EncodeFlags.PCM"/> flag is specified.</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <param name="Procedure">Optional callback function to receive the encoded data... <see langword="null" /> = no callback. To have the encoded data received by a callback function, the encoder needs to be told to output to STDOUT (instead of a file).</param>
        /// <param name="User">User instance data to Password to the callback function.</param>
        /// <returns>The encoder process Handle is returned if the encoder is successfully started, else 0 is returned (use <see cref="Bass.LastError" /> to get the error code).</returns>
        /// <remarks>
        /// <para>
        /// The encoder must be told (via the command-line) to expect input from STDIN, rather than a file.
        /// The command-line should also tell the encoder what filename to write it's output to, unless you're using a callback function, in which case it should be told to write it's output to STDOUT.
        /// </para>
        /// <para>
        /// No user interaction with the encoder is possible, so anything that would cause the encoder to require the user to press any keys should be avoided.
        /// For example, if the encoder asks whether to overwrite files, the encoder should be instructed to always overwrite (via the command-line), or you should delete the existing file before starting the encoder.
        /// </para>
        /// <para>
        /// Standard RIFF files are limited to a little over 4GB in size.
        /// When writing a WAV file, BASSenc will automatically stop at that point, so that the file is valid.
        /// That does not apply when sending data to an encoder though, as the encoder may (possibly via a command-line option) ignore the size restriction, but if it does not, it could mean that the encoder stops after a few hours (depending on the sample format).
        /// If longer encodings are needed, the <see cref="EncodeFlags.NoHeader"/> flag can be used to omit the WAVE header, and the encoder informed of the sample format via the command-line instead.
        /// The 4GB size limit can also be overcome with the <see cref="EncodeFlags.RF64"/> flag, but most encoders are unlikely to support RF64.
        /// </para>
        /// <para>
        /// When writing an RF64 WAV file, a standard RIFF header will still be written initially, which will only be replaced by an RF64 header at the end if the file size has exceeded the standard limit.
        /// When an encoder is used, it is not possible to go back and change the header at the end, so the RF64 header is sent at the beginning in that case.
        /// </para>
        /// <para>
        /// Internally, the sending of sample data to the encoder is implemented via a DSP callback on the channel.
        /// That means when you play the channel (or call <see cref="Bass.ChannelGetData(int,IntPtr,int)" /> if it's a decoding channel), the sample data will be sent to the encoder at the same time. 
        /// It also means that if you use the <see cref="Bass.FloatingPointDSP"/> option, then the sample data will be 32-bit floating-point, and you'll need to use one of the Floating-point flags if the encoder does not support floating-point sample data. 
        /// The <see cref="Bass.FloatingPointDSP"/> setting should not be changed while encoding is in progress.
        /// </para>
        /// <para>The encoder DSP has a priority setting of -1000, so if you want to set DSP/FX on the channel and have them present in the encoding, set their priority above that.</para>
        /// <para>
        /// Besides the automatic DSP system, data can also be manually fed to the encoder via the <see cref="EncodeWrite(int,IntPtr,int)" /> function.
        /// Both methods can be used together, but in general, the "automatic" system ought be paused when using the "manual" system, by use of the <see cref="EncodeFlags.Pause"/> flag or the <see cref="EncodeSetPaused" /> function.
        /// </para>
        /// <para>
        /// When queued encoding is enabled via the <see cref="EncodeFlags.Queue"/> flag, the DSP system or <see cref="EncodeWrite(int,IntPtr,int)" /> call will just buffer the data, and the data will then be fed to the encoder by another thread.
        /// The buffer will grow as needed to hold the queued data, up to a limit specified by the <see cref="Queue"/> config option.
        /// If the limit is exceeded (or there is no free memory), data will be lost; <see cref="EncodeSetNotify(int,EncodeNotifyProcedure,IntPtr)" /> can be used to be notified of that occurrence.
        /// The amount of data that is currently queued, as well as the queue limit and how much data has been lost, is available from <see cref="EncodeGetCount(int,EncodeCount)" />.
        /// </para>
        /// <para>
        /// <see cref="EncodeIsActive" /> can be used to check that the encoder is still running.
        /// When done encoding, use <see cref="EncodeStop(int)" /> to close the encoder.
        /// </para>
        /// <para>The returned process Handle can be used to do things like change the encoder's priority and get it's exit code.</para>
        /// <para>
        /// Multiple encoders can be set on a channel.
        /// For simplicity, the encoder functions will accept either an encoder Handle or a channel Handle.
        /// When using a channel Handle, the function is applied to all encoders that are set on that channel.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>External encoders are not supported on iOS or Windows CE, so only plain PCM file writing with the <see cref="EncodeFlags.PCM"/> flag is possible on those platforms.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.FileOpen">Couldn't start the encoder. Check that the executable exists.</exception>
        /// <exception cref="Errors.Create">The PCM file couldn't be created.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int EncodeStart(int Handle, string CommandLine, EncodeFlags Flags, EncodeProcedure Procedure, IntPtr User = default(IntPtr))
        {
            return BASS_Encode_Start(Handle, CommandLine, Flags | EncodeFlags.Unicode, Procedure, User);
        }

        /// <summary>
        /// Sets up an encoder on a channel, using a CoreAudio codec and sending the output to a user defined function (iOS and Mac).
        /// </summary>
        /// <param name="Handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="ftype">File format identifier.</param>
        /// <param name="atype">Audio data format identifier</param>
        /// <param name="Flags">A combination of <see cref="EncodeFlags"/>.</param>
        /// <param name="Bitrate">The bitrate in bits per second... 0 = the codec's default bitrate.</param>
        /// <param name="Procedure">Callback function to receive the encoded data.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>The encoder handle is returned if the encoder is successfully started, else 0 is returned. Use <see cref="Bass.LastError"/> to get the error code.</returns>
        /// <remarks>
        /// This function allows CoreAudio codecs to be used for encoding.
        /// The available file and audio data identifiers, as well as other information, can be retreived via the Audio File Services and Audio Format Services APIs, eg. the kAudioFileGlobalInfo_WritableTypes and kAudioFormatProperty_EncodeFormatIDs properties.
        /// <para>
        /// Internally, the sending of sample data to the encoder is implemented via a DSP callback on the channel.
        /// That means when you play the channel (or call <see cref="Bass.ChannelGetData(int,IntPtr,int)" /> if it's a decoding channel), the sample data will be sent to the encoder at the same time. 
        /// It also means that if you use the <see cref="Bass.FloatingPointDSP"/> option, then the sample data will be 32-bit floating-point, and you'll need to use one of the Floating-point flags if the encoder does not support floating-point sample data. 
        /// The <see cref="Bass.FloatingPointDSP"/> setting should not be changed while encoding is in progress.
        /// </para>
        /// <para>The encoder DSP has a priority setting of -1000, so if you want to set DSP/FX on the channel and have them present in the encoding, set their priority above that.</para>
        /// <para>
        /// Besides the automatic DSP system, data can also be manually fed to the encoder via the <see cref="EncodeWrite(int,IntPtr,int)" /> function.
        /// Both methods can be used together, but in general, the "automatic" system ought be paused when using the "manual" system, by use of the <see cref="EncodeFlags.Pause"/> flag or the <see cref="EncodeSetPaused" /> function.
        /// </para>
        /// <para>
        /// When queued encoding is enabled via the <see cref="EncodeFlags.Queue"/> flag, the DSP system or <see cref="EncodeWrite(int,IntPtr,int)" /> call will just buffer the data, and the data will then be fed to the encoder by another thread.
        /// The buffer will grow as needed to hold the queued data, up to a limit specified by the <see cref="Queue"/> config option.
        /// If the limit is exceeded (or there is no free memory), data will be lost; <see cref="EncodeSetNotify(int,EncodeNotifyProcedure,IntPtr)" /> can be used to be notified of that occurrence.
        /// The amount of data that is currently queued, as well as the queue limit and how much data has been lost, is available from <see cref="EncodeGetCount(int,EncodeCount)" />.
        /// </para>
        /// <para>
        /// <see cref="EncodeIsActive" /> can be used to check that the encoder is still running.
        /// When done encoding, use <see cref="EncodeStop(int)" /> to close the encoder.
        /// </para>
        /// <para>The returned process Handle can be used to do things like change the encoder's priority and get it's exit code.</para>
        /// <para>
        /// Multiple encoders can be set on a channel.
        /// For simplicity, the encoder functions will accept either an encoder Handle or a channel Handle.
        /// When using a channel Handle, the function is applied to all encoders that are set on that channel.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>This function is only available on OSX and iOS.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid.</exception>
        /// <exception cref="Errors.FileFormat"><paramref name="ftype"/> is not valid.</exception>
        /// <exception cref="Errors.Codec"><paramref name="atype"/> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable"><paramref name="Bitrate"/> is not supported by the codec.</exception>
        /// <exception cref="Errors.SampleFormat">The channel's sample format is not supported by the codec.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_StartCA")]
        public static extern int EncodeStartCA(int Handle, int ftype, int atype, EncodeFlags Flags, int Bitrate, EncodeProcedureEx Procedure, IntPtr User);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_StartCAFile(int Handle, int ftype, int atype, EncodeFlags flags, int bitrate, string filename);

        /// <summary>
        /// Sets up an encoder on a channel, using a CoreAudio codec and sending the output to a user defined function (iOS and Mac).
        /// </summary>
        /// <param name="Handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="ftype">File format identifier.</param>
        /// <param name="atype">Audio data format identifier</param>
        /// <param name="Flags">A combination of <see cref="EncodeFlags"/>.</param>
        /// <param name="Bitrate">The bitrate in bits per second... 0 = the codec's default bitrate.</param>
        /// <param name="Filename">The output filename.</param>
        /// <returns>The encoder handle is returned if the encoder is successfully started, else 0 is returned. Use <see cref="Bass.LastError"/> to get the error code.</returns>
        /// <remarks>
        /// This function allows CoreAudio codecs to be used for encoding.
        /// The available file and audio data identifiers, as well as other information, can be retrieved via the Audio File Services and Audio Format Services APIs, eg. the kAudioFileGlobalInfo_WritableTypes and kAudioFormatProperty_EncodeFormatIDs properties.
        /// <para>
        /// Internally, the sending of sample data to the encoder is implemented via a DSP callback on the channel.
        /// That means when you play the channel (or call <see cref="Bass.ChannelGetData(int,IntPtr,int)" /> if it's a decoding channel), the sample data will be sent to the encoder at the same time. 
        /// It also means that if you use the <see cref="Bass.FloatingPointDSP"/> option, then the sample data will be 32-bit floating-point, and you'll need to use one of the Floating-point flags if the encoder does not support floating-point sample data. 
        /// The <see cref="Bass.FloatingPointDSP"/> setting should not be changed while encoding is in progress.
        /// </para>
        /// <para>The encoder DSP has a priority setting of -1000, so if you want to set DSP/FX on the channel and have them present in the encoding, set their priority above that.</para>
        /// <para>
        /// Besides the automatic DSP system, data can also be manually fed to the encoder via the <see cref="EncodeWrite(int,IntPtr,int)" /> function.
        /// Both methods can be used together, but in general, the "automatic" system ought be paused when using the "manual" system, by use of the <see cref="EncodeFlags.Pause"/> flag or the <see cref="EncodeSetPaused" /> function.
        /// </para>
        /// <para>
        /// When queued encoding is enabled via the <see cref="EncodeFlags.Queue"/> flag, the DSP system or <see cref="EncodeWrite(int,IntPtr,int)" /> call will just buffer the data, and the data will then be fed to the encoder by another thread.
        /// The buffer will grow as needed to hold the queued data, up to a limit specified by the <see cref="Queue"/> config option.
        /// If the limit is exceeded (or there is no free memory), data will be lost; <see cref="EncodeSetNotify(int,EncodeNotifyProcedure,IntPtr)" /> can be used to be notified of that occurrence.
        /// The amount of data that is currently queued, as well as the queue limit and how much data has been lost, is available from <see cref="EncodeGetCount(int,EncodeCount)" />.
        /// </para>
        /// <para>
        /// <see cref="EncodeIsActive" /> can be used to check that the encoder is still running.
        /// When done encoding, use <see cref="EncodeStop(int)" /> to close the encoder.
        /// </para>
        /// <para>The returned process Handle can be used to do things like change the encoder's priority and get it's exit code.</para>
        /// <para>
        /// Multiple encoders can be set on a channel.
        /// For simplicity, the encoder functions will accept either an encoder Handle or a channel Handle.
        /// When using a channel Handle, the function is applied to all encoders that are set on that channel.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>This function is only available on OSX and iOS.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid.</exception>
        /// <exception cref="Errors.FileFormat"><paramref name="ftype"/> is not valid.</exception>
        /// <exception cref="Errors.Codec"><paramref name="atype"/> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable"><paramref name="Bitrate"/> is not supported by the codec.</exception>
        /// <exception cref="Errors.SampleFormat">The channel's sample format is not supported by the codec.</exception>
        /// <exception cref="Errors.Create">The file could not be created.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int EncodeStartCA(int Handle, int ftype, int atype, EncodeFlags Flags, int Bitrate, string Filename)
        {
            return BASS_Encode_StartCAFile(Handle, ftype, atype, Flags | EncodeFlags.Unicode, Bitrate, Filename);
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_StartLimit(int handle, string cmdline, EncodeFlags flags, EncodeProcedure proc, IntPtr user, int limit);

        /// <summary>
        /// Starts encoding on a channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="CommandLine">The encoder command-line, including the executable filename and any options. Or the output filename if the <see cref="EncodeFlags.PCM"/> flag is specified.</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <param name="Procedure">Optional callback function to receive the encoded data... <see langword="null" /> = no callback. To have the encoded data received by a callback function, the encoder needs to be told to output to STDOUT (instead of a file).</param>
        /// <param name="User">User instance data to Password to the callback function.</param>
        /// <param name="Limit">The maximum number of bytes that will be encoded (0 = no limit).</param>
        /// <returns>The encoder process Handle is returned if the encoder is successfully started, else 0 is returned (use <see cref="Bass.LastError" /> to get the error code).</returns>
        /// <remarks>
        /// <para>
        /// This function works exactly like <see cref="EncodeStart(int,string,EncodeFlags,EncodeProcedure,IntPtr)" />, but with a <paramref name="Limit" /> parameter added, which is the maximum number of bytes that will be encoded (0=no limit).
        /// Once the limit is hit, the encoder will die.
        /// <see cref="EncodeSetNotify" /> can be used to be notified of that occurrence.
        /// One thing to note is that the limit is applied after any conversion due to the floating-point flags.
        /// </para>
        /// <para>This can be useful in situations where the encoder needs to know in advance how much data it will be receiving. For example, when using a callback function with a file format that stores the length in the header, as the header cannot then be updated at the end of encoding. The length is communicated to the encoder via the WAVE header, so it requires that the BASS_ENCODE_NOHEAD flag is not used.</para>
        /// <para>
        /// The encoder must be told (via the command-line) to expect input from STDIN, rather than a file.
        /// The command-line should also tell the encoder what filename to write it's output to, unless you're using a callback function, in which case it should be told to write it's output to STDOUT.
        /// </para>
        /// <para>
        /// No user interaction with the encoder is possible, so anything that would cause the encoder to require the user to press any keys should be avoided.
        /// For example, if the encoder asks whether to overwrite files, the encoder should be instructed to always overwrite (via the command-line), or you should delete the existing file before starting the encoder.
        /// </para>
        /// <para>
        /// Standard RIFF files are limited to a little over 4GB in size.
        /// When writing a WAV file, BASSenc will automatically stop at that point, so that the file is valid.
        /// That does not apply when sending data to an encoder though, as the encoder may (possibly via a command-line option) ignore the size restriction, but if it does not, it could mean that the encoder stops after a few hours (depending on the sample format).
        /// If longer encodings are needed, the <see cref="EncodeFlags.NoHeader"/> flag can be used to omit the WAVE header, and the encoder informed of the sample format via the command-line instead.
        /// The 4GB size limit can also be overcome with the <see cref="EncodeFlags.RF64"/> flag, but most encoders are unlikely to support RF64.
        /// </para>
        /// <para>
        /// When writing an RF64 WAV file, a standard RIFF header will still be written initially, which will only be replaced by an RF64 header at the end if the file size has exceeded the standard limit.
        /// When an encoder is used, it is not possible to go back and change the header at the end, so the RF64 header is sent at the beginning in that case.
        /// </para>
        /// <para>
        /// Internally, the sending of sample data to the encoder is implemented via a DSP callback on the channel.
        /// That means when you play the channel (or call <see cref="Bass.ChannelGetData(int,IntPtr,int)" /> if it's a decoding channel), the sample data will be sent to the encoder at the same time. 
        /// It also means that if you use the <see cref="Bass.FloatingPointDSP"/> option, then the sample data will be 32-bit floating-point, and you'll need to use one of the Floating-point flags if the encoder does not support floating-point sample data. 
        /// The <see cref="Bass.FloatingPointDSP"/> setting should not be changed while encoding is in progress.
        /// </para>
        /// <para>The encoder DSP has a priority setting of -1000, so if you want to set DSP/FX on the channel and have them present in the encoding, set their priority above that.</para>
        /// <para>
        /// Besides the automatic DSP system, data can also be manually fed to the encoder via the <see cref="EncodeWrite(int,IntPtr,int)" /> function.
        /// Both methods can be used together, but in general, the "automatic" system ought be paused when using the "manual" system, by use of the <see cref="EncodeFlags.Pause"/> flag or the <see cref="EncodeSetPaused" /> function.
        /// </para>
        /// <para>
        /// When queued encoding is enabled via the <see cref="EncodeFlags.Queue"/> flag, the DSP system or <see cref="EncodeWrite(int,IntPtr,int)" /> call will just buffer the data, and the data will then be fed to the encoder by another thread.
        /// The buffer will grow as needed to hold the queued data, up to a limit specified by the <see cref="Queue"/> config option.
        /// If the limit is exceeded (or there is no free memory), data will be lost; <see cref="EncodeSetNotify(int,EncodeNotifyProcedure,IntPtr)" /> can be used to be notified of that occurrence.
        /// The amount of data that is currently queued, as well as the queue limit and how much data has been lost, is available from <see cref="EncodeGetCount(int,EncodeCount)" />.
        /// </para>
        /// <para>
        /// <see cref="EncodeIsActive" /> can be used to check that the encoder is still running.
        /// When done encoding, use <see cref="EncodeStop(int)" /> to close the encoder.
        /// </para>
        /// <para>The returned process Handle can be used to do things like change the encoder's priority and get it's exit code.</para>
        /// <para>
        /// Multiple encoders can be set on a channel.
        /// For simplicity, the encoder functions will accept either an encoder Handle or a channel Handle.
        /// When using a channel Handle, the function is applied to all encoders that are set on that channel.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>External encoders are not supported on iOS or Windows CE, so only plain PCM file writing with the <see cref="EncodeFlags.PCM"/> flag is possible on those platforms.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.FileOpen">Couldn't start the encoder. Check that the executable exists.</exception>
        /// <exception cref="Errors.Create">The PCM file couldn't be created.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int EncodeStart(int Handle, string CommandLine, EncodeFlags Flags, EncodeProcedure Procedure, IntPtr User, int Limit)
        {
            return BASS_Encode_StartLimit(Handle, CommandLine, Flags | EncodeFlags.Unicode, Procedure, User, Limit);
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_StartUser(int handle, string filename, EncodeFlags flags, EncoderProcedure proc, IntPtr user);

        /// <summary>
        /// Sets up a user-provided encoder on a channel.
        /// </summary>
        /// <param name="Handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Filename">Output filename... <see langword="null" /> = no output file.</param>
        /// <param name="Flags">A combination of <see cref="EncodeFlags"/>.</param>
        /// <param name="Procedure">Callback function to receive the sample data and return the encoded data.</param>
        /// <param name="User">User instance data to Password to the callback function.</param>
        /// <returns>The encoder process handle is returned if the encoder is successfully started, else 0 is returned (use <see cref="Bass.LastError" /> to get the error code).</returns>
        /// <remarks>
        /// <para>
        /// This function allows user-provided encoders to be used, which is most useful for platforms where external encoders are unavailable 
        /// for use with <see cref="EncodeStart(int,string,EncodeFlags,EncodeProcedure,IntPtr)" />.
        /// For example, the LAME library could be used with this function instead of the standalone LAME executable with <see cref="EncodeStart(int,string,EncodeFlags,EncodeProcedure,IntPtr)" />.
        /// </para>
        /// <para>
        /// Internally, the sending of sample data to the encoder is implemented via a DSP callback on the channel.
        /// That means when the channel is played (or <see cref="Bass.ChannelGetData(int,IntPtr,int)" /> is called if it is a decoding channel), the sample data will be sent to the encoder at the same time.
        /// It also means that if the <see cref="Bass.FloatingPointDSP"/> option is enabled, the sample data will be 32-bit floating-point, and one of the floating-point flags will be required if the encoder does not support floating-point sample data.
        /// The <see cref="Bass.FloatingPointDSP"/> setting should not be changed while encoding is in progress.
        /// </para>
        /// <para>
        /// By default, the encoder DSP has a priority setting of -1000, which determines where in the DSP chain the encoding is performed.
        /// That can be changed via the <see cref="DSPPriority"/> config option.
        /// </para>
        /// <para>
        /// Besides the automatic DSP system, data can also be manually fed to the encoder via the <see cref="EncodeWrite(int,IntPtr,int)" /> function. 
        /// Both methods can be used together, but in general, the 'automatic' system ought to be paused when using the 'manual' system, via the <see cref="EncodeFlags.Pause"/> flag or the <see cref="EncodeSetPaused" /> function.
        /// Data fed to the encoder manually does not go through the source channel's DSP chain, so any DSP/FX set on the channel will not be applied to the data.
        /// </para>
        /// <para>
        /// When queued encoding is enabled via the <see cref="EncodeFlags.Queue"/> flag, the DSP system or <see cref="EncodeWrite(int,IntPtr,int)" /> call will just buffer the data, and the data will then be fed to the encoder by another thread.
        /// The buffer will grow as needed to hold the queued data, up to a limit specified by the <see cref="Queue"/> config option.
        /// If the limit is exceeded (or there is no free memory), data will be lost;
        /// <see cref="EncodeSetNotify" /> can be used to be notified of that occurrence. 
        /// The amount of data that is currently queued, as well as the queue limit and how much data has been lost, is available from <see cref="EncodeGetCount" />.
        /// </para>
        /// <para>When done encoding, use <see cref="EncodeStop(int)" /> or <see cref="EncodeStop(int,bool)" /> to close the encoder.</para>
        /// <para>
        /// Multiple encoders can be set on a channel.
        /// For convenience, most of the encoder functions will accept either an encoder handle or a channel handle. 
        /// When a channel handle is used, the function is applied to all encoders that are set on that channel.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Create">The file could not be created.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int EncodeStart(int Handle, string Filename, EncodeFlags Flags, EncoderProcedure Procedure, IntPtr User = default(IntPtr))
        {
	        return Procedure != null ? 
		        BASS_Encode_StartUser(Handle, Filename, Flags | EncodeFlags.Unicode, Procedure, User) :
		        BASS_Encode_Start(Handle, Filename, Flags | EncodeFlags.Unicode, null, User);
        }
        
		/// <summary>
		/// Stops encoding on a channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// This function will free an encoder immediately, without waiting for any data that may be remaining in the queue.
        /// <see cref="EncodeStop(int, bool)" /> can be used to have an encoder process the queue before it is freed.
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Stop")]
        public static extern bool EncodeStop(int Handle);
        
		/// <summary>
		/// Stops async encoding on a channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <param name="Queue">Process the queue first? If so, the encoder will not be freed until after any data remaining in the queue has been processed, and it will not accept any new data in the meantime.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// When an encoder is told to wait for its queue to be processed, this function will return immediately and the encoder will be freed in the background after the queued data has been processed.
		/// <see cref="EncodeSetNotify" /> can be used to request notification of when the encoder has been freed.
        /// <see cref="EncodeStop(int)" /> (or this function with queue = <see langword="false" />) can be used to cancel to queue processing and free the encoder immediately.
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_StopEx")]
        public static extern bool EncodeStop(int Handle, bool Queue);

        #region Encode Write
        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">A pointer to the buffer containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, IntPtr Buffer, int Length);

        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">byte[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, byte[] Buffer, int Length);

        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">short[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, short[] Buffer, int Length);

        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">int[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, int[] Buffer, int Length);

        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">float[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, float[] Buffer, int Length);
        #endregion
    }
}