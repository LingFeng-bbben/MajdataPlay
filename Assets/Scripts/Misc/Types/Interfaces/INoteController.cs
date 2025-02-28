using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay
{
    internal interface INoteController: INoteTimeProvider
    {
        float AudioLength { get; }
        bool IsStart { get; }
        bool IsAutoplay { get; }
        AutoplayMode AutoplayMode { get; }
        JudgeGrade AutoplayGrade { get; }
        JudgeStyleType JudgeStyle { get; }
        Material BreakMaterial { get; }
        Material DefaultMaterial { get; }
        Material HoldShineMaterial { get; }
    }
}
