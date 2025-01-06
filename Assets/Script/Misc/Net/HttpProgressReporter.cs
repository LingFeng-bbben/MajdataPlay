using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net
{
    public class HttpProgressReporter: IHttpProgressReporter
    {
        public long FileSize { get; private set; } = 0;
        public double Progress { get; private set; } = 0;
        public void OnProgressChanged(DLProgress args)
        {
            FileSize = args.Length;
            Progress = args.Progress;
        }
    }
}
