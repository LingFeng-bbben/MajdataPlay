using System;
using System.Runtime.InteropServices;

namespace ManagedBass.DShow
{
	/// <summary>
	/// BassDShow/xVideo is a BASS addon providing a set of functions for rendering direct show audio and video content.
	/// </summary>
	public static class BassDShow
	{
		const string DllName = "xVideo";
                
        /// <summary>
        /// This method is an alternative way to get item names on methods that require callbacks.
        /// </summary>
        /// <param name="CallbackType">The type of value to get (one of the <see cref="BassDShowCallbackItem" /> values or a valid video HSTREAM to return the name of a connected filter from that channel).</param>
        /// <param name="Index">The index of the requered item (must be greater than 0).</param>
        /// <returns>If successful, the name of the requested item type is returned - else <see langword="null" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
        [DllImport(DllName, EntryPoint = "xVideo_CallbackItemByIndex")]
		public static extern string CallbackItemByIndex(BassDShowCallbackItem CallbackType, int Index);

		/// <summary>
		/// Creates a capture stream from audio/video capture devices.
		/// </summary>
		/// <param name="Audio">The audio device index (use 0 to disable audio capture).</param>
		/// <param name="Video">The video device index (use 0 to disable video capture).</param>
		/// <param name="AudioProfile">The audio profile to use (as returned by <see cref="CaptureDeviceProfiles" />).</param>
		/// <param name="VideoProfile">The video profile to use (as returned by <see cref="CaptureDeviceProfiles" />).</param>
		/// <param name="Flags">Any combination of <see cref="BassFlags.Decode"/> and <see cref="BassFlags.DShowStreamMix"/>.</param>
		/// <returns>If successful, the new capture handle is returned - else 0 is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_CaptureCreate")]
		public static extern int CaptureCreate(int Audio, int Video, int AudioProfile, int VideoProfile, BassFlags Flags);

		/// <summary>
		/// Gets the available audio or video capture device profiles.
		/// </summary>
		/// <param name="Device">The device you want to get then profiles from (see <see cref="CaptureGetDevices" />).</param>
		/// <param name="DeviceType">The type of capture device to get (one of the <see cref="BassDShowCapture" /> values, either audio or video).</param>
		/// <param name="Procedure">The user defined callback receiving the available capture devices (see <see cref="EnumProfilesProcedure" />).</param>
		/// <param name="User">User instance data to pass to the callback function.</param>
		/// <returns>If successful, the number of available capture device profiles are returned - else 0 is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_CaptureDeviceProfiles")]
		public static extern int CaptureDeviceProfiles(int Device, BassDShowCapture DeviceType, EnumProfilesProcedure Procedure, IntPtr User);

		/// <summary>
		/// Frees a capture stream.
		/// </summary>
		/// <param name="Handle">The capture stream handle to free (as returned by <see cref="CaptureCreate" />).</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_CaptureFree")]
		public static extern bool CaptureFree(int Handle);

		/// <summary>
		/// Gets the available audio or video capture devices.
		/// </summary>
		/// <param name="DeviceType">The type of capture device to get (one of the <see cref="BassDShowCapture" /> values, either audio or video).</param>
		/// <param name="Procedure">The user defined callback receiving the available capture devices (see <see cref="EnumDevicesProcedure" />) or <see langword="null" /> to retrieve only the number of available devices.</param>
		/// <param name="User">User instance data to pass to the callback function.</param>
		/// <returns>If successful, the number of available capture devices found is returns - else 0 is returned (use <see cref="LastError" /> to get the error code).</returns>
		/// <remarks>
		/// <para>When using <see langword="null" /> as a <paramref name="Procedure" /> callback value this will return the number of the devices from the system only.</para>
		/// <para>Another way to get the capture devices is by using the <see cref="CallbackItemByIndex" /> method.</para>
		/// </remarks>
		[DllImport(DllName, EntryPoint = "xVideo_CaptureGetDevices")]
		public static extern int CaptureGetDevices(BassDShowCapture DeviceType, EnumDevicesProcedure Procedure, IntPtr User);

		/// <summary>
		/// Adds a new media file to a channel (e.g. for mixing multiple videos).
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Filename">The media file to add.</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		/// <remarks>This method can be used to mix multiple videos to the same channel.</remarks>
		public static bool ChannelAddFile(int Handle, string Filename)
		{
			return BASS_DSHOW_ChannelAddFile(Handle, Filename, BassFlags.Unicode);
		}

