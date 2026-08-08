using AOT;
using MajdataPlay.Net.Curl.Core.PInvoke;
using MajdataPlay.UnsafeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net.Curl.Core
{
    public class CurlResponse
    {
        public CurlCode? ResultCode { get; set; }
        public HttpResponseMessage Message { get; }
        public CurlHttpConfig Config { get; }
        internal CurlRequest Request { get; }

        long _currentHeadersLength = 0;

        readonly Action _onResume;
        readonly CurlResponseStream _responseStream;
        readonly static CurlReadOrWriteCallback _onWriteCallback; // Download
        readonly static CurlReadOrWriteCallback _onHeaderReceivedCallback;

        static CurlResponse()
        {
            _onWriteCallback = OnWriteCallback;
            _onHeaderReceivedCallback = OnHeaderReceived;
        }

        internal CurlResponse(CurlRequest request, Action onResume, CurlHttpConfig config)
        {
            Request = request;
            Config = config;
            _onResume = onResume;
            _responseStream = new CurlResponseStream(config.MaxResponseHeadersLength, _onResume);
            

            Message = new();
            Message.Content = new StreamContent(_responseStream);
            
            Request.SetOption(CurlOption.WriteFunction, Marshal.GetFunctionPointerForDelegate(_onWriteCallback));
            Request.SetOption(CurlOption.HeaderFunction, Marshal.GetFunctionPointerForDelegate(_onHeaderReceivedCallback));
        }

        internal void Complete()
        {
            _responseStream.CompleteWriting();
        }
        internal void Abort()
        {
            Abort(new OperationCanceledException("Request was aborted."));
        }
        internal void Abort(Exception abortException)
        {
            _responseStream.Abort(abortException);
        }


        [MonoPInvokeCallback(typeof(CurlReadOrWriteCallback))]
        static unsafe UIntPtr OnWriteCallback(IntPtr ptr, UIntPtr size, UIntPtr nmemb, IntPtr userdata)
        {
            if (!UnsafeHelper.TryGetInstanceFromGCHandle<CurlTask>(userdata, out var curlTask))
            {
                return UIntPtr.Zero;
            }
            else if(curlTask.State != CurlRequestState.HeaderRead)
            {
                curlTask.TryEnterHeaderReadState();
            }
            var curlRsp = curlTask.Response;
            var responseStream = curlRsp._responseStream;
            var length = (int)(size.ToUInt32() * nmemb.ToUInt32());
            var buffer = new ReadOnlySpan<byte>((void*)ptr, length);

            return responseStream.WriteChunk(buffer);
        }
        [MonoPInvokeCallback(typeof(CurlReadOrWriteCallback))]
        static unsafe UIntPtr OnHeaderReceived(IntPtr ptr, UIntPtr size, UIntPtr nmemb, IntPtr userdata)
        {
            if (!UnsafeHelper.TryGetInstanceFromGCHandle<CurlTask>(userdata, out var curlTask))
            {
                return UIntPtr.Zero;
            }
            else if(curlTask.State != CurlRequestState.Submitted)
            {
                return UIntPtr.Zero;
            }
            var curlResponse = curlTask.Response;
            var maxHeadersLength = curlTask.Config.MaxResponseHeadersLength;
            var length = (int)(size.ToUInt32() * nmemb.ToUInt32());
            var buffer = new ReadOnlySpan<byte>((void*)ptr, length);
            var line = buffer;
            var currentHadersLength = Interlocked.Add(ref curlResponse._currentHeadersLength, line.Length);
            TrimHttpHeader(ref line);

            if (currentHadersLength > maxHeadersLength || currentHadersLength + line.Length > maxHeadersLength)
            {
                Interlocked.Add(ref curlResponse._currentHeadersLength, -line.Length);
                return UIntPtr.Zero;
            }

            curlResponse.ParseHttpHeader(buffer, curlResponse.Message);

            return (UIntPtr)length;
        }
        static void TrimHttpHeader(ref ReadOnlySpan<byte> line)
        {
            while (line.Length > 0)
            {
                var last = line[line.Length - 1];
                if (last == '\r' || last == '\n' || last == ' ')
                {
                    line = line.Slice(0, line.Length - 1);
                }
                else
                {
                    break;
                }
            }
        }
        void ParseHttpHeader(ReadOnlySpan<byte> line, HttpResponseMessage response)
        {
            if (line.Length == 0)
            {
                return;
            }
            var isHTTPHeader = line.Length >= 5
                && line[0] == 'H'
                && line[1] == 'T'
                && line[2] == 'T'
                && line[3] == 'P'
                && line[4] == '/';

            // HTTP status code parse
            if (isHTTPHeader)
            {
                var code = ParseStatusCode(line);
                if (code > 0)
                {
                    response.StatusCode = (HttpStatusCode)code;
                    response.Headers.Clear();
                    response.Content.Headers.Clear();
                }
                return;
            }

            // find ':'
            var colonIndex = -1;
            for (var i = 0; i < line.Length; i++)
            {
                if (line[i] == ':')
                {
                    colonIndex = i;
                    break;
                }
            }

            // Colon not found or at the start of the line, invalid header format
            if (colonIndex <= 0)
            {
                return;
            }

            // Header key and value extraction
            var keyBytes = TrimSpan(line.Slice(0, colonIndex));
            var valueBytes = TrimSpan(line.Slice(colonIndex + 1));

            // If the key is empty, skip this header
            if (keyBytes.Length == 0)
            {
                return;
            }

            var keyStr = GetKeyString(keyBytes);
            var valueStr = Encoding.UTF8.GetString(valueBytes);

            if (!response.Headers.TryAddWithoutValidation(keyStr, valueStr))
            {
                response.Content.Headers.TryAddWithoutValidation(keyStr, valueStr);
            }

            var cookieContainer = Config.CookieContainer;
            var requestUri = Request.RequestUri;
            if (cookieContainer != null)
            {
                if (string.Equals(keyStr, "Set-Cookie", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        cookieContainer.SetCookies(requestUri, valueStr);
                    }
                    catch (CookieException)
                    {

                    }
                }
            }

            return;
        }
        static int ParseStatusCode(ReadOnlySpan<byte> line)
        {
            int firstSpace = -1;
            for (int i = 5; i < line.Length; i++)
            {
                if (line[i] == ' ')
                {
                    firstSpace = i;
                    break;
                }
            }

            if (firstSpace > 0 && firstSpace + 3 < line.Length)
            {
                var codeStart = firstSpace + 1;
                while (codeStart < line.Length && line[codeStart] == 32)
                {
                    codeStart++;
                }

                if (codeStart + 2 < line.Length)
                {
                    var d1 = line[codeStart];
                    var d2 = line[codeStart + 1];
                    var d3 = line[codeStart + 2];

                    if (d1 >= '0' && d1 <= '9' &&
                        d2 >= '0' && d2 <= '9' &&
                        d3 >= '0' && d3 <= '9')
                    {
                        return ((d1 - '0') * 100) + ((d2 - '0') * 10) + (d3 - '0');
                    }
                }
            }
            return -1;
        }
        static string GetKeyString(ReadOnlySpan<byte> keyBytes)
        {
            if (IsMatch(keyBytes, "Content-Type"))
            {
                return "Content-Type";
            }
            if (IsMatch(keyBytes, "Content-Length"))
            {
                return "Content-Length";
            }
            if (IsMatch(keyBytes, "Server"))
            {
                return "Server";
            }
            if (IsMatch(keyBytes, "Date"))
            {
                return "Date";
            }
            if (IsMatch(keyBytes, "Set-Cookie"))
            {
                return "Set-Cookie";
            }
            if (IsMatch(keyBytes, "Cache-Control"))
            {
                return "Cache-Control";
            }
            if (IsMatch(keyBytes, "Connection"))
            {
                return "Connection";
            }
            if (IsMatch(keyBytes, "Location"))
            {
                return "Location";
            }
            if (IsMatch(keyBytes, "Transfer-Encoding"))
            {
                return "Transfer-Encoding";
            }

            return Encoding.UTF8.GetString(keyBytes);
        }
        static bool IsMatch(ReadOnlySpan<byte> span, string knownKey)
        {
            if (span.Length != knownKey.Length)
            {
                return false;
            }

            for (var i = 0; i < span.Length; i++)
            {
                var b = span[i];
                var c = knownKey[i];

                if (b == (byte)c)
                {
                    continue;
                }
                if (b >= 'A' && b <= 'Z' && (b + 32) == c)
                {
                    continue;
                }
                if (b >= 'a' && b <= 'z' && (b - 32) == c)
                {
                    continue;
                }

                return false;
            }
            return true;
        }
        static ReadOnlySpan<byte> TrimSpan(ReadOnlySpan<byte> span)
        {
            var start = 0;
            while (start < span.Length && span[start] == 32)
            {
                start++;
            }

            var end = span.Length - 1;
            while (end >= start && span[end] == 32)
            {
                end--;
            }

            var length = end - start + 1;
            if (length <= 0)
            {
                return ReadOnlySpan<byte>.Empty;
            }
            return span.Slice(start, length);
        }
    }
}
