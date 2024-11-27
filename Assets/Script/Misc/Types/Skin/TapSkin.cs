using UnityEngine;

namespace MajdataPlay.Types
{
    public readonly ref struct TapSkin
    {
        public Sprite Normal { get; init; }
        public Sprite Each { get; init; }
        public Sprite Break { get; init; }
        public Sprite Ex { get; init; }

        public Sprite[] NoteLines { get; init; }
        public Color[] ExEffects { get; init; }
    }
}