		[DllImport(DllName, EntryPoint = "xVideo_ChannelAddFile", CharSet = CharSet.Unicode)]
		static extern bool BASS_DSHOW_ChannelAddFile(int Handle, string FileName, BassFlags Flags);

		/// <summary>
		/// Adds a new Video Window to the channel.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="hWnd">A valid window handle.</param>
		/// <returns>If successful, the new video window handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>
		/// You can use this function to a new video window to a channel.
		/// Adding new windows depends on the hardware capabilities (adding many new windows might result in a high CPU usage). 
		/// You can use the <see cref="ChannelRemoveWindow" /> function to remove an added video window.
		/// </remarks>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelAddWindow")]
		public static extern int ChannelAddWindow(int Handle, IntPtr hWnd);

		/// <summary>
		/// Retrieves the color controls range (min,max,step). 
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Id">One of <see cref="BassDShowColorControl" /> values.</param>
		/// <param name="Ctrl">An instance of the <see cref="BassDShowColorRange" /> structure to set the range info to.</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		/// <remarks>This function was tested successfully on VMR9. It may work on other renderers and also will depend on the hardware capabilities.</remarks>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelColorRange")]
		public static extern bool ChannelColorRange(int Handle, BassDShowColorControl Id, out BassDShowColorRange Ctrl);

		/// <summary>
		/// Retrieves the color controls range (min,max,step). 
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Id">One of <see cref="BassDShowColorControl" />) values.</param>
		/// <returns>If an error occurred then <see langword="null" /> is returned, else an instance of <see cref="BassDShowColorRange" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		/// <remarks>This function was tested successfully on VMR9. It may work on other renderers and also will depend on the hardware capabilities.</remarks>
		public static BassDShowColorRange ChannelColorRange(int Handle, BassDShowColorControl Id)
		{
            if (!ChannelColorRange(Handle, Id, out var colorRange))
		        throw new Exception(LastError.ToString());

		    return colorRange;
		}

		/// <summary>
		/// Enables a stream within a video.
		/// </summary>
		/// <param name="Handle">The video channel handle (as e.g. returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Index">The zero-based stream index to enable (use <see cref="ChannelStreamsCount" /> to get the number of streams).</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelEnableStream")]
		public static extern bool ChannelEnableStream(int Handle, int Index);

		/// <summary>
		/// Enumerates connected streams on a video handle.
		/// </summary>
		/// <param name="Handle">The video channel handle (as e.g. returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Procedure">The callback function (see <see cref="VideoStreamsProcedure" />) to retrieve the stream information.</param>
		/// <param name="User">User instance data to pass to the callback function.</param>
		/// <returns>If successfull, the number of streams are returned, else 0 is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelEnumerateStreams")]
		public static extern int ChannelEnumerateStreams(int Handle, VideoStreamsProcedure Procedure, IntPtr User);

		/// <summary>
		/// Gets an attribute value from a HSTREAM.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Attribute">One of <see cref="BassDShowAttribute" /> values.</param>
		/// <returns>If successful, the current attribute value is returned - else 0 is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelGetAttribute")]
		public static extern float ChannelGetAttribute(int Handle, BassDShowAttribute Attribute);
        
		/// <summary>
		/// Retrieves the current video frame.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <returns>If successful, a pointer to an image (HBITMAP) is returned, else <see cref="IntPtr.Zero"/> is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>If the current video renderer is the NULL one, or a stream don't have video then a NULL value will be returned.</remarks>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelGetBitmap")]
		public static extern IntPtr ChannelGetBitmap(int Handle);

		/// <summary>
		/// Retrieves the connected filters to a channel.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Procedure">The user defined callback receiving the connected filters (see <see cref="ConnectedFiltersProcedure" />).</param>
		/// <param name="User">User instance data to pass to the callback function.</param>
		/// <returns>If successful, the number of connected filters is returns - else 0 is returned (use <see cref="LastError" /> to get the error code).</returns>
		/// <remarks>
		/// <para>When using <see langword="null" /> as a <paramref name="Procedure" /> callback value this will return the number of connected filters only.</para>
		/// <para>Another way to get the connected filters is by using the <see cref="CallbackItemByIndex" /> method.</para>
		/// </remarks>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelGetConnectedFilters")]
		public static extern int ChannelGetConnectedFilters(int Handle, ConnectedFiltersProcedure Procedure, IntPtr User);

