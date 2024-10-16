using MajdataPlay.Extensions;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net
{
    public readonly struct DownloadResult
    {
        public bool IsSuccess
        {
            get
            {
                var value = (int)StatusCode;
                if (StatusCode is HttpStatusCode.NoContent)
                    return false;
                else
                    return value.InRange(200, 299) && RequestError == HttpRequestError.NoError;
            }
        }
        public long Length { get; init; }
        public string SavePath { get; init; }
        public DateTime StartAt { get; init; }
        public HttpStatusCode StatusCode { get; init; }
        public HttpRequestError RequestError { get; init; }

        public FileStream ReadAsStream()
        {
            ThrowIfFailed();
            return File.OpenRead(SavePath);
        }
        public string ReadAsString()
        {
            ThrowIfFailed();
            return File.ReadAllText(SavePath);
        }
        public async Task<string> ReadAsStringAsync()
        {
            ThrowIfFailed();
            using var stream = ReadAsStream();
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer, 0, buffer.Length);

            return Encoding.UTF8.GetString(buffer);
        }
        void ThrowIfFailed()
        {
            if (!IsSuccess)
                throw new InvalidOperationException("This download operation was unsuccessful");
        }
    }
}
