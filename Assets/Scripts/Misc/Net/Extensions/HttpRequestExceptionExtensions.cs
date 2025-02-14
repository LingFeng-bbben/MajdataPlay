using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
#nullable enable
namespace MajdataPlay.Net
{
    public static class HttpRequestExceptionExtensions
    {
        public static HttpRequestError GetErrorCode(this HttpRequestException source)
        {
            switch (source.InnerException)
            {
                case SocketException socketEx:
                    return socketEx.SocketErrorCode switch
                    {
                        SocketError.HostNotFound or SocketError.NoData => HttpRequestError.NameResolutionError,
                        _ => HttpRequestError.ConnectionError,
                    };
                case AuthenticationException:
                    return HttpRequestError.SecureConnectionError;
                default:
                    break;
            }

            switch (source.Message)
            {
                case string message when message.Contains("HTTP/2") || message.Contains("HTTP/3"):
                    return HttpRequestError.HttpProtocolError;
                case string message when message.Contains("CONNECT") && message.Contains("WebSocket"):
                    return HttpRequestError.ExtendedConnectNotSupported;
                case string message when message.Contains("authentication"):
                    return HttpRequestError.UserAuthenticationError;
                case string message when message.Contains("version negotiation"):
                    return HttpRequestError.VersionNegotiationError;
                case string message when message.Contains("proxy tunnel"):
                    return HttpRequestError.ProxyTunnelError;
                case string message when message.Contains("malformed") || message.Contains("invalid"):
                    return HttpRequestError.InvalidResponse;
                case string message when message.Contains("response ended prematurely"):
                    return HttpRequestError.ResponseEnded;
                case string message when message.Contains("exceeded the limit"):
                    return HttpRequestError.ConfigurationLimitExceeded;
                default:
                    return HttpRequestError.Unknown;
            }
        }
    }
}
