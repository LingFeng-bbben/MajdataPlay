using MajdataPlay.Collections;
using MajdataPlay.Game;
using MajSimai;
using System;
using System.Collections.Generic;

#nullable enable
namespace MajdataPlay
{
    public class MaiScore
    {
        public Accurate Acc { get; set; }
        public long DXScore { get; set; } = 0;
        public long TotalDXScore { get; set; } = 0;
        public long Fast { get; set; } = 0;
        public long Late { get; set; } = 0;
        public ChartLevel ChartLevel { get; init; } = ChartLevel.Easy;
        public string? Hash { get; init; } = null;
        public long PlayCount { get; set; } = 0;
        public JudgeDetail? JudgeDeatil { get; set; } = null;
        public DateTime Timestamp { get; set; } = DateTime.MinValue;
        public ComboState ComboState { get; set; } = ComboState.None;
        public static MaiScore CreateFromResult(in GameResult result, ChartLevel level)
        {
            var songInfo = result.SongDetail;
            var record = new MaiScore()
            {
                ChartLevel = level,
                Hash = songInfo.Hash,
                PlayCount = 0
            };
            record.Acc = result.Acc;

            record.DXScore = result.DXScore;
            record.TotalDXScore = result.TotalDXScore;

            record.JudgeDeatil = result.JudgeRecord;
            record.Fast = result.Fast;
            record.Late = result.Late;
            record.ComboState = result.ComboState > record.ComboState ? result.ComboState : record.ComboState;
            record.Timestamp = DateTime.Now;
            return record;
        }
    }
}
