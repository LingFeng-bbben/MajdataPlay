using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Types
{
    public class DanInfo
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public string[] SongHashs { get; init; }
        public int[] SongLevels { get; init; }
        public int StartHP { get; init; } = 50;
        public int RestoreHP { get; init; } = 10;
        public bool IsPlayList { get; init; } = false;
        public bool IsForceGameover { get; init; } = false;
        public Dictionary<JudgeGrade, int> Damages { get; init; } = new Dictionary<JudgeGrade, int> {
            { JudgeGrade.Miss,-5 },
            { JudgeGrade.TooFast,-5 },
            { JudgeGrade.LateGood,-3 },
            { JudgeGrade.FastGood,-3 },
            { JudgeGrade.LateGreat,-2 },
            { JudgeGrade.FastGreat,-2 },
            { JudgeGrade.LateGreat1,-2 },
            { JudgeGrade.FastGreat1,-2 },
            { JudgeGrade.LateGreat2,-2 },
            { JudgeGrade.FastGreat2,-2 },
            { JudgeGrade.FastPerfect1,0 },
            { JudgeGrade.LatePerfect1,0 },
            { JudgeGrade.FastPerfect2,0 },
            { JudgeGrade.LatePerfect2,0 },
            { JudgeGrade.Perfect,0 },
        };
    }
}
