using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net
{
    public interface IHttpProgressReporter: IProgressReporter<double,DLProgress>
    {
        long FileSize { get; }
    }
}