		/// <summary>
		/// Retrieves information on a video channel.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Info"><see cref="BassDShowChannelInfo" /> instance where to store the channel information at.</param>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelGetInfo")]
		public static extern void ChannelGetInfo(int Handle, out BassDShowChannelInfo Info);

		/// <summary>
		/// Retrieves information on a video channel.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <returns>A <see cref="BassDShowChannelInfo" /> instance containing the video info.</returns>
		public static BassDShowChannelInfo ChannelGetInfo(int Handle)
		{
            ChannelGetInfo(Handle, out var info);
			return info;
		}

		/// <summary>
		/// Gets the length of the video stream.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Mode">How to get the position. One of <see cref="BassDShowMode" /> values.</param>
		/// <returns>
		/// If successful, the length of the video stream in units according to the <see cref="BassDShowMode" /> used,
		/// else 0 is returned. Use <see cref="LastError" /> to get the error code.
		/// </returns>
		/// <remarks>0 is ussualy returned when the stream doesn't contain video or if an invalid channel handle was specified.</remarks>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelGetLength")]
		public static extern double ChannelGetLength(int Handle, BassDShowMode Mode);

		/// <summary>
		/// Gets the current position of the video stream.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Mode">How to get the position. One of <see cref="BassDShowMode" /> values.</param>
		/// <returns>
		/// If successful, the current position of the video stream in units according to the <see cref="BassDShowMode" /> used,
		/// else 0 is returned. Use <see cref="LastError" /> to get the error code.
		/// </returns>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelGetPosition")]
		public static extern double ChannelGetPosition(int Handle, BassDShowMode Mode);

		/// <summary>
		/// Gets the current state of a channel (playing/stopped/paused).
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <returns>If successful, the current <see cref="BassDShowState" /> is returns - else -1 is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelGetState")]
		public static extern BassDShowState ChannelGetState(int Handle);

		[DllImport(DllName, EntryPoint = "xVideo_ChannelGetStream")]
		static extern bool ChannelGetStream(int Handle, int Index, out BassDShowStreams Streams);

		/// <summary>
		/// Gets information about a video stream.
		/// </summary>
		/// <param name="Handle">The video channel handle (as e.g. returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Index">The zero-based stream index to retrieve the info from (use <see cref="ChannelStreamsCount" /> to get the number of streams).</param>
		/// <returns>If an error occurred then <see langword="null" /> is returned, else the <see cref="BassDShowStreams" /> instance is returned (use <see cref="LastError" /> to get the error code).</returns>
		public static BassDShowStreams ChannelGetStream(int Handle, int Index)
		{
            if (!ChannelGetStream(Handle, Index, out var streams))
		        throw new Exception(LastError.ToString());

		    return streams;
		}

		/// <summary>
		/// Overlays a HDC to the video window.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Bitmap">An instance of the <see cref="BassDShowVideoBitmap" /> class to overlay.</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelOverlayBMP")]
		public static extern bool ChannelOverlayBMP(int Handle, BassDShowVideoBitmap Bitmap);

		/// <summary>
		/// Pauses playback of a video stream.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelPause")]
		public static extern bool ChannelPause(int Handle);

		/// <summary>
		/// Starts playback of a video stream.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelPlay")]
		public static extern bool ChannelPlay(int Handle);

		/// <summary>
		/// Removes a DVP function from a video stream.
		/// </summary>
		/// <param name="Handle">The video channel handle (as e.g. returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Dvp">Handle of the DVP function to remove from the channel (return value of a previous <see cref="ChannelSetDVP" /> call).</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelRemoveDVP")]
		public static extern bool ChannelRemoveDVP(int Handle, int Dvp);

		/// <summary>
		/// Removes an added video window from the channel.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Window">A valid window handle (as returned by <see cref="ChannelAddWindow" />) to remove.</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelAddWindow")]
		public static extern bool ChannelRemoveWindow(int Handle, int Window);

		/// <summary>
		/// Repaints a window less video renderer.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Window">A valid window handle.</param>
		/// <param name="HDC">A valid HDC.</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		/// <remarks>Sometimes window less video renderers needs to be repainted manually (e.g. if they cannot receive the WM_PAINT message).</remarks>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelRepaint")]
		public static extern bool ChannelRepaint(int Handle, IntPtr Window, IntPtr HDC);

