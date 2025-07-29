using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Scenes.Game.Notes.Skins
{
    public readonly ref struct EachLineSkin
    {
        public ReadOnlySpan<Sprite> EachGuideLines { get; init; }
    }
}
