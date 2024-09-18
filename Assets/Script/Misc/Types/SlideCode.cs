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

        public SlideNode ToNode()
        {
            if(Type != SlideCodeType.Node)
                throw new InvalidOperationException("cannot cast Track to SlideNode");

            return new SlideNode()
            {
                Node = Command,
                Index = Param ?? 0
            };
        }
    }
}
