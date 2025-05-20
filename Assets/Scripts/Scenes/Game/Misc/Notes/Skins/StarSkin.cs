using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Game.Notes.Skins
{
    public readonly ref struct StarSkin
    {
        public Sprite Normal { get; init; }
        public Sprite Double { get; init; }
        public Sprite Each { get; init; }
        public Sprite EachDouble { get; init; }
        public Sprite Break { get; init; }
        public Sprite BreakDouble { get; init; }
        public Sprite Ex { get; init; }
        public Sprite ExDouble { get; init; }

        public ReadOnlySpan<Sprite> GuideLines { get; init; }
        public ReadOnlySpan<Color> ExEffects { get; init; }
    }
}
