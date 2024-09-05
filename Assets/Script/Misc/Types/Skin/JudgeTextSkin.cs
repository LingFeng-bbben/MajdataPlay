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
        public Sprite CriticalPerfect { get; init; }
        public Sprite CP_Break { get; init; }
        public Sprite P_Break { get; init; }
        public Sprite Perfect { get; init; }
        public Sprite Great { get; init; }
        public Sprite Good { get; init; }
        public Sprite Miss { get; init; }
        public Sprite Fast { get; init; }
        public Sprite Late { get; init; }

    }
}
