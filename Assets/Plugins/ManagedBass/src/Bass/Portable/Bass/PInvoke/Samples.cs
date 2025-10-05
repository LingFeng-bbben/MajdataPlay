using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    public static partial class Bass
    {
        /// <summary>
        /// Creates/initializes a playback channel for a sample.
        /// </summary>
        /// <param name="Sample">Handle of the sample to play.</param>
        /// <param name="OnlyNew">Do not recycle/override one of the sample's existing channels?</param>
        /// <returns>If successful, the handle of the new channel is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// Use <see cref="SampleGetInfo(int, SampleInfo)" /> and <see cref="SampleSetInfo(int, SampleInfo)" /> to set a sample's default attributes, which are used when creating a channel.
        /// After creation, a channel's attributes can be changed via <see cref="ChannelSetAttribute(int, ChannelAttribute, float)" />, <see cref="ChannelSet3DAttributes" /> and <see cref="ChannelSet3DPosition" />.
        /// <see cref="Apply3D" /> should be called before starting playback of a 3D sample, even if you just want to use the default settings.
        /// </para>
        /// <para>
        /// If a sample has a maximum number of simultaneous playbacks of 1 (the max parameter was 1 when calling <see cref="SampleLoad(string, long, int, int, BassFlags)" /> or <see cref="CreateSample" />), then the HCHANNEL handle returned will be identical to the HSAMPLE handle.
        /// That means you can use the HSAMPLE handle with functions that usually require a HCHANNEL handle, but you must still call this function first to initialize the channel.
        /// </para>
        /// <para>
        /// A sample channel is automatically freed when it's overridden by a new channel, or when stopped manually via <see cref="ChannelStop" />, <see cref="SampleStop" /> or <see cref="Stop" />.
        /// If you wish to stop a channel and re-use it, it should be paused (<see cref="ChannelPause" />) instead of stopped.
        /// Determining whether a channel still exists can be done by trying to use the handle in a function call, eg. <see cref="ChannelGetAttribute(int, ChannelAttribute, out float)" />.
        /// </para>
        /// <para>When channel overriding has been enabled via an override flag and there are multiple candidates for overriding (eg. with identical volume), the oldest of them will be chosen to make way for the new channel.</para>
        /// <para>
        /// The new channel will have an initial state of being paused (<see cref="PlaybackState.Paused"/>).
        /// This prevents the channel being claimed by another call of this function before it has been played, unless it gets overridden due to a lack of free channels.
        /// </para>
        /// <para>All of a sample's channels share the same sample data, and just have their own individual playback state information (volume/position/etc).</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Sample" /> is not a valid sample handle.</exception>
        /// <exception cref="Errors.NoChannel">The sample has no free channels... the maximum number of simultaneous playbacks has been reached, and no override flag was specified for the sample or onlynew = <see langword="true" />.</exception>
        /// <exception cref="Errors.Timeout">The sample's minimum time gap (<see cref="SampleInfo" />) has not yet passed since the last channel was created.</exception>
        public static int SampleGetChannel(int Sample, bool OnlyNew = false) => SampleGetChannel(Sample, OnlyNew ? BassFlags.SampleChannelNew : BassFlags.Default);

        /// <summary>
        /// Creates/initializes a playback channel for a sample.
        /// </summary>
        /// <param name="Sample">Handle of the sample to play.</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <returns>If successful, the handle of the new channel is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// Use <see cref="SampleGetInfo(int, SampleInfo)" /> and <see cref="SampleSetInfo(int, SampleInfo)" /> to set a sample's default attributes, which are used when creating a channel.
        /// After creation, a channel's attributes can be changed via <see cref="ChannelSetAttribute(int, ChannelAttribute, float)" />, <see cref="ChannelSet3DAttributes" /> and <see cref="ChannelSet3DPosition" />.
        /// <see cref="Apply3D" /> should be called before starting playback of a 3D sample, even if you just want to use the default settings.
        /// </para>
        /// <para>
        /// If a sample has a maximum number of simultaneous playbacks of 1 (the max parameter was 1 when calling <see cref="SampleLoad(string, long, int, int, BassFlags)" /> or <see cref="CreateSample" />), then the HCHANNEL handle returned will be identical to the HSAMPLE handle.
        /// That means you can use the HSAMPLE handle with functions that usually require a HCHANNEL handle, but you must still call this function first to initialize the channel.
        /// </para>
        /// <para>
        /// A sample channel is automatically freed when it's overridden by a new channel, or when stopped manually via <see cref="ChannelStop" />, <see cref="SampleStop" /> or <see cref="Stop" />.
        /// If you wish to stop a channel and re-use it, it should be paused (<see cref="ChannelPause" />) instead of stopped.
        /// Determining whether a channel still exists can be done by trying to use the handle in a function call, eg. <see cref="ChannelGetAttribute(int, ChannelAttribute, out float)" />.
        /// </para>
        /// <para>When channel overriding has been enabled via an override flag and there are multiple candidates for overriding (eg. with identical volume), the oldest of them will be chosen to make way for the new channel.</para>
        /// <para>
        /// The new channel will have an initial state of being paused (<see cref="PlaybackState.Paused"/>).
        /// This prevents the channel being claimed by another call of this function before it has been played, unless it gets overridden due to a lack of free channels.
        /// </para>
        /// <para>All of a sample's channels share the same sample data, and just have their own individual playback state information (volume/position/etc).</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Sample" /> is not a valid sample handle.</exception>
        /// <exception cref="Errors.NoChannel">The sample has no free channels... the maximum number of simultaneous playbacks has been reached, and no override flag or <see cref="BassFlags.SampleChannelNew"/> was specified for the sample.</exception>
        /// <exception cref="Errors.Timeout">The sample's minimum time gap (<see cref="SampleInfo" />) has not yet passed since the last channel was created.</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleGetChannel")]
        public static extern int SampleGetChannel(int Sample, BassFlags Flags);

        /// <summary>
        /// Frees a sample's resources.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleFree")]
        public static extern bool SampleFree(int Handle);

        #region Sample Set Data
        /// <summary>
        /// Sets a sample's data.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <param name="Buffer">Pointer to the data to set.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The required length and format of the data can be retrieved via <see cref="SampleGetInfo(int, SampleInfo)" />.
        /// <para>A sample's data can be set at any time, including during playback.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleSetData")]
        public static extern bool SampleSetData(int Handle, IntPtr Buffer);

        /// <summary>
        /// Sets a sample's data.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <param name="Buffer">byte[] containing the data to set.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The required length and format of the data can be retrieved via <see cref="SampleGetInfo(int, SampleInfo)" />.
        /// <para>A sample's data can be set at any time, including during playback.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleSetData")]
        public static extern bool SampleSetData(int Handle, byte[] Buffer);

        /// <summary>
        /// Sets a sample's data.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <param name="Buffer">int[] containing the data to set.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The required length and format of the data can be retrieved via <see cref="SampleGetInfo(int, SampleInfo)" />.
        /// <para>A sample's data can be set at any time, including during playback.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleSetData")]
        public static extern bool SampleSetData(int Handle, int[] Buffer);

        /// <summary>
        /// Sets a sample's data.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <param name="Buffer">short[] containing the data to set.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The required length and format of the data can be retrieved via <see cref="SampleGetInfo(int, SampleInfo)" />.
        /// <para>A sample's data can be set at any time, including during playback.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleSetData")]
        public static extern bool SampleSetData(int Handle, short[] Buffer);

        /// <summary>
        /// Sets a sample's data.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <param name="Buffer">float[] containing the data to set.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The required length and format of the data can be retrieved via <see cref="SampleGetInfo(int, SampleInfo)" />.
        /// <para>A sample's data can be set at any time, including during playback.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleSetData")]
        public static extern bool SampleSetData(int Handle, float[] Buffer);
        #endregion

        /// <summary>
        /// Initiates the creation of a user generated sample.
        /// </summary>
        /// <param name="Length">The sample's length, in bytes.</param>
        /// <param name="Frequency">The default sample rate.</param>
        /// <param name="Channels">The number of channels... 1 = mono, 2 = stereo, etc... More than stereo requires WDM drivers in Windows.</param>
        /// <param name="Max">Maximum number of simultaneous playbacks... 1 (min) - 65535 (max)... use one of the override flags to choose the override decider, in the case of there being no free channel available for playback (ie. the sample is already playing max times).</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <returns>If successful, the new sample's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// The sample's initial content is undefined.
        /// <see cref="SampleSetData(int, IntPtr)" /> should be used to set the sample's data.
        /// </para>
        /// <para>
        /// Unless the BassFlags.SoftwareMixing flag is used, the sample will use hardware mixing if hardware resources are available.
        /// Use <see cref="GetInfo(out BassInfo)" /> to see if there are hardware mixing resources available, and which sample formats are supported by the hardware.
        /// The <see cref="BassFlags.VAM"/> flag allows a sample to be played by both hardware and software, with the decision made when the sample is played rather than when it's loaded.
        /// A sample's VAM options are set via <see cref="SampleSetInfo" />.
        /// </para>
        /// <para>To play a sample, first a channel must be obtained using <see cref="SampleGetChannel" />, which can then be played using <see cref="ChannelPlay" />.</para>
        /// <para>If you want to play a large or one-off sample, then it would probably be better to stream it instead with <see cref="CreateStream(int, int, BassFlags, StreamProcedure, IntPtr)" />.</para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// The <see cref="BassFlags.VAM"/> flag requires DirectX 7 (or above).
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">Sample functions are not available when using the <see cref="NoSoundDevice"/> device.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Max" /> is invalid.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If the sample is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">Could not initialize 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleCreate")]
        public static extern int CreateSample(int Length, int Frequency, int Channels, int Max, BassFlags Flags);

        #region Sample Get Data
        /// <summary>
        /// Retrieves a copy of a sample's data.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <param name="Buffer">Pointer to a buffer to receive the data.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>The buffer must be big enough to receive the sample's data, the size of which can be retrieved via <see cref="SampleGetInfo(int, SampleInfo)" />.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleGetData")]
        public static extern bool SampleGetData(int Handle, IntPtr Buffer);

        /// <summary>
        /// Retrieves a copy of a sample's data.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <param name="Buffer">byte[] to receive the data.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>The buffer must be big enough to receive the sample's data, the size of which can be retrieved via <see cref="SampleGetInfo(int, SampleInfo)" />.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleGetData")]
        public static extern bool SampleGetData(int Handle, [In, Out] byte[] Buffer);

        /// <summary>
        /// Retrieves a copy of a sample's data.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <param name="Buffer">short[] to receive the data.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>The buffer must be big enough to receive the sample's data, the size of which can be retrieved via <see cref="SampleGetInfo(int, SampleInfo)" />.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleGetData")]
        public static extern bool SampleGetData(int Handle, [In, Out] short[] Buffer);

        /// <summary>
        /// Retrieves a copy of a sample's data.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <param name="Buffer">int[] to receive the data.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>The buffer must be big enough to receive the sample's data, the size of which can be retrieved via <see cref="SampleGetInfo(int, SampleInfo)" />.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleGetData")]
        public static extern bool SampleGetData(int Handle, [In, Out] int[] Buffer);

        /// <summary>
        /// Retrieves a copy of a sample's data.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <param name="Buffer">float[] to receive the data.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>The buffer must be big enough to receive the sample's data, the size of which can be retrieved via <see cref="SampleGetInfo(int, SampleInfo)" />.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleGetData")]
        public static extern bool SampleGetData(int Handle, [In, Out] float[] Buffer);
        #endregion

        #region SampleGetInfo
        /// <summary>
        /// Retrieves a sample's default attributes and other information.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <param name="Info">An instance of the <see cref="SampleInfo" /> class to store the sample information at.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleGetInfo")]
        public static extern bool SampleGetInfo(int Handle, [In, Out] SampleInfo Info);

        /// <summary>
        /// Retrieves a sample's default attributes and other information.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <returns>An instance of the <see cref="SampleInfo" /> class is returned. Throws <see cref="BassException"/> on Error.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        public static SampleInfo SampleGetInfo(int Handle)
        {
            var temp = new SampleInfo();
            if (!SampleGetInfo(Handle, temp))
                throw new BassException();
            return temp;
        }
        #endregion

        /// <summary>
        /// Sets a sample's default attributes.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <param name="Info">An instance of the <see cref="SampleInfo" /> class containing the sample information to set.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// Use this function and <see cref="SampleGetInfo(int, SampleInfo)" /> to edit a sample's default attributes.
        /// Changing a sample's default attributes does not affect any existing channels, it only affects channels subsequently created via <see cref="SampleGetChannel" />.
        /// The exception is the VAM settings, changes to that apply to all the sample's channels at their next playback (<see cref="ChannelPlay" />).
        /// Use <see cref="ChannelSetAttribute(int, ChannelAttribute, float)" /> and <see cref="ChannelSet3DAttributes" /> to change the attributes of an existing sample channel.
        /// </para>
        /// <para>
        /// The sample's maximum number of simultaneous playbacks can be changed via the <see cref="SampleInfo.Max"/> member.
        /// If the new maximum is lower than the existing number of channels, the channels will remain existing until they are stopped.
        /// </para>
        /// <para>
        /// <see cref="SampleInfo.Length"/>, <see cref="SampleInfo.OriginalResolution"/> and <see cref="SampleInfo.Channels"/> can't be modified - any changes are ignored.
        /// <see cref="BassFlags.Byte"/>, <see cref="BassFlags.Mono"/>, <see cref="BassFlags.Bass3D"/>, <see cref="BassFlags.MuteMax"/>, BassFlags.SoftwareMixing and <see cref="BassFlags.VAM"/> also cannot be changed.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_SampleSetInfo")]
        public static extern bool SampleSetInfo(int Handle, SampleInfo Info);

        #region SampleGetChannels
        [DllImport(DllName)]
        static extern int BASS_SampleGetChannels(int handle, [In, Out] int[] channels);

        /// <summary>
        /// Retrieves an array of a sample's existing channels.
        /// </summary>
        /// <param name="Handle">Handle of the sample.</param>
        /// <returns>
        /// If successful, the array of existing channels is returned (which might have zero elements), else <see langword="null" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <remarks>
        /// This overload only returns the existing channels in the array.
        /// <para>If you need to determine whether a particular sample channel still exists, it is simplest to just try it in a function call.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid sample handle.</exception>
        public static int[] SampleGetChannels(int Handle)
        {
            var count = BASS_SampleGetChannels(Handle, null);

            if (count < 0)
                return null;

            var Return = new int[count];

            count = BASS_SampleGetChannels(Handle, Return);

            return count < 0 ? null : Return;
        }
        #endregion

        /// <summary>
        /// Stops all instances of a sample.
        /// </summary>
        /// <param name="Handle">The sample handle.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid sample handle.</exception>
        /// <remarks>If a sample is playing simultaneously multiple times, calling this function will stop them all, which is obviously simpler than calling <see cref="ChannelStop" /> multiple times.</remarks>
        [DllImport(DllName, EntryPoint = "BASS_SampleStop")]
        public static extern bool SampleStop(int Handle);

        #region Sample Load
        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_SampleLoad(bool mem, string file, long offset, int Length, int max, BassFlags flags);

        [DllImport(DllName)]
        static extern int BASS_SampleLoad(bool mem, IntPtr file, long offset, int Length, int max, BassFlags flags);

        /// <summary>
        /// Loads a WAV, AIFF, MP3, MP2, MP1, OGG or plugin supported sample.
        /// </summary>
        /// <param name="File">The file name to load the sample from.</param>
        /// <param name="Offset">File offset to load the sample from.</param>
        /// <param name="Length">Data length... 0 = use all data up to the end of file. If length over-runs the end of the file, it'll automatically be lowered to the end of the file.</param>
        /// <param name="MaxNoOfPlaybacks">Maximum number of simultaneous playbacks... 1 (min) - 65535 (max)... use one of the BASS_SAMPLE_OVER flags to choose the override decider, in the case of there being no free channel available for playback (ie. the sample is already playing max times).</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <returns>If successful, the loaded sample's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>Additional format support is available via the plugin system (see <see cref="PluginLoad" />).</para>
        /// <para>
        /// Unless the BassFlags.SoftwareMixing flag is used, the sample will use hardware mixing if hardware resources are available.
        /// Use <see cref="GetInfo(out BassInfo)" /> to see if there are hardware mixing resources available, and which sample formats are supported by the hardware.
        /// The <see cref="BassFlags.VAM"/> flag allows a sample to be played by both hardware and software, with the decision made when the sample is played rather than when it's loaded.
        /// A sample's VAM options are set via <see cref="SampleSetInfo" />.
        /// </para>
        /// <para>To play a sample, first a channel must be obtained using <see cref="SampleGetChannel" />, which can then be played using <see cref="ChannelPlay" />.</para>
        /// <para>If you want to play a large or one-off sample, then it would probably be better to stream it instead with <see cref="CreateStream(string, long, long, BassFlags)" />.</para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// The <see cref="BassFlags.VAM"/> flag requires DirectX 7 (or above).
        /// </para>
        /// <para>
        /// On Windows and Windows CE, ACM codecs are supported with compressed WAV files.
        /// On iOS and OSX, CoreAudio codecs are supported, adding support for any file formats that have a codec installed.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">Sample functions are not available when using the <see cref="NoSoundDevice"/> device.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="MaxNoOfPlaybacks" /> and/or <paramref name="Length" /> is invalid.</exception>
        /// <exception cref="Errors.FileOpen">The <paramref name="File" /> could not be opened.</exception>
        /// <exception cref="Errors.FileFormat">The <paramref name="File" />'s format is not recognised/supported.</exception>
        /// <exception cref="Errors.Codec">The file uses a codec that's not available/supported. This can apply to WAV and AIFF files, and also MP3 files when using the "MP3-free" BASS version.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If the sample is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">Could not initialize 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int SampleLoad(string File, long Offset, int Length, int MaxNoOfPlaybacks, BassFlags Flags)
        {
            return BASS_SampleLoad(false, File, Offset, Length, MaxNoOfPlaybacks, Flags | BassFlags.Unicode);
        }

        /// <summary>
        /// Loads a WAV, AIFF, MP3, MP2, MP1, OGG or plugin supported sample.
        /// <para>This overload uses an unmanaged IntPtr and implements loading a sample from memory.</para>
        /// </summary>
        /// <param name="Memory">An unmanaged IntPtr to the allocated memory block at which the sample data resides.</param>
        /// <param name="Offset">File offset to load the sample from.</param>
        /// <param name="Length">Data length. Should be set to the length of the data contained in memory.</param>
        /// <param name="MaxNoOfPlaybacks">Maximum number of simultaneous playbacks... 1 (min) - 65535 (max)... use one of the override flags to choose the override decider, in the case of there being no free channel available for playback (ie. the sample is already playing max times).</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/> flags.</param>
        /// <returns>If successful, the loaded sample's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>Additional format support is available via the plugin system (see <see cref="PluginLoad" />).</para>
        /// <para>
        /// Unless the BassFlags.SoftwareMixing flag is used, the sample will use hardware mixing if hardware resources are available.
        /// Use <see cref="GetInfo(out BassInfo)" /> to see if there are hardware mixing resources available, and which sample formats are supported by the hardware.
        /// The <see cref="BassFlags.VAM"/> flag allows a sample to be played by both hardware and software, with the decision made when the sample is played rather than when it's loaded.
        /// A sample's VAM options are set via <see cref="SampleSetInfo" />.
        /// </para>
        /// <para>To play a sample, first a channel must be obtained using <see cref="SampleGetChannel" />, which can then be played using <see cref="ChannelPlay" />.</para>
        /// <para>If you want to play a large or one-off sample, then it would probably be better to stream it instead with <see cref="CreateStream(IntPtr, long, long, BassFlags)" />.</para>
        /// <para>There is no need to pin the memory buffer for this method, since after loading a sample from memory, the memory can safely be discarded, as a copy is made.</para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// The <see cref="BassFlags.VAM"/> flag requires DirectX 7 (or above).
        /// </para>
        /// <para>
        /// On Windows and Windows CE, ACM codecs are supported with compressed WAV files.
        /// On iOS and OSX, CoreAudio codecs are supported, adding support for any file formats that have a codec installed.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">Sample functions are not available when using the <see cref="NoSoundDevice"/> device.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="MaxNoOfPlaybacks" /> and/or <paramref name="Length" /> is invalid. Specifying <paramref name="Length" /> is mandatory when loading from memory.</exception>
        /// <exception cref="Errors.FileOpen">The <paramref name="Memory" /> could not be opened.</exception>
        /// <exception cref="Errors.FileFormat">The <paramref name="Memory" />'s format is not recognised/supported.</exception>
        /// <exception cref="Errors.Codec">The file uses a codec that's not available/supported. This can apply to WAV and AIFF files, and also MP3 files when using the "MP3-free" BASS version.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If the sample is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">Could not initialize 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int SampleLoad(IntPtr Memory, long Offset, int Length, int MaxNoOfPlaybacks, BassFlags Flags)
        {
            return BASS_SampleLoad(true, new IntPtr(Memory.ToInt64() + Offset), 0, Length, MaxNoOfPlaybacks, Flags);
        }

        /// <summary>
        /// Loads a WAV, AIFF, MP3, MP2, MP1, OGG or plugin supported sample.
        /// <para>This overload uses an unmanaged IntPtr and implements loading a sample from memory.</para>
        /// </summary>
        /// <param name="Memory">A byte[] with the sample data to load.</param>
        /// <param name="Offset">File offset to load the sample from.</param>
        /// <param name="Length">Data length. Should be set to the length of the data contained in memory.</param>
        /// <param name="MaxNoOfPlaybacks">Maximum number of simultaneous playbacks... 1 (min) - 65535 (max)... use one of the override flags to choose the override decider, in the case of there being no free channel available for playback (ie. the sample is already playing max times).</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/> flags.</param>
        /// <returns>If successful, the loaded sample's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>Additional format support is available via the plugin system (see <see cref="PluginLoad" />).</para>
        /// <para>
        /// Unless the BassFlags.SoftwareMixing flag is used, the sample will use hardware mixing if hardware resources are available.
        /// Use <see cref="GetInfo(out BassInfo)" /> to see if there are hardware mixing resources available, and which sample formats are supported by the hardware.
        /// The <see cref="BassFlags.VAM"/> flag allows a sample to be played by both hardware and software, with the decision made when the sample is played rather than when it's loaded.
        /// A sample's VAM options are set via <see cref="SampleSetInfo" />.
        /// </para>
        /// <para>To play a sample, first a channel must be obtained using <see cref="SampleGetChannel" />, which can then be played using <see cref="ChannelPlay" />.</para>
        /// <para>If you want to play a large or one-off sample, then it would probably be better to stream it instead with <see cref="CreateStream(byte[], long, long, BassFlags)" />.</para>
        /// <para>The <paramref name="Memory"/> can be safely discarded after calling this method, as a copy of it is made by Bass.</para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// The <see cref="BassFlags.VAM"/> flag requires DirectX 7 (or above).
        /// </para>
        /// <para>
        /// On Windows and Windows CE, ACM codecs are supported with compressed WAV files.
        /// On iOS and OSX, CoreAudio codecs are supported, adding support for any file formats that have a codec installed.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">Sample functions are not available when using the <see cref="NoSoundDevice"/> device.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="MaxNoOfPlaybacks" /> and/or <paramref name="Length" /> is invalid. Specifying <paramref name="Length" /> is mandatory when loading from memory.</exception>
        /// <exception cref="Errors.FileOpen">The <paramref name="Memory" /> could not be opened.</exception>
        /// <exception cref="Errors.FileFormat">The <paramref name="Memory" />'s format is not recognised/supported.</exception>
        /// <exception cref="Errors.Codec">The file uses a codec that's not available/supported. This can apply to WAV and AIFF files, and also MP3 files when using the "MP3-free" BASS version.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If the sample is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">Could not initialize 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int SampleLoad(byte[] Memory, long Offset, int Length, int MaxNoOfPlaybacks, BassFlags Flags)
        {
            return GCPin.CreateStreamHelper(Pointer => SampleLoad(Pointer, Offset, Length, MaxNoOfPlaybacks, Flags), Memory);
        }
        #endregion
    }
}
