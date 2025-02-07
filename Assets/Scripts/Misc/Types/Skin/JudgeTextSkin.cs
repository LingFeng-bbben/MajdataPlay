using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Types
{
    public readonly struct JudgeTextSkin
    {
        public Sprite CP_Shine { get; init; }
        public Sprite P_Shine { get; init; }
        public Sprite Break_2600_Shine { get; init; }
        public SpecialTextSkin CriticalPerfect { get; init; }
        public SpecialTextSkin Break_2600 { get; init; }
        public SpecialTextSkin Break_2550 { get; init; }
        public SpecialTextSkin Break_2500 { get; init; }
        public SpecialTextSkin Break_2000 { get; init; }
        public SpecialTextSkin Break_1500 { get; init; }
        public SpecialTextSkin Break_1250 { get; init; }
        public SpecialTextSkin Break_1000 { get; init; }
        public Sprite Break_0 { get; init; }
        public SpecialTextSkin Perfect { get; init; }
        public SpecialTextSkin Great { get; init; }
        public SpecialTextSkin Good { get; init; }
        public Sprite Miss { get; init; }
        public Sprite Fast { get; init; }
        public Sprite Late { get; init; }

    }
}
