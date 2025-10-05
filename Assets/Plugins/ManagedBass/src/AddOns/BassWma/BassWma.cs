using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ManagedBass.Wma
{
    /// <summary>
    /// BassWma is a BASS addon enabling the playback of WMA files and streams (including user file streams), and also WMA file encoding and network broadcasting.
    /// </summary>
    /// <remarks>
    /// Requires the Windows Media Format modules, which come installed with Windows Media Player, or can be installed separately (wmfdist.exe).
    /// <para>Supports: .wma, .wmv</para>
    /// </remarks>
    public static class BassWma
    {
        const string DllName = "basswma";
        
        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_WMA_StreamCreateFile(bool mem, string file, long offset, long length, BassFlags flags);

        [DllImport(DllName)]
        static extern int BASS_WMA_StreamCreateFile(bool mem, IntPtr file, long offset, long length, BassFlags flags);

        /// <summary>Create a stream from file.</summary>
        public static int CreateStream(string File, long Offset = 0, long Length = 0, BassFlags Flags = BassFlags.Default)
        {
            return BASS_WMA_StreamCreateFile(false, File, Offset, Length, Flags | BassFlags.Unicode);
        }

        /// <summary>Create a stream from Memory (IntPtr).</summary>
        public static int CreateStream(IntPtr Memory, long Offset, long Length, BassFlags Flags = BassFlags.Default)
        {
            return BASS_WMA_StreamCreateFile(true, new IntPtr(Memory.ToInt64() + Offset), 0, Length, Flags);
        }

        /// <summary>Create a stream from Memory (byte[]).</summary>
        public static int CreateStream(byte[] Memory, long Offset, long Length, BassFlags Flags)
        {
            return GCPin.CreateStreamHelper(Pointer => CreateStream(Pointer, Offset, Length, Flags), Memory);
        }

        [DllImport(DllName)]
        static extern int BASS_WMA_StreamCreateFileUser(StreamSystem system, BassFlags flags, [In, Out] FileProcedures procs, IntPtr user);

        /// <summary>Create a stream using User File Procedures.</summary>
        public static int CreateStream(StreamSystem System, BassFlags Flags, FileProcedures Procedures, IntPtr User = default(IntPtr))
        {
            var h = BASS_WMA_StreamCreateFileUser(System, Flags, Procedures, User);

            if (h != 0)
                ChannelReferences.Add(h, 0, Procedures);

            return h;
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_WMA_StreamCreateURL(string Url, int Offset, BassFlags Flags, DownloadProcedure Procedure, IntPtr User);

        /// <summary>Create a stream from Url.</summary>
        public static int CreateStream(string Url, int Offset, BassFlags Flags, DownloadProcedure Procedure, IntPtr User = default(IntPtr))
        {
            var h = BASS_WMA_StreamCreateURL(Url, Offset, Flags | BassFlags.Unicode, Procedure, User);

            if (h != 0)
                ChannelReferences.Add(h, 0, Procedure);

            return h;
        }

        /// <summary>
        /// Retrieves a pointer to the IWMReader interface of a WMA stream, or IWMWriter interface of a WMA encoder.
        /// </summary>
        /// <param name="Handle">The WMA stream or encoder handle.</param>
        /// <returns>If succesful, then a pointer to the requested object is returned, otherwise <see cref="IntPtr.Zero" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// This function allows those that are familiar with the Windows Media Format SDK to access the internal object interface, for extra functionality. 
        /// If you create any objects through a retrieved interface, make sure you release the objects before calling <see cref="Bass.StreamFree" />.
        /// </para>
        /// <para>See the Windows Media Format SDK for information on the IWMReader and associated interfaces.</para>
        /// <para>
        /// When streaming local (not internet) files, this function may actually return an IWMSyncReader interface instead of an IWMReader interface. 
        /// The type of interface can be determined by querying other interfaces from it.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WMA_GetWMObject")]
        public static extern IntPtr GetWMObject(int Handle);

        #region CreateStream Auth
        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_WMA_StreamCreateFileAuth(bool mem, IntPtr file, long offset, long length, BassFlags flags, string user, string pass);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_WMA_StreamCreateFileAuth(bool mem, string file, long offset, long length, BassFlags flags, string user, string pass);

        /// <summary>
        /// Streams a WMA file authenticating using given <paramref name="UserName"/> and <paramref name="Password"/>.
        /// </summary>
        public static int CreateStream(string File, BassFlags Flags, string UserName, string Password)
        {
            return BASS_WMA_StreamCreateFileAuth(false, File, 0, 0, Flags | BassFlags.Unicode, UserName, Password);
        }

        /// <summary>
        /// Streams a WMA file from Memory (<see cref="IntPtr"/>) authenticating using given <paramref name="UserName"/> and <paramref name="Password"/>.
        /// </summary>
        public static int CreateStream(IntPtr Memory, long Length, BassFlags Flags, string UserName, string Password)
        {
            return BASS_WMA_StreamCreateFileAuth(true, Memory, 0, Length, Flags | BassFlags.Unicode, UserName, Password);
        }

        /// <summary>
        /// Streams a WMA file from Memory (byte[]) authenticating using given <paramref name="UserName"/> and <paramref name="Password"/>.
        /// </summary>
        public static int CreateStream(byte[] Memory, long Length, BassFlags Flags, string UserName, string Password)
        {
            return GCPin.CreateStreamHelper(Pointer => CreateStream(Pointer, Length, Flags, UserName, Password), Memory);
        }
        #endregion

        #region Encode
        #region Encode Write
        /// <summary>
        /// Encodes sample data, and writes it to the file or network.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Buffer">Pointer to the buffer containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The WMA codec expects 16-bit or 24-bit sample data depending on the <see cref="WMAEncodeFlags.Encode24Bit"/> flag, but BASSWMA will accept 8-bit, 16-bit or floating-point data, and convert it to the appropriate format.
        /// <para>There is generally no need to call this function if the <see cref="WMAEncodeFlags.Source"/> flag has been set on the encoder, as the encoder will automatically be fed the data that its source BASS channel produces.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WMA_EncodeWrite")]
        public static extern bool EncodeWrite(int Handle, IntPtr Buffer, int Length);

        /// <summary>
        /// Encodes sample data, and writes it to the file or network.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Buffer">byte[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The WMA codec expects 16-bit or 24-bit sample data depending on the <see cref="WMAEncodeFlags.Encode24Bit"/> flag, but BASSWMA will accept 8-bit, 16-bit or floating-point data, and convert it to the appropriate format.
        /// <para>There is generally no need to call this function if the <see cref="WMAEncodeFlags.Source"/> flag has been set on the encoder, as the encoder will automatically be fed the data that its source BASS channel produces.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WMA_EncodeWrite")]
        public static extern bool EncodeWrite(int Handle, byte[] Buffer, int Length);

        /// <summary>
        /// Encodes sample data, and writes it to the file or network.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Buffer">short[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The WMA codec expects 16-bit or 24-bit sample data depending on the <see cref="WMAEncodeFlags.Encode24Bit"/> flag, but BASSWMA will accept 8-bit, 16-bit or floating-point data, and convert it to the appropriate format.
        /// <para>There is generally no need to call this function if the <see cref="WMAEncodeFlags.Source"/> flag has been set on the encoder, as the encoder will automatically be fed the data that its source BASS channel produces.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WMA_EncodeWrite")]
        public static extern bool EncodeWrite(int Handle, short[] Buffer, int Length);

        /// <summary>
        /// Encodes sample data, and writes it to the file or network.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Buffer">int[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The WMA codec expects 16-bit or 24-bit sample data depending on the <see cref="WMAEncodeFlags.Encode24Bit"/> flag, but BASSWMA will accept 8-bit, 16-bit or floating-point data, and convert it to the appropriate format.
        /// <para>There is generally no need to call this function if the <see cref="WMAEncodeFlags.Source"/> flag has been set on the encoder, as the encoder will automatically be fed the data that its source BASS channel produces.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WMA_EncodeWrite")]
        public static extern bool EncodeWrite(int Handle, int[] Buffer, int Length);

        /// <summary>
        /// Encodes sample data, and writes it to the file or network.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Buffer">float[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The WMA codec expects 16-bit or 24-bit sample data depending on the <see cref="WMAEncodeFlags.Encode24Bit"/> flag, but BASSWMA will accept 8-bit, 16-bit or floating-point data, and convert it to the appropriate format.
        /// <para>There is generally no need to call this function if the <see cref="WMAEncodeFlags.Source"/> flag has been set on the encoder, as the encoder will automatically be fed the data that its source BASS channel produces.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WMA_EncodeWrite")]
        public static extern bool EncodeWrite(int Handle, float[] Buffer, int Length);
        #endregion

        /// <summary>
        /// Initializes WMA encoding to a UserName defined function.
        /// </summary>
        /// <param name="Frequency">The sample rate in Hz, or a BASS channel handle if the <see cref="WMAEncodeFlags.Source"/> flag is specified.</param>
        /// <param name="Channels">The number of channels (1=mono, 2=stereo, etc.).</param>
        /// <param name="Flags">A combination of <see cref="WMAEncodeFlags"/></param>
        /// <param name="Bitrate">The encoding bitrate (in bits per second, e.g. 128000), or VBR quality (100 or less).</param>
        /// <param name="Procedure">The UserName defined function to receive the encoded data (see <see cref="WMEncodeProcedure" />).</param>
        /// <param name="User">User instance data to Password to the callback function.</param>
        /// <returns>If succesful, the new encoder's handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>Encoding to a UserName defined function allows any storage or delivery method to be used for the encoded WMA data. For example, encoding to memory.</para>
        /// <para>
        /// The WMA codec expects 16-bit or 24-bit sample data depending on the <see cref="WMAEncodeFlags.Encode24Bit"/> flag, but BASSWMA will accept 8-bit, 16-bit or floating-point data, and convert it to the appropriate format.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.WM9">The Windows Media modules (v9 or above) are not installed.</exception>
        /// <exception cref="Errors.NotAvailable">No codec could be found to support the specified sample format and bitrate.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WMA_EncodeOpen")]
        public static extern int EncodeOpen(int Frequency, int Channels, WMAEncodeFlags Flags, int Bitrate, WMEncodeProcedure Procedure, IntPtr User = default(IntPtr));

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_WMA_EncodeOpenFile(int freq, int chans, WMAEncodeFlags flags, int bitrate, string file);

        /// <summary>
        /// Initializes WMA encoding to a file.
        /// </summary>
        /// <param name="Frequency">The sample rate in Hz, or a BASS channel handle if the <see cref="WMAEncodeFlags.Source"/> flag is specified.</param>
        /// <param name="Channels">The number of channels (1=mono, 2=stereo, etc.).</param>
        /// <param name="Flags">A combination of <see cref="WMAEncodeFlags"/>.</param>
        /// <param name="Bitrate">The encoding bitrate (in bits per second, e.g. 128000), or VBR quality (100 or less).</param>
        /// <param name="File">The filename to write.</param>
        /// <returns>If succesful, the new encoder's handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The WMA codec expects 16-bit or 24-bit sample data depending on the <see cref="WMAEncodeFlags.Encode24Bit"/> flag, but BASSWMA will accept 8-bit, 16-bit or floating-point data, and convert it to the appropriate format.
        /// </remarks>
        /// <exception cref="Errors.WM9">The Windows Media modules (v9 or above) are not installed.</exception>
        /// <exception cref="Errors.NotAvailable">No codec could be found to support the specified sample format and bitrate.</exception>
        /// <exception cref="Errors.Create">Could not create the file to write the WMA stream.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int EncodeOpenFile(int Frequency, int Channels, WMAEncodeFlags Flags, int Bitrate, string File)
        {
            return BASS_WMA_EncodeOpenFile(Frequency, Channels, Flags | WMAEncodeFlags.Unicode, Bitrate, File);
        }

        /// <summary>
        /// Initializes WMA encoding to the network.
        /// </summary>
        /// <param name="Frequency">The sample rate in Hz, or a BASS channel handle if the <see cref="WMAEncodeFlags.Source"/> flag is specified.</param>
        /// <param name="Channels">The number of channels (1=mono, 2=stereo, etc.).</param>
        /// <param name="Flags">A combination of <see cref="WMAEncodeFlags"/>.</param>
        /// <param name="Bitrate">The encoding bitrate (in bits per second, e.g. 128000), or VBR quality (100 or less).</param>
        /// <param name="Port">The port number for clients to conenct to... 0 = let the system choose a port.</param>
        /// <param name="Clients">The maximum number of clients (up to 50) that can be connected.</param>
        /// <returns>If succesful, the new encoder's handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>If you chose to let the system select a port, you can retrieve the port number using <see cref="EncodeGetPort" />.</para>
        /// <para>
        /// The WMA codec expects 16-bit or 24-bit sample data depending on the <see cref="WMAEncodeFlags.Encode24Bit"/> flag, but BASSWMA will accept 8-bit, 16-bit or floating-point data, and convert it to the appropriate format.
        /// VBR encoding is not recommended for network encoding.
        /// </para>
        /// <para>The <see cref="WMAEncodeFlags.Queue"/> flag is not necessary with this function as the data is always queued and fed to the encoder asynchronously.</para>
        /// </remarks>
        /// <exception cref="Errors.WM9">The Windows Media modules (v9 or above) are not installed.</exception>
        /// <exception cref="Errors.NotAvailable">No codec could be found to support the specified sample format and bitrate.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Clients" /> is invalid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WMA_EncodeOpenNetwork")]
        public static extern int EncodeOpenNetwork(int Frequency, int Channels, WMAEncodeFlags Flags, int Bitrate, int Port, int Clients);

        /// <summary>
        /// Initializes WMA encoding to the network, using multiple bitrates.
        /// </summary>
        /// <param name="Frequency">The sample rate in Hz, or a BASS channel handle if the <see cref="WMAEncodeFlags.Source"/> flag is specified.</param>
        /// <param name="Channels">The number of channels (1=mono, 2=stereo, etc.).</param>
        /// <param name="Flags">A combination of <see cref="WMAEncodeFlags"/>.</param>
        /// <param name="Bitrates">Array of encoding bitrates (in bits per second, e.g. 128000) to use, terminated with a 0 element (so the number of elements in the array must be one more than the effective bitrates used).</param>
        /// <param name="Port">The port number for clients to conenct to... 0 = let the system choose a port.</param>
        /// <param name="Clients">The maximum number of clients (up to 50) that can be connected.</param>
        /// <returns>If succesful, the new encoder's handle is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// This function is identical to <see cref="EncodeOpenNetwork(int,int,WMAEncodeFlags,int,int,int)" />, but with the additional ability to specify multiple bitrates.
        /// <para>When encoding/broadcasting in multiple bitrates, the UserName will automatically get the best available bitrate for their bandwidth.</para>
        /// <para>
        /// The WMA codec expects 16-bit or 24-bit sample data depending on the <see cref="WMAEncodeFlags.Encode24Bit"/> flag, but BASSWMA will accept 8-bit, 16-bit or floating-point data, and convert it to the appropriate format.
        /// VBR encoding is not recommended for network encoding.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.WM9">The Windows Media modules (v9 or above) are not installed.</exception>
        /// <exception cref="Errors.NotAvailable">No codec could be found to support the specified sample format and bitrate.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Clients" /> is invalid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WMA_EncodeOpenNetworkMulti")]
        public static extern int EncodeOpenNetwork(int Frequency, int Channels, WMAEncodeFlags Flags, int[] Bitrates, int Port, int Clients);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_WMA_EncodeOpenPublish(int freq, int chans, WMAEncodeFlags flags, int bitrate, string url, string user, string pass);

        /// <summary>
        /// Initializes WMA encoding to a publishing point on a Windows Media server.
        /// </summary>
        /// <param name="Frequency">The sample rate in Hz, or a BASS channel handle if the <see cref="WMAEncodeFlags.Source"/> flag is specified.</param>
        /// <param name="Channels">The number of channels (1=mono, 2=stereo, etc.).</param>
        /// <param name="Flags">A combination of <see cref="WMAEncodeFlags"/>.</param>
        /// <param name="Bitrate">The encoding bitrate (in bits per second, e.g. 128000), or VBR quality (100 or less).</param>
        /// <param name="Url">URL of the publishing point on the Windows Media server.</param>
        /// <param name="UserName">Username to use in connecting to the server... if either this or Password is <see langword="null" />, then no username/password is sent to the server.</param>
        /// <param name="Password">Password to use in connecting to the server.</param>
        /// <returns>If succesful, the new encoder's handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// The WMA codec expects 16-bit or 24-bit sample data depending on the <see cref="WMAEncodeFlags.Encode24Bit"/> flag, but BASSWMA will accept 8-bit, 16-bit or floating-point data, and convert it to the appropriate format.
        /// VBR encoding is not recommended for network encoding.</para>
        /// <para>The <see cref="WMAEncodeFlags.Queue"/> flag is not necessary with this function as the data is always queued and fed to the encoder asynchronously.</para>
        /// </remarks>
        /// <exception cref="Errors.WM9">The Windows Media modules (v9 or above) are not installed.</exception>
        /// <exception cref="Errors.FileOpen">Could not connect to the server.</exception>
        /// <exception cref="Errors.NotAvailable">No codec could be found to support the specified sample format and bitrate.</exception>
        /// <exception cref="Errors.WmaAccesDenied">Access was denied. Check the <paramref name="UserName" /> and <paramref name="Password" />.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int EncodeOpenPublish(int Frequency, int Channels, WMAEncodeFlags Flags, int Bitrate, string Url, string UserName, string Password)
        {
            return BASS_WMA_EncodeOpenPublish(Frequency, Channels, Flags | WMAEncodeFlags.Unicode, Bitrate, Url, UserName, Password);
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_WMA_EncodeOpenPublishMulti(int freq, int chans, WMAEncodeFlags flags, int[] bitrate, string url, string user, string pass);

        /// <summary>
        /// Initializes WMA encoding to a publishing point on a Windows Media server, using multiple bitrates.
        /// </summary>
        /// <param name="Frequency">The sample rate in Hz, or a BASS channel handle if the <see cref="WMAEncodeFlags.Source"/> flag is specified.</param>
        /// <param name="Channels">The number of channels (1=mono, 2=stereo, etc.).</param>
        /// <param name="Flags">A combination of <see cref="WMAEncodeFlags"/>.</param>
        /// <param name="Bitrates">Array of encoding bitrates to use, terminated with a 0 (in bits per second, e.g. 128000).</param>
        /// <param name="Url">URL of the publishing point on the Windows Media server.</param>
        /// <param name="UserName">Username to use in connecting to the server... if either this or Password is <see langword="null" />, then no username/password is sent to the server.</param>
        /// <param name="Password">Password to use in connecting to the server.</param>
        /// <returns>If succesful, the new encoder's handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// This function is identical to <see cref="EncodeOpenPublish(int,int,WMAEncodeFlags,int,string,string,string)"/>, but with the additional ability to specify multiple bitrates.
        /// <para>When encoding/broadcasting in multiple bitrates, the user will automatically get the best available bitrate for their bandwidth.</para>
        /// <para>
        /// The WMA codec expects 16-bit or 24-bit sample data depending on the <see cref="WMAEncodeFlags.Encode24Bit"/> flag, but BASSWMA will accept 8-bit, 16-bit or floating-point data, and convert it to the appropriate format.
        /// VBR encoding is not recommended for network encoding.</para>
        /// </remarks>
        /// <exception cref="Errors.WM9">The Windows Media modules (v9 or above) are not installed.</exception>
        /// <exception cref="Errors.FileOpen">Could not connect to the server.</exception>
        /// <exception cref="Errors.NotAvailable">No codec could be found to support the specified sample format and bitrate.</exception>
        /// <exception cref="Errors.WmaAccesDenied">Access was denied. Check the <paramref name="UserName" /> and <paramref name="Password" />.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int EncodeOpenPublish(int Frequency, int Channels, WMAEncodeFlags Flags, int[] Bitrates, string Url, string UserName, string Password)
        {
            return BASS_WMA_EncodeOpenPublishMulti(Frequency, Channels, Flags | WMAEncodeFlags.Unicode, Bitrates, Url, UserName, Password);
        }
        
		/// <summary>
		/// Sets a client connection notification callback on a network encoder.
		/// </summary>
		/// <param name="Handle">The encoder handle.</param>
		/// <param name="Procedure">User defined notification function... <see langword="null" /> = disable notifications.</param>
		/// <param name="User">User instance data to Password to the callback function.</param>
		/// <returns>If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>A previously set notification callback can be changed (or removed) at any time, by calling this function again.</remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The encoder is not a network encoder, so no port is being used.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WMA_EncodeSetNotify")]
        public static extern bool EncodeSetNotify(int Handle, ClientConnectProcedure Procedure, IntPtr User);

        #region EncodeSetTag
        /// <summary>
        /// Sets a tag in a WMA encoding.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Tag">The pointer to the tag to set.</param>
        /// <param name="Value">The pointer to the tag's text/data.</param>
        /// <param name="Format">The format of the tag and value strings.</param>
        /// <returns>If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Where the tags are located in the encoded stream depends on when this function is used.
        /// Calling this function before beginning encoding data puts the tags in the stream's header.
        /// Calling this function after encoding has begun puts the tags in the actual stream data, at the current encoding position.
        /// <para>Header tags must be set before encoding any data - no more header tags can be set once <see cref="EncodeWrite(int,IntPtr,int)" /> has been called.</para>
        /// <para>
        /// To set tags mid-stream (after encoding has begun), the <see cref="WMAEncodeFlags.Script"/> flag needs to have been specified in the encoder's creation.
        /// A mid-stream tag typically used is "Caption", which get's displayed in Windows Media Player 9 and above (if the user has enabled captions).
        /// </para>
        /// <para>
        /// When using a network encoder, it should be noted that while all header tags are sent to newly connecting clients, prior mid-stream tags are not.
        /// So if, for example, you're using the "Caption" tag to indicate the current song title, it should be sent at fairly regular intervals (not only at the start of the song).
        /// </para>
        /// <para>On the playback side, mid-stream tags can be processed using <see cref="Bass.ChannelSetSync" /> (with <see cref="SyncFlags.MetadataReceived"/>).</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The encoder does not have mid-stream tags enabled, so tags can not be set once encoding has begun.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Tag" /> and/or <paramref name="Value" /> is invalid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WMA_EncodeSetTag")]
        public static extern bool EncodeSetTag(int Handle, IntPtr Tag, IntPtr Value, WMATagFormat Format);

        [DllImport(DllName)]
        static extern bool BASS_WMA_EncodeSetTag(int Handle, string Tag, IntPtr Value, WMATagFormat Format);

        [DllImport(DllName)]
        static extern bool BASS_WMA_EncodeSetTag(int Handle, string Tag, [In, Out] byte[] Value, WMATagFormat Format);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern bool BASS_WMA_EncodeSetTag(int Handle, string Tag, string Value, WMATagFormat Format = WMATagFormat.Unicode);

        /// <summary>
        /// Sets a tag in a WMA encoding.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Tag">The tag to set.</param>
        /// <param name="Value">The tag's text/data.</param>
        /// <returns>If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Where the tags are located in the encoded stream depends on when this function is used.
        /// Calling this function before beginning encoding data puts the tags in the stream's header.
        /// Calling this function after encoding has begun puts the tags in the actual stream data, at the current encoding position.
        /// <para>Header tags must be set before encoding any data - no more header tags can be set once <see cref="EncodeWrite(int,IntPtr,int)" /> has been called.</para>
        /// <para>
        /// To set tags mid-stream (after encoding has begun), the <see cref="WMAEncodeFlags.Script"/> flag needs to have been specified in the encoder's creation.
        /// A mid-stream tag typically used is "Caption", which get's displayed in Windows Media Player 9 and above (if the user has enabled captions).
        /// </para>
        /// <para>
        /// When using a network encoder, it should be noted that while all header tags are sent to newly connecting clients, prior mid-stream tags are not.
        /// So if, for example, you're using the "Caption" tag to indicate the current song title, it should be sent at fairly regular intervals (not only at the start of the song).
        /// </para>
        /// <para>On the playback side, mid-stream tags can be processed using <see cref="Bass.ChannelSetSync" /> (with <see cref="SyncFlags.MetadataReceived"/>).</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The encoder does not have mid-stream tags enabled, so tags can not be set once encoding has begun.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Tag" /> and/or <paramref name="Value" /> is invalid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static bool EncodeSetTag(int Handle, string Tag, string Value)
        {
            return BASS_WMA_EncodeSetTag(Handle, Tag, Value);
        }

        /// <summary>
        /// Sets a binary tag in a WMA encoding.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Tag">The tag to set.</param>
        /// <param name="Length">The number of bytes provided as binary data in the tag.</param>
        /// <param name="Value">The pointer to the binary tag's data.</param>
        /// <returns>If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Where the tags are located in the encoded stream depends on when this function is used.
        /// Calling this function before beginning encoding data puts the tags in the stream's header.
        /// Calling this function after encoding has begun puts the tags in the actual stream data, at the current encoding position.
        /// <para>Header tags must be set before encoding any data - no more header tags can be set once <see cref="EncodeWrite(int,IntPtr,int)" /> has been called.</para>
        /// <para>
        /// To set tags mid-stream (after encoding has begun), the <see cref="WMAEncodeFlags.Script"/> flag needs to have been specified in the encoder's creation.
        /// A mid-stream tag typically used is "Caption", which get's displayed in Windows Media Player 9 and above (if the user has enabled captions).
        /// </para>
        /// <para>
        /// When using a network encoder, it should be noted that while all header tags are sent to newly connecting clients, prior mid-stream tags are not.
        /// So if, for example, you're using the "Caption" tag to indicate the current song title, it should be sent at fairly regular intervals (not only at the start of the song).
        /// </para>
        /// <para>On the playback side, mid-stream tags can be processed using <see cref="Bass.ChannelSetSync" /> (with <see cref="SyncFlags.MetadataReceived"/>).</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The encoder does not have mid-stream tags enabled, so tags can not be set once encoding has begun.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Tag" /> and/or <paramref name="Value" /> is invalid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static bool EncodeSetTag(int Handle, string Tag, IntPtr Value, int Length)
        {
            return BASS_WMA_EncodeSetTag(Handle, Tag, Value, (WMATagFormat)BitHelper.MakeLong((short)WMATagFormat.Binary, (short)Length));
        }

        /// <summary>
        /// Sets a binary tag in a WMA encoding.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Tag">The tag to set.</param>
        /// <param name="Length">The number of bytes provided as binary data in the tag.</param>
        /// <param name="Value">The byte[] containing the binary tag's data.</param>
        /// <returns>If succesful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// Where the tags are located in the encoded stream depends on when this function is used.
        /// Calling this function before beginning encoding data puts the tags in the stream's header.
        /// Calling this function after encoding has begun puts the tags in the actual stream data, at the current encoding position.
        /// <para>Header tags must be set before encoding any data - no more header tags can be set once <see cref="EncodeWrite(int,IntPtr,int)" /> has been called.</para>
        /// <para>
        /// To set tags mid-stream (after encoding has begun), the <see cref="WMAEncodeFlags.Script"/> flag needs to have been specified in the encoder's creation.
        /// A mid-stream tag typically used is "Caption", which get's displayed in Windows Media Player 9 and above (if the user has enabled captions).
        /// </para>
        /// <para>
        /// When using a network encoder, it should be noted that while all header tags are sent to newly connecting clients, prior mid-stream tags are not.
        /// So if, for example, you're using the "Caption" tag to indicate the current song title, it should be sent at fairly regular intervals (not only at the start of the song).
        /// </para>
        /// <para>On the playback side, mid-stream tags can be processed using <see cref="Bass.ChannelSetSync" /> (with <see cref="SyncFlags.MetadataReceived"/>).</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The encoder does not have mid-stream tags enabled, so tags can not be set once encoding has begun.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Tag" /> and/or <paramref name="Value" /> is invalid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static bool EncodeSetTag(int Handle, string Tag, byte[] Value, int Length)
        {
            return BASS_WMA_EncodeSetTag(Handle, Tag, Value, (WMATagFormat)BitHelper.MakeLong((short)WMATagFormat.Binary, (short)Length));
        }
        #endregion

        /// <summary>
        /// Retrieves the number of clients currently connected to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <returns>If succesful, the number of clients is returned, else -1 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The encoder was not created with <see cref="EncodeOpenNetwork(int, int, WMAEncodeFlags, int, int, int)" />.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_WMA_EncodeGetClients")]
        public static extern int EncodeGetClients(int Handle);
        
		/// <summary>
		/// Retrieves the network port for clients to connect to.
		/// </summary>
		/// <param name="Handle">The encoder handle.</param>
		/// <returns>If succesful, the port number is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// If you have choosen to let the system select a port (e.g. in your <see cref="EncodeOpenNetwork(int, int, WMAEncodeFlags, int, int, int)" /> or <see cref="EncodeOpenNetwork(int, int, WMAEncodeFlags, int[], int, int)" />), 
		/// this is the function to retrieve the port actually being used.
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The encoder is not a network encoder, so no port is being used.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WMA_EncodeGetPort")]
        public static extern int EncodeGetPort(int Handle);
        
		/// <summary>
		/// Finishes encoding and closes the file or network port.
		/// </summary>
		/// <param name="Handle">The encoder handle.</param>
        /// <returns>If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_WMA_EncodeClose")]
        public static extern bool EncodeClose(int Handle);

        [DllImport(DllName)]
        static extern unsafe int* BASS_WMA_EncodeGetRates(int freq, int chans, WMAEncodeFlags Flags);
        
		/// <summary>
		/// Retrieves the WMA encoding bitrates available for a specified sample format.
		/// </summary>
		/// <param name="Frequency">The sample rate in Hz, or a BASS channel handle if the <see cref="WMAEncodeFlags.Source"/> flag is specified.</param>
		/// <param name="Channels">The number of channels (1=mono, 2=stereo, etc.).</param>
		/// <param name="Flags">A combination of <see cref="WMAEncodeFlags"/>.</param>
		/// <returns>If succesful, an array of the available bitrates is returned (int[], in bits per second), else <see langword="null" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>When requesting VBR rates, the rates returned are quality settings. For example, 10 = 10% quality, 25 = 25% quality, etc... 100% quality is lossless.</para>
		/// <para>
        /// The WMA codec expects 16-bit or 24-bit sample data depending on the <see cref="WMAEncodeFlags.Encode24Bit"/> flag, but BassWma will accept 8-bit, 16-bit or floating-point data, and convert it to the appropriate format.
        /// Of course, it makes little sense to encode 8-bit or 16-bit data in 24-bit.
        /// </para>
		/// <para>
        /// The WMA codec currently supports the following sample rates: 8000, 11025, 16000, 22050, 32000, 44100, 48000, 88200, 96000.
        /// And the following number of channels: 1, 2, 6, 8.
        /// But not all combinations of these are supported.
        /// To encode other sample formats, the data will first have to be resampled to a supported format.
        /// </para>
		/// </remarks>
        /// <exception cref="Errors.WM9">The Windows Media modules (v9 or above) are not installed.</exception>
        /// <exception cref="Errors.NotAvailable">No codec could be found to support the specified sample format.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static unsafe int[] EncodeGetRates(int Frequency, int Channels, WMAEncodeFlags Flags)
        {
            var list = new List<int>();

            var rates = BASS_WMA_EncodeGetRates(Frequency, Channels, Flags);

            if (rates != null)
            {
                while (*rates != 0)
                {
                    list.Add(*rates);
                    rates++;
                }
            }
            else return null;

            return list.ToArray();
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Enable network seeking?
        /// seek (bool): If true seeking in network files/streams is enabled (default is false).
        /// If true, it allows seeking before the entire file has been downloaded/cached.
        /// Seeking is slow that way, so it's disabled by default.
        /// </summary>
        public static bool CanSeekNetworkStreams
        {
            get => Bass.GetConfigBool(Configuration.WmaNetSeek);
            set => Bass.Configure(Configuration.WmaNetSeek, value);
        }

        /// <summary>
        /// Play audio from WMV (video) files?
        /// playwmv (bool): If true (default) BASSWMA will play the audio from WMV video files.
        /// If false WMV files will not be played.
        /// </summary>
        public static bool PlayWMVAudio
        {
            get => Bass.GetConfigBool(Configuration.WmaVideo);
            set => Bass.Configure(Configuration.WmaVideo, value);
        }

        /// <summary>
        /// Prebuffer internet streams on creation, before returning from BassWma.StreamCreateFile()?
        /// prebuf (bool): The Windows Media modules must prebuffer a stream before starting decoding/playback of it.
        /// This option determines when/where to wait for that to be completed.
        /// The Windows Media modules must prebuffer a stream before starting decoding/playback of it.
        /// This option determines whether the stream creation function (eg. BassWma.CreateStream())
        /// will wait for the prebuffering to complete before returning.
        /// If playback of a stream is attempted before it has prebuffered,
        /// it will stall and then resume once it has finished prebuffering.
        /// The prebuffering progress can be monitored via Bass.StreamGetFilePosition() (FileStreamPosition.WmaBuffer).
        /// This option is enabled by default.
        /// </summary>
        public static bool PrebufferInternetStreams
        {
            get => Bass.GetConfigBool(Configuration.WmaNetPreBuffer);
            set => Bass.Configure(Configuration.WmaNetPreBuffer, value);
        }

        /// <summary>
        /// Use BASS file handling.
        /// bassfile (bool): Default is disabled (false).
        /// When enabled (true) BASSWMA uses BASS's file routines when playing local files.
        /// It uses the IStream interface to do that.
        /// This would also allow to support the "offset" parameter for WMA files with Bass.CreateStream().
        /// The downside of enabling this feature is, that it stops playback while encoding from working.
        /// </summary>
        public static bool UseBassFileHandling
        {
            get => Bass.GetConfigBool(Configuration.WmaBassFileHandling);
            set => Bass.Configure(Configuration.WmaBassFileHandling, value);
        }

        /// <summary>
        /// Use a seperate thread to decode the data?
        /// async (bool): If true BASSWMA will decode the data in a seperate thread.
        /// If false (default) the normal file system will be used.
        /// The WM decoder can by synchronous (decodes data on demand) or asynchronous (decodes in the background).
        /// With the background decoding, BASSWMA buffers the data that it receives from the decoder for the STREAMPROC to access.
        /// The start of playback/seeking may well be slightly delayed due to there being no data available immediately.
        /// Internet streams are only supported by the asynchronous system,
        /// but local files can use either, and BASSWMA uses the synchronous system by default.
        /// </summary>
        public static bool AsyncDecoding
        {
            get => Bass.GetConfigBool(Configuration.WmaAsync);
            set => Bass.Configure(Configuration.WmaAsync, value);
        }
        #endregion

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern IntPtr BASS_WMA_GetTags(string File, BassFlags Flags = BassFlags.Unicode);

        /// <summary>
        /// Get the tags from a file, can be used on DRM protected files (not thread-safe!).
        /// </summary>
        /// <param name="File">Filename from which to get the tags.</param>
        /// <returns>If succesful, an array of the requested tags is returned, else <see langword="null" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// This function gives the same tags as <see cref="Bass.ChannelGetTags" /> with <see cref="TagType.WMA"/>, which is a pointer to a series of null-terminated UTF-8 strings (converted to string[]), the final string ending with a double null. 
        /// Unlike <see cref="Bass.ChannelGetTags" />, this function can also be used with DRM-protected WMA files without a DRM licence, as it does not require a stream to be created.
        /// </remarks>
        /// <exception cref="Errors.WmaCodec">The Windows Media modules (v9 or above) are not installed.</exception>
        /// <exception cref="Errors.FileOpen">The file could not be opened, or it is not a WMA file.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static string[] GetTags(string File) => Extensions.ExtractMultiStringUtf8(BASS_WMA_GetTags(File));
    }
}