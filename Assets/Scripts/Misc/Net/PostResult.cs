using MajdataPlay.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net
{
    public readonly struct PostResult
    {
        public bool IsSuccess
        {
            get
            {
                var value = (int)StatusCode;
                return value.InRange(200, 299) && RequestError == HttpRequestError.NoError;
            }
        }
        public long Length { get; init; }
        public DateTime StartAt { get; init; }
        public HttpStatusCode StatusCode { get; init; }
        public HttpRequestError RequestError { get; init; }
        public HttpResponseMessage? ResponseMessage { get; init; }
    }
}