		/// <summary>
		/// Resizes the rendering of a video stream to a new size.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Window">A valid window handle (as returned by <see cref="ChannelAddWindow" />) to resize or 0(zero) for the main video window.</param>
		/// <param name="Left">The left coordinate.</param>
		/// <param name="Top">The top coordinate.</param>
		/// <param name="Right">The right coordinate.</param>
		/// <param name="Bottom">The bottom coordinate.</param>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelResizeWindow")]
		public static extern void ChannelResizeWindow(int Handle, int Window, int Left, int Top, int Right, int Bottom);

		/// <summary>
		/// Sets an attribute value to a HSTREAM.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Attribute">One of the <see cref="BassDShowAttribute" /> values.</param>
		/// <param name="NewValue">The new attribute value.</param>
		/// <remarks>Use <see cref="LastError" /> to get the error code.</remarks>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelSetAttribute")]
		public static extern void ChannelSetAttribute(int Handle, BassDShowAttribute Attribute, float NewValue);

		/// <summary>
		/// Sets new values to color controls.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Id">Any combination of <see cref="BassDShowColorControl"/> flags.</param>
		/// <param name="Colors">An instance of the <see cref="BassDShowVideoColors" /> structure to apply the color changes from.</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		/// <remarks>This function was tested successfully on VMR9. It may work on other renderers and also will depend on the hardware capabilities.</remarks>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelSetColors")]
		public static extern bool ChannelSetColors(int Handle, BassDShowColorControl Id, BassDShowVideoColors Colors);

		/// <summary>
		/// Sets up a user DVP function on a video stream.
		/// </summary>
		/// <param name="Handle">The video channel handle (as e.g. returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Procedure">The callback function (see <see cref="DvpProcedure" />).</param>
		/// <param name="User">User instance data to pass to the callback function.</param>
		/// <returns>If successfull, the new DVP handle is returned, else 0 is returned (use <see cref="LastError" /> to get the error code).</returns>
		/// <remarks>
		/// DVP functions can be set any time on a channel. A channel support a infinite number of DVP callbacks, and it will be processed in the same order that was added.
		/// Use <see cref="ChannelRemoveDVP" /> to remove a DVP.
		/// </remarks>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelSetDVP")]
		public static extern int ChannelSetDVP(int Handle, DvpProcedure Procedure, IntPtr User);

		/// <summary>
		/// Toggles the rendering of a video stream between full screen (on/off).
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="FullScreen"><see langword="true" /> to toggle the rendering to full screen - <see langword="false" /> to turn full screen rendering off.</param>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelSetFullscreen")]
		public static extern void ChannelSetFullscreen(int Handle, bool FullScreen);

		/// <summary>
		/// Sets the playback position of a video stream.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Position">The new position to set.</param>
		/// <param name="Mode">How to retrieve the position. One of the <see cref="BassDShowMode" /> values.</param>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelSetPosition")]
		public static extern bool ChannelSetPosition(int Handle, double Position, BassDShowMode Mode);

		/// <summary>
		/// Sets up a synchronizer on a video channel.
		/// </summary>
		/// <param name="Handle">The video channel handle (as e.g. returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Procedure">The callback function which should be invoked with the sync.</param>
		/// <param name="User">User instance data to pass to the callback function.</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelCallBackSync")]
		public static extern bool ChannelSetSync(int Handle, VideoSyncProcedure Procedure, IntPtr User);

		/// <summary>
		/// Sets s video window handle.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Window">A valid window handle (as returned by <see cref="ChannelAddWindow" />) or 0 for the main video window.</param>
		/// <param name="hWnd">A valid window handle.</param>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelSetWindow")]
		public static extern void ChannelSetWindow(int Handle, int Window, IntPtr hWnd);

		/// <summary>
		/// Stops playback of a video stream.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelStop")]
		public static extern bool ChannelStop(int Handle);

		/// <summary>
		/// Gets the number of streams for a video handle.
		/// </summary>
		/// <param name="Handle">The video channel handle (as e.g. returned by <see cref="StreamCreateFile" />).</param>
		/// <returns>If successfull, the number of streams are returned, else 0 is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_ChannelStreamsCount")]
		public static extern int ChannelStreamsCount(int Handle);

		/// <summary>
		/// Sets DVD MENU options.
		/// </summary>
		/// <param name="Handle">The DVD stream handle (as returned by <see cref="StreamCreateDVD" />).</param>
		/// <param name="Option">One of the <see cref="BassDShowDVDMenu" /> values.</param>
		/// <param name="Value1">The 1st value to set.</param>
		/// <param name="Value2">The 2nd value to set.</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_DVDChannelMenu")]
		public static extern bool DVDChannelMenu(int Handle, BassDShowDVDMenu Option, int Value1, int Value2);

