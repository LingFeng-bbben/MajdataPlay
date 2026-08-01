using MajdataPlay.Scenes.Game.Notes;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MajdataPlay
{
    public class DanInfo
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public string[] SongHashs { get; init; }
        public Dictionary<string, JToken> Mods { get; init; } = new();
        public int[] SongLevels { get; init; }
        public int StartHP { get; init; } = 50;
        public int RestoreHP { get; init; } = 10;
        public bool IsPlayList { get; init; } = false;
        public bool IsForceGameover { get; init; } = false;
        public Dictionary<JudgeGrade, int> Damages { get; init; } = new Dictionary<JudgeGrade, int>
        {
            { JudgeGrade.Miss,-5 },
            { JudgeGrade.TooFast,-5 },
            { JudgeGrade.LateGood,-3 },
            { JudgeGrade.FastGood,-3 },
            { JudgeGrade.LateGreat,-2 },
            { JudgeGrade.FastGreat,-2 },
            { JudgeGrade.LateGreat2nd,-2 },
            { JudgeGrade.FastGreat2nd,-2 },
            { JudgeGrade.LateGreat3rd,-2 },
            { JudgeGrade.FastGreat3rd,-2 },
            { JudgeGrade.FastPerfect2nd,0 },
            { JudgeGrade.LatePerfect2nd,0 },
            { JudgeGrade.FastPerfect3rd,0 },
            { JudgeGrade.LatePerfect3rd,0 },
            { JudgeGrade.Perfect,0 },
        };
    }
}
