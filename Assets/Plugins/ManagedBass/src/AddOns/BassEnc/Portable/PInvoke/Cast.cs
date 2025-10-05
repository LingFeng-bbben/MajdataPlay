using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ManagedBass.Enc
{
    public static partial class BassEnc
    {
        #region Mime
        /// <summary>
        /// Mime type for Mp3.
        /// </summary>
        public const string MimeMp3 = "audio/mpeg";

        /// <summary>
        /// Mime type for Ogg.
        /// </summary>
        public const string MimeOgg = "application/ogg";

        /// <summary>
        /// Mime type for Aac.
        /// </summary>
        public const string MimeAac = "audio/aacp";
        #endregion

        [DllImport(DllName)]
        static extern IntPtr BASS_Encode_CastGetStats(int handle, EncodeStats type, [In] string pass);
        
        /// <summary>
        /// Retrieves stats from the Shoutcast or Icecast server.
        /// </summary>
        /// <param name="Handle">The encoder Handle.</param>
        /// <param name="Type">The type of stats to retrieve.</param>
        /// <param name="Password">Password when retrieving Icecast server stats... <see langword="null" /> = use the password provided in the <see cref="CastInit" /> call. A username can also be included in the form of "username:password". </param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>The stats are returned in XML format.</remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Type"><paramref name="Type" /> is invalid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast of the requested type set on the encoder.</exception>
        /// <exception cref="Errors.Timeout">The server did not respond to the request within the timeout period, as set with the <see cref="Bass.NetTimeOut"/> config option.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static string CastGetStats(int Handle, EncodeStats Type, string Password)
        {
            return Marshal.PtrToStringAnsi(BASS_Encode_CastGetStats(Handle, Type, Password));
        }

        /// <summary>
        /// Initializes sending an encoder's output to a Shoutcast or Icecast server.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Server">The server to send to, in the form of "address:port" (Shoutcast v1) resp. "address:port,sid" (Shoutcast v2) or "address:port/mount" (Icecast).</param>
        /// <param name="Password">The server password. A username can be included in the form of "username:password" when connecting to an Icecast or Shoutcast 2 server.</param>
        /// <param name="Content">
        /// The MIME type of the encoder output.
        /// <para><see cref="MimeMp3"/>, <see cref="MimeOgg"/> or <see cref="MimeAac"/>.</para>
        /// </param>
        /// <param name="Name">The stream name... <see langword="null" /> = no name.</param>
        /// <param name="Url">The URL, for example, of the radio station's webpage... <see langword="null" /> = no URL.</param>
        /// <param name="Genre">The genre... <see langword="null" /> = no genre.</param>
        /// <param name="Description">Description... <see langword="null" /> = no description. This applies to Icecast only.</param>
        /// <param name="Headers">Other headers to send to the server... <see langword="null" /> = none. Each header should end with a carriage return and line feed ("\r\n").</param>
        /// <param name="Bitrate">The bitrate (in kbps) of the encoder output... 0 = undefined bitrate. In cases where the bitrate is a "quality" (rather than CBR) setting, the headers parameter can be used to communicate that instead, eg. "ice-bitrate: Quality 0\r\n".</param>
        /// <param name="Public">Public? If <see langword="true" />, the stream is added to the public directory of streams, at shoutcast.com or dir.xiph.org (or as defined in the server config).</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// This function sets up a Shoutcast/Icecast source client, sending the encoder's output to a server, which listeners can then connect to and receive the data from. 
        /// The Shoutcast and Icecast server software is available from http://www.shoutcast.com/broadcast-tools and http://www.icecast.org/download.php, respectively.
        /// </para>
        /// <para>
        /// An encoder needs to be started (but with no data sent to it yet) before using this function to setup the sending of the encoder's output to a Shoutcast or Icecast server.
        /// If <see cref="EncodeStart(int,string,EncodeFlags,EncodeProcedure,IntPtr)" /> is used, the encoder should be setup to write its output to STDOUT.
        /// Due to the length restrictions of WAVE headers/files, the encoder should also be started with the <see cref="EncodeFlags.NoHeader"/> flag, and the sample format details sent via the command-line.
        /// </para>
        /// <para>
        /// Unless the <see cref="EncodeFlags.UnlimitedCastDataRate"/> flag is set on the encoder, BASSenc automatically limits the rate that data is processed to real-time speed to avoid overflowing the server's buffer, which means that it is safe to simply try to process data as quickly as possible, eg. when the source is a decoding channel.
        /// Encoders set on recording channels are automatically exempt from the rate limiting, as they are inherently real-time.
        /// With BASS 2.4.6 or above, also exempt are encoders that are fed in a playback buffer update cycle (including <see cref="Bass.Update" /> and <see cref="Bass.ChannelUpdate" /> calls), eg. when the source is a playing channel;
        /// that is to avoid delaying the update thread, which could result in playback buffer underruns.
        /// </para>
        /// <para>
        /// Normally, BASSenc will produce the encoded data (with the help of an encoder) that is sent to a Shoutcast/Icecast server, but it is also possible to send already encoded data to a server (without first decoding and re-encoding it) via the PCM encoding option.
        /// The encoder can be set on any BASS channel, as rather than feeding on sample data from the channel, <see cref="EncodeWrite(int,IntPtr,int)" /> would be used to feed in the already encoded data.
        /// BASSenc does not know what the data's bitrate is in that case, so it is up to the user to process the data at the correct rate (real-time speed).
        /// </para>
        /// <para><see cref="ServerInit" /> can be used to setup a server that listeners can connect to directly, without a Shoutcast/Icecast server intermediary.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Already">There is already a cast set on the encoder.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Server" /> doesn't include a port number.</exception>
        /// <exception cref="Errors.FileOpen">Couldn't connect to the server.</exception>
        /// <exception cref="Errors.CastDenied"><paramref name="Password" /> is not valid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_CastInit")]
        public static extern bool CastInit(int Handle,
            string Server,
            string Password,
            string Content,
            string Name,
            string Url,
            string Genre,
            string Description,
            string Headers,
            int Bitrate,
            bool Public);

        [DllImport(DllName)]
        static extern bool BASS_Encode_CastSendMeta(int handle, EncodeMetaDataType type, byte[] data, int length);
        
        /// <summary>
        /// Sends metadata to a Shoutcast 2 server.
        /// </summary>
        /// <param name="Handle">The encoder Handle.</param>
        /// <param name="Type">The type of metadata.</param>
        /// <param name="Buffer">The XML metadata as an UTF-8 encoded byte array.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast set on the encoder.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static bool CastSendMeta(int Handle, EncodeMetaDataType Type, byte[] Buffer)
        {
            return BASS_Encode_CastSendMeta(Handle, Type, Buffer, Buffer.Length);
        }

        /// <summary>
        /// Sends metadata to a Shoutcast 2 server.
        /// </summary>
        /// <param name="Handle">The encoder Handle.</param>
        /// <param name="Type">The type of metadata.</param>
        /// <param name="Metadata">The XML metadata to send.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast set on the encoder.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static bool CastSendMeta(int Handle, EncodeMetaDataType Type, string Metadata)
        {
            if (string.IsNullOrEmpty(Metadata))
                return false;

            var bytes = Encoding.UTF8.GetBytes(Metadata);
            return BASS_Encode_CastSendMeta(Handle, Type, bytes, bytes.Length);
        }

        /// <summary>
        /// Sets the title (ANSI) of a cast stream.
        /// </summary>
        /// <param name="Handle">The encoder Handle.</param>
        /// <param name="Title">The title to set.</param>
        /// <param name="Url">URL to go with the title... <see langword="null" /> = no URL. This applies to Shoutcast only (not Shoutcast 2).</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The ISO-8859-1 (Latin-1) character set should be used with Shoutcast servers, and UTF-8 with Icecast and Shoutcast 2 servers.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast set on the encoder.</exception>
        /// <exception cref="Errors.Timeout">The server did not respond to the request within the timeout period, as set with the <see cref="Bass.NetTimeOut"/> config option.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_CastSetTitle")]
        public static extern bool CastSetTitle(int Handle, string Title, string Url);

        /// <summary>
        /// Sets the title of a cast stream.
        /// </summary>
        /// <param name="Handle">The encoder Handle.</param>
        /// <param name="Title">encoded byte[] containing the title to set.</param>
        /// <param name="Url">encoded byte[] containing the URL to go with the title... <see langword="null" /> = no URL. This applies to Shoutcast only (not Shoutcast 2).</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The ISO-8859-1 (Latin-1) character set should be used with Shoutcast servers, and UTF-8 with Icecast and Shoutcast 2 servers.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast set on the encoder.</exception>
        /// <exception cref="Errors.Timeout">The server did not respond to the request within the timeout period, as set with the <see cref="Bass.NetTimeOut"/> config option.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_CastSetTitle")]
        public static extern bool CastSetTitle(int Handle, byte[] Title, byte[] Url);
    }
}