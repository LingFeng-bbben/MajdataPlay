using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Types.Mods
{
    public class MajGameMod
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public ModType Type { get; init; } = ModType.OTHER_MOD;
        public ModGroupingType SortGroup { get; init; } = ModGroupingType.Other;
        public ModType[] Conflicts { get; init; } = Array.Empty<ModType>();
        public float Value { get; set; }
        public Sprite? Icon { get; init; }
        public bool Active { get; set; } = false;
    }
}
