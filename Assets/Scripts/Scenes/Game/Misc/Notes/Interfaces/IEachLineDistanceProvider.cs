using System;
using System.Collections.Generic;
using System.Text;

namespace MajdataPlay.Game.Notes
{
    internal interface IEachLineDistanceProvider
    {
        float Distance { get; }
        bool IsAnyNoteEnded { get; }
    }
}
