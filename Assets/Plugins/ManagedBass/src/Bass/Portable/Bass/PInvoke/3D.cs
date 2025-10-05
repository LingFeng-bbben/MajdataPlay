using System.Runtime.InteropServices;

namespace ManagedBass
{
    public static partial class Bass
    {
        /// <summary>
        /// Applies changes made to the 3D system.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This function must be called to apply any changes made with <see cref="Set3DFactors"/>, <see cref="Set3DPosition"/>, <see cref="ChannelSet3DAttributes"/> or <see cref="ChannelSet3DPosition"/>.
        /// This allows multiple changes to be synchronized, and also improves performance.
        /// </para>
        /// <para>
        /// This function applies 3D changes on all the initialized devices.
        /// There's no need to re-call it for each individual device when using multiple devices.
        /// </para>
        /// </remarks>
        /// <seealso cref="ChannelSet3DAttributes"/>
        /// <seealso cref="ChannelSet3DPosition"/>
        /// <seealso cref="Set3DFactors"/>
        /// <seealso cref="Set3DPosition"/>
        [DllImport(DllName, EntryPoint = "BASS_Apply3D")]
        public static extern void Apply3D();

        /// <summary>
        /// Retrieves the factors that affect the calculations of 3D sound.
        /// </summary>
        /// <param name="Distance">The distance factor.</param>
        /// <param name="RollOff">The rolloff factor.</param>
        /// <param name="Doppler">The doppler factor.</param>
        /// <returns>
        /// If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.No3D">The device was not initialized with 3D support.</exception>
        /// <remarks>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</remarks>
        /// <seealso cref="Set3DFactors"/>
        [DllImport(DllName, EntryPoint = "BASS_Get3DFactors")]
        public static extern bool Get3DFactors(ref float Distance, ref float RollOff, ref float Doppler);

        /// <summary>
        /// Sets the factors that affect the calculations of 3D sound.
        /// </summary>
        /// <param name="Distance">
        /// The distance factor... less than 0.0 = leave current... examples: 1.0 = use meters, 0.9144 = use yards, 0.3048 = use feet.
        /// By default BASS measures distances in meters, you can change this setting if you are using a different unit of measurement.
        /// </param>
        /// <param name="RollOff">The rolloff factor, how fast the sound quietens with distance... 0.0 (min) - 10.0 (max), less than 0.0 = leave current... examples: 0.0 = no rolloff, 1.0 = real world, 2.0 = 2x real.</param>
        /// <param name="Doppler">
        /// The doppler factor... 0.0 (min) - 10.0 (max), less than 0.0 = leave current... examples: 0.0 = no doppler, 1.0 = real world, 2.0 = 2x real.
        /// The doppler effect is the way a sound appears to change pitch when it is moving towards or away from you (say hello to Einstein!).
        /// The listener and sound velocity settings are used to calculate this effect, this <paramref name="Doppler"/> value can be used to lessen or exaggerate the effect.
        /// </param>
        /// <returns>
        /// If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.No3D">The device was not initialized with 3D support.</exception>
        /// <remarks>
        /// <para>As with all 3D functions, use <see cref="Apply3D" /> to apply the changes.</para>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// </remarks>
        /// <seealso cref="Apply3D"/>
        /// <seealso cref="Get3DFactors"/>
        [DllImport(DllName, EntryPoint = "BASS_Set3DFactors")]
        public static extern bool Set3DFactors(float Distance, float RollOff, float Doppler);

        /// <summary>
        /// Retrieves the position, velocity, and orientation of the listener.
        /// </summary>
        /// <param name="Position">The position of the listener</param>
        /// <param name="Velocity">The listener's velocity</param>
        /// <param name="Front">The direction that the listener's front is pointing</param>
        /// <param name="Top">The direction that the listener's top is pointing</param>
        /// <returns>
        /// If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.No3D">The device was not initialized with 3D support.</exception>
        /// <remarks>
        /// <para>The <paramref name="Front" /> and <paramref name="Top" /> parameters must both be retrieved in a single call, they can not be retrieved individually.</para>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// </remarks>
        /// <seealso cref="Set3DPosition"/>
        /// <seealso cref="Vector3D"/>
        [DllImport(DllName, EntryPoint = "BASS_Get3DPosition")]
        public static extern bool Get3DPosition(ref Vector3D Position, ref Vector3D Velocity, ref Vector3D Front, ref Vector3D Top);

