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
        public HttpStatusCode? StatusCode { get; init; }
        public HttpException()
        {

        }
        public HttpException(string message) : base(message)
        {
            
        }
        public HttpException(string message, Exception inner) : base(message, inner)
        {

        }
        public HttpException(HttpErrorCode errorCode) : this(errorCode, null)
        {
            
        }
        public HttpException(HttpErrorCode errorCode, HttpStatusCode? statusCode) : base()
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
        public HttpException(HttpErrorCode errorCode, HttpStatusCode? statusCode, string message) : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
        public HttpException(HttpErrorCode errorCode, HttpStatusCode? statusCode, string message, Exception inner) : base(message, inner)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }
}
