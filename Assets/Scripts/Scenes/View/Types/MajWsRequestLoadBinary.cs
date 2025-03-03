using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.View.Types
{
    internal readonly struct MajWsRequestLoadBinary
    {
        public byte[] Track { get; init; }
        public byte[] Image { get; init; }
        public byte[] Video { get; init; }
    }
}
