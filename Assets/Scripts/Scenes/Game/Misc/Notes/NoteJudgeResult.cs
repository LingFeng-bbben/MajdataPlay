using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Game.Notes
{
    public readonly ref struct NoteJudgeResult
    {
        public JudgeGrade Grade { get; init; }
        public bool IsBreak { get; init; }
        public bool IsEX { get; init; }
        public bool IsFast => Diff < 0;
        public bool IsMissOrTooFast => Grade is JudgeGrade.Miss or JudgeGrade.TooFast;
        /// <summary>
        /// in milliseconds , less than zero is "Fast"
        /// </summary>
        public float Diff { get; init; }
    }
}