        /// <summary>
        /// Sets the position, velocity, and orientation of the listener (i.e. the player).
        /// </summary>
        /// <param name="Position">The position of the listener... <see langword="null"/> = Leave Current.</param>
        /// <param name="Velocity">The listener's velocity... <see langword="null"/> = Leave Current.</param>
        /// <param name="Front">The direction that the listener's front is pointing... <see langword="null"/> = Leave Current.</param>
        /// <param name="Top">The direction that the listener's top is pointing... <see langword="null"/> = Leave Current.</param>
        /// <returns>
        /// If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.No3D">The device was not initialized with 3D support.</exception>
        /// <remarks>
        /// <para>The <paramref name="Front" /> and <paramref name="Top" /> parameters must both be set in a single call, they can not be set individually.</para>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// </remarks>
        /// <seealso cref="Apply3D"/>
        /// <seealso cref="Get3DPosition"/>
        /// <seealso cref="Set3DFactors"/>
        /// <seealso cref="Vector3D"/>
        [DllImport(DllName, EntryPoint = "BASS_Set3DPosition")]
        public static extern bool Set3DPosition(Vector3D Position, Vector3D Velocity, Vector3D Front, Vector3D Top);

        /// <summary>
        /// The 3D algorithm for software mixed 3D channels.
        /// </summary>
        /// <remarks>
        /// <para>
        /// These algorithms only affect 3D channels that are being mixed in software.
        /// <see cref="ChannelGetInfo(int, out ChannelInfo)"/> can be used to check whether a channel is being software mixed.
        /// </para>
        /// <para>
        /// Changing the algorithm only affects subsequently created or loaded samples, musics, or streams;
        /// it does not affect any that already exist.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// On Windows, DirectX 7 or above is required for this option to have effect.
        /// On other platforms, only the <see cref="ManagedBass.Algorithm3D.Default"/> and <see cref="ManagedBass.Algorithm3D.Off"/> options are available.
        /// </para>
        /// </remarks>
        public static Algorithm3D Algorithm3D
        {
            get => (Algorithm3D)GetConfig(Configuration.Algorithm3D);
            set => Configure(Configuration.Algorithm3D, (int)value);
        }

        /// <summary>
        /// Retrieves the 3D attributes of a sample, stream, or MOD music channel with 3D functionality.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM.</param>
        /// <param name="Mode">The 3D processing mode (see <see cref="Mode3D" />).</param>
        /// <param name="Min">The minimum distance.</param>
        /// <param name="Max">The maximum distance.</param>
        /// <param name="iAngle">The angle of the inside projection cone.</param>
        /// <param name="oAngle">The angle of the outside projection cone.</param>
        /// <param name="OutVol">The delta-volume outside the outer projection cone.</param>
        /// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>The <paramref name="iAngle"/> and <paramref name="oAngle"/> parameters must both be retrieved in a single call to this function (ie. you can't retrieve one without the other).</remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.No3D">The channel does not have 3D functionality.</exception>
        /// <seealso cref="ChannelGet3DPosition"/>
        /// <seealso cref="ChannelGetAttribute(int, ChannelAttribute, out float)"/>
        /// <seealso cref="ChannelSet3DAttributes"/>
        /// <seealso cref="ChannelAttribute.EaxMix"/>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGet3DAttributes")]
        public static extern bool ChannelGet3DAttributes(int Handle, ref Mode3D Mode, ref float Min, ref float Max, ref int iAngle, ref int oAngle, ref float OutVol);