		/// <summary>
		/// Gets DVD properties.
		/// </summary>
		/// <param name="Handle">The DVD stream handle (as returned by <see cref="StreamCreateDVD" />).</param>
		/// <param name="Property">One of the <see cref="BassDShowDVDGetProperty" /> values.</param>
		/// <param name="Value">The value to get.</param>
		/// <returns>The requested property result value (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_DVDGetProperty")]
		public static extern int DVDGetProperty(int Handle, BassDShowDVDGetProperty Property, int Value);

		/// <summary>
		/// Sets DVD properties.
		/// </summary>
		/// <param name="Handle">The DVD stream handle (as returned by <see cref="StreamCreateDVD" />).</param>
		/// <param name="Property">One of the <see cref="BassDShowDVDSetProperty" /> values.</param>
		/// <param name="NewValue">The new value to set.</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_DVDSetProperty")]
		public static extern bool DVDSetProperty(int Handle, BassDShowDVDSetProperty Property, int NewValue);

        [DllImport(DllName, EntryPoint = "xVideo_ErrorGetCode")]
        static extern BassDShowError BASS_DSHOW_ErrorGetCode();

	    /// <summary>
	    /// Retrieves the error code for the most recent BASS_DSHOW function call.
	    /// </summary>
	    /// <returns>
	    /// If no error occured during the last BASS_DSHOW function call then <see cref="BassDShowError.OK"/> is returned, else one of the <see cref="BassDShowError" /> values is returned. 
	    /// See the function description for an explanation of what the error code means.
	    /// </returns>
	    public static BassDShowError LastError => BASS_DSHOW_ErrorGetCode();

		/// <summary>
		/// Frees all resources used by DSHOW, including all it's streams.
		/// </summary>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		/// <remarks>This function will also free all streams created by BASS_DSHOW.</remarks>
		[DllImport(DllName, EntryPoint = "xVideo_Free")]
		public static extern bool Free();

		/// <summary>
		/// Gets the available audio render devices available on the system.
		/// </summary>
		/// <param name="Procedure">The user defined callback receiving the available audio render devices (see <see cref="EnumDevicesProcedure" />).</param>
		/// <param name="User">User instance data to pass to the callback function.</param>
		/// <returns>If successful, the number of available audio render devices found is returns - else 0 is returned (use <see cref="LastError" /> to get the error code).</returns>
		/// <remarks>
		/// <para>When using <see langword="null" /> as a <paramref name="Procedure" /> callback value this will return the number of the audio render devices from the system only.</para>
		/// <para>Another way to get the capture devices is by using the <see cref="CallbackItemByIndex" /> method.</para>
		/// </remarks>
		[DllImport(DllName, EntryPoint = "xVideo_GetAudioRenderers")]
		public static extern int GetAudioRenderers(EnumDevicesProcedure Procedure, IntPtr User);

		/// <summary>
		/// Gets a BASS_DSHOW configuration option.
		/// </summary>
		/// <param name="Option">The option to set (one of the <see cref="BassDShowConfig" /> values).</param>
		/// <returns>If successful, the config option value is returned (see below), else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
		[DllImport(DllName, EntryPoint = "xVideo_GetConfig")]
		public static extern int GetConfig(BassDShowConfig Option);

		/// <summary>
		/// Gets an instance IGraphBuilder interface of a channel.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <returns>If successful, the new video window handle is returned, else <see cref="IntPtr.Zero"/> is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>
		/// After getting an instance of the IGraphBuilder interface and add reference to it, and used it, the instance must be released by user, or memory leaks will appear.
		/// </remarks>
		[DllImport(DllName, EntryPoint = "xVideo_GetGraph")]
		public static extern IntPtr GetGraph(int Handle);

        [DllImport(DllName, EntryPoint = "xVideo_GetVersion")]
        static extern int BASS_DSHOW_GetVersion();

	    /// <summary>
	    /// Retrieves the version number of the BASS_DSHOW that is loaded.
	    /// </summary>
	    public static Version Version => Extensions.GetVersion(BASS_DSHOW_GetVersion());
		
		/// <summary>
		/// Gets a video window layer alpha blend value.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Window">A valid window handle as returned by <see cref="ChannelAddWindow" /> or 0(zero) for the main video window.</param>
		/// <param name="Layer">The video layer (0...15).</param>
		/// <returns>If an error occurred then -1 is returned, else the alpha blend value is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_GetVideoAlpha")]
		public static extern float GetVideoAlpha(int Handle, int Window, int Layer);
        
