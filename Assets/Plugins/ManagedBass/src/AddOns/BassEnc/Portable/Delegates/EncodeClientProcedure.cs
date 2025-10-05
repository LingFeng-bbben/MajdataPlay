using System;

namespace ManagedBass.Enc
{
    /// <summary>
    /// User defined callback function to receive notification of client connections and disconnections, and optionally refuse connections.
    /// </summary>
    /// <param name="Handle">The encoder/server that the client is connecting to or disconnecting from.</param>
    /// <param name="Connect">The client is connecting? true = connecting, false = disconnecting.</param>
    /// <param name="Client">The client's IP address and port number... "IP:port".</param>
    /// <param name="Headers">
    /// The request headers... <see langword="null" /> = the client is disconnecting or HTTP headers have been disabled via the <see cref="EncodeServer.NoHTTP"/> flag.
    /// The headers are in the same form as would be given by <see cref="Bass.ChannelGetTags" />, which is a series of null-terminated strings, the final string ending with a double null.
    /// The request headers can optionally be replaced with response headers to send back to the client, each ending with a carriage return and line feed ("\r\n").
    /// The response headers should not exceed 1KB in length.
    /// </param>
    /// <param name="User">The user instance data given when <see cref="BassEnc.ServerInit" /> was called.</param>
    /// <returns>If the client is connecting, <see langword="false" /> means the connection is denied, otherwise it is accepted. The return value is ignored if the client is disconnecting.</returns>
    /// <remarks>
    /// <para>
    /// This function can be used to keep track of how many clients are connected, and who is connected.
    /// The request headers can be used to authenticate clients, and response headers can be used to pass information back to the clients.
    /// By default, connecting clients will be sent an "HTTP/1.0 200 OK" status line if accepted, and an "HTTP/1.0 403 Forbidden" status line if denied.
    /// That can be overridden in the first response header.
    /// </para>
    /// <para>Disconnection notifications will be received for clients that have disconnected themselves or that have been kicked by <see cref="BassEnc.ServerKick" />, but there will no notification of any clients that are disconnected by the encoder being freed.</para>
    /// <para>Each server has its own thread that handles new connections and sends data to its clients. The notification callbacks also come from that thread, so the callback function should avoid introducing long delays as that could result in clients missing some data and delay other clients connecting.</para>
    /// <para>
    /// Use <see cref="Extensions.ExtractMultiStringAnsi"/> to get the <paramref name="Headers"/>.
    /// </para>
    /// </remarks>
    public delegate bool EncodeClientProcedure(int Handle, bool Connect, string Client, IntPtr Headers, IntPtr User);
}