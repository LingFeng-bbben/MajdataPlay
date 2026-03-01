using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net
{
    public interface INetProgress: IProgress<float>
    {
        long TotalBytes { get; set; }
        long ReadBytes { get; set; }
    }
}
