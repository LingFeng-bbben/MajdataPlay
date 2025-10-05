using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Cd
{
    /// <summary>
    /// BassCd is a BASS addon enabling digital streaming and ripping of audio CDs. Also includes analog playback routines.
    /// </summary>
    /// <remarks>
    /// Supports: .cda
    /// </remarks>
    public static class BassCd
    {
        const string DllName = "basscd";
        		
        /// <summary>
        /// Track Pregap constant
        /// </summary>
        public const int TrackPregap = 0xFFFF;
        static IntPtr _cddbServer;
        
        /// <summary>
        /// Gets the number of CD Drives available.
        /// </summary>
        public static int DriveCount
        {
            get
            {
                int i;
                for (i = 0; GetInfo(i, out var info); i++) { }

                return i;
            }
        }

        #region Configuration
        /// <summary>
        /// Automatically free an existing stream when creating a new one on the same drive? (enabled by Default)
        /// </summary>
        /// <remarks>
        /// Only one stream can exist at a time per CD drive. 
        /// So if a stream using the same drive already exists, stream creation function calls
        /// will fail, unless this config option is enabled to automatically free the existing stream.
        /// </remarks>
        public static bool FreeOld
        {
            get => Bass.GetConfigBool(Configuration.CDFreeOld);
            set => Bass.Configure(Configuration.CDFreeOld, value);
        }

        /// <summary>
        /// Number of times to retry after a read error... 0 = don't retry, default = 2.
        /// </summary>
        public static int RetryCount
        {
            get => Bass.GetConfig(Configuration.CDRetry);
            set => Bass.Configure(Configuration.CDRetry, value);
        }

        /// <summary>
        /// Automatically reduce the read speed when a read error occurs? (default is disabled).
        /// If true, the read speed will be halved when a read error occurs, before retrying (if the <see cref="RetryCount"/> setting allows).
        /// </summary>
        public static bool AutoSpeedReduction
        {
            get => Bass.GetConfigBool(Configuration.CDAutoSpeed);
            set => Bass.Configure(Configuration.CDAutoSpeed, value);
        }

        /// <summary>
        /// Skip past read errors?
        /// If true, reading will skip onto the next frame when a read error occurs, otherwise reading will stop.
        /// When skipping an error, it will be replaced with silence, so that the track Length is unaffected. 
        /// Before skipping past an error, BassCd will first retry according to the <see cref="RetryCount"/> setting.
        /// </summary>
        public static bool SkipError
        {
            get => Bass.GetConfigBool(Configuration.CDSkipError);
            set => Bass.Configure(Configuration.CDSkipError, value);
        }

        /// <summary>
        /// The server address to use in CDDB requests, in the form of "User:pass@server:port/path" (default = "freedb.freedb.org").
        /// </summary>
        /// <remarks>
        /// The "User:pass@", ":port" and "/path" parts are optional; only the "server" part is required.
        /// If not provided, the port and path default to 80 and "/~cddb/cddb.cgi", respectively.
        /// The proxy server, as configured via the <see cref="Bass.NetProxy"/> option, is used when connecting to the CDDB server.
        /// </remarks>
        public static string CDDBServer
        {
            get => Marshal.PtrToStringAnsi(Bass.GetConfigPtr(Configuration.CDDBServer));
            set
            {
                if (_cddbServer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_cddbServer);
                    _cddbServer = IntPtr.Zero;
                }

                _cddbServer = Marshal.StringToHGlobalAnsi(value);

                Bass.Configure(Configuration.CDDBServer, _cddbServer);
            }
        }
        #endregion
        
		/// <summary>
		/// Releases a drive to allow other applications to access it.
		/// </summary>
		/// <param name="Drive">The drive to release... 0 = the first drive.</param>
		/// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// When using the SPTI interface, some applications may require BassCd to release a CD drive before the app is able to use it.
		/// After a drive has been released, BassCd will attempt to re-acquire it in the next BassCd function call made on it.
		/// </remarks>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        /// <exception cref="Errors.NotAvailable">The ASPI interface is being used.</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_Release")]
        public static extern bool Release(int Drive);
        
		/// <summary>
		/// Retrieves the current read speed setting of a drive.
		/// </summary>
		/// <param name="Drive">The drive... 0 = the first drive.</param>
		/// <returns>If successful, the read speed (in KB/s) is returned, else -1 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>Divide the speed by 176.4 to get the real-time speed multiplier, eg. 5645 / 176.4 = "32x speed".</remarks>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        /// <exception cref="Errors.NotAvailable">The read speed is unavailable.</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_GetSpeed")]
        public static extern int GetSpeed(int Drive);
        
		/// <summary>
		/// Retrieves the current read speed multiplier of a drive.
		/// </summary>
		/// <param name="Drive">The drive... 0 = the first drive.</param>
		/// <returns>If successful, the read speed multiplier is returned, else -1 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        /// <exception cref="Errors.NotAvailable">The read speed is unavailable.</exception>
        public static double GetSpeedMultiplier(int Drive)
        {
            double speed = GetSpeed(Drive);

            return speed < 0 ? -1 : speed / 176.4;
        }

		/// <summary>
		/// Sets the read speed of a drive in KB/s.
		/// </summary>
		/// <param name="Drive">The drive... 0 = the first drive.</param>
		/// <param name="Speed">The speed, in KB/s.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// The speed is automatically restricted (rounded down) to what's supported by the drive, so may not be exactly what was requested.
        /// <see cref="GetSpeed" /> can be used to check that. 
		/// The maximum supported speed can be retrieved via <see cref="GetInfo(int, out CDInfo)" />.
        /// </para>
		/// <para>To use a real-time speed multiplier, multiply it by 176.4 (and round up) to get the KB/s speed to use with this function, eg. "32x speed" = 32 * 176.4 = 5645.</para>
		/// </remarks>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_SetSpeed")]
        public static extern bool SetSpeed(int Drive, int Speed);
        
		/// <summary>
		/// Checks if there is a CD ready in a drive.
		/// </summary>
		/// <param name="Drive">The drive to check... 0 = the first drive.</param>
		/// <returns>If there is a CD ready in the drive, then <see langword="true" /> is returned, else <see langword="false" /> is returned.</returns>
		/// <remarks>This function only returns <see langword="true" /> once there's a CD in the drive, and it's ready to be accessed.</remarks>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_IsReady")]
        public static extern bool IsReady(int Drive);
        
		/// <summary>
		/// Retrieves information on a drive.
		/// </summary>
		/// <param name="Drive">The drive to get info on... 0 = the first drive.</param>
		/// <param name="Info">An instance of the <see cref="CDInfo" /> structure to store the information at.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_GetInfo")]
        public static extern bool GetInfo(int Drive, out CDInfo Info);
        
		/// <summary>
		/// Retrieves information on a drive.
		/// </summary>
		/// <param name="Drive">The drive to get info on... 0 = the first drive.</param>
		/// <returns>An instance of the <see cref="CDInfo" /> structure is returned. Throws <see cref="BassException"/> on Error.</returns>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        public static CDInfo GetInfo(int Drive)
        {
            if (!GetInfo(Drive, out var info))
                throw new BassException();
            return info;
        }

        #region CreateStream
		/// <summary>
		/// Creates a sample stream from an audio CD track.
		/// </summary>
		/// <param name="Drive">The drive... 0 = the first drive.</param>
		/// <param name="Track">The track... 0 = the first track, <see cref="TrackPregap"/> = 1st track pregap (not all drives support reading of the 1st track pregap).</param>
		/// <param name="Flags">A combination of <see cref="BassFlags"/></param>
		/// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// Only one stream can exist at a time per CD drive.
        /// If a stream using the drive already exists, this function will fail, unless the <see cref="FreeOld"/> config option is enabled. 
		/// Note that <see cref="StreamSetTrack" /> can be used to change track without creating a new stream.
        /// </para>
		/// <para>
        /// The sample format of a CD audio stream is always 44100hz stereo 16-bit, unless the <see cref="BassFlags.Float"/> flag is used, in which case it's converted to 32-bit.
        /// When reading sub-channel data, the sample rate will be 45900hz, taking the additional sub-channel data into account.
        /// </para>
		/// <para>
        /// When reading sub-channel data, BASSCD will automatically de-interleave the data if the drive can't.
        /// You can check whether the drive can de-interleave the data itself (or even read sub-channel data at all) in the the <see cref="CDInfo.ReadWriteFlags"/> member.
        /// </para>
		/// <para>
        /// When using the <see cref="BassFlags.Decode"/> flag, it's not possible to play the stream, but seeking is still possible.
        /// Because the decoded sample data is not outputted, "decoding channels" can still be used when there is no output device (using the <see cref="Bass.NoSoundDevice"/> device with <see cref="Bass.Init" />).
        /// </para>
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Bass.Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        /// <exception cref="Errors.Already">A stream using this drive already exists.</exception>
        /// <exception cref="Errors.Parameter">The <see cref="BassFlags.CDSubChannel"/> and <see cref="BassFlags.CdC2Errors"/> flags cannot be used without the <see cref="BassFlags.Decode"/> flag or with the <see cref="BassFlags.Float"/> flag. See <see cref="CreateStream(int,int,BassFlags,CDDataProcedure,IntPtr)" />.</exception>
        /// <exception cref="Errors.NoCD">There's no CD in the drive.</exception>
        /// <exception cref="Errors.CDTrack"><paramref name="Track" /> is invalid.</exception>
        /// <exception cref="Errors.NotAudioTrack">The track is not an audio track.</exception>
        /// <exception cref="Errors.NotAvailable">Reading sub-channel data and/or C2 error info is not supported by the drive, or a read offset is in effect. In case of the latter, see <see cref="CreateStream(int,int,BassFlags,CDDataProcedure,IntPtr)" />.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If using the <see cref="BassFlags.Float"/> flag, it could be that floating-point channels are not supported (ie. no WDM drivers).</exception>
        /// <exception cref="Errors.Speaker">The device/drivers do not support the requested speaker(s), or you're attempting to assign a stereo stream to a mono speaker.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_StreamCreate")]
        public static extern int CreateStream(int Drive, int Track, BassFlags Flags);

        [DllImport(DllName)]
        static extern int BASS_CD_StreamCreateEx(int Drive, int Track, BassFlags Flags, CDDataProcedure proc, IntPtr user);

        /// <summary>
        /// Creates a sample stream from an audio CD track, optionally providing a callback function to receive sub-channel data and/or C2 error info.
        /// </summary>
        /// <param name="Drive">The drive... 0 = the first drive.</param>
        /// <param name="Track">The track... 0 = the first track, <see cref="TrackPregap"/> = 1st track pregap (not all drives support reading of the 1st track pregap).</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <param name="Procedure">A callback function to receive sub-channel data and C2 error info... <see langword="null" /> = no callback. If a callback function is provided, sub-channel data and C2 error info will be delivered to it rather than being inserted amongst the sample data.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// This function is identical to <see cref="CreateStream(int,int,BassFlags)" />, but with the additional option of providing a callback function to receive sub-channel data and C2 error info.
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Bass.Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        /// <exception cref="Errors.Already">A stream using this drive already exists.</exception>
        /// <exception cref="Errors.Parameter">The <see cref="BassFlags.CDSubChannel"/> and <see cref="BassFlags.CdC2Errors"/> flags cannot be used without the <see cref="BassFlags.Decode"/> flag or with the <see cref="BassFlags.Float"/> flag, unless a <see cref="CDDataProcedure" /> is provided.</exception>
        /// <exception cref="Errors.NoCD">There's no CD in the drive.</exception>
        /// <exception cref="Errors.CDTrack"><paramref name="Track" /> is invalid.</exception>
        /// <exception cref="Errors.NotAudioTrack">The track is not an audio track.</exception>
        /// <exception cref="Errors.NotAvailable">Reading sub-channel data and/or C2 error info is not supported by the drive, or a read offset is in effect, in which case a , in which case a <see cref="CDDataProcedure" /> must be provided to receive sub-channel data or C2 error info.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If using the <see cref="BassFlags.Float"/> flag, it could be that floating-point channels are not supported (ie. no WDM drivers).</exception>
        /// <exception cref="Errors.Speaker">The device/drivers do not support the requested speaker(s), or you're attempting to assign a stereo stream to a mono speaker.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int CreateStream(int Drive, int Track, BassFlags Flags, CDDataProcedure Procedure, IntPtr User = default(IntPtr))
        {
            var h = BASS_CD_StreamCreateEx(Drive, Track, Flags, Procedure, User);

            if (h != 0)
                ChannelReferences.Add(h, 0, Procedure);

            return h;
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_CD_StreamCreateFile(string File, BassFlags Flags);

        /// <summary>
        /// Creates a sample stream from an audio CD track, using a CDA file on the CD.
        /// </summary>
        /// <param name="File">The CDA filename... for example, "D:\Track01.cda".</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// Only one stream can exist at a time per CD drive.
        /// If a stream using the drive already exists, this function will fail, unless the <see cref="FreeOld"/> config option is enabled. 
        /// Note that <see cref="StreamSetTrack" /> can be used to change track without creating a new stream.
        /// </para>
        /// <para>
        /// The sample format of a CD audio stream is always 44100hz stereo 16-bit, unless the <see cref="BassFlags.Float"/> flag is used, in which case it's converted to 32-bit.
        /// When reading sub-channel data, the sample rate will be 45900hz, taking the additional sub-channel data into account.
        /// </para>
        /// <para>
        /// When reading sub-channel data, BASSCD will automatically de-interleave the data if the drive can't.
        /// You can check whether the drive can de-interleave the data itself (or even read sub-channel data at all) in the the <see cref="CDInfo.ReadWriteFlags"/> member.
        /// </para>
        /// <para>
        /// When using the <see cref="BassFlags.Decode"/> flag, it's not possible to play the stream, but seeking is still possible.
        /// Because the decoded sample data is not outputted, "decoding channels" can still be used when there is no output device (using the <see cref="Bass.NoSoundDevice"/> device with <see cref="Bass.Init" />).
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Bass.Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Already">A stream using this drive already exists.</exception>
        /// <exception cref="Errors.FileOpen">The file could not be opened.</exception>
        /// <exception cref="Errors.FileFormat">The file was not recognised as a CDA file.</exception>
        /// <exception cref="Errors.Parameter">The <see cref="BassFlags.CDSubChannel"/> and <see cref="BassFlags.CdC2Errors"/> flags cannot be used without the <see cref="BassFlags.Decode"/> flag or with the <see cref="BassFlags.Float"/> flag. See <see cref="CreateStream(string,BassFlags,CDDataProcedure,IntPtr)" />.</exception>
        /// <exception cref="Errors.NoCD">There's no CD in the drive.</exception>
        /// <exception cref="Errors.NotAudioTrack">The track is not an audio track.</exception>
        /// <exception cref="Errors.NotAvailable">Reading sub-channel data and/or C2 error info is not supported by the drive, or a read offset is in effect. In case of the latter, see <see cref="CreateStream(string,BassFlags,CDDataProcedure,IntPtr)" />.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If using the <see cref="BassFlags.Float"/> flag, it could be that floating-point channels are not supported (ie. no WDM drivers).</exception>
        /// <exception cref="Errors.Speaker">The device/drivers do not support the requested speaker(s), or you're attempting to assign a stereo stream to a mono speaker.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int CreateStream(string File, BassFlags Flags)
        {
            return BASS_CD_StreamCreateFile(File, Flags | BassFlags.Unicode);
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_CD_StreamCreateFileEx(string File, BassFlags Flags, CDDataProcedure proc, IntPtr user = default(IntPtr));

        /// <summary>
		/// Creates a sample stream from an audio CD track, using a CDA file on the CD, optionally providing a callback function to receive sub-channel data and/or C2 error info.
		/// </summary>
		/// <param name="File">The CDA filename... for example, "D:\Track01.cda".</param>
		/// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
		/// <param name="Procedure">A callback function to receive sub-channel data and C2 error info... <see langword="null" /> = no callback. If a callback function is provided, sub-channel data and C2 error info will be delivered to it rather than being inserted amongst the sample data.</param>
		/// <param name="User">User instance data to pass to the callback function.</param>
		/// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// This function is identical to <see cref="CreateStream(string,BassFlags)" />, but with the additional option of providing a callback function to receive sub-channel data and C2 error info.
		/// </remarks>
        /// <exception cref="Errors.Init"><see cref="Bass.Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.Already">A stream using this drive already exists.</exception>
        /// <exception cref="Errors.FileOpen">The file could not be opened.</exception>
        /// <exception cref="Errors.FileFormat">The file was not recognised as a CDA file.</exception>
        /// <exception cref="Errors.Parameter">The <see cref="BassFlags.CDSubChannel"/> and <see cref="BassFlags.CdC2Errors"/> flags cannot be used without the <see cref="BassFlags.Decode"/> flag or with the <see cref="BassFlags.Float"/> flag, unless a <see cref="CDDataProcedure" /> is provided.</exception>
        /// <exception cref="Errors.NoCD">There's no CD in the drive.</exception>
        /// <exception cref="Errors.NotAudioTrack">The track is not an audio track.</exception>
        /// <exception cref="Errors.NotAvailable">Reading sub-channel data and/or C2 error info is not supported by the drive, or a read offset is in effect, in which case a <see cref="CDDataProcedure" /> must be provided to receive sub-channel data or C2 error info.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If using the <see cref="BassFlags.Float"/> flag, it could be that floating-point channels are not supported (ie. no WDM drivers).</exception>
        /// <exception cref="Errors.Speaker">The device/drivers do not support the requested speaker(s), or you're attempting to assign a stereo stream to a mono speaker.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
		public static int CreateStream(string File, BassFlags Flags, CDDataProcedure Procedure, IntPtr User = default(IntPtr))
        {
            var h = BASS_CD_StreamCreateFileEx(File, Flags | BassFlags.Unicode, Procedure, User);

            if (h != 0)
                ChannelReferences.Add(h, 0, Procedure);

            return h;
        }
        #endregion

        /// <summary>
        /// Retrieves the drive and track number of a CD stream.
        /// </summary>
        /// <param name="Handle">The CD stream handle.</param>
        /// <returns>
        /// If an error occurs, -1 is returned, use <see cref="Bass.LastError" /> to get the error code. 
        /// If successful, the track number is returned in the low word (low 16-bits), and the drive is returned in the high word (high 16-bits).
        /// </returns>
        /// <remarks>
        /// If the track has just changed, this function will give the new track number even if the old track is still being heard due to buffering.
        /// The <see cref="PositionFlags.CDTrack"/> mode can be used with <see cref="Bass.ChannelGetPosition(int, PositionFlags)" /> to get the track currently being heard.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_StreamGetTrack")]
        public static extern int StreamGetTrack(int Handle);

        /// <summary>
        /// Changes the track of a CD stream.
        /// </summary>
        /// <param name="Handle">The CD stream handle.</param>
        /// <param name="Track">The new track... 0 = the first track, <see cref="TrackPregap"/> = 1st track pregap (not all drives support reading of the 1st track pregap).</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The stream's current position is set to the start of the new track.
        /// <para>
        /// This function is identical to using the <see cref="PositionFlags.CDTrack"/> mode with <see cref="Bass.ChannelSetPosition(int, long, PositionFlags)" />.
        /// Either can be used with a <see cref="SyncFlags.End"/> sync (set via <see cref="Bass.ChannelSetSync(int, SyncFlags, long, SyncProcedure, IntPtr)" />) to play one track after another.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NoCD">There's no CD in the drive.</exception>
        /// <exception cref="Errors.CDTrack"><paramref name="Track" /> is invalid.</exception>
        /// <exception cref="Errors.NotAudioTrack">The track is not an audio track.</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_StreamSetTrack")]
        public static extern bool StreamSetTrack(int Handle, int Track);
        
		/// <summary>
		/// Opens, closes, locks or unlocks a drive door.
		/// </summary>
		/// <param name="Drive">The drive... 0 = the first drive.</param>
		/// <param name="Action">The action to perform... one of <see cref="CDDoorAction"/>.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Action" /> is invalid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem! Could be that the door is locked.</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_Door")]
        public static extern bool Door(int Drive, CDDoorAction Action);
        
		/// <summary>
		/// Checks if a drive door/tray is locked.
		/// </summary>
		/// <param name="Drive">The drive to check... 0 = the first drive.</param>
		/// <returns><see langword="true" /> is returned if the door is locked, else <see langword="false" /> is returned.</returns>
		/// <remarks>
        /// It is not possible to get the drive's current door status via the WIO interface. 
		/// So the last known status will be returned in that case, which may not be accurate if the door has been opened or closed by another application.
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_CD_DoorIsLocked")]
        public static extern bool DoorIsLocked(int Drive);
        
		/// <summary>
		/// Checks if a drive door/tray is open.
		/// </summary>
		/// <param name="Drive">The drive to check... 0 = the first drive.</param>
		/// <returns><see langword="true" /> is returned if the door is open, else <see langword="false" /> is returned.</returns>
		/// <remarks>
        /// It is not possible to get the drive's current door status via the WIO interface. 
		/// So the last known status will be returned in that case, which may not be accurate if the door has been opened or closed by another application, or manually.
        /// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_CD_DoorIsOpen")]
        public static extern bool DoorIsOpen(int Drive);
        
		/// <summary>
		/// Sets the interface to use to access CD drives
		/// </summary>
		/// <param name="iface">The interface to use, which can be one of the <see cref="CDInterface"/> values.</param>
		/// <returns>If successful, the interface being used is returned, else -1 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// The interface can be changed at any time, but any existing CD streams will be freed in doing so. 
		/// The current interface can also be reinitialized, to detect any newly connected drives.
		/// <para>Use of this function is optional. If it is not used, BassCd will automatically detect an available interface.</para>
		/// </remarks>
        /// <exception cref="Errors.Parameter"><paramref name="iface" /> is invalid.</exception>
        /// <exception cref="Errors.NotAvailable">The interface is not available, or has no drives available.</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_SetInterface")]
        public static extern int SetInterface(CDInterface iface);
        
		/// <summary>
		/// Sets the read offset of a drive.
		/// </summary>
		/// <param name="Drive">The drive... 0 = the first drive.</param>
		/// <param name="Offset">The offset (in samples; bytes/4) to set.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// This function can be used to compensate for the fact that most drives will read audio data from CDs at a slight offset from where they ideally should.
		/// Different drive models will have differing offsets.
		/// <para>
        /// When a negative offset is used, reading the beginning of the first track will require accessing the lead-in, and when a positive offset is used, reading the end of the last track will require accessing the lead-out.
		/// The drive may not support that (overreading), in which case those parts will be replaced with silence.
        /// </para>
		/// <para>Changes do not affect an existing CD stream, unless <see cref="StreamSetTrack" /> is called (and any sub-channel/C2 reading is using a <see cref="CDDataProcedure" />).</para>
		/// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_CD_SetOffset")]
        public static extern bool SetOffset(int Drive, int Offset);
        
        [DllImport(DllName)]
        static extern IntPtr BASS_CD_GetID(int Drive, CDID ID);

        /// <summary>
        /// Retrieves identification info from the CD in a drive.
        /// </summary>
        /// <param name="Drive">The drive... 0 = the first drive.</param>
        /// <param name="ID">The identification to retrieve. For <see cref="CDID.Text"/> use <see cref="GetIDText"/>.</param>
        /// <returns>The identication info on success, else<see langword="null" />. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        /// <exception cref="Errors.NoCD">There's no CD in the drive.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="ID" /> is invalid.</exception>
        /// <exception cref="Errors.NotAvailable">The CD does not have a UPC, ISRC or CD-TEXT info, or the CDDB Read entry number is not valid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static string GetID(int Drive, CDID ID)
        {
            var ptr = BASS_CD_GetID(Drive, ID);
            
            switch (ID)
            {
                case CDID.Text:
                    throw new InvalidOperationException($"Use {nameof(GetIDText)} overload instead.");

                case CDID.Query:
                case CDID.Read:
                case CDID.ReadCache:
                    return Extensions.PtrToStringUtf8(ptr);

                default:
                    return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <summary>
        /// Retrieves CD-Text identification info from the CD in a drive.
        /// </summary>
        /// <param name="Drive">The drive... 0 = the first drive.</param>
        /// <returns>
        /// If an error occurs, <see langword="null" /> is returned, use <see cref="Bass.LastError" /> to get the error code. 
        /// If successful, a string array of all CD-Text tags is returned in the form of "tag=text".
        /// </returns>
        /// <remarks>
        /// The returned identification string will remain in memory until the next call to this function, when it'll be overwritten by the next result.
        /// If you need to keep the contents of an identification string, then you should copy it before calling this function again.
        /// </remarks>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        /// <exception cref="Errors.NoCD">There's no CD in the drive.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Drive" /> is invalid.</exception>
        /// <exception cref="Errors.NotAvailable">The CD does not have a UPC, ISRC or CD-TEXT info.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static string[] GetIDText(int Drive) => Extensions.ExtractMultiStringAnsi(BASS_CD_GetID(Drive, CDID.Text));

        /// <summary>
        /// Retrieves the TOC from the CD in a drive.
        /// </summary>
        /// <param name="Drive">The drive to get info on... 0 = the first drive.</param>
        /// <param name="Mode">TOC Mode.</param>
        /// <param name="TOC">An instance of the <see cref="TOC" /> structure to store the information at.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>This function gives the TOC in the form that it is delivered by the drive, except that the byte order may be changed to match the system's native byte order (the TOC is originally big-endian).</remarks>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is not valid.</exception>
        /// <exception cref="Errors.NoCD">There's no CD in the drive.</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_GetTOC")]
        public static extern bool GetTOC(int Drive, TOCMode Mode, out TOC TOC);
        
		/// <summary>
		/// Retrieves the TOC from the CD in a drive.
		/// </summary>
		/// <param name="Drive">The drive to get info on... 0 = the first drive.</param>
		/// <param name="Mode">TOC Mode.</param>
		/// <returns>If successful, an instance of the <see cref="TOC" /> strucure is returned. Throws <see cref="BassException"/> on Error.</returns>
		/// <remarks>This function gives the TOC in the form that it is delivered by the drive, except that the byte order may be changed to match the system's native byte order (the TOC is originally big-endian).</remarks>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is not valid.</exception>
        /// <exception cref="Errors.NoCD">There's no CD in the drive.</exception>
        public static TOC GetTOC(int Drive, TOCMode Mode)
        {
            if (!GetTOC(Drive, Mode, out var toc))
                throw new BassException();
            return toc;
        }
        
		/// <summary>
		/// Retrieves the number of tracks on the CD in a drive.
		/// </summary>
		/// <param name="Drive">The drive... 0 = the first drive.</param>
		/// <returns>If an error occurs, -1 is returned, use <see cref="Bass.LastError" /> to get the error code. If successful, the number of tracks on the CD is returned.</returns>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is not valid.</exception>
        /// <exception cref="Errors.NoCD">There's no CD in the drive.</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_GetTracks")]
        public static extern int GetTracks(int Drive);

        /// <summary>
        /// Retrieves the length (in bytes) of a track.
        /// </summary>
        /// <param name="Drive">The drive... 0 = the first drive.</param>
        /// <param name="Track">The track to retrieve the length of... 0 = the first track.</param>
        /// <returns>If an error occurs, -1 is returned, use <see cref="Bass.LastError" /> to get the error code. If successful, the length of the track is returned.</returns>
        /// <remarks>
        /// CD audio is always 44100hz stereo 16-bit.
        /// That's 176400 bytes per second.
        /// So dividing the track length by 176400 gives the length in seconds.
        /// </remarks>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        /// <exception cref="Errors.NoCD">There's no CD in the drive.</exception>
        /// <exception cref="Errors.CDTrack">The <paramref name="Track" /> number is invalid.</exception>
        /// <exception cref="Errors.NotAudioTrack">The track is not an audio track.</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_GetTrackLength")]
        public static extern int GetTrackLength(int Drive, int Track);

        /// <summary>
        /// Retrieves the pregap length (in bytes) of a track.
        /// </summary>
        /// <param name="Drive">The drive... 0 = the first drive.</param>
        /// <param name="Track">The track to retrieve the pregap length of... 0 = the first track.</param>
        /// <returns>If an error occurs, -1 is returned, use <see cref="Bass.LastError" /> to get the error code. 
        /// If successful, the pregap length of the track is returned. To translate the pregap length from bytes to frames, divide by 2352.</returns>
        /// <remarks>
        /// The drive needs to support sub-channel reading in order to detect all but the first pregap length. 
        /// <see cref="CDInfo.ReadWriteFlags" /> can be used to check whether the drive can read sub-channel data.
        /// <para>
        /// A track's pregap is actually played as part of the preceeding track. 
        /// So to remove the gap from the end of a track, you would get the pregap length of the following track. 
        /// The gap will usually contain silence, but it doesn't have to - it could contain crowd noise in a live recording, for example.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        /// <exception cref="Errors.NoCD">There's no CD in the drive.</exception>
        /// <exception cref="Errors.CDTrack">The <paramref name="Track" /> number is invalid.</exception>
        /// <exception cref="Errors.NotAudioTrack">The track is not an audio track.</exception>
        /// <exception cref="Errors.NotAvailable">Reading sub-channel data is not supported by the drive.</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_GetTrackPregap")]
        public static extern int GetTrackPregap(int Drive, int Track);

        #region Analog
        /// <summary>
        /// Retrieves the current position and track on a drive.
        /// </summary>
        /// <param name="Drive">The drive... 0 = the first drive.</param>
        /// <returns>
        /// If an error occurs, -1 is returned, use <see cref="Bass.LastError" /> to get the error code. 
        /// If successful, the HIWORD contains the track number (0=first), and the LOWORD contains the offset (in frames).
        /// </returns>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_Analog_GetPosition")]
        public static extern int AnalogGetPosition(int Drive);

        /// <summary>
        /// Checks if analog playback is in progress on a drive.
        /// </summary>
        /// <param name="Drive">The drive... 0 = the first drive.</param>
        /// <returns>The return value is <see cref="PlaybackState.Stopped"/> or <see cref="PlaybackState.Playing"/>.</returns>
        [DllImport(DllName, EntryPoint = "BASS_CD_Analog_IsActive")]
        public static extern bool AnalogIsActive(int Drive);

        /// <summary>
        /// Starts analog playback of an audio CD track.
        /// </summary>
        /// <param name="Drive">The drive... 0 = the first drive.</param>
        /// <param name="Track">The track... 0 = the first track.</param>
        /// <param name="Position">Position (in frames) to start playback from. There are 75 frames per second.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// Some old CD drives may not be able to digitally extract audio data (or not quickly enough to sustain playback), so that it's not possible to use <see cref="CreateStream(int, int, BassFlags)" /> to stream CD tracks. 
        /// This is where the analog playback option can come in handy.
        /// </para>
        /// <para>
        /// In analog playback, the sound bypasses Bass - it goes directly from the CD drive to the soundcard (assuming the drive is cabled up to the soundcard). 
        /// This means that Bass output does not need to be initialized to use analog playback.
        /// It also means it's not possible to apply any DSP/FX to the sound, and nor is it possible to visualise it (unless you record the sound from the soundcard).
        /// </para>
        /// <para>
        /// Analog playback is not possible while digital streaming is in progress - the streaming will kill the analog playback.
        /// So if you wish to switch from digital to analog playback, you should first free the stream using <see cref="Bass.StreamFree" />.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is not valid.</exception>
        /// <exception cref="Errors.NoCD">There's no CD in the drive.</exception>
        /// <exception cref="Errors.CDTrack"><paramref name="Track" /> is invalid.</exception>
        /// <exception cref="Errors.NotAudioTrack">The track is not an audio track.</exception>
        /// <exception cref="Errors.Position"><paramref name="Position" /> is invalid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_Analog_Play")]
        public static extern bool AnalogPlay(int Drive, int Track, int Position);

        /// <summary>
        /// Starts analog playback of an audio CD track, using a CDA file on the CD.
        /// </summary>
        /// <param name="FileName">The CDA filename... for example, "D:\Track01.cda".</param>
        /// <param name="Position">Position (in frames) to start playback from. There are 75 frames per second.</param>
        /// <returns>If successful, the number of the drive being used is returned, else -1 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// Some old CD drives may not be able to digitally extract audio data (or not quickly enough to sustain playback), so that it's not possible to use <see cref="CreateStream(string, BassFlags)" /> to stream CD tracks.
        /// This is where the analog playback option can come in handy.
        /// </para>
        /// <para>
        /// In analog playback, the sound bypasses Bass - it goes directly from the CD drive to the soundcard (assuming the drive is cabled up to the soundcard). 
        /// This means that Bass output does not need to be initialized to use analog playback.
        /// It also means it's not possible to apply any DSP/FX to the sound, and nor is it possible to visualise it (unless you record the sound from the soundcard).
        /// </para>
        /// <para>
        /// Analog playback is not possible while digital streaming is in progress - the streaming will kill the analog playback.
        /// So if you wish to switch from digital to analog playback, you should first free the stream using <see cref="Bass.StreamFree" />.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.FileOpen">The file could not be opened.</exception>
        /// <exception cref="Errors.FileFormat">The file was not recognised as a CDA file.</exception>
        /// <exception cref="Errors.Device">The Drive could not be found.</exception>
        /// <exception cref="Errors.Position"><paramref name="Position" /> is invalid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_Analog_PlayFile")]
        public static extern bool AnalogPlay(string FileName, int Position);
        
		/// <summary>
		/// Stops analog playback on a drive.
		/// </summary>
		/// <param name="Drive">The drive... 0 = the first drive.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>Pausing can be achieved by getting the position (<see cref="AnalogGetPosition" />) just before stopping, and then using that position in a call to <see cref="AnalogPlay(int, int, int)" /> to resume.</remarks>
        /// <exception cref="Errors.Device"><paramref name="Drive" /> is invalid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_CD_Analog_Stop")]
        public static extern bool AnalogStop(int Drive);
        #endregion
    }
}