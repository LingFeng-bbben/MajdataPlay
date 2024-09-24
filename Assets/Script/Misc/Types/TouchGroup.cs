using MajdataPlay.Interfaces;
using System;
using System.Collections.Generic;

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
                return results.Count / (float)Members.Length;
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
        List<JudgeType> results = new();

        public void RegisterResult(in JudgeType result)
        {
            if (result == JudgeType.Miss)
                return;
            results.Add(result);
        }

        JudgeType? _judgeResult = null;
    }
}
