using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Types
{
    public readonly ref struct JudgeParam
    {
        public Range<float> Critical { get; init; }
    }
}
