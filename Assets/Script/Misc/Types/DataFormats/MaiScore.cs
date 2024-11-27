using MajdataPlay.Collections;
using MajSimaiDecode;
using System;
using System.Collections.Generic;

#nullable enable
namespace MajdataPlay.Types
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
    }
}