        /// <summary>
        /// Initialize the DSHOW library. This will initialize the library for use.
        /// </summary>
        /// <param name="hWnd">The application's main window or 0 = the current foreground window (use this for console applications).</param>
        /// <param name="Flags">Any combination of <see cref="BassDShowInit"/> flags.</param>
        /// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
        /// <remarks>Call this method prior to any other BASS_DSHOW methods.</remarks>
        [DllImport(DllName, EntryPoint = "xVideo_Init")]
        public static extern bool Init(IntPtr hWnd, BassDShowInit Flags);
        
		/// <summary>
		/// Loads a BASS_DSHOW plugin.
		/// </summary>
		/// <param name="File">The plugin filename to load.</param>
		/// <returns>If successful, the plugin handle is returned - else 0 is returned (use <see cref="LastError" /> to get the error code).</returns>
		public static int LoadPlugin(string File) => BASS_DSHOW_LoadPlugin(File, BassFlags.Unicode);

	    /// <summary>
		/// Loads a DirectShow plugin.
		/// </summary>
		/// <param name="Guid">The CLSID/GUID string of the DirectShow filter to use.</param>
		/// <param name="Name">The name of the filter.</param>
		/// <returns>If successful, the plugin handle is returned - else 0 is returned (use <see cref="LastError" /> to get the error code).</returns>
		/// <remarks>The necesary DirectShow codec to decode the video must be available!</remarks>
		public static int LoadPluginDS(string Guid, string Name)
		{
			return BASS_DSHOW_LoadPluginDS(Guid, Name, BassFlags.Unicode);
		}

		[DllImport(DllName, CharSet = CharSet.Unicode, EntryPoint = "xVideo_LoadPluginDS")]
		static extern int BASS_DSHOW_LoadPluginDS(string Guid, string Name, BassFlags Flags);

		[DllImport(DllName, CharSet = CharSet.Unicode, EntryPoint = "xVideo_LoadPlugin")]
		static extern int BASS_DSHOW_LoadPlugin(string File, BassFlags Flags);

		/// <summary>
		/// Resizes a mixing channel
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Layer">The video layer (0...15).</param>
		/// <param name="Left">The left position of the video.</param>
		/// <param name="Top">The top position of the video.</param>
		/// <param name="Right">The right position of the video.</param>
		/// <param name="Bottom">The bottom position of the video.</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_MIXChannelResize")]
		public static extern bool MIXChannelResize(int Handle, int Layer, int Left, int Top, int Right, int Bottom);
        
		/// <summary>
		/// Gets information about a loaded plugin.
		/// </summary>
		/// <param name="Plugin">The plugin handled (as returned by <see cref="LoadPlugin" />).</param>
		/// <returns>If an error occurred then <see langword="null" /> is returned, else an instance of <see cref="BassDShowPluginInfo" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		public static BassDShowPluginInfo PluginGetInfo(int Plugin)
		{
            if (PluginGetInfo(Plugin, out var info))
		        throw new Exception(LastError.ToString());

		    return info;
		}

        /// <summary>
        /// Gets information about a loaded plugin.
        /// </summary>
        /// <param name="Plugin">The plugin handled (as returned by <see cref="LoadPlugin" />).</param>
        /// <param name="Info">An instance of <see cref="BassDShowPluginInfo" /> to store the information at.</param>
        /// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
        [DllImport(DllName, EntryPoint = "xVideo_PluginGetInfo")]
		public static extern bool PluginGetInfo(int Plugin, out BassDShowPluginInfo Info);

		/// <summary>
		/// Registers you DSHOW/xVideo license.
		/// </summary>
		/// <param name="Email">Your email address.</param>
		/// <param name="Code">Your registration code.</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		public static bool Register(string Email, string Code)
		{
			return BASS_DSHOW_Register(Email, Code, BassFlags.Unicode);
		}

		[DllImport(DllName, CharSet = CharSet.Unicode, EntryPoint = "xVideo_Register")]
		static extern bool BASS_DSHOW_Register(string Email, string Code, BassFlags Flags);

		/// <summary>
		/// Removes a loaded plugin and frees all resources.
		/// </summary>
		/// <param name="Plugin">The plugin handled to unload (as returned by <see cref="LoadPlugin" />).</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_RemovePlugin")]
		public static extern bool RemovePlugin(int Plugin);

