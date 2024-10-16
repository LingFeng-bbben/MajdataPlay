using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net
{
    public readonly struct DownloadResult
    {
        public long Length { get; init; }
        public string SavePath { get; init; }
        public DateTime StartAt { get; init; }
        public HttpStatusCode StatusCode { get; init; }
        public HttpRequestError RequestError { get; init; }
    }
}
