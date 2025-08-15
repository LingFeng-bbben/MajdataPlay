using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Scenes.Game.Notes.Skins
{
    public readonly ref struct TouchSkin
    {
        public Sprite Normal { get; init; }
        public Sprite Each { get; init; }
        public Sprite Break { get; init; }
        public Sprite Point_Normal { get; init; }
        public Sprite Point_Each { get; init; }
        public Sprite Point_Break { get; init; }
        public ReadOnlySpan<Sprite> Border_Normal { get; init; }
        public ReadOnlySpan<Sprite> Border_Each { get; init; }
        public ReadOnlySpan<Sprite> Border_Break { get; init; }

        public Sprite JustBorder { get; init; }
    }
}