		/// <summary>
		/// Sets a BASS_DSHOW configuration option.
		/// </summary>
		/// <param name="Option">The option to set (one of the <see cref="BassDShowConfig" /> values).</param>
		/// <param name="NewValue">The value to use.</param>
		[DllImport(DllName, EntryPoint = "xVideo_SetConfig")]
		public static extern void SetConfig(BassDShowConfig Option, int NewValue);

		/// <summary>
		/// Sets a BASS_DSHOW configuration option.
		/// </summary>
		/// <param name="Option">The option to set (one of the <see cref="BassDShowConfig" /> values).</param>
		/// <param name="NewValue">One of the <see cref="BassDShowConfigFlag" /> values to use.</param>
		[DllImport(DllName, EntryPoint = "xVideo_SetConfig")]
		public static extern void SetConfig(BassDShowConfig Option, BassDShowConfigFlag NewValue);

		/// <summary>
		/// Sets a video window alpha blend value.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Window">A valid window handle as returned by <see cref="ChannelAddWindow" /> or 0(zero) for the main video window.</param>
		/// <param name="Layer">The video layer (0...15).</param>
		/// <param name="Alpha">The new alpha blend value (0...100).</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_SetVideoAlpha")]
		public static extern bool SetVideoAlpha(int Handle, int Window, int Layer, float Alpha);

		/// <summary>
		/// Shows a filter property page.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Filter">The filter number (must be greater than 0, see <see cref="ChannelGetConnectedFilters" />).</param>
		/// <param name="Parent">The parent window handle to use when showing th property page dialog.</param>
		[DllImport(DllName, EntryPoint = "xVideo_ShowFilterPropertyPage")]
		public static extern void ShowFilterPropertyPage(int Handle, int Filter, IntPtr Parent);

		/// <summary>
		/// Creates a sample stream from a DVD or VOB file.
		/// </summary>
		/// <param name="Dvd">The DVD or VOB filename or <see langword="null" /> to render first DVD device.</param>
		/// <param name="Window">An initial window handle (can be <see cref="IntPtr.Zero"/>).</param>
		/// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
		/// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>The necesary codec to decode the video must be available!</remarks>
		public static int StreamCreateDVD(string Dvd, IntPtr Window, BassFlags Flags)
		{
			return BASS_DSHOW_StreamCreateDVD(Dvd, Window, Flags | BassFlags.Unicode);
		}

		[DllImport(DllName, CharSet = CharSet.Unicode, EntryPoint = "xVideo_StreamCreateDVD")]
		static extern int BASS_DSHOW_StreamCreateDVD(string Dvd, IntPtr Window, BassFlags Flags);

		/// <summary>
		/// Creates a sample stream from a supported file.
		/// </summary>
		/// <param name="File">The filename to render.</param>
		/// <param name="Position">The position to begin playing from.</param>
		/// <param name="Window">An initial window handle (can be <see cref="IntPtr.Zero" />).</param>
		/// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
		/// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>The necesary codec to decode the video must be available!</para>
		/// <para>When a start position is specified and the requested position is not available, playback it will start automaticaly from the begginning.</para>
		/// </remarks>
		public static int StreamCreateFile(string File, int Position, IntPtr Window, BassFlags Flags)
		{
			return BASS_DSHOW_StreamCreateFile(File, Position, Window, Flags | BassFlags.Unicode);
		}

		/// <summary>
		/// Creates a sample stream from memory data.
		/// </summary>
		/// <param name="Memory">An unmanaged pointer to the memory location as an IntPtr.</param>
		/// <param name="Length">Data length (needs to be set to the length of the memory stream in bytes which should be played).</param>
		/// <param name="Window">An initial window handle (can be <see cref="IntPtr.Zero" />).</param>
		/// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
		/// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>The necesary codec to decode the video must be available!</remarks>
		[DllImport(DllName, EntryPoint = "xVideo_StreamCreateFileMem")]
		public static extern int StreamCreateMemory(IntPtr Memory, long Length, IntPtr Window, BassFlags Flags);

		[DllImport(DllName, CharSet = CharSet.Unicode, EntryPoint = "xVideo_StreamCreateFile")]
		static extern int BASS_DSHOW_StreamCreateFile(string File, int Position, IntPtr Window, BassFlags Flags);

