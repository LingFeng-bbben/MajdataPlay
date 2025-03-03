using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.View.Types
{
    internal readonly struct MajWsRequestParse
    {
        public double StartAt { get; init; }
        public string SimaiFumen { get; init; }
        public double Offset { get; init; }
    }
}
