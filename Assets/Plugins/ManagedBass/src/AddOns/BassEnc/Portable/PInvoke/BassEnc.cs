using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Enc
{
    /// <summary>
    /// BassEnc is a BASS addon that allows BASS channels to be encoded using any command-line encoder with STDIN support (LAME, OGGENC, etc), or OS codec (ACM, CoreAudio, etc).
    /// Also includes Shoutcast and Icecast sourcing features, and PCM or WAV file writing.
    /// </summary>
    public static partial class BassEnc
    {
#if __STATIC_LINKING__
        const string DllName = "__Internal";
#else
        const string DllName = "bassenc";
#endif
        
        #region Version
        [DllImport(DllName)]
        static extern int BASS_Encode_GetVersion();

        /// <summary>
        /// Gets the Version of BassEnc that is loaded.
        /// </summary>
        public static Version Version => Extensions.GetVersion(BASS_Encode_GetVersion());
        #endregion

        #region Configure
        /// <summary>
        /// Encoder DSP priority (default -1000) which determines where in the DSP chain the encoding is performed. 
        /// </summary>
        /// <remarks>
        /// All DSP with a higher priority will be present in the encoding.
        /// Changes only affect subsequent encodings, not those that have already been started.
        /// </remarks>
        public static int DSPPriority
        {
            get => Bass.GetConfig(Configuration.EncodePriority);
            set => Bass.Configure(Configuration.EncodePriority, value);
        }

        /// <summary>
        /// The maximum queue Length of the async encoder (default 10000, 0 = Unlimited) in milliseconds.
        /// </summary>
        /// <remarks>
        /// When queued encoding is enabled, the queue's Buffer will grow as needed to hold the queued data, up to a limit specified by this config option.
        /// Changes only apply to new encoders, not any already existing encoders.
        /// </remarks>
        public static int Queue
        {
            get => Bass.GetConfig(Configuration.EncodeQueue);
            set => Bass.Configure(Configuration.EncodeQueue, value);
        }

        /// <summary>
        /// The time to wait (in milliseconds) to send data to a cast server (default 5000ms)
        /// </summary>
        /// <remarks>
        /// When an attempt to send data is timed-out, the data is discarded. 
        /// <see cref="EncodeSetNotify"/> can be used to receive a notification of when this happens.
        /// Changes take immediate effect.
        /// </remarks>
        public static int CastTimeout
        {
            get => Bass.GetConfig(Configuration.EncodeCastTimeout);
            set => Bass.Configure(Configuration.EncodeCastTimeout, value);
        }

        /// <summary>
        /// Proxy server settings when connecting to Icecast and Shoutcast (in the form of "[User:Password@]server:port"... <see langword="null"/> (default) = don't use a proxy but a direct connection).
        /// </summary>
        /// <remarks>
        /// If only the "server:port" part is specified, then that proxy server is used without any authorization credentials.
        /// This setting affects how the following functions connect to servers: <see cref="CastInit"/>, <see cref="CastGetStats"/>, <see cref="CastSetTitle(int, string, string)"/>.
        /// When a proxy server is used, it needs to support the HTTP 'CONNECT' method.
        /// The default setting is <see langword="null"/> (do not use a proxy).
        /// Changes take effect from the next internet stream creation call.
        /// </remarks>
        public static string CastProxy
        {
            get => Marshal.PtrToStringAnsi(Bass.GetConfigPtr(Configuration.EncodeCastProxy));
            set
            {
                var castProxy = Marshal.StringToHGlobalAnsi(value);

                Bass.Configure(Configuration.EncodeCastProxy, castProxy);

                Marshal.FreeHGlobal(castProxy);
            }
        }
        #endregion
        
        #region Server
        /// <summary>
        /// Initializes a server to send an encoder's output to connecting clients.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Port">
        /// The IP address and port number to accept client connections on... "xxx.xxx.xxx.xxx:port", <see langword="null" /> = an available port on all local addresses.
        /// The IP address should be local and the port number should be lower than 65536.
        /// If the address is "0.0.0.0" or omitted, then the server will accept connections on all local addresses.
        /// If the port is "0" or omitted, then an available port will be assigned.
        /// </param>
        /// <param name="Buffer">The server's buffer length in bytes.</param>
        /// <param name="Burst">The amount of buffered data to send to new clients. This will be capped at the size of the buffer.</param>
        /// <param name="Flags"><see cref="EncodeServer"/> flags.</param>
        /// <param name="Procedure">Callback function to receive notification of clients connecting and disconnecting... <see langword="null" /> = no callback.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>If successful, the new server's port number is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// This function allows remote (or local) clients to receive the encoder's output by setting up a TCP server for them to connect to, using <see cref="Bass.CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)" /> for example. 
        /// Connections can be refused by the <see cref="EncodeClientProcedure" /> callback function, and already connected clients can be kicked with the <see cref="ServerKick" /> function.
        /// </para>
        /// <para>
        /// The server buffers the data that it receives from the encoder, and the data is then sent from the buffer to the connected clients.
        /// The buffer should be at least big enough to account for the time that it takes for the clients to receive the data.
        /// If a client falls too far behind (beyond the buffer length), it will miss some data.
        /// When a client connects, buffered data can be "burst" to the client, allowing it to prebuffer and begin playback more quickly.
        /// </para>
        /// <para>
        /// An encoder needs to be started, but with no data yet sent to it, before using this function to setup the server.
        /// If <see cref="EncodeStart(int,string,EncodeFlags,EncodeProcedure,IntPtr)" /> is used, the encoder should be setup to write its output to STDOUT.
        /// Due to the length restrictions of WAVE headers/files, the encoder should also be started with the <see cref="EncodeFlags.NoHeader"/> flag, and the sample format details sent via the command-line.
        /// </para>
        /// <para>
        /// Normally, BASSenc will produce the encoded data (with the help of an encoder) that is sent to a clients, but it is also possible to send already encoded data (without first decoding and re-encoding it) via the PCM encoding option.
        /// The encoder can be set on any BASS channel, as rather than feeding on sample data from the channel, <see cref="EncodeWrite(int,IntPtr,int)" /> would be used to feed in the already encoded data.
        /// BASSenc does not know what the data's bitrate is in that case, so it is up to the user to process the data at the correct rate (real-time speed).
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>This function is not available on Windows CE.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Already">There is already a server set on the encoder.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Port" /> is not valid.</exception>
        /// <exception cref="Errors.Busy">The port is in use.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_ServerInit")]
        public static extern int ServerInit(int Handle, string Port, int Buffer, int Burst, EncodeServer Flags, EncodeClientProcedure Procedure, IntPtr User);
        
		/// <summary>
		/// Kicks clients from a server.
		/// </summary>
		/// <param name="Handle">The encoder Handle.</param>
		/// <param name="Client">The client(s) to kick... "" (empty string) = all clients. Unless a port number is included, this string is compared with the start of the connected clients' IP address.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// The clients may not be kicked immediately, but shortly after the call.
        /// If the server has been setup with an <see cref="EncodeClientProcedure" /> callback function, that will receive notification of the disconnections.
        /// </para>
		/// <para><b>Platform-specific</b></para>
		/// <para>This function is not available on Windows CE.</para>
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">No matching clients were found.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_ServerKick")]
        public static extern int ServerKick(int Handle, string Client = "");
        #endregion
    }
}