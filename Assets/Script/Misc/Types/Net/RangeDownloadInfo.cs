using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net
{
    public readonly struct RangeDownloadInfo
    {
        public int Index { get; init; }
        public long StartAt { get; init; }
        public long SegmentLength { get; init; }
        public int MaxRetryCount { get; init; }
        public string SavePath { get; init; }
        public string UserAgent { get; init; }
        public IProgress<ReportEventArgs> Reporter { get; init; }
        public Uri RequestAddress { get; init; }
    }
}
