using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Scenes.Game.Notes.Skins
{
    public readonly ref struct HoldSkin
    {
        public Sprite Normal { get; init; }
        public Sprite Normal_On { get; init; }
        public Sprite Off { get; init; }
        public Sprite Each { get; init; }
        public Sprite Each_On { get; init; }
        public Sprite Break { get; init; }
        public Sprite Break_On { get; init; }
        public Sprite Ex { get; init; }

        public ReadOnlySpan<Sprite> Ends { get; init; }

        public ReadOnlySpan<Sprite> GuideLines { get; init; }
        public ReadOnlySpan<Color> ExEffects { get; init; }
    }
}
