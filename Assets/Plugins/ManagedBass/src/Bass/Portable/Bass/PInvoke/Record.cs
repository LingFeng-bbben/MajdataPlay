using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    public static partial class Bass
    {
        /// <summary>
        /// Initializes a recording device.
        /// </summary>
        /// <param name="Device">The device to use... -1 = default device, 0 = first. <see cref="RecordGetDeviceInfo(int, out DeviceInfo)" /> or <see cref="RecordingDeviceCount" /> can be used to get the total number of devices.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError"/> to get the error code.</returns>
        /// <remarks>
        /// This function must be successfully called before using the recording features.
        /// <para>
        /// Simultaneously using multiple devices is supported in the BASS API via a context switching system - instead of there being an extra "device" parameter in the function calls, the device to be used is set prior to calling the functions.
        /// <see cref="CurrentRecordingDevice" /> is used to switch the current recording device.
        /// When successful, <see cref="RecordInit" /> automatically sets the current thread's device to the one that was just initialized
        /// </para>
        /// <para>
        /// When using the default device (device = -1), <see cref="CurrentRecordingDevice" /> can be used to find out which device it was mapped to.
        /// On Windows, it'll always be the first device.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// Recording support requires DirectX 5 (or above) on Windows.
        /// On Linux, a "Default" device is hardcoded to device number 0, which uses the default input set in the ALSA config;
        /// that could map directly to one of the other devices or it could use ALSA plugins.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.DirectX">A sufficient version of DirectX is not installed.</exception>
        /// <exception cref="Errors.Device"><paramref name="Device" /> is invalid.</exception>
        /// <exception cref="Errors.Already">The device has already been initialized. <see cref="RecordFree" /> must be called before it can be initialized again.</exception>
        /// <exception cref="Errors.Driver">There is no available device driver.</exception>
        [DllImport(DllName, EntryPoint = "BASS_RecordInit")]
        public static extern bool RecordInit(int Device = DefaultDevice);

        /// <summary>
        /// Frees all resources used by the recording device.
        /// </summary>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>This function should be called for all initialized recording devices before your program exits.</para>
        /// <para>When using multiple recording devices, the current thread's device setting (as set with <see cref="CurrentRecordingDevice" />) determines which device this function call applies to.</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="RecordInit" /> has not been successfully called - there are no initialized devices.</exception>
        [DllImport(DllName, EntryPoint = "BASS_RecordFree")]
        public static extern bool RecordFree();

        #region RecordStart
        [DllImport(DllName)]
        static extern int BASS_RecordStart(int freq, int chans, BassFlags flags, RecordProcedure proc, IntPtr User);

        /// <summary>
        /// Starts recording.
        /// </summary>
        /// <param name="Frequency">The sample rate to record at.</param>
        /// <param name="Channels">The number of channels... 1 = mono, 2 = stereo, etc.</param>
        /// <param name="Flags">Any combination of <see cref="BassFlags.Byte"/>, <see cref="BassFlags.Float"/> and <see cref="BassFlags.RecordPause"/>.</param>
        /// <param name="Procedure">The user defined function to receive the recorded sample data... can be <see langword="null" /> if you do not wish to use a callback.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>If successful, the new recording's handle is returned, else <see langword="false" /> is returned. Use <see cref="LastError"/> to get the error code.</returns>
        /// <remarks>
        /// Use <see cref="ChannelStop" /> to stop the recording, and <see cref="ChannelPause" /> to pause it.
        /// Recording can also be started in a paused state (via the <see cref="BassFlags.RecordPause"/> flag), allowing DSP/FX to be set on it before any data reaches the callback function.
        /// <para>The sample data will generally arrive from the recording device in blocks rather than in a continuous stream, so when specifying a very short period between callbacks, some calls may be skipped due to there being no new data available since the last call.</para>
        /// <para>
        /// When not using a callback (proc = <see langword="null" />), the recorded data is instead retrieved via <see cref="ChannelGetData(int, IntPtr, int)" />.
        /// To keep latency at a minimum, the amount of data in the recording buffer should be monitored (also done via <see cref="ChannelGetData(int, IntPtr, int)" />, with the <see cref="DataFlags.Available"/> flag) to check that there is not too much data;
        /// freshly recorded data will only be retrieved after the older data in the buffer is.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// Multiple simultaneous recordings can be made from the same device on Windows XP and later, but generally not on older Windows.
        /// Multiple simultaneous recordings are possible on iOS and OSX, but may not always be on Linux or Windows CE.
        /// On OSX and iOS, the device is instructed (when possible) to deliver data at the period set in the HIWORD of flags, even when a callback function is not used.
        /// On other platforms, it is up the the system when data arrives from the device.
        /// </para>
        /// <para>
        /// Unlike Bass.Net, a reference to <paramref name="Procedure"/> doesn't need to be held by you manually.
        /// ManagedBass automatically holds a reference and frees it when the Channel is freed.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="RecordInit" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Busy">
        /// The device is busy.
        /// An existing recording must be stopped before starting another one.
        /// Multiple simultaneous recordings can be made from the same device on Windows XP and Vista, but generally not on older Windows.
        /// </exception>
        /// <exception cref="Errors.NotAvailable">
        /// The recording device is not available.
        /// Another application may already be recording with it, or it could be a half-duplex device and is currently being used for playback.
        /// </exception>
        /// <exception cref="Errors.SampleFormat">
        /// The specified format is not supported.
        /// If using the <see cref="BassFlags.Float"/> flag, it could be that floating-point recording is not supported.
        /// </exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int RecordStart(int Frequency, int Channels, BassFlags Flags, RecordProcedure Procedure, IntPtr User = default(IntPtr))
        {
            var h = BASS_RecordStart(Frequency, Channels, Flags, Procedure, User);

            if (h != 0)
                ChannelReferences.Add(h, 0, Procedure);

            return h;
        }

        /// <summary>
        /// Starts recording.
        /// </summary>
        /// <param name="Frequency">The sample rate to record at.</param>
        /// <param name="Channels">The number of channels... 1 = mono, 2 = stereo.</param>
        /// <param name="Flags">Any combination of <see cref="BassFlags.Byte"/>, <see cref="BassFlags.Float"/> and <see cref="BassFlags.RecordPause"/>.</param>
        /// <param name="Period">
        /// Set the period (in milliseconds) between calls to the callback function (<see cref="RecordProcedure" />).
        /// The minimum period is 5ms, the maximum the maximum is half the <see cref="RecordingBufferLength"/> setting.
        /// If the period specified is outside this range, it is automatically capped. The default is 100ms.
        /// </param>
        /// <param name="Procedure">The user defined function to receive the recorded sample data... can be <see langword="null" /> if you do not wish to use a callback.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>If successful, the new recording's handle is returned, else <see langword="false" /> is returned. Use <see cref="LastError"/> to get the error code.</returns>
        /// <exception cref="Errors.Init"><see cref="RecordInit" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Busy">
        /// The device is busy.
        /// An existing recording must be stopped before starting another one.
        /// Multiple simultaneous recordings can be made from the same device on Windows XP and Vista, but generally not on older Windows.
        /// </exception>
        /// <exception cref="Errors.NotAvailable">
        /// The recording device is not available.
        /// Another application may already be recording with it, or it could be a half-duplex device and is currently being used for playback.
        /// </exception>
        /// <exception cref="Errors.SampleFormat">
        /// The specified format is not supported.
        /// If using the <see cref="BassFlags.Float"/> flag, it could be that floating-point recording is not supported.
        /// </exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int RecordStart(int Frequency, int Channels, BassFlags Flags, int Period, RecordProcedure Procedure, IntPtr User = default(IntPtr))
        {
            return RecordStart(Frequency, Channels, (BassFlags)BitHelper.MakeLong((short)Flags, (short)Period), Procedure, User);
        }
        #endregion

        #region Current Recording Device
        [DllImport(DllName)]
        static extern int BASS_RecordGetDevice();

        [DllImport(DllName)]
        static extern bool BASS_RecordSetDevice(int Device);

        /// <summary>
        /// Gets or Sets the recording device setting in the current thread... 0 = first recording device.
        /// </summary>
        /// <remarks>
        /// <para>A value of -1 indicates error. Use <see cref="LastError" /> to get the error code.  Throws <see cref="BassException"/> on Error while setting value.</para>
        /// <para>Simultaneously using multiple devices is supported in the BASS API via a context switching system - instead of there being an extra "device" parameter in the function calls, the device to be used is set prior to calling the functions. The device setting is local to the current thread, so calling functions with different devices simultaneously in multiple threads is not a problem.</para>
        /// <para>The functions that use the recording device selection are the following:
        /// <see cref="RecordFree" />, <see cref="RecordGetInfo(out RecordInfo)" />, <see cref="RecordGetInput(int, out float)" />, <see cref="RecordGetInputName(int)" />, <see cref="RecordSetInput(int, InputFlags, float)" />, <see cref="RecordStart(int, int, BassFlags, RecordProcedure, IntPtr)" />.</para>
        /// <para>When one of the above functions is called, BASS will check the current thread's recording device setting, and if no device is selected (or the selected device is not initialized), BASS will automatically select the lowest device that is initialized.
        /// This means that when using a single device, there is no need to use this function - BASS will automatically use the device that's initialized.
        /// Even if you free the device, and initialize another, BASS will automatically switch to the one that is initialized.</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="RecordInit" /> has not been successfully called - there are no initialized.</exception>
        /// <exception cref="Errors.Device">Specified device number is invalid.</exception>
        /// <seealso cref="RecordInit"/>
        public static int CurrentRecordingDevice
        {
            get => BASS_RecordGetDevice();
            set
            {
                if (!BASS_RecordSetDevice(value))
                    throw new BassException();
            }
        }
        #endregion

        #region Record Get Device Info
        /// <summary>
        /// Retrieves information on a recording device.
        /// </summary>
        /// <param name="Device">The device to get the information of... 0 = first.</param>
        /// <param name="Info">A <see cref="DeviceInfo" /> object to retreive the information into.</param>
        /// <returns>
        /// If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <remarks>
        /// This function can be used to enumerate the available recording devices for a setup dialog.
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// Recording support requires DirectX 5 (or above) on Windows.
        /// On Linux, a "Default" device is hardcoded to device number 0, which uses the default input set in the ALSA config.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Device">The device number specified is invalid.</exception>
        /// <exception cref="Errors.DirectX">A sufficient version of DirectX is not installed.</exception>
        [DllImport(DllName, EntryPoint = "BASS_RecordGetDeviceInfo")]
        public static extern bool RecordGetDeviceInfo(int Device, out DeviceInfo Info);

        /// <summary>
        /// Retrieves information on a recording device.
        /// </summary>
        /// <param name="Device">The device to get the information of... 0 = first.</param>
        /// <returns>An instance of the <see cref="DeviceInfo" /> structure is returned.A value of -1 indicates error. Use <see cref="LastError" /> to get the error code. Throws <see cref="BassException"/> on Error.</returns>
        /// <remarks>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// Recording support requires DirectX 5 (or above) on Windows.
        /// On Linux, a "Default" device is hardcoded to device number 0, which uses the default input set in the ALSA config.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Device">The device number specified is invalid.</exception>
        /// <exception cref="Errors.DirectX">A sufficient version of DirectX is not installed.</exception>
        public static DeviceInfo RecordGetDeviceInfo(int Device)
        {
            if (!RecordGetDeviceInfo(Device, out var info))
                throw new BassException();
            return info;
        }
        #endregion

        #region Record Get Info
        /// <summary>
        /// Retrieves information on the recording device being used.
        /// </summary>
        /// <param name="info">A <see cref="RecordInfo" /> object to retrieve the information into.</param>
        /// <returns>
        /// If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <exception cref="Errors.Init"><see cref="RecordInit" /> has not been successfully called - there are no initialized devices.</exception>
        [DllImport(DllName, EntryPoint = "BASS_RecordGetInfo")]
        public static extern bool RecordGetInfo(out RecordInfo info);

        /// <summary>
        /// Retrieves information on the recording device being used.
        /// </summary>
        /// <returns>An instance of the <see cref="RecordInfo" /> structure is returned. Throws <see cref="BassException"/> on Error.</returns>
        /// <exception cref="Errors.Init"><see cref="RecordInit" /> has not been successfully called - there are no initialized devices.</exception>
        public static RecordInfo RecordingInfo
        {
            get
            {
                if (!RecordGetInfo(out var info))
                    throw new BassException();
                return info;
            }
        }
        #endregion

        /// <summary>
        /// The Buffer Length for recording channels in milliseconds... 1000 (min) - 5000 (max). default = 2000.
        /// </summary>
        /// <remarks>
        /// If the Length specified is outside this range, it is automatically capped.
        /// Unlike a playback Buffer, where the aim is to keep the Buffer full, a recording
        /// Buffer is kept as empty as possible and so this setting has no effect on latency.
        /// Unless processing of the recorded data could cause significant delays, or you want to
        /// use a large recording period with <see cref="RecordStart(int, int, BassFlags, RecordProcedure, IntPtr)"/>, there should be no need to increase this.
        /// Using this config option only affects the recording channels that are created afterwards,
        /// not any that have already been created.
        /// So you can have channels with differing Buffer lengths by using this config option each time before creating them.
        /// </remarks>
        public static int RecordingBufferLength
        {
            get => GetConfig(Configuration.RecordingBufferLength);
            set => Configure(Configuration.RecordingBufferLength, value);
        }

        /// <summary>
        /// No of Recording devices available.
        /// </summary>
        public static int RecordingDeviceCount
        {
            get
            {
                int i;
                for (i = 0; RecordGetDeviceInfo(i, out var info); i++) { }

                return i;
            }
        }

        /// <summary>
        /// Retrieves the settings of a recording input source.
        /// </summary>
        /// <param name="Input">The input to get the settings of... 0 = first, -1 = master.</param>
        /// <param name="Volume">Reference to a variable to receive the current volume.</param>
        /// <returns>
        /// If an error occurs, -1 is returned, use <see cref="LastError" /> to get the error code.
        /// If successful, then the settings are returned.
        /// The <see cref="InputFlags.Off"/> flag will be set if the input is disabled, otherwise the input is enabled.
        /// The type of input (see <see cref="InputTypeFlags" />) is also indicated in the high 8-bits.
        /// Use <see cref="InputTypeFlags.InputTypeMask"/> to test the return value.
        /// If the volume is requested but not available, volume will receive -1.
        /// </returns>
        /// <remarks>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// The input type information is only available on Windows.
        /// There is no "what you hear" type of input defined;
        /// if the device has one, it will typically come under <see cref="InputTypeFlags.Analog"/> or <see cref="InputTypeFlags.Undefined"/>.
        /// </para>
        /// <para>On OSX, there is no master input (-1), and only the currently enabled input has its volume setting available (if it has a volume control).</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="RecordInit" /> has not been successfully called - there are no initialized devices.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Input" /> is invalid.</exception>
        /// <exception cref="Errors.NotAvailable">A master input is not available.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_RecordGetInput")]
        public static extern int RecordGetInput(int Input, out float Volume);

        [DllImport(DllName)]
        static extern int BASS_RecordGetInput(int Input, IntPtr Volume);

        /// <summary>
        /// Retrieves the settings of a recording input source (does not retrieve Volume).
        /// </summary>
        /// <param name="Input">The input to get the settings of... 0 = first, -1 = master.</param>
        /// <returns>
        /// If an error occurs, -1 is returned, use <see cref="LastError" /> to get the error code.
        /// If successful, then the settings are returned.
        /// The <see cref="InputFlags.Off"/> flag will be set if the input is disabled, otherwise the input is enabled.
        /// The type of input (see <see cref="InputTypeFlags" />) is also indicated in the high 8-bits.
        /// Use <see cref="InputTypeFlags.InputTypeMask"/> to test the return value.
        /// If the volume is requested but not available, volume will receive -1.
        /// </returns>
        /// <remarks>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// The input type information is only available on Windows.
        /// There is no "what you hear" type of input defined;
        /// if the device has one, it will typically come under <see cref="InputTypeFlags.Analog"/> or <see cref="InputTypeFlags.Undefined"/>.
        /// </para>
        /// <para>On OSX, there is no master input (-1), and only the currently enabled input has its volume setting available (if it has a volume control).</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="RecordInit" /> has not been successfully called - there are no initialized devices.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Input" /> is invalid.</exception>
        /// <exception cref="Errors.NotAvailable">A master input is not available.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int RecordGetInput(int Input) => BASS_RecordGetInput(Input, IntPtr.Zero);

        [DllImport(DllName)]
        static extern IntPtr BASS_RecordGetInputName(int input);

        /// <summary>
        /// Retrieves the text description of a recording input source.
        /// </summary>
        /// <param name="Input">The input to get the description of... 0 = first, -1 = master.</param>
        /// <returns>If succesful, then the description is returned, else <see langword="null" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// The returned string is in ANSI or UTF-8 form on Windows, depending on the UnicodeDeviceInformation setting.
        /// It is in UTF-16 form ("WCHAR" rather than "char") on Windows CE, and in UTF-8 form on other platforms.
        /// </para>
        /// <para>On OSX, there is no master input (-1).</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="RecordInit" /> has not been successfully called - there are no initialized devices.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Input" /> is invalid.</exception>
        /// <exception cref="Errors.NotAvailable">A master input is not available.</exception>
        public static string RecordGetInputName(int Input)
        {
            var ptr = BASS_RecordGetInputName(Input);

            if (ptr == IntPtr.Zero)
                return null;

            var unicodeWin = GetConfig(Configuration.UnicodeDeviceInformation);

            switch (unicodeWin)
            {
                case -1:
                    return Extensions.PtrToStringUtf8(ptr);

                case 0:
                    return Marshal.PtrToStringAnsi(ptr);

                default:
                    return Extensions.PtrToStringUtf8(ptr);
            }
        }

        /// <summary>
        /// Adjusts the settings of a recording input source.
        /// </summary>
        /// <param name="Input">The input to adjust the settings of... 0 = first, -1 = master.</param>
        /// <param name="Setting">The new setting... a combination of <see cref="InputFlags"/>.</param>
        /// <param name="Volume">The volume level... 0 (silent) to 1 (max), less than 0 = leave current.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// The actual volume level may not be exactly the same as requested, due to underlying precision differences.
        /// <see cref="RecordGetInput(int, out float)" /> can be used to confirm what the volume is.
        /// </para>
        /// <para>The volume curve used by this function is always linear, the <see cref="LogarithmicVolumeCurve"/> config option setting has no effect on this.</para>
        /// <para>Changes made by this function are system-wide, ie. other software using the device will be affected by it.</para>
        /// <para><b>Platform-specific</b></para>
        /// <para>On OSX, there is no master input (-1), and only the currently enabled input has its volume setting available (if it has a volume control).</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="RecordInit" /> has not been successfully called - there are no initialized devices.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Input" /> or <paramref name="Volume" /> is invalid.</exception>
        /// <exception cref="Errors.NotAvailable">The soundcard/driver doesn't allow you to change the input or it's volume.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_RecordSetInput")]
        public static extern bool RecordSetInput(int Input, InputFlags Setting, float Volume);
    }
}
