using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    public static partial class Bass
    {
        /// <summary>
        /// Index of No Sound Device.
        /// </summary>
        public const int NoSoundDevice = 0;

        /// <summary>
        /// Index of Default Device.
        /// </summary>
        public const int DefaultDevice = -1;

        /// <summary>
        /// Initializes an output device.
        /// </summary>
        /// <param name="Device">The device to use... -1 = default device, 0 = no sound, 1 = first real output device.
        /// <see cref="GetDeviceInfo(int,out DeviceInfo)" /> or <see cref="DeviceCount" /> can be used to get the total number of devices.
        /// </param>
        /// <param name="Frequency">Output sample rate.</param>
        /// <param name="Flags">Any combination of <see cref="DeviceInitFlags"/>.</param>
        /// <param name="Win">The application's main window... <see cref="IntPtr.Zero" /> = the desktop window (use this for console applications).</param>
        /// <param name="ClsID">Class identifier of the object to create, that will be used to initialize DirectSound... <see langword="null" /> = use default</param>
        /// <returns>If the device was successfully initialized, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Device">The device number specified is invalid.</exception>
        /// <exception cref="Errors.Already">The device has already been initialized. You must call <see cref="Free()" /> before you can initialize it again.</exception>
        /// <exception cref="Errors.Driver">There is no available device driver... the device may already be in use.</exception>
        /// <exception cref="Errors.SampleFormat">The specified format is not supported by the device. Try changing the <paramref name="Frequency" /> and <paramref name="Flags" /> parameters.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">The device has no 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        /// <remarks>
        /// <para>This function must be successfully called before using any sample, stream or MOD music functions. The recording functions may be used without having called this function.</para>
        /// <para>Playback is not possible with the <see cref="NoSoundDevice"/> device, but it does allow the use of "decoding channels", eg. to decode files.</para>
        /// <para>When specifying a class identifier (<paramref name="ClsID"/>), after successful initialization, you can use GetDSoundObject(DSInterface) to retrieve the DirectSound object, and through that access any special interfaces that the object may provide.</para>
        /// <para>
        /// Simultaneously using multiple devices is supported in the BASS API via a context switching system - instead of there being an extra "device" parameter in the function calls, the device to be used is set prior to calling the functions. <see cref="CurrentDevice" /> is used to switch the current device.
        /// When successful, <see cref="Init"/> automatically sets the current thread's device to the one that was just initialized.
        /// </para>
        /// <para>
        /// When using the default device (device = -1), <see cref="CurrentDevice" /> can be used to find out which device it was mapped to.
        /// On Windows, it'll always be the first device.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// On Linux, a 'Default' device is hardcoded to device number 1, which uses the default output set in the ALSA config; that could map directly to one of the other devices or it could use ALSA plugins.
        /// If the IncludeDefaultDevice config option has been enbled, a "Default" device is also available on Windows, who's output will follow default device changes on Windows 7.
        /// In both cases, the "Default" device will also be the default device (device = -1).
        /// </para>
        /// <para>
        /// The sample format specified in the <paramref name="Frequency" /> and <paramref name="Flags" /> parameters has no effect on the device output on iOS or OSX, and not on Windows unless VxD drivers are used (on Windows 98/95);
        /// with WDM drivers (on Windows XP/2000/Me/98SE), the output format is automatically set depending on the format of what is played and what the device supports, while on Vista and above, the output format is determined by the user's choice in the Sound control panel.
        /// On Linux the output device will use the specified format if possible, but will otherwise use a format as close to it as possible.
        /// If the <see cref="DeviceInitFlags.Frequency"/> flag is specified on iOS or OSX, then the device's output rate will be set to the freq parameter (if possible).
        /// The <see cref="DeviceInitFlags.Frequency"/> flag has no effect on other platforms.
        /// <see cref="GetInfo" /> can be used to check what the output format actually is.
        /// </para>
        /// <para>
        /// The <paramref name="Win" /> and <paramref name="ClsID" /> parameters are only used on Windows and are ignored on other platforms.
        /// That applies to the <see cref="DeviceInitFlags.CPSpeakers"/> and <see cref="DeviceInitFlags.ForcedSpeakerAssignment"/> flags too, as the number of available speakers is always accurately detected on the other platforms.
        /// The <see cref="DeviceInitFlags.Latency"/> flag is also ignored on Linux/OSX/Android/Windows CE, as latency information is available without it.
        /// The latency is also available without it on iOS, but not immediately following this function call unless the flag is used.
        /// </para>
        /// <para>
        /// The DeviceInitFlags.DMix flag is only available on Linux, and allows multiple applications to share the device (if they all use 'dmix').
        /// It may also be possible for multiple applications to use exclusive access if the device is capable of hardware mixing.
        /// If exclusive access initialization fails, the DeviceInitFlags.DMix flag will automatically be tried;
        /// if that happens, it can be detected via <see cref="GetInfo" /> and the <see cref="BassInfo.InitFlags"/>.
        /// </para>
        /// <para>On Linux and Windows CE, the length of the device's buffer can be set via the <see cref="PlaybackBufferLength" /> config option.</para>
        /// </remarks>
        /// <seealso cref="Free()"/>
        /// <seealso cref="CPUUsage"/>
        /// <seealso cref="GetDeviceInfo(int, out DeviceInfo)"/>
        /// <seealso cref="GetInfo(out BassInfo)"/>
        /// <seealso cref="MusicLoad(string, long, int, BassFlags, int)"/>
        /// <seealso cref="CreateSample"/>
        /// <seealso cref="SampleLoad(string, long, int, int, BassFlags)"/>
        [DllImport(DllName, EntryPoint = "BASS_Init")]
        public static extern bool Init(int Device = DefaultDevice, int Frequency = 44100, DeviceInitFlags Flags = DeviceInitFlags.Default, IntPtr Win = default(IntPtr), IntPtr ClsID = default(IntPtr));

        /// <summary>
        /// Starts (or resumes) the output.
        /// </summary>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Init"><see cref="Init"/> has not been successfully called.</exception>
        /// <remarks>
        /// <para>The output is automatically started by <see cref="Init"/>, so there is no need to use this function unless you have stopped or paused the output.</para>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice"/>) determines which device this function call applies to.</para>
        /// </remarks>
        /// <seealso cref="Pause"/>
        /// <seealso cref="Stop"/>
        [DllImport(DllName, EntryPoint = "BASS_Start")]
        public static extern bool Start();

        /// <summary>
        /// Stops the output, pausing all musics/samples/streams.
        /// </summary>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>Use <see cref="Start" /> to resume the output and paused channels.</para>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Pause")]
        public static extern bool Pause();

        /// <summary>
        /// Stops the output, stopping all musics/samples/streams.
        /// </summary>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>This function can be used after <see cref="Pause" /> to stop the paused channels, so that they will not be resumed the next time <see cref="Start" /> is called.</para>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Stop")]
        public static extern bool Stop();

        /// <summary>
        /// Frees all resources used by the output device, including all it's samples, streams, and MOD musics.
        /// </summary>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>This function should be called for all initialized devices before your program exits. It's not necessary to individually free the samples/streams/musics as these are all automatically freed by this function.</para>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Free")]
        public static extern bool Free();

        /// <summary>
        /// Retrieves the device that the channel is using.
        /// </summary>
        /// <param name="Handle">The channel handle... a HCHANNEL, HMUSIC, HSTREAM, or HRECORD. HSAMPLE handles may also be used.</param>
        /// <returns>If successful, the device number is returned, else -1 is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Recording devices are indicated by the HIWORD of the return value being 1, when this function is called with a HRECORD channel.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelGetDevice")]
        public static extern int ChannelGetDevice(int Handle);

        /// <summary>
        /// Changes the device that a stream, MOD music or sample is using.
        /// </summary>
        /// <param name="Handle">The channel or sample handle... only HMUSIC, HSTREAM or HSAMPLE are supported.</param>
        /// <param name="Device">The device to use...0 = no sound, 1 = first real output device.</param>
        /// <returns>If succesful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// All of the channel's current settings are carried over to the new device, but if the channel is using the "with FX flag" DX8 effect implementation,
        /// the internal state (eg. buffers) of the DX8 effects will be reset. Using the "without FX flag" DX8 effect implementation, the state of the DX8 effects is preserved.
        /// <para>
        /// When changing a sample's device, all the sample's existing channels (HCHANNELs) are freed.
        /// It's not possible to change the device of an individual sample channel.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not a valid channel.</exception>
        /// <exception cref="Errors.Device"><paramref name="Device" /> is invalid.</exception>
        /// <exception cref="Errors.Init">The requested device has not been initialized.</exception>
        /// <exception cref="Errors.Already">The channel is already using the requested device.</exception>
        /// <exception cref="Errors.NotAvailable">Only decoding channels are allowed to use the <see cref="NoSoundDevice"/> device.</exception>
        /// <exception cref="Errors.SampleFormat">
        /// The sample format is not supported by the device/drivers.
        /// If the channel is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.
        /// </exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_ChannelSetDevice")]
        public static extern bool ChannelSetDevice(int Handle, int Device);

        /// <summary>
        /// Gets the number of Playback Devices available.
        /// </summary>
        public static int DeviceCount
        {
            get
            {
                int i;
                for (i = 0; GetDeviceInfo(i, out var info); i++) { }

                return i;
            }
        }

        #region Current Device Volume
        [DllImport(DllName)]
        static extern float BASS_GetVolume();

        [DllImport(DllName)]
        static extern bool BASS_SetVolume(float volume);

        /// <summary>
        /// Gets or sets the current output master volume level... 0 (silent) to 1 (max).
        /// </summary>
        /// <remarks>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// <para>A return value of -1 indicates error. Use <see cref="LastError" /> to get the error code. Throws <see cref="BassException"/> on Error while setting value.</para>
        /// <para>The actual volume level may not be exactly the same as set, due to underlying precision differences.</para>
        /// <para>
        /// This function affects the volume level of all applications using the same output device.
        /// If you wish to only affect the level of your app's sounds, <see cref="ChannelSetAttribute(int, ChannelAttribute, float)" />
        /// and/or the <see cref="GlobalMusicVolume"/>, <see cref="GlobalSampleVolume"/> and <see cref="GlobalStreamVolume"/> config options should be used instead.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">There is no volume control when using the <see cref="NoSoundDevice">No Sound Device</see>.</exception>
        /// <exception cref="Errors.Parameter">Invalid volume.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static double Volume
        {
            get => BASS_GetVolume();
            set
            {
                if (!BASS_SetVolume((float)value))
                    throw new BassException();
            }
        }
        #endregion

        #region Current Device
        [DllImport(DllName)]
        static extern int BASS_GetDevice();

        [DllImport(DllName)]
        static extern bool BASS_SetDevice(int Device);

        /// <summary>
        /// Gets or sets the device setting of the current thread... 0 = no sound, 1 = first real output device.
        /// </summary>
        /// <remarks>
        /// <para>A return value of -1 indicates error. Use <see cref="LastError" /> to get the error code. Throws <see cref="BassException"/> on Error while setting value.</para>
        /// <para>
        /// Simultaneously using multiple devices is supported in the BASS API via a context switching system -
        /// instead of there being an extra "device" parameter in the function calls, the device to be used is set prior to calling the functions.
        /// The device setting is local to the current thread, so calling functions with different devices simultaneously in multiple threads is not a problem.
        /// </para>
        /// <para>The functions that use the device selection are the following:
        /// <see cref="Free()" />, GetDSoundObject(int), <see cref="GetInfo" />, <see cref="Start" />, <see cref="Stop" />, <see cref="Pause" />, <see cref="Volume" />, <see cref="Set3DFactors" />, <see cref="Get3DFactors" />, <see cref="Set3DPosition" />, <see cref="Get3DPosition" />, SetEAXParameters, GetEAXParameters.
        /// It also determines which device is used by a new sample/stream/music: <see cref="MusicLoad(string,long,int,BassFlags,int)" />, <see cref="SampleLoad(string,long,int,int,BassFlags)" />, <see cref="CreateStream(string,long,long,BassFlags)" />, etc...
        /// </para>
        /// <para>
        /// When one of the above functions is called, BASS will check the current thread's device setting, and if no device is selected (or the selected device is not initialized), BASS will automatically select the lowest device that is initialized.
        /// This means that when using a single device, there is no need to use this function;
        /// BASS will automatically use the device that is initialized.
        /// Even if you free the device, and initialize another, BASS will automatically switch to the one that is initialized.
        /// </para>
        /// </remarks>
        public static int CurrentDevice
        {
            get => BASS_GetDevice();
            set
            {
                if (!BASS_SetDevice(value))
                    throw new BassException();
            }
        }
        #endregion

        #region Get Device Info
        /// <summary>
        /// Retrieves information on an output device.
        /// </summary>
        /// <param name="Device">The device to get the information of... 0 = first.</param>
        /// <param name="Info">A <see cref="DeviceInfo" /> object to retrieve the information into.</param>
        /// <returns>
        /// If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <remarks>
        /// This function can be used to enumerate the available devices for a setup dialog.
        /// Device 0 is always the <see cref="NoSoundDevice"/> device, so if you should start at device 1 if you only want to list real devices.
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// On Linux, a "Default" device is hardcoded to device number 1, which uses the default output set in the ALSA config, and the real devices start at number 2.
        /// That is also the case on Windows when the IncludeDefaultDevice option is enabled.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Device">The device number specified is invalid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_GetDeviceInfo")]
        public static extern bool GetDeviceInfo(int Device, out DeviceInfo Info);

        /// <summary>
        /// Retrieves information on an output device.
        /// </summary>
        /// <param name="Device">The device to get the information of... 0 = first.</param>
        /// <returns>An instance of the <see cref="DeviceInfo" /> structure is returned. Throws <see cref="BassException"/> on Error.</returns>
        /// <remarks>
        /// This function can be used to enumerate the available devices for a setup dialog.
        /// Device 0 is always the <see cref="NoSoundDevice"/> device, so if you should start at device 1 if you only want to list real devices.
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// On Linux, a "Default" device is hardcoded to device number 1, which uses the default output set in the ALSA config, and the real devices start at number 2.
        /// That is also the case on Windows when the IncludeDefaultDevice option is enabled.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Device">The device number specified is invalid.</exception>
        public static DeviceInfo GetDeviceInfo(int Device)
        {
            if (!GetDeviceInfo(Device, out var info))
                throw new BassException();
            return info;
        }
        #endregion

        #region Info
        /// <summary>
        /// Retrieves information on the device being used.
        /// </summary>
        /// <param name="Info"><see cref="BassInfo"/> object to receive the information.</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Init"><see cref="Init"/> has not been successfully called.</exception>
        /// <remarks>
        /// When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice"/>) determines which device this function call applies to.
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_GetInfo")]
        public static extern bool GetInfo(out BassInfo Info);

        /// <summary>
        /// Retrieves information on the device being used.
        /// </summary>
        /// <returns><see cref="BassInfo"/> structure with the retreived information. Throws <see cref="BassException"/> on Error.</returns>
        /// <exception cref="Errors.Init"><see cref="Init"/> has not been successfully called.</exception>
        /// <remarks>
        /// When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice"/>) determines which device this function call applies to.
        /// </remarks>
        public static BassInfo Info
        {
            get
            {
                if (!GetInfo(out var info))
                    throw new BassException();
                return info;
            }
        }
        #endregion
    }
}
