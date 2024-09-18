using MajdataPlay.Types;

namespace MajdataPlay.Types
{
    public readonly struct SlideNode
    {
        public SlideCommand Node { get; init; }
        public int Index { get; init; }
    }
}
