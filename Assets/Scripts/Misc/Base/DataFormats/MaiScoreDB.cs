using MajdataPlay.Collections;
using MajdataPlay.Scenes.Game;
using Newtonsoft.Json;
using SQLite;
using System;

#nullable enable
namespace MajdataPlay
{
    [Table("MaiScores")]
    public class MaiScoreDB
    {
        [PrimaryKey]
        public string HashLevel { get; set; } = string.Empty;

        [Indexed]
        public string Hash { get; set; } = string.Empty;

        public int ChartLevelInt { get; set; }

        public double AccDX { get; set; }
        public double AccClassic { get; set; }
        public long DXScore { get; set; }
        public long TotalDXScore { get; set; }
        public long Fast { get; set; }
        public long Late { get; set; }
        public long PlayCount { get; set; }
        public string? JudgeDetailJson { get; set; }
        public long TimestampTicks { get; set; }
        public int ComboStateInt { get; set; }

        static readonly JsonSerializerSettings _jsonSettings = new()
        {
            Converters = new System.Collections.Generic.List<JsonConverter>
            {
                new Json.JudgeDetailConverter(),
                new Json.JudgeInfoConverter(),
            }
        };

        public static string MakeKey(string hash, ChartLevel level) => $"{hash}|{(int)level}";
        public static string MakeKey(string hash, int levelInt) => $"{hash}|{levelInt}";

        public MaiScore ToMaiScore()
        {
            JudgeDetail? judgeDetail = null;
            if (!string.IsNullOrEmpty(JudgeDetailJson))
            {
                try
                {
                    judgeDetail = JsonConvert.DeserializeObject<JudgeDetail>(JudgeDetailJson, _jsonSettings);
                }
                catch
                {
                    judgeDetail = JudgeDetail.Empty;
                }
            }

            return new MaiScore()
            {
                Hash = Hash,
                ChartLevel = (ChartLevel)ChartLevelInt,
                Acc = new Accurate { DX = AccDX, Classic = AccClassic },
                DXScore = DXScore,
                TotalDXScore = TotalDXScore,
                Fast = Fast,
                Late = Late,
                PlayCount = PlayCount,
                JudgeDeatil = judgeDetail ?? JudgeDetail.Empty,
                Timestamp = new DateTime(TimestampTicks, DateTimeKind.Local),
                ComboState = (ComboState)ComboStateInt,
            };
        }

        public static MaiScoreDB FromMaiScore(MaiScore score)
        {
            string? judgeJson = null;
            if (score.JudgeDeatil is not null)
            {
                try
                {
                    judgeJson = JsonConvert.SerializeObject(score.JudgeDeatil, _jsonSettings);
                }
                catch
                {
                    judgeJson = null;
                }
            }

            var hash = score.Hash ?? string.Empty;

            return new MaiScoreDB()
            {
                HashLevel = MakeKey(hash, score.ChartLevel),
                Hash = hash,
                ChartLevelInt = (int)score.ChartLevel,
                AccDX = score.Acc.DX,
                AccClassic = score.Acc.Classic,
                DXScore = score.DXScore,
                TotalDXScore = score.TotalDXScore,
                Fast = score.Fast,
                Late = score.Late,
                PlayCount = score.PlayCount,
                JudgeDetailJson = judgeJson,
                TimestampTicks = score.Timestamp.Ticks,
                ComboStateInt = (int)score.ComboState,
            };
        }
    }
}
