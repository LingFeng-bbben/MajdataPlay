using MajdataPlay.Scenes.Game.Notes.Behaviours;
using System;
using System.Collections.Generic;
using System.Text;

namespace MajdataPlay.Game.Notes
{
    internal interface IEachLineNoteBinding
    {
        void Bind(NoteDrop instance);
        void Unbind(NoteDrop instance);
    }
}
