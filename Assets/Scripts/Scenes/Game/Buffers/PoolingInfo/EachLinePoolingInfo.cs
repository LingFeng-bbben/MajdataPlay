using MajdataPlay.Game.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Game.Buffers
{
    internal class EachLinePoolingInfo : NotePoolingInfo
    {
        public NotePoolingInfo? MemberA { get; init; } = null;
        public NotePoolingInfo? MemberB { get; init; } = null;
        public int CurvLength { get; init; }
    }
}
