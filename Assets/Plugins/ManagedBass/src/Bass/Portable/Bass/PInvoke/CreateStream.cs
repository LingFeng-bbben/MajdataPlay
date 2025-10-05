using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    public static partial class Bass
    {
        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_StreamCreateFile(bool mem, string file, long offset, long length, BassFlags flags);

        [DllImport(DllName)]
        static extern int BASS_StreamCreateFile(bool mem, IntPtr file, long offset, long length, BassFlags flags);
        
        /// <summary>
        /// Creates a sample stream from an MP3, MP2, MP1, OGG, WAV, AIFF or plugin supported file.
        /// </summary>
        /// <param name="File">Filename for which a stream should be created.</param>
        /// <param name="Offset">File Offset to begin streaming from.</param>
        /// <param name="Length">Data length... 0 = use all data up to the end of the file.</param>
        /// <param name="Flags">Any combination of <see cref="BassFlags"/>.</param>
        /// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// BASS has built-in support for MPEG, OGG, WAV and AIFF files.
        /// Support for additional formats is available via add-ons, which can be downloaded from the BASS website: <a href="http://www.un4seen.com">www.un4seen.com</a>.
        /// </para>
        /// <para>
        /// MPEG 1.0, 2.0 and 2.5 layer 3 (MP3) files are supported, layers 1 (MP1) and 2 (MP2) are also supported.
        /// Standard RIFF and RF64 WAV files are supported, with the sample data in a PCM format or compressed with an ACM codec, but the codec is required to be installed on the user's system for the WAV to be decoded.
        /// So you should either distribute the codec with your software, or use a codec that comes with Windows (eg. Microsoft ADPCM).
        /// All PCM formats from 8 to 32-bit are supported in WAV and AIFF files, but the output will be restricted to 16-bit unless the <see cref="BassFlags.Float"/> flag is used.
        /// 64-bit floating-point WAV and AIFF files are also supported, but are rendered in 16-bit or 32-bit floating-point depending on the flags.
        /// The file's original resolution is available via <see cref="ChannelGetInfo(int, out ChannelInfo)" />.
        /// </para>
        /// <para>
        /// Chained OGG files containing multiple logical bitstreams are supported, but seeking within them is only fully supported if the <see cref="BassFlags.Prescan"/> flag is used (or the <see cref="OggPreScan"/> config option is enabled) to have them pre-scanned.
        /// Without pre-scanning, seeking will only be possible back to the start.
        /// The <see cref="PositionFlags.OGG"/> mode can be used with <see cref="ChannelGetLength" /> to get the number of bitstreams and with <see cref="ChannelSetPosition" /> to seek to a particular one.
        /// A <see cref="SyncFlags.OggChange"/> sync can be set via <see cref="ChannelSetSync(int, SyncFlags, long, SyncProcedure, IntPtr)" /> to be informed of when a new bitstream begins during decoding/playback.
        /// </para>
        /// <para>Multi-channel (ie. more than stereo) OGG, WAV and AIFF files are supported.</para>
        /// <para>
        /// Use <see cref="ChannelGetInfo(int, out ChannelInfo)" /> to retrieve information on the format (sample rate, resolution, channels) of the stream.
        /// The playback length of the stream can be retrieved using <see cref="ChannelGetLength" />.
        /// </para>
        /// <para>
        /// If <paramref name="Length"/> = 0 (use all data up to the end of the file), and the file length increases after creating the stream (ie. the file is still being written), then BASS will play the extra data too, but the length returned by <see cref="ChannelGetLength" /> will not be updated until the end is reached.
        /// The <see cref="StreamGetFilePosition" /> return values will be updated during playback of the extra data though.
        /// </para>
        /// <para>
        /// To stream a file from the internet, use <see cref="CreateStream(string, int, BassFlags, DownloadProcedure, IntPtr)" />.
        /// To stream from other locations, see <see cref="CreateStream(StreamSystem, BassFlags, FileProcedures, IntPtr)" />.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// On Windows and Windows CE, ACM codecs are supported with compressed WAV files.
        /// Media Foundation codecs are also supported on Windows 7 and updated versions of Vista, including support for AAC/MP4 and WMA.
        /// On iOS and OSX, CoreAudio codecs are supported, adding support for any file formats that have a codec installed.
        /// Media Foundation and CoreAudio codecs are only tried after the built-in decoders and any plugins have rejected the file.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">Only decoding channels (<see cref="BassFlags.Decode"/>) are allowed when using the <see cref="NoSoundDevice"/> device. The <see cref="BassFlags.AutoFree"/> flag is also unavailable to decoding channels.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Length" /> must be specified when streaming from memory.</exception>
        /// <exception cref="Errors.FileOpen">The <paramref name="File"/> could not be opened.</exception>
        /// <exception cref="Errors.FileFormat">The file's format is not recognised/supported.</exception>
        /// <exception cref="Errors.Codec">The file uses a codec that's not available/supported. This can apply to WAV and AIFF files, and also MP3 files when using the "MP3-free" BASS version.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If the stream is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.</exception>
        /// <exception cref="Errors.Speaker">The specified SPEAKER flags are invalid. The device/drivers do not support them, they are attempting to assign a stereo stream to a mono speaker or 3D functionality is enabled.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">Could not initialize 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>  
        public static int CreateStream(string File, long Offset = 0, long Length = 0, BassFlags Flags = BassFlags.Default)
        {
            return BASS_StreamCreateFile(false, File, Offset, Length, Flags | BassFlags.Unicode);
        }
        
        /// <summary>
        /// Creates a sample stream from an MP3, MP2, MP1, OGG, WAV, AIFF or plugin supported memory IntPtr.
        /// </summary>
        /// <param name="Memory">An unmanaged pointer to the memory location as an IntPtr.</param>
        /// <param name="Offset">Offset to begin streaming from.</param>
        /// <param name="Length">Data length (needs to be set to the length of the memory stream in bytes which should be played).</param>
        /// <param name="Flags">Any combination of <see cref="BassFlags"/>.</param>
        /// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// BASS has built-in support for MPEG, OGG, WAV and AIFF files.
        /// Support for additional formats is available via add-ons, which can be downloaded from the BASS website: <a href="http://www.un4seen.com">www.un4seen.com</a>.
        /// </para>
        /// <para>
        /// MPEG 1.0, 2.0 and 2.5 layer 3 (MP3) files are supported, layers 1 (MP1) and 2 (MP2) are also supported.
        /// Standard RIFF and RF64 WAV files are supported, with the sample data in a PCM format or compressed with an ACM codec, but the codec is required to be installed on the user's system for the WAV to be decoded.
        /// So you should either distribute the codec with your software, or use a codec that comes with Windows (eg. Microsoft ADPCM).
        /// All PCM formats from 8 to 32-bit are supported in WAV and AIFF files, but the output will be restricted to 16-bit unless the <see cref="BassFlags.Float"/> flag is used.
        /// 64-bit floating-point WAV and AIFF files are also supported, but are rendered in 16-bit or 32-bit floating-point depending on the flags.
        /// The file's original resolution is available via <see cref="ChannelGetInfo(int, out ChannelInfo)" />.
        /// </para>
        /// <para>
        /// Chained OGG files containing multiple logical bitstreams are supported, but seeking within them is only fully supported if the <see cref="BassFlags.Prescan"/> flag is used (or the <see cref="OggPreScan"/> config option is enabled) to have them pre-scanned.
        /// Without pre-scanning, seeking will only be possible back to the start.
        /// The <see cref="PositionFlags.OGG"/> mode can be used with <see cref="ChannelGetLength" /> to get the number of bitstreams and with <see cref="ChannelSetPosition" /> to seek to a particular one.
        /// A <see cref="SyncFlags.OggChange"/> sync can be set via <see cref="ChannelSetSync(int, SyncFlags, long, SyncProcedure, IntPtr)" /> to be informed of when a new bitstream begins during decoding/playback.
        /// </para>
        /// <para>Multi-channel (ie. more than stereo) OGG, WAV and AIFF files are supported.</para>
        /// <para>
        /// Use <see cref="ChannelGetInfo(int, out ChannelInfo)" /> to retrieve information on the format (sample rate, resolution, channels) of the stream.
        /// The playback length of the stream can be retrieved using <see cref="ChannelGetLength" />.
        /// </para>
        /// <para>
        /// If <paramref name="Length"/> = 0 (use all data up to the end of the file), and the file length increases after creating the stream (ie. the file is still being written), then BASS will play the extra data too, but the length returned by <see cref="ChannelGetLength" /> will not be updated until the end is reached.
        /// The <see cref="StreamGetFilePosition" /> return values will be updated during playback of the extra data though.
        /// </para>
        /// <para>
        /// When streaming from memory, the memory must not be freed before the stream is freed.
        /// There may be exceptions to that with some add-ons (see the documentation).
        /// </para>
        /// <para>
        /// To stream a file from the internet, use <see cref="CreateStream(string, int, BassFlags, DownloadProcedure, IntPtr)" />.
        /// To stream from other locations, see <see cref="CreateStream(StreamSystem, BassFlags, FileProcedures, IntPtr)" />.
        /// </para>
        /// <para>The Memory buffer must be pinned when using this overload.</para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// On Windows and Windows CE, ACM codecs are supported with compressed WAV files.
        /// Media Foundation codecs are also supported on Windows 7 and updated versions of Vista, including support for AAC/MP4 and WMA.
        /// On iOS and OSX, CoreAudio codecs are supported, adding support for any file formats that have a codec installed.
        /// Media Foundation and CoreAudio codecs are only tried after the built-in decoders and any plugins have rejected the file.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">Only decoding channels (<see cref="BassFlags.Decode"/>) are allowed when using the <see cref="NoSoundDevice"/> device. The <see cref="BassFlags.AutoFree"/> flag is also unavailable to decoding channels.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Length" /> must be specified when streaming from memory.</exception>
        /// <exception cref="Errors.FileFormat">The file's format is not recognised/supported.</exception>
        /// <exception cref="Errors.Codec">The file uses a codec that's not available/supported. This can apply to WAV and AIFF files, and also MP3 files when using the "MP3-free" BASS version.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If the stream is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.</exception>
        /// <exception cref="Errors.Speaker">The specified SPEAKER flags are invalid. The device/drivers do not support them, they are attempting to assign a stereo stream to a mono speaker or 3D functionality is enabled.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">Could not initialize 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int CreateStream(IntPtr Memory, long Offset, long Length, BassFlags Flags = BassFlags.Default)
        {
            return BASS_StreamCreateFile(true, new IntPtr(Memory.ToInt64() + Offset), 0, Length, Flags);
        }
        
        /// <summary>
        /// Creates a sample stream from an MP3, MP2, MP1, OGG, WAV, AIFF or plugin supported file in memory (byte[]).
        /// </summary>
        /// <param name="Memory">A byte[] containing file data.</param>
        /// <param name="Offset">Offset to begin streaming from.</param>
        /// <param name="Length">Data length (needs to be set to the length of the memory stream in bytes which should be played).</param>
        /// <param name="Flags">Any combination of <see cref="BassFlags"/>.</param>
        /// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// BASS has built-in support for MPEG, OGG, WAV and AIFF files.
        /// Support for additional formats is available via add-ons, which can be downloaded from the BASS website: <a href="http://www.un4seen.com">www.un4seen.com</a>.
        /// </para>
        /// <para>
        /// MPEG 1.0, 2.0 and 2.5 layer 3 (MP3) files are supported, layers 1 (MP1) and 2 (MP2) are also supported.
        /// Standard RIFF and RF64 WAV files are supported, with the sample data in a PCM format or compressed with an ACM codec, but the codec is required to be installed on the user's system for the WAV to be decoded.
        /// So you should either distribute the codec with your software, or use a codec that comes with Windows (eg. Microsoft ADPCM).
        /// All PCM formats from 8 to 32-bit are supported in WAV and AIFF files, but the output will be restricted to 16-bit unless the <see cref="BassFlags.Float"/> flag is used.
        /// 64-bit floating-point WAV and AIFF files are also supported, but are rendered in 16-bit or 32-bit floating-point depending on the flags.
        /// The file's original resolution is available via <see cref="ChannelGetInfo(int, out ChannelInfo)" />.
        /// </para>
        /// <para>
        /// Chained OGG files containing multiple logical bitstreams are supported, but seeking within them is only fully supported if the <see cref="BassFlags.Prescan"/> flag is used (or the <see cref="OggPreScan"/> config option is enabled) to have them pre-scanned.
        /// Without pre-scanning, seeking will only be possible back to the start.
        /// The <see cref="PositionFlags.OGG"/> mode can be used with <see cref="ChannelGetLength" /> to get the number of bitstreams and with <see cref="ChannelSetPosition" /> to seek to a particular one.
        /// A <see cref="SyncFlags.OggChange"/> sync can be set via <see cref="ChannelSetSync(int, SyncFlags, long, SyncProcedure, IntPtr)" /> to be informed of when a new bitstream begins during decoding/playback.
        /// </para>
        /// <para>Multi-channel (ie. more than stereo) OGG, WAV and AIFF files are supported.</para>
        /// <para>
        /// Use <see cref="ChannelGetInfo(int, out ChannelInfo)" /> to retrieve information on the format (sample rate, resolution, channels) of the stream.
        /// The playback length of the stream can be retrieved using <see cref="ChannelGetLength" />.
        /// </para>
        /// <para>
        /// If <paramref name="Length"/> = 0 (use all data up to the end of the file), and the file length increases after creating the stream (ie. the file is still being written), then BASS will play the extra data too, but the length returned by <see cref="ChannelGetLength" /> will not be updated until the end is reached.
        /// The <see cref="StreamGetFilePosition" /> return values will be updated during playback of the extra data though.
        /// </para>
        /// <para>
        /// The <paramref name="Memory"/> is pinned by this overload and freed when the stream is freed.
        /// </para>
        /// <para>
        /// To stream a file from the internet, use <see cref="CreateStream(string, int, BassFlags, DownloadProcedure, IntPtr)" />.
        /// To stream from other locations, see <see cref="CreateStream(StreamSystem, BassFlags, FileProcedures, IntPtr)" />.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// On Windows and Windows CE, ACM codecs are supported with compressed WAV files.
        /// Media Foundation codecs are also supported on Windows 7 and updated versions of Vista, including support for AAC/MP4 and WMA.
        /// On iOS and OSX, CoreAudio codecs are supported, adding support for any file formats that have a codec installed.
        /// Media Foundation and CoreAudio codecs are only tried after the built-in decoders and any plugins have rejected the file.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">Only decoding channels (<see cref="BassFlags.Decode"/>) are allowed when using the <see cref="NoSoundDevice"/> device. The <see cref="BassFlags.AutoFree"/> flag is also unavailable to decoding channels.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Length" /> must be specified when streaming from memory.</exception>
        /// <exception cref="Errors.FileFormat">The file's format is not recognised/supported.</exception>
        /// <exception cref="Errors.Codec">The file uses a codec that's not available/supported. This can apply to WAV and AIFF files, and also MP3 files when using the "MP3-free" BASS version.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If the stream is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.</exception>
        /// <exception cref="Errors.Speaker">The specified SPEAKER flags are invalid. The device/drivers do not support them, they are attempting to assign a stereo stream to a mono speaker or 3D functionality is enabled.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">Could not initialize 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int CreateStream(byte[] Memory, long Offset, long Length, BassFlags Flags)
        {
            return GCPin.CreateStreamHelper(Pointer => CreateStream(Pointer, Offset, Length, Flags), Memory);
        }

        [DllImport(DllName)]
        static extern int BASS_StreamCreateFileUser(StreamSystem system, BassFlags flags, [In, Out] FileProcedures procs, IntPtr user);
        
        /// <summary>
        /// Creates a sample stream from an MP3, MP2, MP1, OGG, WAV, AIFF or plugin supported file via user callback functions.
        /// </summary>
        /// <param name="System">File system to use.</param>
        /// <param name="Flags">Any combination of <see cref="BassFlags"/>.</param>
        /// <param name="Procedures">The user defined file function (see <see cref="FileProcedures" />).</param>
        /// <param name="User">User instance data to pass to the callback functions.</param>
        /// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// The buffered file system (<see cref="StreamSystem.Buffer"/>) is what is used by <see cref="CreateStream(string, int, BassFlags, DownloadProcedure, IntPtr)" />.
        /// As the name suggests, data from the file is buffered so that it's readily available for decoding - BASS creates a thread dedicated to "downloading" the data.
        /// This is ideal for when the data is coming from a source that has high latency, like the internet.
        /// It's not possible to seek in buffered file streams, until the download has reached the requested position - it's not possible to seek at all if it's being streamed in blocks.
        /// </para>
        /// <para>
        /// The push buffered file system (<see cref="StreamSystem.BufferPush"/>) is the same, except that instead of the file data being pulled from the <see cref="FileReadProcedure" /> function in a "download" thread, the data is pushed to BASS via <see cref="StreamPutFileData(int, IntPtr, int)" />.
        /// A <see cref="FileReadProcedure" /> function is still required, to get the initial data used in the creation of the stream.
        /// </para>
        /// <para>
        /// The unbuffered file system (<see cref="StreamSystem.NoBuffer"/>) is what is used by <see cref="CreateStream(string, long, long, BassFlags)" />.
        /// In this system, BASS does not do any intermediate buffering - it simply requests data from the file as and when it needs it.
        /// This means that reading (<see cref="FileReadProcedure" />) must be quick, otherwise the decoding will be delayed and playback buffer underruns (old data repeated) are a possibility.
        /// It's not so important for seeking (<see cref="FileSeekProcedure" />) to be fast, as that is generally not required during decoding, except when looping a file.
        /// </para>
        /// <para>In all cases, BASS will automatically stall playback of the stream when insufficient data is available, and resume it when enough data does become available.</para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// On Windows and Windows CE, ACM codecs are supported with compressed WAV files.
        /// Media Foundation codecs are also supported on Windows 7 and updated versions of Vista, including support for AAC/MP4 and WMA.
        /// On iOS and OSX, CoreAudio codecs are supported, adding support for any file formats that have a codec installed.
        /// Media Foundation and CoreAudio codecs are only tried after the built-in decoders and any plugins have rejected the file.
        /// </para>
        /// <para>
        /// A copy is made of the <paramref name="Procedures"/> callback function table, so it does not have to persist beyond this function call.
        /// Unlike Bass.Net, a reference to <paramref name="Procedures"/> doesn't need to be held by you manually.
        /// ManagedBass automatically holds a reference and frees it when the Channel is freed.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">Only decoding channels (<see cref="BassFlags.Decode"/>) are allowed when using the <see cref="NoSoundDevice"/> device. The <see cref="BassFlags.AutoFree"/> flag is also unavailable to decoding channels.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="System" /> is not valid.</exception>
        /// <exception cref="Errors.FileFormat">The file's format is not recognised/supported.</exception>
        /// <exception cref="Errors.Codec">The file uses a codec that's not available/supported. This can apply to WAV and AIFF files, and also MP3 files when using the "MP3-free" BASS version.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If the stream is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.</exception>
        /// <exception cref="Errors.Speaker">The specified SPEAKER flags are invalid. The device/drivers do not support them, they are attempting to assign a stereo stream to a mono speaker or 3D functionality is enabled.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">Could not initialize 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int CreateStream(StreamSystem System, BassFlags Flags, FileProcedures Procedures, IntPtr User = default(IntPtr))
        {
            var h = BASS_StreamCreateFileUser(System, Flags, Procedures, User);

            if (h != 0)
                ChannelReferences.Add(h, 0, Procedures);

            return h;
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_StreamCreateURL(string Url, int Offset, BassFlags Flags, DownloadProcedure Procedure, IntPtr User);
        
        /// <summary>
        /// Creates a sample stream from an MP3, MP2, MP1, OGG, WAV, AIFF or plugin supported file on the internet, optionally receiving the downloaded data in a callback.
        /// </summary>
        /// <param name="Url">
        /// URL of the file to stream.
        /// Should begin with "http://", "https://" or "ftp://", or another add-on supported protocol.
        /// The URL can be followed by custom HTTP request headers to be sent to the server;
        /// the URL and each header should be terminated with a carriage return and line feed ("\r\n").
        /// </param>
        /// <param name="Offset">File position to start streaming from. This is ignored by some servers, specifically when the file length is unknown, for example a Shout/Icecast server.</param>
        /// <param name="Flags">A combination of <see cref="BassFlags" /></param>
        /// <param name="Procedure">Callback function to receive the file as it is downloaded... <see langword="null" /> = no callback.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// Use <see cref="ChannelGetInfo(int, out ChannelInfo)" /> to retrieve information on the format (sample rate, resolution, channels) of the stream.
        /// The playback length of the stream can be retrieved using <see cref="ChannelGetLength(int, PositionFlags)" />.
        /// </para>
        /// <para>
        /// When playing the stream, BASS will stall the playback if there is insufficient data to continue playing.
        /// Playback will automatically be resumed when sufficient data has been downloaded.
        /// <see cref="ChannelIsActive" /> can be used to check if the playback is stalled, and the progress of the file download can be checked with <see cref="StreamGetFilePosition" />.
        /// </para>
        /// <para>When streaming in blocks (<see cref="BassFlags.StreamDownloadBlocks"/>), be careful not to stop/pause the stream for too long, otherwise the connection may timeout due to there being no activity and the stream will end prematurely.</para>
        /// <para>
        /// When streaming from Shoutcast servers, metadata (track titles) may be sent by the server.
        /// The data can be retrieved with <see cref="ChannelGetTags" />.
        /// A sync can also be set (using <see cref="ChannelSetSync" />) so that you are informed when metadata is received.
        /// A <see cref="SyncFlags.OggChange"/> sync can be used to be informed of when a new logical bitstream begins in an Icecast/OGG stream.
        /// </para>
        /// <para>
        /// When using an <paramref name="Offset" />, the file length returned by <see cref="StreamGetFilePosition" /> can be used to check that it was successful by comparing it with the original file length.
        /// Another way to check is to inspect the HTTP headers retrieved with <see cref="ChannelGetTags" />.
        /// </para>
        /// <para>Custom HTTP request headers may be ignored by some plugins, notably BassWma.</para>
        /// <para>
        /// Unlike Bass.Net, a reference to <paramref name="Procedure"/> doesn't need to be held by you manually.
        /// ManagedBass automatically holds a reference and frees it when the Channel is freed.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// On Windows and Windows CE, ACM codecs are supported with compressed WAV files.
        /// Media Foundation codecs are also supported on Windows 7 and updated versions of Vista, including support for AAC and WMA.
        /// On iOS and OSX, CoreAudio codecs are supported, including support for AAC and ALAC.
        /// Media Foundation and CoreAudio codecs are only tried after the built-in decoders and any plugins have rejected the file.
        /// Built-in support for IMA and Microsoft ADPCM WAV files is provided on Linux/Android/Windows CE, while they are supported via ACM and CoreAudio codecs on Windows and OSX/iOS.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">Only decoding channels (<see cref="BassFlags.Decode"/>) are allowed when using the <see cref="NoSoundDevice"/> device. The <see cref="BassFlags.AutoFree"/> flag is also unavailable to decoding channels.</exception>
        /// <exception cref="Errors.NoInternet">No internet connection could be opened. Can be caused by a bad proxy setting.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Url" /> is not a valid URL.</exception>
        /// <exception cref="Errors.Timeout">The server did not respond to the request within the timeout period, as set with <see cref="NetTimeOut"/> config option.</exception>
        /// <exception cref="Errors.FileOpen">The file could not be opened.</exception>
        /// <exception cref="Errors.FileFormat">The file's format is not recognised/supported.</exception>
        /// <exception cref="Errors.Codec">The file uses a codec that's not available/supported. This can apply to WAV and AIFF files, and also MP3 files when using the "MP3-free" BASS version.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If the stream is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.</exception>
        /// <exception cref="Errors.Speaker">The specified Speaker flags are invalid. The device/drivers do not support them, they are attempting to assign a stereo stream to a mono speaker or 3D functionality is enabled.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">Could not initialize 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int CreateStream(string Url, int Offset, BassFlags Flags, DownloadProcedure Procedure, IntPtr User = default(IntPtr))
        {
            var h = BASS_StreamCreateURL(Url, Offset, Flags | BassFlags.Unicode, Procedure, User);

            if (h != 0)
                ChannelReferences.Add(h, 0, Procedure);

            return h;
        }
    }
}