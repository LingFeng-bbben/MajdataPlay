using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Scenes.Game.Notes.Skins
{
    public readonly ref struct TouchHoldSkin
    {
        public ReadOnlySpan<Sprite> Fans { get; init; }
        public ReadOnlySpan<Sprite> Fans_Break { get; init; }
        public ReadOnlySpan<Sprite> Fans_Mine { get; init; }
        public ReadOnlySpan<Sprite> Fans_Break_Mine { get; init; }
        public Sprite Boader { get; init; }
        public Sprite Boader_Break { get; init; }
        public Sprite Boader_Mine { get; init; }
        public Sprite Boader_Break_Mine { get; init; }
        public Sprite Point { get; init; }
        public Sprite Point_Each { get; init; }
        public Sprite Point_Break { get; init; }
        public Sprite Point_Mine { get; init; }
        public Sprite Point_Break_Mine { get; init; }
        public Sprite Off { get; init; }

    }
}
