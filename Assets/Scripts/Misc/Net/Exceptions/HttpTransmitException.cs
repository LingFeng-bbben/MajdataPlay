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
    internal class HttpException : HttpRequestException
    {
        public HttpErrorCode ErrorCode { get; init; }
        public HttpException()
        {

        }
        public HttpException(string message) : base(message)
        {
            
        }
        public HttpException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
