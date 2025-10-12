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
        public HttpErrorCode ErrorCode { get; }
        public HttpStatusCode? StatusCode { get; }

        readonly string _errMsg;

        public HttpException(HttpErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
            _errMsg = message;
        }
        public HttpException(HttpErrorCode errorCode, string message, Exception inner) : base(message, inner)
        {
            ErrorCode = errorCode;
            _errMsg = message;
        }
        public HttpException(HttpErrorCode errorCode)
        {
            ErrorCode = errorCode;
            _errMsg = $"An error occurred while sending the HTTP request:\nErrorCode: {errorCode}";
        }
        public HttpException(HttpErrorCode errorCode, HttpStatusCode? statusCode) : base()
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;

            if (statusCode is not null)
            {
                _errMsg = $"An error occurred while sending the HTTP request:\nErrorCode: {errorCode}\nStatusCode: {statusCode}";
            }
            else
            {
                _errMsg = $"An error occurred while sending the HTTP request:\nErrorCode: {errorCode}";
            }
        }
        public HttpException(HttpErrorCode errorCode, HttpStatusCode? statusCode, string message) : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;

            if (statusCode is not null)
            {
                _errMsg = $"An error occurred while sending the HTTP request:\nErrorCode: {errorCode}\nStatusCode: {statusCode}";
            }
            else
            {
                _errMsg = $"An error occurred while sending the HTTP request:\nErrorCode: {errorCode}";
            }
        }
        public HttpException(HttpErrorCode errorCode, HttpStatusCode? statusCode, string message, Exception inner) : base(message, inner)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;

            if (statusCode is not null)
            {
                _errMsg = $"An error occurred while sending the HTTP request:\nErrorCode: {errorCode}\nStatusCode: {statusCode}";
            }
            else
            {
                _errMsg = $"An error occurred while sending the HTTP request:\nErrorCode: {errorCode}";
            }
        }
        public override string ToString()
        {
            return _errMsg;
        }
    }
}
