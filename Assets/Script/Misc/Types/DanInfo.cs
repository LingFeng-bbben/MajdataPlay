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
        public Dictionary<JudgeType, int> Damages { get; init; } = new Dictionary<JudgeType, int> {
            { JudgeType.Miss,-5 },
            { JudgeType.TooFast,-5 },
            { JudgeType.LateGood,-3 },
            { JudgeType.FastGood,-3 },
            { JudgeType.LateGreat,-2 },
            { JudgeType.FastGreat,-2 },
            { JudgeType.LateGreat1,-2 },
            { JudgeType.FastGreat1,-2 },
            { JudgeType.LateGreat2,-2 },
            { JudgeType.FastGreat2,-2 },
            { JudgeType.FastPerfect1,0 },
            { JudgeType.LatePerfect1,0 },
            { JudgeType.FastPerfect2,0 },
            { JudgeType.LatePerfect2,0 },
            { JudgeType.Perfect,0 },
        };
    }
}
