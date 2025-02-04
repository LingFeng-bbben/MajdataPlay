using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net
{
    public readonly struct PostRequest
    {
        public Uri RequestAddress { get; init; }
        public int MaxRetryCount { get; init; }
        public HttpContent Content { get; init; }
    }
}
