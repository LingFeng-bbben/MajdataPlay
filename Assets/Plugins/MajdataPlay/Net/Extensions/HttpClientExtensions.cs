using MajdataPlay.Buffers;
using MajdataPlay.Numerics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MajdataPlay.Net
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> GetPartialAsync(this HttpClient client,
            string requestUri,
            HttpRange range,
            CancellationToken token = default)
        {
            return GetPartialAsync(client, requestUri, range, HttpCompletionOption.ResponseContentRead, token);
        }
        public static Task<HttpResponseMessage> GetPartialAsync(this HttpClient client,
            string requestUri,
            HttpRange range,
            HttpCompletionOption completionOption,
            CancellationToken token = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Range = range.ToRangeHeaderValue();

            return GetPartialAsyncInternal(client, request, completionOption, token);
        }
        public static async Task GetPartialAsync(this HttpClient client,
            string requestUri,
            Stream dst,
            HttpRange range,
            IProgress<float> progress = null,
            CancellationToken token = default)
        {
            using var response = await GetPartialAsync(client,
                requestUri,
                range,
                HttpCompletionOption.ResponseHeadersRead,
                token);

            await GetPartialAsyncInternal(client, response, dst, range, progress, token);
        }

        public static Task<HttpResponseMessage> GetPartialAsync(this HttpClient client,
            Uri uri,
            HttpRange range,
            CancellationToken token = default)
        {
            return GetPartialAsync(client, uri, range, HttpCompletionOption.ResponseContentRead, token);
        }
        public static Task<HttpResponseMessage> GetPartialAsync(this HttpClient client,
            Uri uri,
            HttpRange range,
            HttpCompletionOption completionOption,
            CancellationToken token = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Range = range.ToRangeHeaderValue();

            return GetPartialAsyncInternal(client, request, completionOption, token);
        }
        public static async Task GetPartialAsync(this HttpClient client, 
            Uri uri, 
            Stream dst,
            HttpRange range,
            IProgress<float> progress = null,
            CancellationToken token = default)
        {
            using var response = await GetPartialAsync(client,
                uri,
                range,
                HttpCompletionOption.ResponseHeadersRead,
                token);

            await GetPartialAsyncInternal(client, response, dst, range, progress, token);
        }

        static async Task<HttpResponseMessage> GetPartialAsyncInternal(HttpClient client, 
            HttpRequestMessage request,
            HttpCompletionOption completionOption,
            CancellationToken token)
        {
            var response = await client.SendAsync(request, completionOption, token);

            // Verify if the server actually supports partial content (HTTP 206)
            if (response.StatusCode == HttpStatusCode.OK)
            {
                throw new NotSupportedException("The server does not support partial downloads (HTTP 206 was not returned).");
            }

            return response;
        }

        static async Task GetPartialAsyncInternal(HttpClient client,
            HttpResponseMessage response,
            Stream dst,
            HttpRange range,
            IProgress<float> progress,
            CancellationToken token)
        {
            response.EnsureSuccessStatusCode();

            var totalBytesToDownload = response.Content.Headers.ContentLength ?? -1L;
            var bytesTransferred = 0;

            using var contentStream = await response.Content.ReadAsStreamAsync();

            using (var lease = ArrayLease<byte>.Rent(8192, false))
            {
                var buffer = lease.Array;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                {
                    await dst.WriteAsync(buffer, 0, bytesRead, token);

                    bytesTransferred += bytesRead;

                    if (progress != null && totalBytesToDownload != -1)
                    {
                        var percent = (float)bytesTransferred / totalBytesToDownload;
                        progress.Report(percent);
                    }
                }
            }
        }
    }
}
