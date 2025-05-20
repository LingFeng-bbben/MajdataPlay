using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Threading
{
    interface IIOThreadNotifySource: IThreadNotifySource
    {
        ReadOnlySpan<byte> ReadBuffer { get; }
        Span<byte> WriteBuffer { get; }
    }
}
