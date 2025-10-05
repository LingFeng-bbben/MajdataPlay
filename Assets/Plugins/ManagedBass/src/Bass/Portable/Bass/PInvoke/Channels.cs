using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    public static partial class Bass
    {
        #region ChannelGetInfo
        /// <summary>
        /// Retrieves information on a channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD.</param>
        /// <param name="Info"><see cref="ChannelInfo" /> instance where to store the channel information at.</param>
        /// <returns>
        /// If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError"/> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetInfo")]
        public static extern bool ChannelGetInfo(int Handle, out ChannelInfo Info);

        /// <summary>
        /// Retrieves information on a channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD.</param>
        /// <returns>An instance of the <see cref="ChannelInfo" /> structure. Throws <see cref="BassException"/> on Error.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        public static ChannelInfo ChannelGetInfo(int Handle)
        {
            if (!ChannelGetInfo(Handle, out var info))
                throw new BassException();
            return info;
        }
        #endregion

        #region ChannelDSP
        [DllImport(DllName)]
        static extern int BASS_ChannelSetDSP(int Handle, DSPProcedure Procedure, IntPtr User, int Priority);

        /// <summary>
        /// Sets up a User DSP function on a stream, MOD music, or recording channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Procedure">The callback function (see <see cref="DSPProcedure" />).</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <param name="Priority">
        /// The priority of the new DSP, which determines it's position in the DSP chain.
        /// DSPs with higher priority are called before those with lower.
        /// </param>
        /// <returns>
        /// If succesful, then the new DSP's Handle is returned, else 0 is returned.
        /// Use <see cref="LastError"/> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <remarks>
        /// <para>The channel does not have to be playing to set a DSP function, they can be set before and while playing.</para>
        /// <para>
        /// Equally, you can also remove them at any time.
        /// Use <see cref="ChannelRemoveDSP"/> to remove a DSP function.
        /// </para>
        /// <para>
        /// Multiple DSP functions may be used per channel, in which case the order that the functions are called is determined by their priorities.
        /// Any DSPs that have the same priority are called in the order that they were added.
        /// </para>
        /// <para>
        /// DSP functions can be applied to MOD musics and streams, but not samples.
        /// If you want to apply a DSP function to a sample, then you should stream the sample.
        /// </para>
        /// <para>
        /// Unlike Bass.Net, a reference to <paramref name="Procedure"/> doesn't need to be held by you manually.
        /// ManagedBass automatically holds a reference and frees it when the Channel is freed or DSP is removed via <see cref="ChannelRemoveDSP"/>.
        /// </para>
        /// </remarks>
        public static int ChannelSetDSP(int Handle, DSPProcedure Procedure, IntPtr User = default(IntPtr), int Priority = 0)
        {
            var h = BASS_ChannelSetDSP(Handle, Procedure, User, Priority);

            if (h != 0)
                ChannelReferences.Add(Handle, h, Procedure);

            return h;
        }

        [DllImport(DllName)]
        static extern bool BASS_ChannelRemoveDSP(int Handle, int DSP);

        /// <summary>
        /// Removes a DSP function from a stream, MOD music, or recording channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="DSP">Handle of the DSP function to remove from the channel (return value of a previous <see cref="ChannelSetDSP" /> call).</param>
        /// <returns>If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle">At least one of <paramref name="Handle" /> and <paramref name="DSP" /> is not valid.</exception>
        public static bool ChannelRemoveDSP(int Handle, int DSP)
        {
            var b = BASS_ChannelRemoveDSP(Handle, DSP);

            if (b)
                ChannelReferences.Remove(Handle, DSP);

            return b;
        }
        #endregion

        #region ChannelSync
        [DllImport(DllName)]
        static extern int BASS_ChannelSetSync(int Handle, SyncFlags Type, long Parameter, SyncProcedure Procedure, IntPtr User);

        /// <summary>
        /// Sets up a synchronizer on a MOD music, stream or recording channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HMUSIC, HSTREAM or HRECORD.</param>
        /// <param name="Type">The Type of sync (see <see cref="SyncFlags" />).</param>
        /// <param name="Parameter">The sync parameters, depends on the sync Type (see <see cref="SyncFlags"/>).</param>
        /// <param name="Procedure">The callback function which should be invoked with the sync.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>
        /// If succesful, then the new synchronizer's Handle is returned, else 0 is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Type">An illegal <paramref name="Type" /> was specified.</exception>
        /// <exception cref="Errors.Parameter">An illegal <paramref name="Parameter" /> was specified.</exception>
        /// <remarks>
        /// <para>
        /// Multiple synchronizers may be used per channel, and they can be set before and while playing.
        /// Equally, synchronizers can also be removed at any time, using <see cref="ChannelRemoveSync" />.
        /// If the <see cref="SyncFlags.Onetime"/> flag is used, then the sync is automatically removed after its first occurrence.
        /// </para>
        /// <para>The <see cref="SyncFlags.Mixtime"/> flag can be used with <see cref="SyncFlags.End"/> or <see cref="SyncFlags.Position"/>/<see cref="SyncFlags.MusicPosition"/> syncs to implement custom looping, by using <see cref="ChannelSetPosition" /> in the callback.
        /// A <see cref="SyncFlags.Mixtime"/> sync can also be used to add or remove DSP/FX at specific points, or change a HMUSIC channel's flags or attributes (see <see cref="ChannelFlags" />).
        /// The <see cref="SyncFlags.Mixtime"/> flag can also be useful with a <see cref="SyncFlags.Seeking"/> sync, to reset DSP states after seeking.</para>
        /// <para>
        /// Several of the sync types are triggered in the process of rendering the channel's sample data;
        /// for example, <see cref="SyncFlags.Position"/> and <see cref="SyncFlags.End"/> syncs, when the rendering reaches the sync position or the end, respectively.
        /// Those sync types should be set before starting playback or pre-buffering (ie. before any rendering), to avoid missing any early sync events.
        /// </para>
        /// <para>With recording channels, <see cref="SyncFlags.Position"/> syncs are triggered just before the <see cref="RecordProcedure" /> receives the block of data containing the sync position.</para>
        /// <para>
        /// Unlike Bass.Net, a reference to <paramref name="Procedure"/> doesn't need to be held by you manually.
        /// ManagedBass automatically holds a reference and frees it when the Channel is freed or Sync is removed via <see cref="ChannelRemoveSync"/>.
        /// </para>
        /// </remarks>
        public static int ChannelSetSync(int Handle, SyncFlags Type, long Parameter, SyncProcedure Procedure, IntPtr User = default(IntPtr))
        {
            // Define a dummy SyncProcedure for OneTime syncs.
            var proc = Type.HasFlag(SyncFlags.Onetime)
                ? ((I, Channel, Data, Ptr) =>
                {
                    Procedure(I, Channel, Data, Ptr);
                    ChannelReferences.Remove(Channel, I);
                }) : Procedure;

            var h = BASS_ChannelSetSync(Handle, Type, Parameter, proc, User);

            if (h != 0)
                ChannelReferences.Add(Handle, h, proc);

            return h;
        }

        [DllImport(DllName)]
        static extern bool BASS_ChannelRemoveSync(int Handle, int Sync);

        /// <summary>
        /// Removes a synchronizer from a MOD music or stream channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HMUSIC, HSTREAM or HRECORD.</param>
        /// <param name="Sync">Handle of the synchronizer to remove (return value of a previous <see cref="ChannelSetSync" /> call).</param>
        /// <returns>
        /// If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        public static bool ChannelRemoveSync(int Handle, int Sync)
        {
            var b = BASS_ChannelRemoveSync(Handle, Sync);

            if (b)
                ChannelReferences.Remove(Handle, Sync);

            return b;
        }
        #endregion

        /// <summary>
        /// Starts (or resumes) playback of a sample, stream, MOD music, or recording.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL / HMUSIC / HSTREAM / HRECORD Handle.</param>
        /// <param name="Restart">
        /// Restart playback from the beginning? If Handle is a User stream, it's current Buffer contents are flushed.
        /// If it's a MOD music, it's BPM/etc are automatically reset to their initial values.
        /// </param>
        /// <returns>
        /// If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError"/> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Start">The output is paused/stopped, use <see cref="Start" /> to start it.</exception>
        /// <exception cref="Errors.Decode">The channel is not playable, it's a "decoding channel".</exception>
        /// <exception cref="Errors.BufferLost">Should not happen... check that a valid window Handle was used with <see cref="Init"/>.</exception>
        /// <exception cref="Errors.NoHW">
        /// No hardware voices are available (HCHANNEL only).
        /// This only occurs if the sample was loaded/created with the <see cref="BassFlags.VAM"/> flag,
        /// and <see cref="VAMMode.Hardware"/> is set in the sample's VAM mode,
        /// and there are no hardware voices available to play it.
        /// </exception>
        /// <remarks>
        /// When streaming in blocks (<see cref="BassFlags.StreamDownloadBlocks"/>), the restart parameter is ignored as it's not possible to go back to the start.
        /// The <paramref name="Restart" /> parameter is also of no consequence with recording channels.
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelPlay")]
        public static extern bool ChannelPlay(int Handle, bool Restart = false);

        /// <summary>
        /// Pauses a sample, stream, MOD music, or recording.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL / HMUSIC / HSTREAM / HRECORD Handle.</param>
        /// <returns>
        /// If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.NotPlaying">The channel is not playing (or <paramref name="Handle" /> is not a valid channel).</exception>
        /// <exception cref="Errors.Decode">The channel is not playable, it's a "decoding channel".</exception>
        /// <exception cref="Errors.Already">The channel is already paused.</exception>
        /// <remarks>
        /// Use <see cref="ChannelPlay" /> to resume a paused channel.
        /// <see cref="ChannelStop" /> can be used to stop a paused channel.
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelPause")]
        public static extern bool ChannelPause(int Handle);

        /// <summary>
        /// Stops a sample, stream, MOD music, or recording.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM or HRECORD Handle.</param>
        /// <returns>
        /// If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <remarks>
        /// <para>
        /// Stopping a User stream (created with <see cref="CreateStream(int,int,BassFlags,StreamProcedure,IntPtr)" />) will clear its Buffer contents,
        /// and stopping a sample channel (HCHANNEL) will result in it being freed.
        /// Use <see cref="ChannelPause" /> instead if you wish to stop a User stream and then resume it from the same point.
        /// </para>
        /// <para>
        /// When used with a "decoding channel" (<see cref="BassFlags.Decode"/> was used at creation),
        /// this function will end the channel at its current position, so that it's not possible to decode any more data from it.
        /// Any <see cref="SyncFlags.End"/> syncs that have been set on the channel will not be triggered by this, they are only triggered when reaching the natural end.
        /// <see cref="ChannelSetPosition" /> can be used to reset the channel and start decoding again.
        /// </para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelStop")]
        public static extern bool ChannelStop(int Handle);

        /// <summary>
        /// Locks a stream, MOD music or recording channel to the current thread.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HMUSIC, HSTREAM or HRECORD Handle.</param>
        /// <param name="Lock">If <see langword="false" />, unlock the channel, else lock it.</param>
        /// <returns>
        /// If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <remarks>
        /// Locking a channel prevents other threads from performing most functions on it, including Buffer updates.
        /// Other threads wanting to access a locked channel will block until it is unlocked, so a channel should only be locked very briefly.
        /// A channel must be unlocked in the same thread that it was locked.
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelLock")]
        public static extern bool ChannelLock(int Handle, bool Lock = true);

        /// <summary>
        /// Checks if a sample, stream, or MOD music is active (playing) or stalled. Can also check if a recording is in progress.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD.</param>
        /// <returns><see cref="PlaybackState" /> indicating whether the state of the channel.
        /// </returns>
        /// <remarks>
        /// <para>
        /// When using this function with a decoding channel, <see cref="PlaybackState.Playing"/> will be returned while there is still data to decode.
        /// Once the end has been reached, <see cref="PlaybackState.Stopped"/> will be returned.
        /// <see cref="PlaybackState.Stalled"/> is never returned for decoding channels;
        /// you can tell a decoding channel is stalled if <see cref="ChannelGetData(int,IntPtr,int)" /> returns less data than requested,
        /// and this function still returns <see cref="PlaybackState.Playing"/>.
        /// </para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelIsActive")]
        public static extern PlaybackState ChannelIsActive(int Handle);

        /// <summary>
        /// Links two MOD music or stream channels together.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HMUSIC or HSTREAM.</param>
        /// <param name="Channel">The Handle of the channel to have linked with it... a HMUSIC or HSTREAM.</param>
        /// <returns>
        /// If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle">At least one of <paramref name="Handle" /> and <paramref name="Channel" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Decode">At least one of <paramref name="Handle" /> and <paramref name="Channel" /> is a "decoding channel", so can't be linked.</exception>
        /// <exception cref="Errors.Already"><paramref name="Channel" /> is already linked to <paramref name="Handle" />.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        /// <remarks>
        /// <para>
        /// Linked channels are started/stopped/paused/resumed together.
        /// Links are one-way, for example, channel <paramref name="Channel" /> will be started by channel <paramref name="Handle" />,
        /// but not vice versa unless another link has been set in that direction.
        /// </para>
        /// <para>
        /// If a linked channel has reached the end, it will not be restarted when a channel it is linked to is started.
        /// If you want a linked channel to be restarted, you need to have resetted it's position using <see cref="ChannelSetPosition" /> beforehand.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// Except for on Windows, linked channels on the same device are guaranteed to start playing simultaneously.
        /// On Windows, it is possible for there to be a slight gap between them, but it will generally be shorter (and never longer) than starting them individually.
        /// </para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelSetLink")]
        public static extern bool ChannelSetLink(int Handle, int Channel);

        /// <summary>
        /// Removes a links between two MOD music or stream channels.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HMUSIC or HSTREAM.</param>
        /// <param name="Channel">The Handle of the channel to have unlinked with it... a HMUSIC or HSTREAM.</param>
        /// <returns>
        /// If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Already">Either <paramref name="Channel" /> is not a valid channel, or it is already not linked to <paramref name="Handle" />.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelRemoveLink")]
        public static extern bool ChannelRemoveLink(int Handle, int Channel);

        #region Channel Flags
        /// <summary>
        /// Modifies and retrieves a channel's flags.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM.</param>
        /// <param name="Flags">
        /// A combination of flags that can be toggled (see <see cref="BassFlags" />).
        /// Speaker assignment flags can also be toggled (HSTREAM/HMUSIC).
        /// </param>
        /// <param name="Mask">
        /// The flags (as above) to modify. Flags that are not included in this are left as they are, so it can be set to 0 in order to just retrieve the current flags.
        /// To modify the speaker flags, any of the Speaker flags can be used in the mask (no need to include all of them).
        /// </param>
        /// <returns>
        /// If successful, the channel's updated flags are returned, else -1 is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <remarks>
        /// <para>
        /// Some flags may not be adjustable in some circumstances, so the return value should be checked to confirm any changes.
        /// The flags listed above are just the flags that can be modified, and there may be additional flags present in the return value.
        /// See the <see cref="ChannelInfo" /> documentation for a full list of flags.
        /// </para>
        /// <para>
        /// Streams that are created by add-ons may have additional flags available.
        /// There is a limited number of possible flag values though, so some add-ons may use the same flag value for different things.
        /// This means that when using add-on specific flags with a stream created via the plugin system,
        /// it is a good idea to first confirm that the add-on is handling the stream, by checking its ctype via <see cref="ChannelGetInfo(int,out ChannelInfo)" />.
        /// </para>
        /// <para>
        /// During playback, the effects of flag changes are not heard instantaneously, due to buffering.
        /// To reduce the delay, use the <see cref="PlaybackBufferLength" /> config option to reduce the Buffer Length.
        /// </para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelFlags")]
        public static extern BassFlags ChannelFlags(int Handle, BassFlags Flags, BassFlags Mask);

        /// <summary>
        /// Checks if a flag is present on a channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM.</param>
        /// <param name="Flag">see <see cref="BassFlags" /></param>
        public static bool ChannelHasFlag(int Handle, BassFlags Flag) => ChannelFlags(Handle, 0, 0).HasFlag(Flag);

        /// <summary>
        /// Adds a flag to a channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM.</param>
        /// <param name="Flag">see <see cref="BassFlags" /></param>
        public static bool ChannelAddFlag(int Handle, BassFlags Flag) => ChannelFlags(Handle, Flag, Flag).HasFlag(Flag);

        /// <summary>
        /// Removes a flag from a channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM.</param>
        /// <param name="Flag">see <see cref="BassFlags" /></param>
        public static bool ChannelRemoveFlag(int Handle, BassFlags Flag) => !ChannelFlags(Handle, 0, Flag).HasFlag(Flag);
        #endregion

        #region Channel Attributes
        /// <summary>
        /// Retrieves the value of an attribute of a sample, stream or MOD music.
        /// Can also get the sample rate of a recording channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM or HRECORD.</param>
        /// <param name="Attribute">The attribute to set the value of (one of <see cref="ChannelAttribute" />)</param>
        /// <param name="Value">Reference to a float to receive the attribute value.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Type"><paramref name="Attribute" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetAttribute")]
        public static extern bool ChannelGetAttribute(int Handle, ChannelAttribute Attribute, out float Value);

        /// <summary>
        /// Retrieves the value of an attribute of a sample, stream or MOD music.
        /// Can also get the sample rate of a recording channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM or HRECORD.</param>
        /// <param name="Attribute">The attribute to set the value of (one of <see cref="ChannelAttribute" />)</param>
        /// <returns>If successful, the attribute value is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Type"><paramref name="Attribute" /> is not valid.</exception>
        public static double ChannelGetAttribute(int Handle, ChannelAttribute Attribute)
        {
            ChannelGetAttribute(Handle, Attribute, out float temp);
            return temp;
        }

        /// <summary>
        /// Retrieves the value of a channel's attribute.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM  or HRECORD.</param>
        /// <param name="Attribute">The attribute to get the value of (e.g. <see cref="ChannelAttribute.ScannedInfo"/>)</param>
        /// <param name="Value">Pointer to a buffer to receive the attribute data.</param>
        /// <param name="Size">The size of the attribute data... 0 = get the size of the attribute without getting the data.</param>
        /// <returns>If successful, the size of the attribute data is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>This function also supports the floating-point attributes supported by <see cref="ChannelGetAttribute(int, ChannelAttribute, out float)" />.</remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.NotAvailable">The <paramref name="Attribute" /> is not available.</exception>
        /// <exception cref="Errors.Type"><paramref name="Attribute" /> is not valid.</exception>
        /// <exception cref="Errors.Parameter">The <paramref name="Value" /> content or <paramref name="Size" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetAttributeEx")]
        public static extern int ChannelGetAttribute(int Handle, ChannelAttribute Attribute, IntPtr Value, int Size);

        /// <summary>
        /// Sets the value of an attribute of a sample, stream or MOD music.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM  or HRECORD.</param>
        /// <param name="Attribute">The attribute to set the value of.</param>
        /// <param name="Value">The new attribute value. See the attribute's documentation for details on the possible values.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The actual attribute value may not be exactly the same as requested, due to precision differences.
        /// For example, an attribute might only allow whole number values.
        /// <see cref="ChannelGetAttribute(int, ChannelAttribute, out float)" /> can be used to confirm what the value is.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Type"><paramref name="Attribute" /> is not valid.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Value" /> is not valid. See the attribute's documentation for the valid range of values.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelSetAttribute")]
        public static extern bool ChannelSetAttribute(int Handle, ChannelAttribute Attribute, float Value);

        /// <summary>
        /// Sets the value of an attribute of a sample, stream or MOD music.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM  or HRECORD.</param>
        /// <param name="Attribute">The attribute to set the value of.</param>
        /// <param name="Value">The new attribute value. See the attribute's documentation for details on the possible values.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The actual attribute value may not be exactly the same as requested, due to precision differences.
        /// For example, an attribute might only allow whole number values.
        /// <see cref="ChannelGetAttribute(int, ChannelAttribute)" /> can be used to confirm what the value is.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Type"><paramref name="Attribute" /> is not valid.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Value" /> is not valid. See the attribute's documentation for the valid range of values.</exception>
        public static bool ChannelSetAttribute(int Handle, ChannelAttribute Attribute, double Value)
        {
            return ChannelSetAttribute(Handle, Attribute, (float)Value);
        }

        /// <summary>
        /// Sets the value of a channel's attribute.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM  or HRECORD.</param>
        /// <param name="Attribute">The attribute to set the value of. (e.g. <see cref="ChannelAttribute.ScannedInfo"/>)</param>
        /// <param name="Value">The pointer to the new attribute data.</param>
        /// <param name="Size">The size of the attribute data.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Type"><paramref name="Attribute" /> is not valid.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Value" /> is not valid. See the attribute's documentation for the valid range of values.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelSetAttributeEx")]
        public static extern bool ChannelSetAttribute(int Handle, ChannelAttribute Attribute, IntPtr Value, int Size);
        #endregion

        /// <summary>
        /// Retrieves the requested tags/headers from a channel, if they are available.
        /// </summary>
        /// <param name="Handle">The channel handle...a HMUSIC or HSTREAM.</param>
        /// <param name="Tags">The tags/headers wanted...</param>
        /// <returns>If succesful, a pointer to the data of the tags/headers is returned, else <see cref="IntPtr.Zero" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Some tags (eg. <see cref="TagType.ID3"/>) are located at the end of the file, so when streaming a file from the internet, the tags will not be available until the download is complete.
        /// A <see cref="SyncFlags.Downloaded"/> sync can be set via <see cref="ChannelSetSync(int, SyncFlags, long, SyncProcedure, IntPtr)" />, to be informed of when the download is complete.
        /// A <see cref="SyncFlags.MetadataReceived"/> sync can be used to be informed of new Shoutcast metadata, and a <see cref="SyncFlags.OggChange"/> sync for when a new logical bitstream begins in a chained OGG stream, which generally brings new OGG tags.
        /// <para>
        /// In a chained OGG file containing multiple bitstreams, each bitstream will have its own tags.
        /// To get the tags from a particular one, <see cref="ChannelSetPosition(int, long, PositionFlags)" /> can be first used to seek to it.
        /// </para>
        /// <para>When a Media Foundation codec is in use, the <see cref="TagType.WaveFormat"/> tag can be used to find out what the source format is.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The requested tags are not available.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetTags")]
        public static extern IntPtr ChannelGetTags(int Handle, TagType Tags);

        /// <summary>
        /// Retrieves the playback Length of a channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM. HSAMPLE handles may also be used.</param>
        /// <param name="Mode">How to retrieve the Length (one of the <see cref="PositionFlags" /> flags).</param>
        /// <returns>
        /// If succesful, then the channel's Length is returned, else -1 is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.NotAvailable">The Length is not available.</exception>
        /// <remarks>
        /// <para>
        /// The exact Length of a stream will be returned once the whole file has been streamed, but until then it is not always possible to 100% accurately estimate the Length.
        /// The Length is always exact for MP3/MP2/MP1 files when the <see cref="BassFlags.Prescan"/> flag is used in the <see cref="CreateStream(string,long,long,BassFlags)" /> call, otherwise it is an (usually accurate) estimation based on the file size.
        /// The Length returned for OGG files will usually be exact (assuming the file is not corrupt), but when streaming from the internet (or "buffered" User file), it can be a very rough estimation until the whole file has been downloaded.
        /// It will also be an estimate for chained OGG files that are not pre-scanned.
        /// </para>
        /// <para>Unless an OGG file contains a single bitstream, the number of bitstreams it contains will only be available if it was pre-scanned at the stream's creation.</para>
        /// <para>Retrieving the Length of a MOD music requires that the <see cref="BassFlags.Prescan"/> flag was used in the <see cref="MusicLoad(string,long,int,BassFlags,int)" /> call.</para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetLength")]
        public static extern long ChannelGetLength(int Handle, PositionFlags Mode = PositionFlags.Bytes);

        /// <summary>
        /// Translates a byte position into time (seconds), based on a channel's format.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD. HSAMPLE handles may also be used.</param>
        /// <param name="Position">The position in Bytes to translate.</param>
        /// <returns>If successful, then the translated Length in seconds is returned, else a negative value is returned. Use <see cref="LastError"/> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <remarks>The translation is based on the channel's initial sample rate, when it was created.</remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelBytes2Seconds")]
        public static extern double ChannelBytes2Seconds(int Handle, long Position);

        /// <summary>
        /// Translates a time (seconds) position into bytes, based on a channel's format.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD. HSAMPLE handles may also be used.</param>
        /// <param name="Position">The position to translate (in seconds, e.g. 0.03 = 30ms).</param>
        /// <returns>
        /// If successful, then the translated Length in Bytes is returned, else -1 is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <remarks>
        /// <para>The translation is based on the channel's initial sample rate, when it was created.</para>
        /// <para>The return value is rounded down to the position of the nearest sample.</para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelSeconds2Bytes")]
        public static extern long ChannelSeconds2Bytes(int Handle, double Position);

        /// <summary>
        /// Retrieves the playback position of a sample, stream, or MOD music. Can also be used with a recording channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD.</param>
        /// <param name="Mode">How to retrieve the position</param>
        /// <returns>
        /// If an error occurs, -1 is returned, use <see cref="LastError" /> to get the error code.
        /// If successful, the position is returned.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.NotAvailable">The requested position is not available.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        /// <remarks>With MOD music you might use the <see cref="BitHelper.LoWord" /> and <see cref="BitHelper.HiWord" /> methods to retrieve the order and the row values respectively.</remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetPosition")]
        public static extern long ChannelGetPosition(int Handle, PositionFlags Mode = PositionFlags.Bytes);

        /// <summary>
        /// Sets the playback position of a sample, MOD music, or stream.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HSTREAM or HMUSIC.</param>
        /// <param name="Position">The position, in units determined by the <paramref name="Mode" />.</param>
        /// <param name="Mode">How to set the position.</param>
        /// <returns>
        /// If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.NotFile">The stream is not a file stream.</exception>
        /// <exception cref="Errors.Position">The requested position is invalid, eg. beyond the end.</exception>
        /// <exception cref="Errors.NotAvailable">The download has not yet reached the requested position.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        /// <remarks>
        /// <para>
        /// Setting the position of a MOD music in bytes (other than 0) requires that the <see cref="BassFlags.Prescan"/> flag was used in the <see cref="MusicLoad(string,long,int,BassFlags,int)" /> call.
        /// When setting the position in orders/rows, the channel's byte position (as reported by <see cref="ChannelGetPosition" />) is reset to 0.
        /// This is because it's not possible to get the byte position of an order/row position - it's possible that a position may never be played in the normal cause of events, or it may be played multiple times.
        /// </para>
        /// <para>
        /// When changing the position of a MOD music, and the <see cref="BassFlags.MusicPositionReset"/> flag is active on the channel, all notes that were playing before the position changed will be stopped.
        /// Otherwise, the notes will continue playing until they are stopped in the MOD music.
        /// When setting the position in bytes, the BPM, "speed" and "global volume" are updated to what they would normally be at the new position.
        /// Otherwise they are left as they were prior to the postion change, unless the seek position is 0 (the start), in which case they are also reset to the starting values (when using the <see cref="BassFlags.MusicPositionReset"/> flag).
        /// When the <see cref="BassFlags.MusicPositionResetEx"/> flag is active, the BPM, speed and global volume are reset with every seek.
        /// </para>
        /// <para>
        /// For MP3/MP2/MP1 streams, unless the file is scanned via the <see cref="PositionFlags.Scan"/> or the <see cref="BassFlags.Prescan"/> flag at stream creation, seeking will be approximate but generally still quite accurate.
        /// Besides scanning, exact seeking can also be achieved with the <see cref="PositionFlags.DecodeTo"/> flag.
        /// </para>
        /// <para>Seeking in internet file (and "buffered" User file) streams is possible once the download has reached the requested position, so long as the file is not being streamed in blocks <see cref="BassFlags.StreamDownloadBlocks"/>.</para>
        /// <para>User streams (created with <see cref="CreateStream(int,int,BassFlags,StreamProcedure,IntPtr)" />) are not seekable, but it is possible to reset a User stream (including its Buffer contents) by setting its position to byte 0.</para>
        /// <para>The <see cref="PositionFlags.DecodeTo"/> flag can be used to seek forwards in streams that are not normally seekable, like custom streams or internet streams that are using the <see cref="BassFlags.StreamDownloadBlocks"/> flag, but it will only go as far as what is currently available; it will not wait for more data to be downloaded, for example. <see cref="ChannelGetPosition" /> can be used to confirm what the new position actually is.</para>
        /// <para>In some cases, particularly when the <see cref="PositionFlags.Inexact"/> flag is used, the new position may not be what was requested. <see cref="ChannelGetPosition" /> can be used to confirm what the new position actually is.</para>
        /// <para>The <see cref="PositionFlags.Scan"/> flag works the same way as the <see cref="CreateStream(string,long,long,BassFlags)" /> <see cref="BassFlags.Prescan"/> flag, and can be used to delay the scanning until after the stream has been created. When a position beyond the end is requested, the call will fail (<see cref="Errors.Position"/> error code) but the seek table and exact Length will have been scanned.
        /// When a file has been scanned, all seeking (even without the <see cref="PositionFlags.Scan"/> flag) within the scanned part of it will use the scanned infomation.</para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_ChannelSetPosition")]
        public static extern bool ChannelSetPosition(int Handle, long Position, PositionFlags Mode = PositionFlags.Bytes);

        /// <summary>
        /// Checks if an attribute (or any attribute) of a sample, stream, or MOD music is sliding.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HMUSIC, HSTREAM or HRECORD.</param>
        /// <param name="Attribute">The attribute to check for sliding (0 for any attribute).</param>
        /// <returns>If the attribute (or any) is sliding, then <see langword="true" /> is returned, else <see langword="false" /> is returned.</returns>
        [DllImport(DllName, EntryPoint = "BASS_ChannelIsSliding")]
        public static extern bool ChannelIsSliding(int Handle, ChannelAttribute Attribute);

        const int SlideLog = 0x1000000;

        [DllImport(DllName)]
        static extern bool BASS_ChannelSlideAttribute(int Handle, int Attribute, float Value, int Time);

        /// <summary>
        /// Slides a channel's attribute from its current value to a new value.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HCHANNEL, HSTREAM or HMUSIC, or HRECORD.</param>
        /// <param name="Attribute">The attribute to slide the value of.</param>
        /// <param name="Value">The new attribute value. See the attribute's documentation for details on the possible values.</param>
        /// <param name="Time">The Length of time (in milliseconds) that it should take for the attribute to reach the <paramref name="Value" />.</param>
        /// <param name="Logarithmic">Slide logarithmically.</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Type"><paramref name="Attribute" /> is not valid.</exception>
        /// <remarks>
        /// <para>This function is similar to <see cref="Bass.ChannelSetAttribute(int,ChannelAttribute,float)" />, except that the attribute is ramped to the value over the specified period of time.
        /// Another difference is that the value is not pre-checked. If it is invalid, the slide will simply end early.</para>
        /// <para>If an attribute is already sliding, then the old slide is stopped and replaced by the new one.</para>
        /// <para><see cref="Bass.ChannelIsSliding" /> can be used to check if an attribute is currently sliding. A BASS_SYNC_SLIDE sync can also be set via <see cref="Bass.ChannelSetSync" />, to be triggered at the end of a slide.
        /// The sync will not be triggered in the case of an existing slide being replaced by a new one.</para>
        /// <para>Attribute slides are unaffected by whether the channel is playing, paused or stopped. They carry on regardless.</para>
        /// </remarks>
        public static bool ChannelSlideAttribute(int Handle, ChannelAttribute Attribute, float Value, int Time, bool Logarithmic = false)
        {
            var attr = (int)Attribute;

            if (Logarithmic)
                attr |= SlideLog;

            return BASS_ChannelSlideAttribute(Handle, attr, Value, Time);
        }

        #region Channel Get Level
        /// <summary>
        /// Retrieves the level (peak amplitude) of a sample, stream, MOD music or recording channel.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD.</param>
        /// <returns>
        /// If an error occurs, -1 is returned, use <see cref="LastError" /> to get the error code.
        /// <para>
        /// If successful, the level of the left channel is returned in the low word (low 16-bits), and the level of the right channel is returned in the high word (high 16-bits).
        /// If the channel is mono, then the low word is duplicated in the high word.
        /// The level ranges linearly from 0 (silent) to 32768 (max).
        /// 0 will be returned when a channel is stalled.
        /// </para>
        /// </returns>
        /// <remarks>
        /// <para>
        /// This function measures the level of the channel's sample data, not the level of the channel in the final output mix,
        /// so the channel's volume and panning/balance (as set with <see cref="ChannelSetAttribute(int,ChannelAttribute,float)" />, <see cref="ChannelAttribute.Volume"/> or <see cref="ChannelAttribute.Pan"/>) does not affect it.
        /// The effect of any DSP/FX set on the channel is present in the measurement, except for DX8 effects when using the "With FX flag" DX8 effect implementation.
        /// </para>
        /// <para>
        /// For channels that are more than stereo, the left level will include all left channels (eg. front-left, rear-left, center), and the right will include all right (front-right, rear-right, LFE).
        /// If there are an odd number of channels then the left and right levels will include all channels.
        /// If the level of each individual channel is required, that is available from the other overload(s).
        /// </para>
        /// <para>
        /// 20ms of data is inspected to calculate the level.
        /// When used with a decoding channel, that means 20ms of data needs to be decoded from the channel in order to calculate the level, and that data is then gone, eg. it is not available to a subsequent <see cref="ChannelGetData(int,IntPtr,int)" /> call.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.NotPlaying">The channel is not playing.</exception>
        /// <exception cref="Errors.Ended">The decoding channel has reached the end.</exception>
        /// <exception cref="Errors.BufferLost">Should not happen... check that a valid window handle was used with <see cref="Init" />.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetLevel")]
        public static extern int ChannelGetLevel(int Handle);

        /// <summary>
        /// Gets the Level of the Left Channel.
        /// </summary>
        public static int ChannelGetLevelLeft(int Handle)
        {
            int value = ChannelGetLevel(Handle);

            if (value == -1)
                return -1;

            return value.LoWord();
        }

        /// <summary>
        /// Gets the Level of the Right Channel.
        /// </summary>
        public static int ChannelGetLevelRight(int Handle)
        {
            int value = ChannelGetLevel(Handle);

            if (value == -1)
                return -1;

            return value.HiWord();
        }

        /// <summary>
        /// Retrieves the level (peak amplitude) of a sample, stream, MOD music or recording channel.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD.</param>
        /// <param name="Levels">The array in which the levels are to be returned.</param>
        /// <param name="Length">How much data (in seconds) to look at to get the level (limited to 1 second).</param>
        /// <param name="Flags">What levels to retrieve.</param>
        /// <returns>
        /// On success <see langword="true" /> is returned - else <see langword="false" />, use <see cref="LastError" /> to get the error code.
        /// <para>If successful, the requested levels are returned in the <paramref name="Levels" /> array.</para>
        /// </returns>
        /// <remarks>
        /// This function operates in the same way as <see cref="ChannelGetLevel(int)" /> but has greater flexibility on how the level is measured.
        /// The levels are not clipped, so may exceed +/-1.0 on floating-point channels.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.NotPlaying">The channel is not playing.</exception>
        /// <exception cref="Errors.Ended">The decoding channel has reached the end.</exception>
        /// <exception cref="Errors.BufferLost">Should not happen... check that a valid window handle was used with <see cref="Init" />.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetLevelEx")]
        public static extern bool ChannelGetLevel(int Handle, [In, Out] float[] Levels, float Length, LevelRetrievalFlags Flags);

        /// <summary>
        /// Retrieves the level (peak amplitude) of a sample, stream, MOD music or recording channel.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD.</param>
        /// <param name="Length">How much data (in seconds) to look at to get the level (limited to 1 second).</param>
        /// <param name="Flags">What levels to retrieve.</param>
        /// <returns>Array of levels on success, else <see langword="null" />. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// This function operates in the same way as <see cref="ChannelGetLevel(int)" /> but has greater flexibility on how the level is measured.
        /// The levels are not clipped, so may exceed +/-1.0 on floating-point channels.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.NotPlaying">The channel is not playing.</exception>
        /// <exception cref="Errors.Ended">The decoding channel has reached the end.</exception>
        /// <exception cref="Errors.BufferLost">Should not happen... check that a valid window handle was used with <see cref="Init" />.</exception>
        public static float[] ChannelGetLevel(int Handle, float Length, LevelRetrievalFlags Flags)
        {
            var n = ChannelGetInfo(Handle).Channels;

            if (Flags.HasFlag(LevelRetrievalFlags.Mono))
                n = 1;
            else if (Flags.HasFlag(LevelRetrievalFlags.Stereo))
                n = 2;

            var levels = new float[n];

            return ChannelGetLevel(Handle, levels, Length, Flags) ? levels : null;
        }
        #endregion

        #region Channel Get Data
        /// <summary>
        /// Retrieves the immediate sample data (or an FFT representation of it) of a sample channel, stream, MOD music, or recording channel.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD.</param>
        /// <param name="Buffer">Location to write the data as an <see cref="IntPtr"/> (can be <see cref="IntPtr.Zero" /> when handle is a recording channel (HRECORD), to discard the requested amount of data from the recording buffer).</param>
        /// <param name="Length">Number of bytes wanted, and/or the <see cref="DataFlags" /></param>
        /// <returns>If an error occurs, -1 is returned, use <see cref="LastError" /> to get the error code.
        /// <para>When requesting FFT data, the number of bytes read from the channel (to perform the FFT) is returned.</para>
        /// <para>When requesting sample data, the number of bytes written to buffer will be returned (not necessarily the same as the number of bytes read when using the <see cref="DataFlags.Float"/> or DataFlags.Fixed flag).</para>
        /// <para>When using the <see cref="DataFlags.Available"/> flag, the number of bytes in the channel's buffer is returned.</para>
        /// </returns>
        /// <remarks>
        /// <para>
        /// This function can only return as much data as has been written to the channel's buffer, so it may not always be possible to get the amount of data requested, especially if you request large amounts.
        /// If you really do need large amounts, then increase the buffer lengths (<see cref="PlaybackBufferLength"/>).
        /// The <see cref="DataFlags.Available"/> flag can be used to check how much data a channel's buffer contains at any time, including when stopped or stalled.
        /// </para>
        /// <para>When requesting data from a decoding channel, data is decoded directly from the channel's source (no playback buffer) and as much data as the channel has available can be decoded at a time.</para>
        /// <para>When retrieving sample data, 8-bit samples are unsigned (0 to 255), 16-bit samples are signed (-32768 to 32767), 32-bit floating-point samples range from -1 to +1 (not clipped, so can actually be outside this range).
        /// That is unless the <see cref="DataFlags.Float"/> flag is used, in which case, the sample data will be converted to 32-bit floating-point if it is not already, or if the DataFlags.Fixed flag is used, in which case the data will be coverted to 8.24 fixed-point.
        /// </para>
        /// <para>
        /// Unless complex data is requested via the <see cref="DataFlags.FFTComplex"/> flag, the magnitudes of the first half of an FFT result are returned.
        /// For example, with a 2048 sample FFT, there will be 1024 floating-point values returned.
        /// If the DataFlags.Fixed flag is used, then the FFT values will be in 8.24 fixed-point form rather than floating-point.
        /// Each value, or "bin", ranges from 0 to 1 (can actually go higher if the sample data is floating-point and not clipped).
        /// The 1st bin contains the DC component, the 2nd contains the amplitude at 1/2048 of the channel's sample rate, followed by the amplitude at 2/2048, 3/2048, etc.
        /// A Hann window is applied to the sample data to reduce leakage, unless the <see cref="DataFlags.FFTNoWindow"/> flag is used.
        /// When a window is applied, it causes the DC component to leak into the next bin, but that can be removed (reduced to 0) by using the <see cref="DataFlags.FFTRemoveDC"/> flag.
        /// Doing so slightly increases the processing required though, so it should only be done when needed, which is when a window is applied and the 2nd bin value is important.
        /// </para>
        /// <para>
        /// Channels that have 2 or more sample channels (ie. stereo or above) may have FFT performed on each individual channel, using the <see cref="DataFlags.FFTIndividual"/> flag.
        /// Without this flag, all of the channels are combined, and a single mono FFT is performed.
        /// Performing the extra individual FFTs of course increases the amount of processing required.
        /// The return values are interleaved in the same order as the channel's sample data, eg. stereo = left,right,left,etc.
        /// </para>
        /// <para>This function is most useful if you wish to visualize (eg. spectrum analyze) the sound.</para>
        /// <para><b>Platform-specific:</b></para>
        /// <para>The DataFlags.Fixed flag is only available on Android and Windows CE.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Ended">The channel has reached the end.</exception>
        /// <exception cref="Errors.NotAvailable">The <see cref="DataFlags.Available"/> flag was used with a decoding channel.</exception>
        /// <exception cref="Errors.BufferLost">Should not happen... check that a valid window handle was used with <see cref="Init" />.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetData")]
        public static extern int ChannelGetData(int Handle, IntPtr Buffer, int Length);

        /// <summary>
        /// Retrieves the immediate sample data (or an FFT representation of it) of a sample channel, stream, MOD music, or recording channel.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD.</param>
        /// <param name="Buffer">Location to write the data as a byte[].</param>
        /// <param name="Length">Number of bytes wanted, and/or the <see cref="DataFlags" /></param>
        /// <returns>If an error occurs, -1 is returned, use <see cref="LastError" /> to get the error code.
        /// <para>When requesting FFT data, the number of bytes read from the channel (to perform the FFT) is returned.</para>
        /// <para>When requesting sample data, the number of bytes written to buffer will be returned (not necessarily the same as the number of bytes read when using the <see cref="DataFlags.Float"/> or DataFlags.Fixed flag).</para>
        /// <para>When using the <see cref="DataFlags.Available"/> flag, the number of bytes in the channel's buffer is returned.</para>
        /// </returns>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetData")]
        public static extern int ChannelGetData(int Handle, [In, Out] byte[] Buffer, int Length);

        /// <summary>
        /// Retrieves the immediate sample data (or an FFT representation of it) of a sample channel, stream, MOD music, or recording channel.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD.</param>
        /// <param name="Buffer">Location to write the data as a short[].</param>
        /// <param name="Length">Number of bytes wanted, and/or the <see cref="DataFlags" /></param>
        /// <returns>If an error occurs, -1 is returned, use <see cref="LastError" /> to get the error code.
        /// <para>When requesting FFT data, the number of bytes read from the channel (to perform the FFT) is returned.</para>
        /// <para>When requesting sample data, the number of bytes written to buffer will be returned (not necessarily the same as the number of bytes read when using the <see cref="DataFlags.Float"/> or DataFlags.Fixed flag).</para>
        /// <para>When using the <see cref="DataFlags.Available"/> flag, the number of bytes in the channel's buffer is returned.</para>
        /// </returns>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetData")]
        public static extern int ChannelGetData(int Handle, [In, Out] short[] Buffer, int Length);

        /// <summary>
        /// Retrieves the immediate sample data (or an FFT representation of it) of a sample channel, stream, MOD music, or recording channel.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD.</param>
        /// <param name="Buffer">Location to write the data as a int[].</param>
        /// <param name="Length">Number of bytes wanted, and/or the <see cref="DataFlags" /></param>
        /// <returns>If an error occurs, -1 is returned, use <see cref="LastError" /> to get the error code.
        /// <para>When requesting FFT data, the number of bytes read from the channel (to perform the FFT) is returned.</para>
        /// <para>When requesting sample data, the number of bytes written to buffer will be returned (not necessarily the same as the number of bytes read when using the <see cref="DataFlags.Float"/> or DataFlags.Fixed flag).</para>
        /// <para>When using the <see cref="DataFlags.Available"/> flag, the number of bytes in the channel's buffer is returned.</para>
        /// </returns>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetData")]
        public static extern int ChannelGetData(int Handle, [In, Out] int[] Buffer, int Length);

        /// <summary>
        /// Retrieves the immediate sample data (or an FFT representation of it) of a sample channel, stream, MOD music, or recording channel.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD.</param>
        /// <param name="Buffer">Location to write the data as a float[].</param>
        /// <param name="Length">Number of bytes wanted, and/or the <see cref="DataFlags" /></param>
        /// <returns>If an error occurs, -1 is returned, use <see cref="LastError" /> to get the error code.
        /// <para>When requesting FFT data, the number of bytes read from the channel (to perform the FFT) is returned.</para>
        /// <para>When requesting sample data, the number of bytes written to buffer will be returned (not necessarily the same as the number of bytes read when using the <see cref="DataFlags.Float"/> or DataFlags.Fixed flag).</para>
        /// <para>When using the <see cref="DataFlags.Available"/> flag, the number of bytes in the channel's buffer is returned.</para>
        /// </returns>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetData")]
        public static extern int ChannelGetData(int Handle, [In, Out] float[] Buffer, int Length);
        #endregion

        /// <summary>
        /// Updates the playback buffer of a stream or MOD music.
        /// </summary>
        /// <param name="Handle">The channel handle... a HMUSIC or HSTREAM.</param>
        /// <param name="Length">
        /// The amount to render, in milliseconds... 0 = default (2 x <see cref="UpdatePeriod" />).
        /// This is capped at the space available in the buffer.
        /// </param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// When starting playback of a stream or MOD music, after creating it or changing its position, there will be a slight delay while the initial data is decoded for playback.
        /// Usually the delay is not noticeable or important, but if you need playback to start instantly when you call <see cref="ChannelPlay" />, then use this function first.
        /// The length parameter should be at least equal to the <see cref="UpdatePeriod" />.
        /// </para>
        /// <para>
        /// It may not always be possible to render the requested amount of data, in which case this function will still succeed.
        /// <see cref="ChannelGetData(int, IntPtr, int)" /> (<see cref="DataFlags.Available"/>) can be used to check how much data a channel has buffered for playback.
        /// </para>
        /// <para>
        /// When automatic updating is disabled (<see cref="UpdatePeriod" /> = 0 or <see cref="UpdateThreads" /> = 0),
        /// this function could be used instead of <see cref="Update" /> to implement different update periods for different channels,
        /// instead of a single update period for all.
        /// Unlike <see cref="Update" />, this function can also be used while automatic updating is enabled.
        /// </para>
        /// <para>The CPU usage of this function is not included in the <see cref="CPUUsage" /> reading.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.NotAvailable">Decoding channels do not have playback buffers.</exception>
        /// <exception cref="Errors.Ended">The channel has ended.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelUpdate")]
        public static extern bool ChannelUpdate(int Handle, int Length);
    }
}
