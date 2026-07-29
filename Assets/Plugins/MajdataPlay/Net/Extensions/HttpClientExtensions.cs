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
        public static async Task GetPartialAsync(this HttpClient client, 
            Uri uri, 
            Stream dst,
            HttpRange range,
            IProgress<float> progress = null,
            CancellationToken token = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Range = range.ToRangeHeaderValue();

            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                token);

            response.EnsureSuccessStatusCode();

            // Verify if the server actually supports partial content (HTTP 206)
            if (response.StatusCode != HttpStatusCode.PartialContent)
            {
                throw new NotSupportedException("The server does not support partial downloads (HTTP 206 was not returned).");
            }

            var totalBytesToDownload = response.Content.Headers.ContentLength ?? -1L;
            var bytesTransferred = 0;

            using var contentStream = await response.Content.ReadAsStreamAsync();

            var buffer = new byte[8192];
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
