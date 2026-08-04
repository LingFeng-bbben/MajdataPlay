using System;
using System.Collections.Generic;

namespace MajdataPlay.Scenes.Game.Notes.Touch
{
    public sealed class TouchGroup
    {
        public float Percent
        {
            get
            {
                if (Members.Length == 0)
                {
                    return 0f;
                }
                return _results.Count / (float)Members.Length;
            }
        }
        public JudgeGrade? JudgeResult
        {
            get => _judgeResult;
            set
            {
                if (Percent > 0.5f)
                {
                    return;
                }
                else
                {
                    _judgeResult = value;
                }
            }
        }
        public float JudgeDiff { get; set; } = 0;
        public object[] Members { get; set; } = Array.Empty<IStatefulNote>();
        List<JudgeGrade> _results = new();

        public void RegisterResult(in JudgeGrade result)
        {
            if (result.IsMissOrTooFast())
            {
                return;
            }
            _results.Add(result);
        }

        JudgeGrade? _judgeResult = null;
    }
}