        /// <summary>
        /// Sets the 3D attributes of a sample, stream, or MOD music channel with 3D functionality.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM.</param>
        /// <param name="Mode">The 3D processing mode</param>
        /// <param name="Min">The minimum distance. The channel's volume is at maximum when the listener is within this distance... less than 0.0 = leave current.</param>
        /// <param name="Max">The maximum distance. The channel's volume stops decreasing when the listener is beyond this distance... less than 0.0 = leave current.</param>
        /// <param name="iAngle">The angle of the inside projection cone in degrees... 0 (no cone) - 360 (sphere), -1 = leave current.</param>
        /// <param name="oAngle">The angle of the outside projection cone in degrees... 0 (no cone) - 360 (sphere), -1 = leave current.</param>
        /// <param name="OutVol">The delta-volume outside the outer projection cone... 0 (silent) - 100 (same as inside the cone), -1 = leave current.</param>
        /// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.No3D">The channel does not have 3D functionality.</exception>
        /// <exception cref="Errors.Parameter">One or more of the attribute values is invalid.</exception>
        /// <remarks>
        /// <para>The <paramref name="iAngle"/> and <paramref name="oAngle"/> parameters must both be set in a single call to this function (ie. you can't set one without the other).
        /// The <paramref name="iAngle"/> and <paramref name="oAngle"/> angles decide how wide the sound is projected around the orientation angle. Within the inside angle the volume level is the channel volume, as set with <see cref="ChannelSetAttribute(int,ChannelAttribute,float)" />.
        /// Outside the outer angle, the volume changes according to the <paramref name="OutVol"/> value. Between the inner and outer angles, the volume gradually changes between the inner and outer volume levels.
        /// If the inner and outer angles are 360 degrees, then the sound is transmitted equally in all directions.</para>
        /// <para>As with all 3D functions, use <see cref="Apply3D" /> to apply the changes made.</para>
        /// </remarks>
        /// <seealso cref="ChannelGet3DAttributes"/>
        /// <seealso cref="ChannelSet3DPosition"/>
        /// <seealso cref="ChannelSetAttribute(int, ChannelAttribute, float)"/>
        /// <seealso cref="ChannelAttribute.EaxMix"/>
        [DllImport(DllName, EntryPoint = "BASS_ChannelSet3DAttributes")]
        public static extern bool ChannelSet3DAttributes(int Handle, Mode3D Mode, float Min, float Max, int iAngle, int oAngle, float OutVol);

        /// <summary>
        /// Retrieves the 3D position of a sample, stream, or MOD music channel with 3D functionality.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM.</param>
        /// <param name="Position">Position of the sound.</param>
        /// <param name="Orientation">Orientation of the sound.</param>
        /// <param name="Velocity">Velocity of the sound.</param>
        /// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.No3D">The channel does not have 3D functionality.</exception>
        /// <seealso cref="ChannelGet3DAttributes"/>
        /// <seealso cref="ChannelGetAttribute(int, ChannelAttribute, out float)"/>
        /// <seealso cref="ChannelSet3DPosition"/>
        /// <seealso cref="Get3DFactors"/>
        /// <seealso cref="Get3DPosition"/>
        /// <seealso cref="Vector3D"/>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGet3DPosition")]
        public static extern bool ChannelGet3DPosition(int Handle, ref Vector3D Position, ref Vector3D Orientation, ref Vector3D Velocity);

        /// <summary>
        /// Sets the 3D position of a sample, stream, or MOD music channel with 3D functionality.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM.</param>
        /// <param name="Position">Position of the sound.</param>
        /// <param name="Orientation">Orientation of the sound.</param>
        /// <param name="Velocity">Velocity of the sound. This is only used to calculate the doppler effect, and has no effect on the sound's position.</param>
        /// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>As with all 3D functions, <see cref="Apply3D" /> must be called to apply the changes made.</remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.No3D">The channel does not have 3D functionality.</exception>
        /// <seealso cref="Apply3D"/>
        /// <seealso cref="ChannelGet3DPosition"/>
        /// <seealso cref="ChannelSet3DAttributes"/>
        /// <seealso cref="ChannelSetAttribute(int, ChannelAttribute, float)"/>
        /// <seealso cref="Set3DFactors"/>
        /// <seealso cref="Set3DPosition"/>
        /// <seealso cref="Vector3D"/>
        [DllImport(DllName, EntryPoint = "BASS_ChannelSet3DPosition")]
        public static extern bool ChannelSet3DPosition(int Handle, Vector3D Position, Vector3D Orientation, Vector3D Velocity);
    }
}
