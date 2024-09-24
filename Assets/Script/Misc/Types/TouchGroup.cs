using MajdataPlay.Interfaces;
using System;
using System.Linq;

namespace MajdataPlay.Types
{
    public class TouchGroup
    {
        public float Percent
        {
            get
            {
                if (Members.Length == 0)
                    return 0f;
                var finished = Members.Where(x => x.State == NoteStatus.Destroyed);
                return finished.Count() / (float)Members.Length;
            }
        }
        public JudgeType? JudgeResult
        {
            get => _judgeResult;
            set
            {
                if (Percent > 0.5f)
                    return;
                else
                    _judgeResult = value;
            }
        }
        public float JudgeDiff { get; set; } = 0;
        public IStatefulNote[] Members { get; set; } = Array.Empty<IStatefulNote>();
        JudgeType? _judgeResult = null;
    }
}
