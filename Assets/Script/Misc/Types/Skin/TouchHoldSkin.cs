using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Types
{
    public readonly ref struct TouchHoldSkin
    {
        public Sprite[] Fans { get; init; }
        public Sprite[] Fans_Break { get; init; }
        public Sprite Boader { get; init; }
        public Sprite Boader_Break { get; init; }
        public Sprite Point { get; init; }
        public Sprite Point_Break { get; init; }
        public Sprite Off {  get; init; }

        public Material BreakMaterial { get; init; }
        public Material DefaultMaterial { get; init; }
    }
}
