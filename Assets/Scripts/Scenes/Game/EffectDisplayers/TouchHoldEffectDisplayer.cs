using MajdataPlay.Scenes.Game.Notes;
using MajdataPlay.Scenes.Game.Notes.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Scenes.Game
{
    internal sealed class TouchHoldEffectDisplayer : TapEffectDisplayer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool PlayResult(in NoteJudgeResult judgeResult)
        {
            bool canPlay;
            if (judgeResult.IsBreak)
            {
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Settings.Display.BreakJudgeType, judgeResult);
            }
            else
            {
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Settings.Display.TouchJudgeType, judgeResult);
            }
            if (!canPlay)
            {
                return false;
            }
            JudgeTextDisplayer.Play(judgeResult);
            return true;
        }
    }
}