		/// <summary>
		/// Creates a sample stream using user file procedures.
		/// </summary>
		/// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
		/// <param name="Window">An initial window handle (can be <see cref="IntPtr.Zero" />).</param>
		/// <param name="Procedures">The user defined file function (see <see cref="FileProcedures" />).</param>
		/// <param name="User">User instance data to pass to the callback functions.</param>
		/// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>The necesary codec to decode the video must be available!</remarks>
		[DllImport(DllName, EntryPoint = "xVideo_StreamCreateFileUser")]
		public static extern int StreamCreateFileUser(BassFlags Flags, IntPtr Window, [In, Out] FileProcedures Procedures, IntPtr User);

		/// <summary>
		/// Creates a sample stream from a supported file using a given filter.
		/// </summary>
		/// <param name="File">The filename to render.</param>
		/// <param name="Filter">The CLSID/GUID string of the filter to use.</param>
		/// <param name="Window">An initial window handle (can be <see cref="IntPtr.Zero" />).</param>
		/// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
		/// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="LastError" /> to get the error code.</returns>
		/// <remarks>The necesary codec to decode the video must be available!</remarks>
		public static int StreamCreateFilter(string File, string Filter, IntPtr Window, BassFlags Flags)
		{
			return BASS_DSHOW_StreamCreateFilter(File, Filter, Window, Flags | BassFlags.Unicode);
		}

		[DllImport(DllName, CharSet = CharSet.Unicode, EntryPoint = "xVideo_StreamCreateFilter")]
		static extern int BASS_DSHOW_StreamCreateFilter(string File, string Filter, IntPtr Window, BassFlags Flags);

		/// <summary>
		/// Frees all resources of a video stream.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		[DllImport(DllName, EntryPoint = "xVideo_StreamFree")]
		public static extern bool StreamFree(int Handle);

		/// <summary>
		/// Send video from user file data to a virtual webcam.
		/// </summary>
		/// <param name="Window">A Handle to the application window.</param>
		/// <param name="Data">The pointer to the video data to push.</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		/// <remarks>
		/// Make sure that xVirtualCam is registered and running when calling this function, otherwise this function will return <see langword="false" />.
		/// <para>The <paramref name="Data"/> parameter can't be <see cref="IntPtr.Zero" />, and MUST be 320 * 240 * 4 in size (320 Width, 240 Height, 32 bits).</para>
		/// </remarks>
		[DllImport(DllName, EntryPoint = "xVideo_VCamPush")]
		public static extern bool VCamPush(int Window, IntPtr Data);

        /// <summary>
        /// Send video from user file data to a virtual webcam.
        /// </summary>
        /// <param name="Window">A Handle to the application window.</param>
        /// <param name="Data">The video data to push.</param>
        /// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
        /// <remarks>
        /// Make sure that xVirtualCam is registered and running when calling this function, otherwise this function will return <see langword="false" />.
        /// <para>The <paramref name="Data"/> parameter can't be <see cref="IntPtr.Zero" />, and MUST be 320 * 240 * 4 in size (320 Width, 240 Height, 32 bits).</para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "xVideo_VCamPush")]
		public static extern bool VCamPush(int Window, byte[] Data);

        /// <summary>
        /// Send video from user file data to a virtual webcam.
        /// </summary>
        /// <param name="Window">A Handle to the application window.</param>
        /// <param name="Data">The video data to push.</param>
        /// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
        /// <remarks>
        /// Make sure that xVirtualCam is registered and running when calling this function, otherwise this function will return <see langword="false" />.
        /// <para>The <paramref name="Data"/> parameter can't be <see cref="IntPtr.Zero" />, and MUST be 320 * 240 * 4 in size (320 Width, 240 Height, 32 bits).</para>
        /// </remarks>
        [DllImport(DllName, EntryPoint = "xVideo_VCamPush")]
		public static extern bool VCamPush(int Window, long[] Data);

		/// <summary>
		/// Send video data from a HSTREAM to a virtual webcam.
		/// </summary>
		/// <param name="Handle">The video channel handle (as returned by <see cref="StreamCreateFile" />).</param>
		/// <param name="Stream">Enable (<see langword="true" />) or disable (<see langword="false" />) the streaming of a channel.</param>
		/// <returns>If an error occurred then <see langword="false" /> is returned, else <see langword="true" /> is returned (use <see cref="LastError" /> to get the error code).</returns>
		/// <remarks>Make sure that xVirtualCam is registered and running when calling this function, otherwise this function will return <see langword="false" />.</remarks>
		[DllImport(DllName, EntryPoint = "xVideo_VCamStreamChannel")]
		public static extern bool VCamStreamChannel(int Handle, bool Stream);
	}
}