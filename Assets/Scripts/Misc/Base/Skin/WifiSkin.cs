using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Types
{
    public readonly ref struct WifiSkin
    {
        public StarSkin Star { get; init; }
        public ReadOnlySpan<Sprite> Normal { get; init; }
        public ReadOnlySpan<Sprite> Each { get; init; }
        public ReadOnlySpan<Sprite> Break { get; init; }
    }
}
