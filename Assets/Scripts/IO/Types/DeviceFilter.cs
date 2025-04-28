using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.IO
{
    internal readonly struct DeviceFilter
    {
        public int Index { get; init; }
        public int ProductId { get; init; }
        public int VendorId { get; init; }
        public string? DeviceName { get; init; }
    }
}
