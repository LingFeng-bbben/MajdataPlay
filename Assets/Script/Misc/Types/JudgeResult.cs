using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Types
{
    public readonly ref struct JudgeResult
    {
        public JudgeType Result { get; init; }
        public bool IsBreak { get; init; }
        public bool IsFast => Diff < 0;
        public bool IsMiss => Result == JudgeType.Miss;
        /// <summary>
        /// in milliseconds , less than zero is "Fast"
        /// </summary>
        public float Diff { get; init; }
    }
}
