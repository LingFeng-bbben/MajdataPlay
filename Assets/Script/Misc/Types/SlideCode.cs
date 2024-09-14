using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Types
{
    public readonly struct SlideCode
    {
        public SlideCommand Command { get; init; }
        public int? Param { get; init; }
        public SlideCodeType Type { get; init; }
    }
}
