using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Asio
{
    public static partial class BassAsio
    {
        /// <summary>
		/// Enable/disable processing of an Asio channel.
		/// </summary>
		/// <param name="Input">Dealing with an input channel? <see langword="false" /> = an output channel.</param>
		/// <param name="Channel">The input/output channel number... 0 = first.</param>
		/// <param name="Procedure">The user defined function to process the channel... <see langword="null" /> = disable the channel.</param>
		/// <param name="User">User instance data to pass to the callback function.</param>
		/// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// All ASIO channels are mono.
        /// Stereo (and above) channels can be formed by joining multiple channels together using <see cref="ChannelJoin" />.
        /// </para>
		/// <para>Use <see cref="Start" /> to begin processing the enabled channels.</para>
		/// <para>
        /// You might also use this function on an already enabled ASIO channel if you just want to change the <see cref="AsioProcedure" /> which should be used.
		/// However changing the callback procedure to <see langword="null" /> would disable the channel - which is only possible, if the ASIO device is stopped.
        /// </para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Start">The device has been started - it needs to be stopped before (dis)enabling channels.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Input" /> and <paramref name="Channel" /> combination is invalid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ASIO_ChannelEnable")]
        public static extern bool ChannelEnable(bool Input, int Channel, AsioProcedure Procedure, IntPtr User = default(IntPtr));

        /// <summary>
        /// Enables a channel, and sets it to use a BASS channel.
        /// </summary>
        /// <param name="Input">Dealing with an input channel? <see langword="false" /> = an output channel.</param>
        /// <param name="Channel">The input/output channel number... 0 = first.</param>
        /// <param name="Handle">The BASS channel handle.</param>
        /// <param name="Join">Join the next ASIO channels according to the number of audio channels in the BASS channel?</param>
        /// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// This function allows BASS channels to be used directly, without needing an <see cref="AsioProcedure"/> callback function. The ASIO channel's format and rate are set accordingly.
        /// If the BASS channel is not mono then multiple ASIO channels should also be joined accordingly. That can be done automatically via the join parameter, or manually
        /// with <see cref="ChannelJoin"/>. If the device does not have enough channels, the BASSmix add-on can be used to downmix the BASS channel.
        /// </para>
        /// <para>
        /// In the case of output channels, the BASS channel must have the <see cref="BassFlags.Decode"/> flag set. In the case of input channels, the BASS channel must be a "push" stream,
        /// created with <see cref="Bass.CreateStream(int,int,BassFlags,StreamProcedureType)"/> and <see cref="StreamProcedureType.Push"/>, which will receive the data from the input channel(s).
        /// </para>
        /// <para>
        /// Raw DSD streams are supported (with the BASSDSD add-on) but the device needs to have been successfully set to DSD mode first with <see cref="SetDSD"/>.
        /// The device's sample rate should also be set to the DSD stream's rate (its BASS_ATTRIB_DSD_RATE attribute) via <see cref="Rate"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Start">The device has been started - it needs to be stopped before (dis)enabling channels.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Input" /> and <paramref name="Channel" /> combination is invalid.</exception>
        /// <exception cref="Errors.Handle">Handle is invalid</exception>
        /// <exception cref="Errors.SampleFormat">8-bit BASS channels are not supported; the <see cref="BassFlags.Float"/> flag can be used to avoid them.</exception>
        /// <exception cref="Errors.NoChannel">The device does not have enough channels to accommodate the BASS channel.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ASIO_ChannelEnableBASS")]
        public static extern bool ChannelEnableBass(bool Input, int Channel, int Handle, bool Join);

		/// <summary>
		/// Enables an output channel, and makes it mirror another channel.
		/// </summary>
		/// <param name="Channel">The output channel number... 0 = first.</param>
		/// <param name="Input2">Mirroring an input channel? <see langword="false" /> = an output channel.</param>
		/// <param name="Channel2">The channel to mirror.</param>
		/// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// This function allows an input or output channel to be duplicated in other output channel.
        /// This can be achieved using normal <see cref="AsioProcedure" /> processing, but it's more efficient to let BassAsio simply copy the data from one channel to another.
        /// </para>
		/// <para>
        /// Mirror channels can't be joined together to form multi-channel mirrors.
        /// Instead, to mirror multiple channels, an individual mirror should be setup for each of them.
        /// </para>
		/// <para>After <see cref="Start" /> has been called to begin processing, it's not possible to setup new mirror channels, but it is still possible to change the channel that a mirror is mirroring.</para>
		/// <para>
        /// When mirroring an output channel that hasn't been enabled, the mirror channel will just produce silence.
        /// When mirroring an input channel that hasn't already been enabled, the channel is automatically enabled for processing when <see cref="Start" /> is called, so that it can be mirrored.
		/// If the mirror is switched to a disabled input channel once processing has begun, then it will produce silence.
        /// </para>
		/// <para>
        /// A mirror channel can be made to have a different volume level to the channel that it's mirroring, using <see cref="ChannelSetVolume" />.
        /// The volume setting is cumulative.
		/// For example, if the mirror channel has a volume setting of 0.5 and the mirrored channel has a volume setting of 0.4, the effective volume of the mirror channel will be 0.2 (0.5 x 0.4).
        /// </para>
		/// <para><see cref="ChannelEnable" /> can be used to disable a mirror channel.</para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Start">The device has been started - it needs to be stopped before enabling channels.</exception>
        /// <exception cref="Errors.Parameter">At least one of the channels is invalid.</exception>
        /// <exception cref="Errors.SampleFormat">It is not possible to mirror channels that do not have the same sample format.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ASIO_ChannelEnableMirror")]
        public static extern bool ChannelEnableMirror(int Channel, bool Input2, int Channel2);
        
		/// <summary>
		/// Retrieves a channel's sample format.
		/// </summary>
		/// <param name="Input">Dealing with an input channel? <see langword="false" /> = an output channel.</param>
		/// <param name="Channel">The input/output channel number... 0 = first.</param>
		/// <returns>If an error occurs, -1 (<see cref="AsioSampleFormat.Unknown"/>) is returned, use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Input" /> and <paramref name="Channel" /> combination is invalid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ASIO_ChannelGetFormat")]
        public static extern AsioSampleFormat ChannelGetFormat(bool Input, int Channel);
        
        #region ChannelGetInfo
		/// <summary>
		/// Retrieves information on an Asio channel.
		/// </summary>
		/// <param name="Input">Dealing with an input channel? <see langword="false" /> = an output channel.</param>
		/// <param name="Channel">The input/output channel number... 0 = first.</param>
		/// <param name="Info">An instance of the <see cref="AsioChannelInfo" /> structure to store the information at.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Input" /> and <paramref name="Channel" /> combination is invalid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ASIO_ChannelGetInfo")]
        public static extern bool ChannelGetInfo(bool Input, int Channel, out AsioChannelInfo Info);
                
		/// <summary>
		/// Retrieves information on an Asio channel.
		/// </summary>
		/// <param name="Input">Dealing with an input channel? <see langword="false" /> = an output channel.</param>
		/// <param name="Channel">The input/output channel number... 0 = first.</param>
		/// <returns>An instance of the <see cref="AsioChannelInfo" /> structure is returned. Throws <see cref="BassException"/> on Error.</returns>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Input" /> and <paramref name="Channel" /> combination is invalid.</exception>
        public static AsioChannelInfo ChannelGetInfo(bool Input, int Channel)
        {
            if (!ChannelGetInfo(Input, Channel, out var info))
                throw new BassException(LastError);
            return info;
        }
        #endregion

        #region ChannelGetLevel
        [DllImport(DllName)]
        static extern float BASS_ASIO_ChannelGetLevel(bool input, int channel);
        
		/// <summary>
		/// Retrieves the level (peak amplitude) of a channel.
		/// </summary>
		/// <param name="Input">Dealing with an input channel? <see langword="false" /> = an output channel.</param>
		/// <param name="Channel">The input/output channel number... 0 = first. The <see cref="AsioChannelGetLevelFlags.Rms"/> flag can optionally be used to get the RMS level, otherwise the peak level is given.</param>
		/// <returns>
        /// If an error occurs, -1 is returned, use <see cref="LastError" /> to get the error code. 
		/// If successful, the level of the channel is returned, ranging from 0 (silent) to 1 (max).
        /// If the channel's native sample format is floating-point, it is actually possible for the level to go above 1.
        /// </returns>
		/// <remarks>
        /// This function measures the level of a single channel, and is not affected by any other channels that are joined with it.
		/// <para>Volume settings made via <see cref="ChannelSetVolume" /> affect the level reading of output channels, but not input channels.</para>
		/// <para>
        /// When an input channel is paused, it is still possible to get its level.
        /// Paused output channels will have a level of 0.
        /// </para>
		/// <para>Level retrieval is not supported when the sample format is DSD.</para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Input" /> and <paramref name="Channel" /> combination is invalid.</exception>
        /// <exception cref="Errors.Start">The device hasn't been started, or the channel isn't enabled.</exception>
        /// <exception cref="Errors.SampleFormat">Level retrieval is not supported for the channel's sample format (please report).</exception>
        public static double ChannelGetLevel(bool Input, int Channel) => BASS_ASIO_ChannelGetLevel(Input, Channel);
        #endregion
        
		/// <summary>
		/// Retrieves a channel's sample rate.
		/// </summary>
		/// <param name="Input">Dealing with an input channel? <see langword="false" /> = an output channel.</param>
		/// <param name="Channel">The input/output channel number... 0 = first.</param>
		/// <returns>If succesful, the channel's sample rate is returned (0 = device rate), else -1 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Input" /> and <paramref name="Channel" /> combination is invalid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ASIO_ChannelGetRate")]
        public static extern double ChannelGetRate(bool Input, int Channel);

        #region ChannelGetVolume
        [DllImport(DllName)]
        static extern float BASS_ASIO_ChannelGetVolume(bool input, int channel);
        
		/// <summary>
		/// Retrieves a channel's volume setting.
		/// </summary>
		/// <param name="Input">Dealing with an input channel? <see langword="false" /> = an output channel.</param>
		/// <param name="Channel">The input/output channel number... 0 = first, -1 = master.</param>
		/// <returns>If successful, the channel's volume setting is returned, else -1 is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>To set a channel volume use <see cref="ChannelSetVolume" />.</remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Input" /> and <paramref name="Channel" /> combination is invalid.</exception>
        public static double ChannelGetVolume(bool Input, int Channel) => BASS_ASIO_ChannelGetVolume(Input, Channel);
        #endregion
        
		/// <summary>
		/// Checks if a channel is enabled for processing.
		/// </summary>
		/// <param name="Input">Dealing with an input channel? <see langword="false" /> = an output channel.</param>
		/// <param name="Channel">The input/output channel number... 0 = first.</param>
		/// <returns>One <see cref="AsioChannelActive"/> value is returned.</returns>
		/// <remarks>
		/// When a channel is joined to another, the status of the other channel is returned, as that is what determines whether the channel is enabled for processing - whether it's been enabled itself is of no consequence while it is joined to another. 
		/// For example, if channel B is joined to channel A, and channel A is not enabled, then neither is channel B.
		/// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ASIO_ChannelIsActive")]
        public static extern AsioChannelActive ChannelIsActive(bool Input, int Channel);
        
		/// <summary>
		/// Join a channel to another.
		/// </summary>
		/// <param name="Input">Dealing with input channels? <see langword="false" /> = output channels.</param>
		/// <param name="Channel">The input/output channel number... 0 = first.</param>
		/// <param name="Channel2">The channel to join it to... -1 = remove current join.</param>
		/// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// All ASIO channels are mono.
        /// By joining them, stereo (and above) channels can be formed, making it simpler to process stereo (and above) sample data.
        /// </para>
		/// <para>
        /// By default, channels can only be joined to preceding channels.
        /// For example, channel 1 can be joined to channel 0, but not vice versa.
		/// The <see cref="AsioInitFlags.JoinOrder"/> flag can be used in the <see cref="Init" /> call to remove that restriction.
        /// When joining a group of channels, there should be one channel enabled via <see cref="ChannelEnable" /> with the rest joined to it - 
		/// do not join a channel to a channel that is itself joined to another channel.
        /// Mirror channels, setup using <see cref="ChannelEnableMirror" />, cannot be joined with.
        /// </para>
		/// <para>
        /// If a channel has two or more other channels joined to it, then the joined channels will default to being in numerically ascending order in the <see cref="AsioProcedure" /> callback function's sample data unless the <see cref="AsioInitFlags.JoinOrder"/> flag was used in the <see cref="Init" /> call, 
		/// in which case they will be in the order in which they were joined via this function.
        /// In the latter case, if this function is called on an already joined channel, the channel will be moved to the end of the joined group.
        /// </para>
		/// <para>
        /// While a channel is joined to another, it automatically takes on the attributes of the other channel - the other channel's settings determine the sample format, the sample rate and whether it is enabled.
		/// The volume setting remains individual though, allowing balance control over the joined channels.
        /// </para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Start">The device has been started - it needs to be stopped before (dis)enabling channels.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Input" /> and <paramref name="Channel" /> combination is invalid.</exception>
        /// <exception cref="Errors.SampleFormat">It is not possible to join channels that do not have the same sample format.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ASIO_ChannelJoin")]
        public static extern bool ChannelJoin(bool Input, int Channel, int Channel2);
        
		/// <summary>
		/// Suspends processing of a channel.
		/// </summary>
		/// <param name="Input">Dealing with an input channel? <see langword="false" /> = an output channel.</param>
		/// <param name="Channel">The input/output channel number... 0 = first.</param>
		/// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// Channels can only be disabled when the device is stopped.
        /// When you want to stop processing only some of the enabled channels, there are few ways that could be done.
        /// You could quickly stop the device, disable the unwanted channels, and restart the device. 
		/// In the case of output channels, you could fill the channels' buffers with silence (0s) in the <see cref="AsioProcedure" />.
        /// Or you could pause the channels, using this function. 
		/// The less channels BassAsio has to process, the less CPU it'll use, so stopping and restarting the device would be the most efficient, but that could cause a slight break in the sound of the other channels. 
		/// Filling the buffers with silence is the least efficient, as BassAsio will still process the data as if it was "normal", but it does mean that other channels are unaffected.
		/// Pausing is a compromise between the two - the channels will still be enabled, but BassAsio will bypass any additional processing (resampling/etc) that may normally be required.</para>
		/// <para>Use <see cref="ChannelReset" /> to resume processing of a paused channel.</para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Input" /> and <paramref name="Channel" /> combination is invalid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ASIO_ChannelPause")]
        public static extern bool ChannelPause(bool Input, int Channel);
                
		/// <summary>
		/// Resets the attributes of a channel (or all channels).
		/// </summary>
		/// <param name="Input">Dealing with an input channel? <see langword="false" /> = an output channel.</param>
		/// <param name="Channel">The input/output channel number... 0 = first, -1 = all channels.</param>
		/// <param name="Flags">The attributes to reset. A combination of <see cref="AsioChannelResetFlags"/>.</param>
		/// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>When resetting all channels (channel = -1), the resetting only applies to all channels of the specified type, ie. input or output, not both.</remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Start">The device has been started - it needs to be stopped before disabling or unjoining channels.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Input" /> and <paramref name="Channel" /> combination is invalid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ASIO_ChannelReset")]
        public static extern bool ChannelReset(bool Input, int Channel, AsioChannelResetFlags Flags);
        
		/// <summary>
		/// Sets a channel's sample format.
		/// </summary>
		/// <param name="Input">Dealing with an input channel? <see langword="false" /> = an output channel.</param>
		/// <param name="Channel">The input/output channel number... 0 = first.</param>
		/// <param name="Format">The sample format.</param>
		/// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// The sample format can vary between ASIO devices/drivers, which could mean a lot of extra/duplicate code being required.
        /// To avoid that extra work, BassAsio can automatically convert the sample data, whenever necessary, to/from a format of your choice. 
		/// The native format of a channel can be retrieved via <see cref="ChannelGetInfo(bool, int, out AsioChannelInfo)" />.
        /// </para>
		/// <para>
        /// The PCM format options are only available when the device's format is PCM, and the DSD format options are only available when the device's format is DSD. 
		/// If a device supports both, it can be switched between DSD and PCM via <see cref="SetDSD" />.
        /// </para>
		/// <para>For performance reasons, it's best not to use 24-bit sample data whenever possible, as 24-bit data requires a bit more processing than the other formats.</para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Input" /> and <paramref name="Channel" /> combination, or <paramref name="Format" /> is invalid.</exception>
        /// <exception cref="Errors.SampleFormat">Format conversion is not available for the channel's native sample format (please report).</exception>
        [DllImport(DllName, EntryPoint = "BASS_ASIO_ChannelSetFormat")]
        public static extern bool ChannelSetFormat(bool Input, int Channel, AsioSampleFormat Format);
        
		/// <summary>
		/// Sets a channel's sample rate.
		/// </summary>
		/// <param name="Input">Dealing with an input channel? <see langword="false" /> = an output channel.</param>
		/// <param name="Channel">The input/output channel number... 0 = first.</param>
		/// <param name="Rate">The sample rate... 0 = device rate.</param>
		/// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// For optimal quality and performance, it is best to set the device to the sample rate you want via <see cref="Rate" />, but that's not always possible. 
		/// Which is where this function and resampling comes into play.
        /// 16 point sinc interpolation is used, giving a good blend of sound quality and performance.
        /// It is also SSE2 and 3DNow optimized for an extra boost with supporting CPUs.
        /// </para>
		/// <para>When a channel's sample rate is the same as the device rate, resampling is bypassed, so there's no unnecessary performance hit.</para>
		/// <para>Resampling is not supported when the sample format is DSD.</para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Input" /> and <paramref name="Channel" /> combination is invalid, or <paramref name="Rate" /> is below 0.</exception>
        /// <exception cref="Errors.SampleFormat">Format conversion is not available for the channel's native sample format (please report).</exception>
        [DllImport(DllName, EntryPoint = "BASS_ASIO_ChannelSetRate")]
        public static extern bool ChannelSetRate(bool Input, int Channel, double Rate);

        #region ChannelSetVolume
        [DllImport(DllName)]
        static extern bool BASS_ASIO_ChannelSetVolume(bool input, int channel, float volume);
        
		/// <summary>
		/// Sets a channel's volume.
		/// </summary>
		/// <param name="Input">Dealing with an input channel? <see langword="false" /> = an output channel.</param>
		/// <param name="Channel">The input/output channel number... 0 = first, -1 = master.</param>
		/// <param name="Volume">The volume level... 0 (silent)...1.0 (normal). Above 1.0 amplifies the sound.</param>
		/// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para></para>
		/// <para>
        /// Apart from the master volume (channel = -1), this function applies a volume level to a single channel, and does not affect any other channels that are joined with it. 
		/// This allows balance control over joined channels, by setting the individual volume levels accordingly.
        /// The final level of a channel is = master volume * channel volume.
        /// </para>
		/// <para>The volume "curve" is linear, but logarithmic levels can be easily used. See the example below.</para>
		/// <para>
        /// ASIO drivers do not provide volume control themselves, so the volume adjustments are applied to the sample data by BassAsio. 
		/// This also means that changes do not persist across sessions, and the channel volume levels will always start at 1.0.
        /// </para>
		/// <para>When the channel's sample format is DSD, a 0 volume setting will mute the channel and anything else will be treated as 1.0 (normal).</para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Input" /> and <paramref name="Channel" /> combination is invalid, or <paramref name="Volume" /> is below 0.</exception>
        public static bool ChannelSetVolume(bool Input, int Channel, double Volume) => BASS_ASIO_ChannelSetVolume(Input, Channel, (float)Volume);
        #endregion
    }
}