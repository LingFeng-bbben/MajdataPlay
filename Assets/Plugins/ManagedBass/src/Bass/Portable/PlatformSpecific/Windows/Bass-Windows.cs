using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    public static partial class Bass
    {
        #region GetDSoundObject
        /// <summary>
        /// Retrieves a pointer to a DirectSound object interface. (Available only on Windows) (Not much useful in .Net)
        /// </summary>
        /// <param name="obj">The interface to retrieve.</param>
        /// <returns>
        /// If successful, then a pointer to the requested object is returned, otherwise <see cref="IntPtr.Zero"/> is returned.
        /// Use <see cref="LastError"/> to get the error code.
        /// </returns>
        [DllImport(DllName, EntryPoint = "BASS_GetDSoundObject")]
        public static extern IntPtr GetDSoundObject(DSInterface obj);

        /// <summary>
        /// Retrieves a pointer to a DirectSound object interface. (Available only on Windows) (Not much useful in .Net)
        /// </summary>
        /// <param name="Channel">An HCHANNEL, HMUSIC or HSTREAM handle of which IDirectSoundBuffer is to be retrieved.</param>
        /// <returns>
        /// If successful, then a pointer to an IDirectSoundBuffer is returned, otherwise <see cref="IntPtr.Zero"/> is returned.
        /// Use <see cref="LastError"/> to get the error code.
        /// </returns>
        [DllImport(DllName, EntryPoint = "BASS_GetDSoundObject")]
        public static extern IntPtr GetDSoundObject(int Channel);
        #endregion

        #region EAX
        /// <summary>
        /// Retrieves the current type of EAX environment and it's parameters.
        /// </summary>
        /// <param name="Environment">The EAX environment to get (one of the <see cref="EAXEnvironment" /> values).</param>
        /// <param name="Volume">The volume of the reverb.</param>
        /// <param name="Decay">The decay duration.</param>
        /// <param name="Damp">The damping.</param>
        /// <returns>
        /// If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NoEAX">The current device does not support EAX.</exception>
        /// <remarks>
        /// When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.
        /// <para><b>Platform-specific</b></para>
        /// <para>EAX and this function are only available on Windows</para>
        /// </remarks>
        /// <seealso cref="SetEAXParameters"/>
        [DllImport(DllName, EntryPoint = "BASS_GetEAXParameters")]
        public static extern bool GetEAXParameters(ref EAXEnvironment Environment, ref float Volume, ref float Decay, ref float Damp);

        /// <summary>
        /// Sets the parameters of EAX from a Preset.
        /// </summary>
        /// <param name="Environment">The EAX Environment.</param>
        /// <returns></returns>
        public static bool SetEAXPreset(EAXEnvironment Environment)
        {
            switch (Environment)
            {
                case EAXEnvironment.Generic:
                    return SetEAXParameters(Environment, 0.5f, 1.493f, 0.5f);

                case EAXEnvironment.PaddedCell:
                    return SetEAXParameters(Environment, 0.25f, 0.1f, 0);

                case EAXEnvironment.Room:
                    return SetEAXParameters(Environment, 0.417f, 0.4f, 0.666f);

                case EAXEnvironment.Bathroom:
                    return SetEAXParameters(Environment, 0.653f, 1.499f, 0.166f);

                case EAXEnvironment.Livingroom:
                    return SetEAXParameters(Environment, 0.208f, 0.478f, 0);

                case EAXEnvironment.Stoneroom:
                    return SetEAXParameters(Environment, 0.5f, 2.309f, 0.888f);

                case EAXEnvironment.Auditorium:
                    return SetEAXParameters(Environment, 0.403f, 4.279f, 0.5f);

                case EAXEnvironment.ConcertHall:
                    return SetEAXParameters(Environment, 0.5f, 3.961f, 0.5f);

                case EAXEnvironment.Cave:
                    return SetEAXParameters(Environment, 0.5f, 2.886f, 1.304f);

                case EAXEnvironment.Arena:
                    return SetEAXParameters(Environment, 0.361f, 7.284f, 0.332f);

                case EAXEnvironment.Hangar:
                    return SetEAXParameters(Environment, 0.5f, 10, 0.3f);

                case EAXEnvironment.CarpetedHallway:
                    return SetEAXParameters(Environment, 0.153f, 0.259f, 2);

                case EAXEnvironment.Hallway:
                    return SetEAXParameters(Environment, 0.361f, 1.493f, 0);

                case EAXEnvironment.StoneCorridor:
                    return SetEAXParameters(Environment, 0.444f, 2.697f, 0.638f);

                case EAXEnvironment.Alley:
                    return SetEAXParameters(Environment, 0.25f, 1.752f, 0.776f);

                case EAXEnvironment.Forest:
                    return SetEAXParameters(Environment, 0.111f, 3.145f, 0.472f);

                case EAXEnvironment.City:
                    return SetEAXParameters(Environment, 0.111f, 2.767f, 0.224f);

                case EAXEnvironment.Mountains:
                    return SetEAXParameters(Environment, 0.194f, 7.841f, 0.472f);

                case EAXEnvironment.Quarry:
                    return SetEAXParameters(Environment, 1, 1.499f, 0.5f);

                case EAXEnvironment.Plain:
                    return SetEAXParameters(Environment, 0.097f, 2.767f, 0.224f);

                case EAXEnvironment.ParkingLot:
                    return SetEAXParameters(Environment, 0.208f, 1.652f, 1.5f);

                case EAXEnvironment.SewerPipe:
                    return SetEAXParameters(Environment, 0.652f, 2.886f, 0.25f);

                case EAXEnvironment.Underwater:
                    return SetEAXParameters(Environment, 1, 1.499f, 0);

                case EAXEnvironment.Drugged:
                    return SetEAXParameters(Environment, 0.875f, 8.392f, 1.388f);

                case EAXEnvironment.Dizzy:
                    return SetEAXParameters(Environment, 0.139f, 17.234f, 0.666f);

                case EAXEnvironment.Psychotic:
                    return SetEAXParameters(Environment, 0.486f, 7.563f, 0.806f);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Sets the type of EAX environment and it's parameters.
        /// </summary>
        /// <param name="Environment">The EAX environment.</param>
        /// <param name="Volume">The volume of the reverb... 0.0 (off) - 1.0 (max), less than 0.0 = leave current.</param>
        /// <param name="Decay">The time in seconds it takes the reverb to diminish by 60dB... 0.1 (min) - 20.0 (max), less than 0.0 = leave current.</param>
        /// <param name="Damp">The damping, high or low frequencies decay faster... 0.0 = high decays quickest, 1.0 = low/high decay equally, 2.0 = low decays quickest, less than 0.0 = leave current.</param>
        /// <returns>
        /// If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NoEAX">The current device does not support EAX.</exception>
        /// <remarks>
        /// <para>
        /// The use of EAX requires that the output device supports EAX.
        /// <see cref="GetInfo" /> can be used to check that.
        /// EAX only affects 3D channels, but EAX functions do Not require <see cref="Apply3D" /> to apply the changes.
        /// </para>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// <para><b>Platform-specific</b></para>
        /// <para>This function is only available on Windows.</para>
        /// </remarks>
        /// <seealso cref="GetEAXParameters"/>
        /// <seealso cref="ChannelAttribute.EaxMix"/>
        [DllImport(DllName, EntryPoint = "BASS_SetEAXParameters")]
        public static extern bool SetEAXParameters(EAXEnvironment Environment, float Volume, float Decay, float Damp);
        #endregion

        #region Config
        /// <summary>
        /// Enable DirectSound's true play position mode on Windows Vista and newer? (default is true).
        /// </summary>
        /// <remarks>
        /// Unless this option is enabled, the reported playback position will advance in 10ms steps on Windows Vista and newer.
        /// As well as affecting the precision of <see cref="ChannelGetPosition"/>, this also affects the timing of non-mixtime syncs.
        /// When this option is enabled, it allows finer position reporting but it also increases latency.
        /// Changes only affect channels that are created afterwards, not any that already exist.
        /// The <see cref="BassInfo.Latency"/> and <see cref="BassInfo.MinBufferLength"/> values
        /// in the <see cref="BassInfo"/> structure reflect the setting at the time of the device's <see cref="Init"/> call.
        /// </remarks>
        public static bool VistaTruePlayPosition
        {
            get => GetConfigBool(Configuration.TruePlayPosition);
            set => Configure(Configuration.TruePlayPosition, value);
        }

        /// <summary>
        /// Include a "Default" entry in the output device list? (default is false).
        /// </summary>
        /// <remarks>
        /// BASS does not usually include a "Default" entry in its device list,
        /// as that would ultimately map to one of the other devices and be a duplicate entry.
        /// When the default device is requested in a <see cref="Init"/> call (with device = -1),
        /// BASS will check the default device at that time, and initialize it.
        /// But Windows 7 has the ability to automatically switch the default output to the new default device whenever it changes,
        /// and in order for that to happen, the default device (rather than a specific device) needs to be used.
        /// That is where this option comes in.
        /// When enabled, the "Default" device will also become the default device to <see cref="Init"/> (with device = -1).
        /// When the "Default" device is used, the <see cref="Volume"/> functions work a bit differently to usual;
        /// they deal with the "session" volume, which only affects the current process's output on the device, rather than the device's volume.
        /// This option can only be set before <see cref="GetDeviceInfo(int,out DeviceInfo)"/> or <see cref="Init"/> has been called.
        /// </remarks>
        public static bool IncludeDefaultDevice
        {
            get => GetConfigBool(Configuration.IncludeDefaultDevice);
            set => Configure(Configuration.IncludeDefaultDevice, value);
        }

        /// <summary>
        /// Enable speaker assignment with panning/balance control on Windows Vista and newer?
        /// </summary>
        /// <remarks>
        /// Panning/balance control via the <see cref="ChannelAttribute.Pan"/> attribute is not available
        /// when speaker assignment is used on Windows due to the way that the speaker assignment needs to be implemented there.
        /// The situation is improved with Windows Vista, and speaker assignment can generally
        /// be done in a way that does permit panning/balance control to be used at the same time,
        /// but there may still be some drivers that it does not work properly with,
        /// so it is disabled by default and can be enabled via this config option.
        /// Changes only affect channels that are created afterwards, not any that already exist.
        /// <para>
        /// <b>Platform-specific</b>: This config option is only available on Windows.
        /// It is available on all Windows versions (not including CE), but only has effect on Windows Vista and newer.
        /// Speaker assignment with panning/balance control is always possible on other platforms,
        /// where BASS generates the final mix.
        /// </para>
        /// </remarks>
        public static bool VistaSpeakerAssignment
        {
            get => GetConfigBool(Configuration.VistaSpeakerAssignment);
            set => Configure(Configuration.VistaSpeakerAssignment, value);
        }

        /// <summary>
        /// Gets or Sets the Unicode character set in device information.
        /// If true, device information will be in UTF-8 form.
        /// Otherwise it will be ANSI.
        /// </summary>
        /// <remarks>
        /// This config option determines what character set is used in the
        /// <see cref="DeviceInfo"/> structure and by the <see cref="RecordGetInputName"/> function.
        /// The default setting is ANSI, and it can only be changed before <see cref="GetDeviceInfo(int,out DeviceInfo)"/> or <see cref="Init"/>
        /// or <see cref="RecordGetDeviceInfo(int,out DeviceInfo)"/> or <see cref="RecordInit"/> has been called.
        /// <para><b>Platform-specific</b>: This config option is only available on Windows.</para>
        /// </remarks>
        public static bool UnicodeDeviceInformation
        {
            get => GetConfigBool(Configuration.UnicodeDeviceInformation);
            set => Configure(Configuration.UnicodeDeviceInformation, value);
        }

        /// <summary>
        /// Play the audio from video files using Media Foundation?
        /// </summary>
        /// <remarks>
        /// This config option is only available on Windows, and only has effect on Windows Vista and newer.
        /// </remarks>
        public static bool MFVideo
        {
            get => GetConfigBool(Configuration.MFVideo);
            set => Configure(Configuration.MFVideo, value);
        }

        /// <summary>
        /// UNDOCUMENTED: Disables Bass from setting system timer resolution.
        /// </summary>
        public static bool NoTimerResolution
        {
            get => GetConfigBool(Configuration.NoTimerResolution);
            set => Configure(Configuration.NoTimerResolution, value);
        }
        #endregion
    }
}
