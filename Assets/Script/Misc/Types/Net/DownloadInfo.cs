using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net
{
    public readonly struct DownloadInfo
    {
        public string SavePath { get; init; }
        public Uri RequestAddress { get; init; }
        public bool MultiThread { get; init; }
        public int ThreadCount { get; init; }
        public int MaxRetryCount { get; init; }
        public Action<DLProgress>? OnProgressUpdated { get; init; }
    }
}
