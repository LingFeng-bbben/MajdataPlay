using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Types
{
    public class TouchHoldPoolingInfo : NotePoolingInfo
    {
        public float LastFor { get; init; }
        public char AreaPos { get; init; }
        public bool IsFirework { get; init; }
    }
}
