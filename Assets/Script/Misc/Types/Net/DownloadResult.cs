using MajdataPlay.Extensions;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
#nullable enable
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
        public HttpResponseMessage? ResponseMessage { get; init; }
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
        public void ThrowIfFailed()
        {
            if (!IsSuccess)
            {
                if(ResponseMessage is null)
                    throw new ArgumentNullException("This result object has no HttpResponseMessage");
                throw new HttpTransmitException("This download operation was unsuccessful")
                {
                    ResponseMessage = ResponseMessage,
                    StatusCode = StatusCode,
                    RequestError = RequestError,
                };
            }
        }
    }
}
