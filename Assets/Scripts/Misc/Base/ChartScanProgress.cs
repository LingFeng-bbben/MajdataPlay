using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Types
{
    public struct ChartScanProgress
    {
        public ChartStorageLocation StorageType { get; init; }
        public string Message { get; init; }
    }
}
