using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Types
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

        public Sprite[] Ends { get; init; } 

        public Sprite[] NoteLines { get; init; }
        public Color[] ExEffects { get; init; }
    }
}
