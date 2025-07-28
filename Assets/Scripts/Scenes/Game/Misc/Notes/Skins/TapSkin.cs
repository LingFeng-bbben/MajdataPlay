using System;
using UnityEngine;

namespace MajdataPlay.Scenes.Game.Notes.Skins
{
    public readonly ref struct TapSkin
    {
        public Sprite Normal { get; init; }
        public Sprite Each { get; init; }
        public Sprite Break { get; init; }
        public Sprite Ex { get; init; }

        public ReadOnlySpan<Sprite> GuideLines { get; init; }
        public ReadOnlySpan<Color> ExEffects { get; init; }
    }
}
