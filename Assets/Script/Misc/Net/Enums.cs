namespace MajdataPlay.Net
{
    public enum HttpRequestError
    {
        /// <summary>
        /// A generic or unknown error occurred.
        /// </summary>

        Unknown = 0,
        /// <summary>
        /// The DNS name resolution failed.
        /// </summary>

        NameResolutionError = 1,
        /// <summary>
        /// A transport-level failure occurred while connecting to the remote endpoint.
        /// </summary>

        ConnectionError = 2,
        /// <summary>
        /// An error occurred during the TLS handshake.
        /// </summary>

        SecureConnectionError = 3,
        /// <summary>
        /// An HTTP/2 or HTTP/3 protocol error occurred.
        /// </summary>

        HttpProtocolError = 4,
        /// <summary>
        /// Extended CONNECT for WebSockets over HTTP/2 is not supported by the peer.
        /// </summary>

        ExtendedConnectNotSupported = 5,
        /// <summary>
        /// Cannot negotiate the HTTP version requested.
        /// </summary>

        VersionNegotiationError = 6,
        /// <summary>
        /// The authentication failed.
        /// </summary>

        UserAuthenticationError = 7,

        /// <summary>
        /// An error occurred while establishing a connection to the proxy tunnel.
        /// </summary>
        ProxyTunnelError = 8,

        /// <summary>
        /// An invalid or malformed response has been received.
        /// </summary>
        InvalidResponse = 9,

        /// <summary>
        /// The response ended prematurely.
        /// </summary>
        ResponseEnded = 10,

        /// <summary>
        /// The response exceeded a pre-configured limit such as System.Net.Http.HttpClient.MaxResponseContentBufferSize or System.Net.Http.HttpClientHandler.MaxResponseHeadersLength.
        /// </summary>
        ConfigurationLimitExceeded = 11,
        NoError
    }
}
