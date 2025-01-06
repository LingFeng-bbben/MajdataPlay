using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Throws an exception if the System.Net.Http.HttpResponseMessage.IsSuccessStatusCode property for the HTTP response is false.
        /// </summary>
        /// <param name="source"></param>
        /// <exception cref="HttpTransmitException"></exception>
        public static void ThrowIfTransmitFailure(this HttpResponseMessage source)
        {
            try
            {
                source.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                var errorCode = e.GetErrorCode();
                throw new HttpTransmitException(e)
                {
                    RequestError = errorCode,
                    StatusCode = source.StatusCode,
                    ResponseMessage = source
                };
            }
        }
    }
}
