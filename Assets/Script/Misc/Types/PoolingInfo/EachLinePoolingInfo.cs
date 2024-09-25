using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Types
{
    public class EachLinePoolingInfo : NotePoolingInfo
    {
        public NotePoolingInfo? MemberA { get; init; }
        public NotePoolingInfo? MemberB { get; init; }
        public int CurvLength { get; init; }
    }
}
