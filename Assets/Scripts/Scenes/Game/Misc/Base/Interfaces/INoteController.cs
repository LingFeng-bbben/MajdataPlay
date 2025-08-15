using MajdataPlay.Scenes.Game.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Scenes.Game
{
    public interface INoteController : INoteTimeProvider
    {
        float AudioLength { get; }
        bool IsStart { get; }
        bool IsAutoplay { get; }
        public GameModInfo ModInfo { get; }
        JudgeGrade AutoplayGrade { get; }
        Material BreakMaterial { get; }
        Material DefaultMaterial { get; }
        Material HoldShineMaterial { get; }
    }
}
