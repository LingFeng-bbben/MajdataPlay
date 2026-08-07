using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    internal readonly struct CurlVersionInfo
    {
        public int Age { get; init; }

        public string Version { get; init; }

        public uint VersionNum { get; init; }

        public string Host { get; init; }

        public int Features { get; init; }

        public string SslVersion { get; init; }

        public long SslVersionNum { get; init; }

        public string LibzVersion { get; init; }

        public string[] Protocols { get; init; }
    }
}
