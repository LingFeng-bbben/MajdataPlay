using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net
{
    internal class HttpTransmitException : Exception
    {
        public HttpResponseMessage ResponseMessage { get; init; }
        public HttpStatusCode StatusCode { get; init; }
        public HttpRequestError RequestError { get; init; }
        public HttpTransmitException(string message):base(message)
        {
            
        }
        public HttpTransmitException(HttpRequestException e): base(e.Message)
        {

        }
    }
}
