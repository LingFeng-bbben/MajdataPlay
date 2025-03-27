using MajdataPlay.Extensions;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;

namespace MajdataPlay.Game.Notes.Touch
{
    public class TouchGroup
    {
        public float Percent
        {
            get
            {
                if (Members.Length == 0)
                    return 0f;
                return results.Count / (float)Members.Length;
            }
        }
        public JudgeGrade? JudgeResult
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
        public object[] Members { get; set; } = Array.Empty<IStatefulNote>();
        List<JudgeGrade> results = new();

        public void RegisterResult(in JudgeGrade result)
        {
            if (result.IsMissOrTooFast())
                return;
            results.Add(result);
        }

        JudgeGrade? _judgeResult = null;
    }
}
