using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    public static partial class Bass
    {
        /// <summary>
        /// Sets the parameters of an effect
        /// </summary>
        /// <param name="Handle">The effect handle</param>
        /// <param name="Parameters">Pointer to the parameters structure. The structure used depends on the effect type.</param>
        /// <returns>
        /// If successful, <see langword="true"/> is returned, else <see langword="false"/> is returned.
        /// Use <see cref="LastError"/> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid.</exception>
        /// <exception cref="Errors.Parameter">One or more of the parameters are invalid, make sure all the values are within the valid ranges.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        /// <seealso cref="ChannelSetFX"/>
        /// <seealso cref="FXGetParameters(int,IntPtr)"/>
        [DllImport(DllName, EntryPoint = "BASS_FXSetParameters")]
        public static extern bool FXSetParameters(int Handle, IntPtr Parameters);

        /// <summary>
        /// Sets the parameters of an effect
        /// </summary>
        /// <param name="Handle">The effect handle</param>
        /// <param name="Parameters">The parameters structure. The structure used depends on the effect type.</param>
        /// <returns>
        /// If successful, <see langword="true"/> is returned, else <see langword="false"/> is returned.
        /// Use <see cref="LastError"/> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid.</exception>
        /// <exception cref="Errors.Parameter">One or more of the parameters are invalid, make sure all the values are within the valid ranges.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        /// <seealso cref="ChannelSetFX"/>
        /// <seealso cref="FXGetParameters(int,IntPtr)"/>
        public static bool FXSetParameters(int Handle, IEffectParameter Parameters)
        {
            using (var gcp = new GCPin(Parameters))
                return FXSetParameters(Handle, gcp.Pointer);
        }

        /// <summary>
        /// Retrieves the parameters of an effect
        /// </summary>
        /// <param name="Handle">The effect handle</param>
        /// <param name="Parameters">Pointer to the parameters structure to fill. The structure used depends on the effect type.</param>
        /// <returns>
        /// If successful, <see langword="true"/> is returned, else <see langword="false"/> is returned.
        /// Use <see cref="LastError"/> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid.</exception>
        /// <seealso cref="ChannelSetFX"/>
        /// <seealso cref="FXSetParameters(int,IntPtr)"/>
        [DllImport(DllName, EntryPoint = "BASS_FXGetParameters")]
        public static extern bool FXGetParameters(int Handle, IntPtr Parameters);

        /// <summary>
        /// Retrieves the parameters of an effect
        /// </summary>
        /// <param name="Handle">The effect handle</param>
        /// <param name="Parameters">The parameters structure to fill. The structure used depends on the effect type.</param>
        /// <returns>
        /// If successful, <see langword="true"/> is returned, else <see langword="false"/> is returned.
        /// Use <see cref="LastError"/> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is not valid.</exception>
        /// <seealso cref="ChannelSetFX"/>
        /// <seealso cref="FXSetParameters(int,IntPtr)"/>
        public static bool FXGetParameters(int Handle, IEffectParameter Parameters)
        {
            using (var gcp = new GCPin(Parameters))
                return FXGetParameters(Handle, gcp.Pointer);
        }

        /// <summary>
        /// Resets the state of an effect or all effects on a channel.
        /// </summary>
        /// <param name="Handle">The effect or channel handle... a HFX, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <returns>
        /// If successful, <see langword="true"/> is returned, else <see langword="false"/> is returned.
        /// Use <see cref="LastError"/> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is invalid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        /// <seealso cref="ChannelSetFX"/>
        /// <remarks>
        /// This function flushes the internal buffers of the effect(s).
        /// Effects are automatically reset by <see cref="ChannelSetPosition"/>,
        /// except when called from a "<see cref="SyncFlags.Mixtime"/>" <see cref="SyncProcedure"/>.
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_FXReset")]
        public static extern bool FXReset(int Handle);

        /// <summary>
        /// Sets an effect on a stream, MOD music, or recording channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Type">Type of effect, one of <see cref="EffectType" />.</param>
        /// <param name="Priority">
        /// The priority of the new FX, which determines it's position in the DSP chain.
        /// DSP/FX with higher priority are applied before those with lower.
        /// This parameter has no effect with DX8 effects when the "with FX flag" DX8 effect implementation is used.
        /// </param>
        /// <returns>
        /// If succesful, then the new effect's Handle is returned, else 0 is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Type">An illegal <paramref name="Type" /> was specified.</exception>
        /// <exception cref="Errors.NoFX">DX8 effects are unavailable.</exception>
        /// <exception cref="Errors.SampleFormat">
        /// The channel's format is not supported by the effect.
        /// It may be floating-point (without DX9) or more than stereo.
        /// </exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        /// <remarks>
        /// <para>
        /// Multiple effects may be used per channel. Use <see cref="ChannelRemoveFX" /> to remove an effect.
        /// Use <see cref="FXSetParameters(int,IntPtr)" /> to set an effect's parameters.
        /// </para>
        /// <para>
        /// Effects can be applied to MOD musics and streams, but not samples.
        /// If you want to apply an effect to a sample, you could use a stream instead.
        /// </para>
        /// <para>
        /// Depending on the DX8 effect implementation being used by the channel, the channel may have to be stopped before adding or removing DX8 effects on it.
        /// If necessary, that is done automatically and the channel is resumed afterwards.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// DX8 effects are a Windows feature requiring DirectX 8, or DirectX 9 for floating-point support.
        /// On other platforms, they are emulated by BASS, except for the following which are currently unsupported: DXCompressor, DXGargle, and DX_I3DL2Reverb.
        /// On Windows CE, only PARAMEQ is supported.
        /// </para>
        /// </remarks>
        /// <seealso cref="ChannelLock"/>
        /// <seealso cref="ChannelRemoveFX"/>
        /// <seealso cref="FXGetParameters(int,IntPtr)"/>
        /// <seealso cref="FXSetParameters(int,IntPtr)"/>
        /// <seealso cref="ChannelSetDSP"/>
        [DllImport(DllName, EntryPoint = "BASS_ChannelSetFX")]
        public static extern int ChannelSetFX(int Handle, EffectType Type, int Priority);

        /// <summary>
        /// Removes an effect from a stream, MOD music, or recording channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="FX">Handle of the effect to remove from the channel (return value of a previous <see cref="ChannelSetFX" /> call).</param>
        /// <returns>
        /// If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Handle">At least one of <paramref name="Handle" /> and <paramref name="FX" /> is not valid.</exception>
        /// <remarks>
        /// Depending on the DX8 effect implementation being used by the channel, the channel may have to be stopped before removing a DX8 effect on it.
        /// If necessary, that is done automatically and the channel is resumed afterwards.
        /// <para><see cref="ChannelRemoveDSP" /> can also be used to remove effects.</para>
        /// </remarks>
        /// <seealso cref="ChannelSetFX"/>
        [DllImport(DllName, EntryPoint = "BASS_ChannelRemoveFX")]
        public static extern bool ChannelRemoveFX(int Handle, int FX);

        /// <summary>
        /// Sets the priority of an effect or DSP function, which determines its position in the DSP chain.
        /// </summary>
        /// <param name="Handle">The DSP/FX handle... a HDSP or HFX.</param>
        /// <param name="Priority">The new priority of the DSP/FX.</param>
        /// <returns>If successful, <see langword="true"/> is returned, else <see langword="false"/> is returned. Use <see cref="LastError"/> to get the error code.</returns>
        /// <remarks>If there are multiple DSP/FX with the same priority value, they will be applied in the order in which they were given that priority.</remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle"/> is invalid.</exception>
        /// <exception cref="Errors.NotAvailable">Priority is not supported on DX8 effects when the "with FX flag" DX8 effect implementation is used.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FXSetPriority")]
        public static extern bool FXSetPriority(int Handle, int Priority);
    }
}
