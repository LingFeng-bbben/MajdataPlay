using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Wasapi
{
    /// <summary>
    /// Wraps basswasapi.dll: Windows Audio Session API driver library
    /// </summary>
    /// <remarks>
    /// <para>BASSWASAPI is basically a wrapper for Windows Audio Session API drivers, with the addition of channel joining, format conversion and resampling.</para>
    /// <para>
    /// BASSWASAPI requires a soundcard with a Windows Session API drivers installed (Vista or above).
    /// It also makes use of SSE2 and 3DNow optimizations, but is fully functional without them.
    /// BASS is not required by BASSWASAPI, but BASS can of course be used to decode, apply DSP/FX, etc.
    /// </para>
    /// </remarks>
    public static class BassWasapi
    {
        const string DllName = "basswasapi";
        
        /// <summary>
        /// Identifier for Default Device.
        /// </summary>
        public const int DefaultDevice = -1;

        /// <summary>
        /// Identifier for Default Recording Device.
        /// </summary>
        public const int DefaultInputDevice = -2;
        
        /// <summary>
        /// Identifier for Default Loopback Device.
        /// </summary>
        public const int DefaultLoopbackDevice = -3;

        /// <summary>
        /// Instead of BASSWASAPI pulling data from a WASAPIPROC function, data is pushed to
        /// BASSWASAPI via BASS_WASAPI_PutData. This cannot be used with input devices or the
        /// BASS_WASAPI_EVENT flag. 
        /// </summary>
        public static readonly IntPtr WasapiProc_Push = new IntPtr(0);

        /// <summary>
        /// Feed data to/from a BASS channel, specified in the user parameter.
        /// It must be a decoding channel (using BASS_STREAM_DECODE) for an output device,
        /// or a "push" or "dummy" stream (using STREAMPROC_PUSH or STREAMPROC_DUMMY) for
        /// an input device. The freq and chans parameters are ignored and the sample format
        /// of the BASS channel is used instead, but it must be floating-point (BASS_SAMPLE_FLOAT).  
        /// </summary>
        public static readonly IntPtr WasapiProc_Bass = new IntPtr(-1);

        #region CPU
        [DllImport(DllName)]
        static extern float BASS_WASAPI_GetCPU();

        /// <summary>
        /// Retrieves the current CPU usage of BASSWASAPI.
        /// </summary>
        /// <returns>The BASSWASAPI CPU usage as a percentage of total CPU time.</returns>
        /// <remarks>This function includes the time taken by the <see cref="WasapiProcedure" /> callback functions.</remarks>
        public static double CPUUsage => BASS_WASAPI_GetCPU();
        #endregion

        #region Current Device
        [DllImport(DllName)]
        static extern int BASS_WASAPI_GetDevice();

        [DllImport(DllName)]
        static extern bool BASS_WASAPI_SetDevice(int device);

		/// <summary>
		/// Gets or Sets the Wasapi device to use for susequent calls in the current thread... 0 = first device. Use <see cref="Bass.LastError" /> to get the error code.
		/// </summary>
		/// <remarks>
        /// <para>Throws <see cref="BassException"/> on Error setting value.</para>
		/// <para>
        /// Simultaneously using multiple devices is supported in the BASS API via a context switching system;
        /// instead of there being an extra "device" parameter in the function calls, the device to be used is set prior to calling the functions.
        /// The device setting is local to the current thread, so calling functions with different devices simultaneously in multiple threads is not a problem.
        /// </para>
		/// <para>
        /// All of the BassWasapi functions that do not have their own "device" parameter make use of this device selection.
        /// When one of them is called, BassWasapi will check the current thread's device setting, and if no device is selected (or the selected device is not initialized), BassWasapi will automatically select the lowest device that is initialized.
        /// This means that when using a single device, there is no need to use this function;
        /// BassWasapi will automatically use the device that is initialized.
        /// Even if you free the device, and initialize another, BassWasapi will automatically switch to the one that is initialized.
        /// </para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Device">The device number specified is invalid.</exception>
        public static int CurrentDevice
        {
            get => BASS_WASAPI_GetDevice();
		    set 
            { 
                if (!BASS_WASAPI_SetDevice(value))
                    throw new BassException(); 
            }
        }
        #endregion

        #region GetDeviceInfo
        /// <summary>
		/// Retrieves information on a Wasapi device (endpoint).
		/// </summary>
		/// <param name="Device">The device to get the information of... 0 = first.</param>
		/// <param name="Info">An instance of the <see cref="WasapiDeviceInfo" /> class to store the information at.</param>
		/// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// This function can be used to enumerate the available Wasapi devices (endpoints) for a setup dialog. 
		/// <para>Note: Input (capture) devices can be determined by evaluating <see cref="WasapiDeviceInfo.IsInput"/> and <see cref="WasapiDeviceInfo.IsLoopback"/> members.</para>
		/// </remarks>
		/// <exception cref="Errors.Wasapi">WASAPI is not available</exception>
		/// <exception cref="Errors.Device">The device number specified is invalid.</exception>
		[DllImport(DllName, EntryPoint = "BASS_WASAPI_GetDeviceInfo")]
        public static extern bool GetDeviceInfo(int Device, out WasapiDeviceInfo Info);

        /// <summary>
        /// Retrieves information on a Wasapi device (endpoint).
        /// </summary>
        /// <param name="Device">The device to get the information of... 0 = first.</param>
        /// <returns>An instance of <see cref="WasapiDeviceInfo"/> structure is returned. Throws <see cref="BassException"/> on Error.</returns>
        /// <remarks>
        /// This function can be used to enumerate the available Wasapi devices (endpoints) for a setup dialog. 
        /// <para>Note: Input (capture) devices can be determined by evaluating <see cref="WasapiDeviceInfo.IsInput"/> and <see cref="WasapiDeviceInfo.IsLoopback"/> members.</para>
        /// </remarks>
        /// <exception cref="Errors.Wasapi">WASAPI is not available</exception>
        /// <exception cref="Errors.Device">The device number specified is invalid.</exception>
        public static WasapiDeviceInfo GetDeviceInfo(int Device)
        {
            if (!GetDeviceInfo(Device, out var info))
                throw new BassException();
            return info;
        }
        #endregion

		/// <summary>
		/// Sets a device change notification callback.
		/// </summary>
		/// <param name="Procedure">User defined notification function... <see langword="null" /> = disable notifications.</param>
		/// <param name="User">User instance data to pass to the callback function.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>A previously set notification callback can be changed (or removed) at any time, by calling this function again.</remarks>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_SetNotify")]
        public static extern bool SetNotify(WasapiNotifyProcedure Procedure, IntPtr User = default(IntPtr));
                
		/// <summary>
		/// Gets the total number of available Wasapi devices.
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

        /// <summary>
        /// Checks if a particular sample format is supported by a device (endpoint).
        /// </summary>
        /// <param name="Device">The device to use... 0 = first device, -1 = default device, -2 = default input device. <see cref="GetDeviceInfo(int,out WasapiDeviceInfo)" /> can be used to enumerate the available devices.</param>
        /// <param name="Frequency">The sample rate to check.</param>
        /// <param name="Channels">The number of channels to check... 1 = mono, 2 = stereo, etc.</param>
        /// <param name="Flags">
        /// Any combination of <see cref="WasapiInitFlags.Shared"/> and <see cref="WasapiInitFlags.Exclusive"/>.
        /// The HIWORD can be used to limit the sample formats that are checked in exclusive mode.
        /// The default is to check 32-bit floating-point, 32-bit integer, 24-bit integer, 16-bit integer, 8-bit integer, in that order.
        /// A <see cref="WasapiFormat"/> value can be used to bypass the formats that precede it in that list.
        /// </param>
        /// <returns>If the sample format is supported, the maximum supported resolution (a <see cref="WasapiFormat" /> value) is returned, else -1 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Call this method prior to <see cref="Init" /> in order to make sure the requested format is supported by the Wasapi output device/driver (endpoint).
        /// <para>
        /// Shared and exclusive modes may have different sample formats available.
        /// Only the "mix format" (available from <see cref="GetDeviceInfo(int,out WasapiDeviceInfo)" />) is generally supported in shared mode.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Wasapi">WASAPI is not available.</exception>
        /// <exception cref="Errors.Device">The <paramref name="Device" /> number specified is invalid.</exception>
        /// <exception cref="Errors.Driver">The driver could not be initialized.</exception>
        /// <exception cref="Errors.SampleFormat">Unsupported sample format or number of channels.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_CheckFormat")]
        public static extern WasapiFormat CheckFormat(int Device, int Frequency, int Channels, WasapiInitFlags Flags);
        
        #region GetInfo
		/// <summary>
		/// Retrieves information on the Wasapi device being used.
		/// </summary>
		/// <param name="Info">An instance of the <see cref="WasapiInfo" /> structure to store the information at.</param>
		/// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
        /// This method can be used to get the effective settings used with an initialized Wasapi device (endpoint).
		/// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_GetInfo")]
        public static extern bool GetInfo(out WasapiInfo Info);
        
		/// <summary>
		/// Retrieves information on the Wasapi device being used.
		/// </summary>
		/// <returns>An instance of the <see cref="WasapiInfo" /> structure is returned. Throws <see cref="BassException"/> on Error.</returns>
		/// <remarks>
        /// This method can be used to get the effective settings used with an initialized Wasapi device (endpoint).
		/// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        public static WasapiInfo Info
        {
            get
            {
                if (!GetInfo(out var info))
                    throw new BassException();
                return info;
            }
        }
        #endregion

		/// <summary>
		/// Frees the Wasapi device/driver (endpoint).
		/// </summary>
		/// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
        /// This function should be called for all initialized devices before the program closes.
        /// Freed devices do not need to have been stopped with <see cref="Stop" /> beforehand.
		/// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_Free")]
        public static extern bool Free();

        /// <summary>
        /// Retrieves the immediate sample data (or an FFT representation of it) of the current Wasapi device/driver (endpoint).
        /// </summary>
        /// <param name="Buffer">An <see cref="IntPtr"/> to write the data to.</param>
        /// <param name="Length">Number of bytes wanted, and/or <see cref="DataFlags"/>.</param>
        /// <returns>
        /// If an error occurs, -1 is returned, use <see cref="Bass.LastError" /> to get the error code. 
        /// <para>When requesting FFT data, the number of bytes read from the device (to perform the FFT) is returned.</para>
        /// <para>When requesting sample data, the number of bytes written to buffer will be returned (not necessarily the same as the number of bytes read when using the <see cref="DataFlags.Float"/> flag).</para>
        /// <para>When using the <see cref="DataFlags.Available"/> flag, the number of bytes in the device's buffer is returned.</para>
        /// </returns>
        /// <remarks>
        /// <para>
        /// This function is like the standard <see cref="Bass.ChannelGetData(int,IntPtr,int)" />, but it gets the data from the device's buffer instead of decoding it from a channel, 
        /// which means that the device doesn't miss out on any data.
        /// In order to do this, the device must have buffering enabled, via the <see cref="BassFlags.MixerBuffer"/> flag.
        /// </para>
        /// <para>
        /// Internally, a BASS stream is used for that, so the usual <see cref="DataFlags"/> are supported.
        /// That also means that BASS needs to have been initialized first; it specifically uses the <see cref="Bass.NoSoundDevice"/>. 
        /// If the device is subsequently freed, this method call will fail.
        /// </para>
        /// <para>
        /// As in BASS, simultaneously using multiple devices is supported in the BASSWASAPI API via a context switching system - instead of there being an extra "device" parameter in the function calls, 
        /// the device to be used needs to be set via <see cref="CurrentDevice" /> prior to calling the function. 
        /// The device setting is local to the current thread, so calling functions with different devices simultaneously in multiple threads is not a problem.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">The device was not initialized using buffering (<see cref="WasapiInitFlags.Buffer"/>).</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_GetData")]
        public static extern int GetData(IntPtr Buffer, int Length);

        /// <summary>
        /// Retrieves the immediate sample data (or an FFT representation of it) of the current Wasapi device/driver (endpoint).
        /// </summary>
        /// <param name="Buffer">A float[] to write the data to.</param>
        /// <param name="Length">Number of bytes wanted, and/or <see cref="DataFlags"/>.</param>
        /// <returns>
        /// If an error occurs, -1 is returned, use <see cref="Bass.LastError" /> to get the error code. 
        /// <para>When requesting FFT data, the number of bytes read from the device (to perform the FFT) is returned.</para>
        /// <para>When requesting sample data, the number of bytes written to buffer will be returned (not necessarily the same as the number of bytes read when using the <see cref="DataFlags.Float"/> flag).</para>
        /// <para>When using the <see cref="DataFlags.Available"/> flag, the number of bytes in the device's buffer is returned.</para>
        /// </returns>
        /// <remarks>
        /// <para>
        /// This function is like the standard <see cref="Bass.ChannelGetData(int,IntPtr,int)" />, but it gets the data from the device's buffer instead of decoding it from a channel, 
        /// which means that the device doesn't miss out on any data.
        /// In order to do this, the device must have buffering enabled, via the <see cref="BassFlags.MixerBuffer"/> flag.
        /// </para>
        /// <para>
        /// Internally, a BASS stream is used for that, so the usual <see cref="DataFlags"/> are supported.
        /// That also means that BASS needs to have been initialized first; it specifically uses the <see cref="Bass.NoSoundDevice"/>. 
        /// If the device is subsequently freed, this method call will fail.
        /// </para>
        /// <para>
        /// As in BASS, simultaneously using multiple devices is supported in the BASSWASAPI API via a context switching system - instead of there being an extra "device" parameter in the function calls, 
        /// the device to be used needs to be set via <see cref="CurrentDevice" /> prior to calling the function. 
        /// The device setting is local to the current thread, so calling functions with different devices simultaneously in multiple threads is not a problem.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">The device was not initialized using buffering (<see cref="WasapiInitFlags.Buffer"/>).</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_GetData")]
        public static extern int GetData([In, Out] float[] Buffer, int Length);

        /// <summary>
        /// Adds sample data to an output device buffer ("push" device).
        /// </summary>
        /// <param name="Buffer">The pointer to the sample data to provide.</param>
        /// <param name="Length">The amount of data in bytes. <see cref="GetData(IntPtr,int)" /> with the <see cref="DataFlags.Available"/> flag can be used to check how much data is queued.</param>
        /// <returns>
        /// If successful, the the amount of data copied from the provided buffer will be returned
        /// (which may be less than requested if it doesn't all fit in the device buffer, see the <see cref="WasapiInfo.BufferLength"/> property), else -1 is returned.
        /// Use <see cref="Bass.LastError" /> to get the error code.
        /// </returns>
        /// <remarks>
        /// You must have initialized the device via <see cref="Init" /> with <see cref="WasapiProcedure" /> = <see langword="null" />.
        /// <para>As much data as possible will be placed in the device's buffer; this function will have to be called again for any remainder.</para>
        /// <para>
        /// Data should be provided at a rate sufficent to sustain playback.
        /// If the buffer gets exhausted, ouput will stall until more data is provided.
        /// <see cref="GetData(IntPtr,int)" /> with the <see cref="DataFlags.Available"/> flag can be used to check how much data is buffered.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">The device is being fed by a <see cref="WasapiProcedure"/> callback function, or it is an input device.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Length" /> is not valid, it must equate to a whole number of samples.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_PutData")]
        public static extern int PutData(IntPtr Buffer, int Length);

        /// <summary>
        /// Adds sample data to an output device buffer ("push" device).
        /// </summary>
        /// <param name="Buffer">float[] providing the sample data.</param>
        /// <param name="Length">The amount of data in bytes. <see cref="GetData(IntPtr,int)" /> with the <see cref="DataFlags.Available"/> flag can be used to check how much data is queued.</param>
        /// <returns>
        /// If successful, the the amount of data copied from the provided buffer will be returned
        /// (which may be less than requested if it doesn't all fit in the device buffer, see the <see cref="WasapiInfo.BufferLength"/> property), else -1 is returned.
        /// Use <see cref="Bass.LastError" /> to get the error code.
        /// </returns>
        /// <remarks>
        /// You must have initialized the device via <see cref="Init" /> with <see cref="WasapiProcedure" /> = <see langword="null" />.
        /// <para>As much data as possible will be placed in the device's buffer; this function will have to be called again for any remainder.</para>
        /// <para>
        /// Data should be provided at a rate sufficent to sustain playback.
        /// If the buffer gets exhausted, ouput will stall until more data is provided.
        /// <see cref="GetData(IntPtr,int)" /> with the <see cref="DataFlags.Available"/> flag can be used to check how much data is buffered.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">The device is being fed by a <see cref="WasapiProcedure"/> callback function, or it is an input device.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Length" /> is not valid, it must equate to a whole number of samples.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_PutData")]
        public static extern int PutData(float[] Buffer, int Length);
                
		/// <summary>
		/// Locks the device to the current thread.
		/// </summary>
		/// <param name="State">If <see langword="false" />, unlock WASAPI, else lock it.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// Locking a device prevents other threads from accessing the device buffer, including a <see cref="WasapiProcedure"/>.
        /// Other threads wanting to access a locked device will block until it is unlocked, so a device should only be locked very briefly.
		/// A device must be unlocked in the same thread that it was locked.
		/// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_Lock")]
        public static extern bool Lock(bool State = true);

        /// <summary>
        /// Gets the mute status of the current Wasapi device/driver (endpoint).
        /// </summary>
        /// <param name="Mode">The type of volume to get.</param>
        /// <returns><see langword="true" />, if the device/session is muted and <see langword="false" /> if unmuted, else -1. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">There is no volume control available.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_GetMute")]
        public static extern bool GetMute(WasapiVolumeTypes Mode = WasapiVolumeTypes.Device);

        /// <summary>
        /// Sets the mute status of the current Wasapi device/driver (endpoint).
        /// </summary>
        /// <param name="Mode">The type of volume to set.</param>
        /// <param name="Mute"><see langword="true" /> to mute the device, <see langword="false" /> to unmute the device.</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">There is no volume control available.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_SetMute")]
        public static extern bool SetMute(WasapiVolumeTypes Mode, bool Mute);
        
		/// <summary>
		/// Gets the audio meter information of the current Wasapi device/driver (endpoint).
		/// </summary>
		/// <param name="Device">The device to use... 0 = first device. <see cref="GetDeviceInfo(int, out WasapiDeviceInfo)" /> can be used to get the total number of devices.</param>
		/// <param name="Channel">The channel number to get the audio level meter information from (0=first, -1=all).</param>
		/// <returns>The audio level between 0.0 (silence) and 1.0 (maximum).</returns>
		/// <remarks>
        /// This method returns the global session level for the device which might include the level of other applications using the same device in shared-mode.
		/// <para>
        /// This function gets the level from the device/driver, or WASAPI if the device does not have its own level meter.
        /// If the latter case, the level will be unavailable when exclusive mode is active.
        /// </para>
		/// </remarks>
        /// <exception cref="Errors.Wasapi">WASAPI is not available.</exception>
        /// <exception cref="Errors.Device"><paramref name="Device" /> is not valid.</exception>
        /// <exception cref="Errors.Driver">The device driver does not support level retrieval.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Channel" /> is not valid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_GetDeviceLevel")]
        public static extern float GetDeviceLevel(int Device, int Channel = -1);

        /// <summary>
        /// Retrieves the current volume level.
        /// </summary>
        /// <param name="Curve">Volume curve to use.</param>
        /// <returns>If successful, the volume level is returned, else -1 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Session volume always uses <see cref="WasapiVolumeTypes.WindowsHybridCurve"/>.
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">There is no volume control available.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_GetVolume")]
        public static extern float GetVolume(WasapiVolumeTypes Curve = WasapiVolumeTypes.Device);

        /// <summary>
        /// Sets the volume of the current Wasapi device/driver (endpoint).
        /// </summary>
        /// <param name="Curve">Volume curve to use.</param>
        /// <param name="Volume">The new volume to set between 0.0 (silent) and 1.0 (maximum) if linear, or else a dB level.</param>
        /// <returns>Returns <see langword="true" /> on success, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Session volume only affects the current process, so other users of the device are unaffected.
        /// It has no effect on exclusive mode output, and maps to the device volume with input devices (so does affect other users).
        /// Session volume always uses <see cref="WasapiVolumeTypes.WindowsHybridCurve"/>.
        /// If you need to control the volume of the stream only, you need to apply that directly within the <see cref="WasapiProcedure" /> yourself.
        /// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">There is no volume control available.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Volume" /> is invalid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_SetVolume")]
        public static extern bool SetVolume(WasapiVolumeTypes Curve, float Volume);

        /// <summary>
        /// Initializes a Wasapi device/driver (endpoint).
        /// </summary>
        /// <param name="Device">The device to use... 0 = first device, -1 = default output device, -2 = default input device. <see cref="GetDeviceInfo(int,out WasapiDeviceInfo)" /> can be used to enumerate the available devices.</param>
        /// <param name="Frequency">The sample rate to use... 0 = "mix format" sample rate.</param>
        /// <param name="Channels">The number of channels to use... 0 = "mix format" channels, 1 = mono, 2 = stereo, etc.</param>
        /// <param name="Flags">A combination of <see cref="WasapiInitFlags"/>.</param>
        /// <param name="Buffer">
        /// The length of the device's buffer in seconds.
        /// This is a minimum and the driver may choose to use a larger buffer;
        /// <see cref="Info" /> can be used to confirm what the buffer size is.
        /// For an output device, the buffer size determines the latency.
        /// </param>
        /// <param name="Period">
        /// The interval (in seconds) between callback function calls... 0 = use default.
        /// If the specified period is below the minimum update period, it will automatically be raised to that.
        /// <para>
        /// The update period specifies the time between <see cref="WasapiProcedure" /> calls.
        /// The <see cref="WasapiDeviceInfo" /> (see <see cref="GetDeviceInfo(int,out WasapiDeviceInfo)" />) "minperiod" and "defperiod" values are actually minimum/default update periods.
        /// </para>
        /// </param>
        /// <param name="Procedure">
        /// The user defined function to process the channel.
        /// Use <see langword="null" /> to create a Wasapi "push" device (to which you can feed sample data via <see cref="PutData(IntPtr,int)" />).
        /// </param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>If the device was successfully initialized, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// For convenience, devices are always initialized to use their highest sample resolution and that is then converted to 32-bit floating-point, so that <see cref="WasapiProcedure"/> callback functions and the <see cref="PutData(IntPtr,int)" /> and <see cref="GetData(IntPtr,int)" /> functions are always dealing with the same sample format.
        /// The device's sample format can be obtained via <see cref="Info" />.
        /// </para>
        /// <para>
        /// WASAPI does not support arbitrary sample formats, like DirectSound does.
        /// In particular, only the "mix format" (available from <see cref="GetDeviceInfo(int,out WasapiDeviceInfo)" />) is generally supported in shared mode.
        /// <see cref="CheckFormat" /> can be used to check whether a particular sample format is supported.
        /// The BASSmix add-on can be used to play (or record) in otherwise unsupported sample formats, as well as playing multiple sources.
        /// </para>
        /// <para>The initialized device will not begin processing data until <see cref="Start" /> is called.</para>
        /// <para>
        /// Simultaneously using multiple devices is supported in the BASS API via a context switching system; instead of there being an extra "device" parameter in the function calls, the device to be used is set prior to calling the functions.
        /// <see cref="CurrentDevice" /> is used to switch the current device.
        /// When successful, <see cref="Init" /> automatically sets the current thread's device to the one that was just initialized.
        /// </para>
        /// <para>When using the default output or input device, <see cref="CurrentDevice" /> can be used to find out which device it was mapped to.</para>
        /// <para>In SHARED mode you must initialize the device with the current WASAPI mixer sample rate and number of channels (see the <see cref="WasapiDeviceInfo" /> "mixfreq" and "mixchans" properties).</para>
        /// <para>In EXCLUSIVE mode you might use any sample rate and number of channels which are supported by the device/driver.</para>
        /// <para>This function must be successfully called before any input or output can be performed.</para>
        /// <para>
        /// In EXCLUSIVE mode, the "period" value will affect what's an acceptable "buffer" value (it appears that the buffer must be at least 4x the period).
        /// In SHARED mode, it's the other way round, the "period" will be reduced to fit the "buffer" if necessary (with a minimum of the "defperiod" value).
        /// The system will limit them to an acceptable range, so for example, you could use a very small value (eg. 0.0001) for both, to get the minimum possible latency.
        /// </para>
        /// <para>
        /// Note: When initializing an input (capture or loopback) device, it might be the case, that the device is automatically muted once initialized.
        /// You can use the <see cref="GetMute" />/<see cref="SetMute" /> methods to check and probably toggle this.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Wasapi">WASAPI is not available.</exception>
        /// <exception cref="Errors.Device">The <paramref name="Device" /> number specified is invalid.</exception>
        /// <exception cref="Errors.Already">A device has already been initialized. You must call <see cref="Free" /> before you can initialize again.</exception>
        /// <exception cref="Errors.Parameter">An illegal parameter was specified (a <see cref="WasapiProcedure"/> must be provided for an input device).</exception>
        /// <exception cref="Errors.Driver">The driver could not be initialized.</exception>
        /// <exception cref="Errors.SampleFormat">The specified format is not supported by the device. If the <see cref="WasapiInitFlags.AutoFormat"/> flag was specified, no other format could be found either.</exception>
        /// <exception cref="Errors.Init">The <see cref="Bass.NoSoundDevice"/> has not been initialized.</exception>
        /// <exception cref="Errors.Busy">The device is busy (eg. in "exclusive" use by another process).</exception>
        /// <exception cref="Errors.Unknown">Some other mystery error.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_Init")]
        public static extern bool Init(int Device,
                                       int Frequency = 0,
                                       int Channels = 0,
                                       WasapiInitFlags Flags = WasapiInitFlags.Shared,
                                       float Buffer = 0,
                                       float Period = 0,
                                       WasapiProcedure Procedure = null,
                                       IntPtr User = default(IntPtr));

        /// <summary>
        /// Initializes a Wasapi device/driver (endpoint).
        /// </summary>
        /// <param name="Device">The device to use... 0 = first device, -1 = default output device, -2 = default input device. <see cref="GetDeviceInfo(int,out WasapiDeviceInfo)" /> can be used to enumerate the available devices.</param>
        /// <param name="Frequency">The sample rate to use... 0 = "mix format" sample rate.</param>
        /// <param name="Channels">The number of channels to use... 0 = "mix format" channels, 1 = mono, 2 = stereo, etc.</param>
        /// <param name="Flags">A combination of <see cref="WasapiInitFlags"/>.</param>
        /// <param name="Buffer">
        /// The length of the device's buffer in seconds.
        /// This is a minimum and the driver may choose to use a larger buffer;
        /// <see cref="Info" /> can be used to confirm what the buffer size is.
        /// For an output device, the buffer size determines the latency.
        /// </param>
        /// <param name="Period">
        /// The interval (in seconds) between callback function calls... 0 = use default.
        /// If the specified period is below the minimum update period, it will automatically be raised to that.
        /// <para>
        /// The update period specifies the time between <see cref="WasapiProcedure" /> calls.
        /// The <see cref="WasapiDeviceInfo" /> (see <see cref="GetDeviceInfo(int,out WasapiDeviceInfo)" />) "minperiod" and "defperiod" values are actually minimum/default update periods.
        /// </para>
        /// </param>
        /// <param name="Procedure">
        /// The user defined function to process the channel.
        /// Use <see langword="null" /> to create a Wasapi "push" device (to which you can feed sample data via <see cref="PutData(IntPtr,int)" />).
        /// </param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>If the device was successfully initialized, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// For convenience, devices are always initialized to use their highest sample resolution and that is then converted to 32-bit floating-point, so that <see cref="WasapiProcedure"/> callback functions and the <see cref="PutData(IntPtr,int)" /> and <see cref="GetData(IntPtr,int)" /> functions are always dealing with the same sample format.
        /// The device's sample format can be obtained via <see cref="Info" />.
        /// </para>
        /// <para>
        /// WASAPI does not support arbitrary sample formats, like DirectSound does.
        /// In particular, only the "mix format" (available from <see cref="GetDeviceInfo(int,out WasapiDeviceInfo)" />) is generally supported in shared mode.
        /// <see cref="CheckFormat" /> can be used to check whether a particular sample format is supported.
        /// The BASSmix add-on can be used to play (or record) in otherwise unsupported sample formats, as well as playing multiple sources.
        /// </para>
        /// <para>The initialized device will not begin processing data until <see cref="Start" /> is called.</para>
        /// <para>
        /// Simultaneously using multiple devices is supported in the BASS API via a context switching system; instead of there being an extra "device" parameter in the function calls, the device to be used is set prior to calling the functions.
        /// <see cref="CurrentDevice" /> is used to switch the current device.
        /// When successful, <see cref="Init" /> automatically sets the current thread's device to the one that was just initialized.
        /// </para>
        /// <para>When using the default output or input device, <see cref="CurrentDevice" /> can be used to find out which device it was mapped to.</para>
        /// <para>In SHARED mode you must initialize the device with the current WASAPI mixer sample rate and number of channels (see the <see cref="WasapiDeviceInfo" /> "mixfreq" and "mixchans" properties).</para>
        /// <para>In EXCLUSIVE mode you might use any sample rate and number of channels which are supported by the device/driver.</para>
        /// <para>This function must be successfully called before any input or output can be performed.</para>
        /// <para>
        /// In EXCLUSIVE mode, the "period" value will affect what's an acceptable "buffer" value (it appears that the buffer must be at least 4x the period).
        /// In SHARED mode, it's the other way round, the "period" will be reduced to fit the "buffer" if necessary (with a minimum of the "defperiod" value).
        /// The system will limit them to an acceptable range, so for example, you could use a very small value (eg. 0.0001) for both, to get the minimum possible latency.
        /// </para>
        /// <para>
        /// Note: When initializing an input (capture or loopback) device, it might be the case, that the device is automatically muted once initialized.
        /// You can use the <see cref="GetMute" />/<see cref="SetMute" /> methods to check and probably toggle this.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Wasapi">WASAPI is not available.</exception>
        /// <exception cref="Errors.Device">The <paramref name="Device" /> number specified is invalid.</exception>
        /// <exception cref="Errors.Already">A device has already been initialized. You must call <see cref="Free" /> before you can initialize again.</exception>
        /// <exception cref="Errors.Parameter">An illegal parameter was specified (a <see cref="WasapiProcedure"/> must be provided for an input device).</exception>
        /// <exception cref="Errors.Driver">The driver could not be initialized.</exception>
        /// <exception cref="Errors.SampleFormat">The specified format is not supported by the device. If the <see cref="WasapiInitFlags.AutoFormat"/> flag was specified, no other format could be found either.</exception>
        /// <exception cref="Errors.Init">The <see cref="Bass.NoSoundDevice"/> has not been initialized.</exception>
        /// <exception cref="Errors.Busy">The device is busy (eg. in "exclusive" use by another process).</exception>
        /// <exception cref="Errors.Unknown">Some other mystery error.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_Init")]
        public static extern bool InitEx(int Device,
                                        int Frequency = 0,
                                        int Channels = 0,
                                        WasapiInitFlags Flags = WasapiInitFlags.Shared,
                                        float Buffer = 0,
                                        float Period = 0,
                                        IntPtr Procedure = default(IntPtr),
                                        IntPtr User = default(IntPtr));

       

        #region IsStarted
        [DllImport(DllName)]
        static extern bool BASS_WASAPI_IsStarted();
        
		/// <summary>
		/// Checks, if the current Wasapi device/driver (endpoint) has been already started (via <see cref="Start" />).
		/// </summary>
		/// <returns>Returns <see langword="true" />, if the device has been started, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
		/// </remarks>
        public static bool IsStarted => BASS_WASAPI_IsStarted();
        #endregion
        
		/// <summary>
		/// Starts processing the current Wasapi device/driver (endpoint).
		/// </summary>
		/// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
        /// Before starting the device, it must be initialized using <see cref="Init" />.
        /// Use <see cref="Stop" /> to stop processing the device.
		/// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_Start")]
        public static extern bool Start();
        
		/// <summary>
		/// Stops the current Wasapi device/driver (endpoint).
		/// </summary>
		/// <param name="Reset">Flush the device buffer?
		/// <para>
        /// <see langword="true" /> will clear the output buffer.
        /// Otherwise it is like pausing, eg. <see cref="Start" /> will resume playing the buffered data.
        /// </para>
		/// </param>
		/// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>If the device buffer is left unflushed (<paramref name="Reset"/> = <see langword="false"/>), a subsequent <see cref="Start" /> call will resume things with the buffered data, otherwise it will resume with fresh data.</para>
		/// <para>When using multiple devices, the current thread's device setting (as set with <see cref="CurrentDevice" />) determines which device this function call applies to.</para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Start">The device hasn't been started.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_Stop")]
        public static extern bool Stop(bool Reset = true);
        
		/// <summary>
		/// Retrieves the level (peak amplitude) of the current Wasapi device/driver (endpoint).
		/// </summary>
		/// <returns>If an error occurs, -1 is returned, use <see cref="Bass.LastError" /> to get the error code.
		/// <para>
        /// If successful, the level of the left channel is returned in the low word (low 16-bits), and the level of the right channel is returned in the high word (high 16-bits).
        /// If the channel is mono, then the low word is duplicated in the high word. 
		/// The level ranges linearly from 0 (silent) to 32768 (max). 0 will be returned when a channel is stalled.
        /// </para>
		/// </returns>
		/// <remarks>
        /// This function is like the standard <see cref="Bass.ChannelGetLevel(int)" />, but it gets the level from the devices's buffer instead of decoding data from a channel, which means that the device doesn't miss out on any data. 
		/// The <see cref="WasapiInitFlags.Buffer"/> flag needs to have been specified in the device's initialization to enable the use of this function.
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">The device was not initialized using buffering (<see cref="WasapiInitFlags.Buffer"/>).</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_GetLevel")]
        public static extern int GetLevel();

        /// <summary>
        /// Retreives the level
        /// </summary>
        /// <param name="Levels">An array to receive the levels.</param>
        /// <param name="Length"></param>
        /// <param name="Flags"></param>
        /// <returns>true on success, else false. Use <see cref="Bass.LastError"/> to get the Error code.</returns>
        /// <remarks>
        /// This function uses <see cref="Bass.ChannelGetLevel(int,float[],float,LevelRetrievalFlags)"/> internally, so it behaves identically to that.
        /// The <see cref="WasapiInitFlags.Buffer"/> flag needs to have been specified in the device's initialization to enable the use of this function.
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">The device was not initialized using buffering (<see cref="WasapiInitFlags.Buffer"/>).</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Length"/> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WASAPI_GetLevelEx")]
        public static extern int GetLevel([In, Out] float[] Levels, float Length, LevelRetrievalFlags Flags);

        #region Version
        [DllImport(DllName)]
        static extern int BASS_WASAPI_GetVersion();

        /// <summary>
        /// Gets the Version of BassWasapi that is loaded.
        /// </summary>
        public static Version Version => Extensions.GetVersion(BASS_WASAPI_GetVersion());
        #endregion

        /// <summary>
        /// Gets the Bass device index for a Wasapi Device.
        /// </summary>
        public static int GetBassDevice(int WasapiDevice)
        {
            int i;

            var id = GetDeviceInfo(WasapiDevice).ID;

            for (i = 0; Bass.GetDeviceInfo(i, out var info); ++i)
                if (info.Driver == id)
                    return i;

            return -1;
        }
    }
}