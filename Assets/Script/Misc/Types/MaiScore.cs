using MajSimaiDecode;
using System;
using System.Collections.Generic;

#nullable enable
namespace MajdataPlay.Types
{
    public class MaiScore
    {
        public double Accurate { get; set; } = 0.0000;
        public double Accurate_Classic { get; set; } = 0.00;
        public long DXScore { get; set; } = 0;
        public long Fast { get; set; } = 0;
        public long Late { get; set; } = 0;
        public string? Hash { get; set; } = null;
        public long PlayCount { get; set; } = 0;
        public JudgeDetail? JudgeDeatil { get; set; } = null;
        public DateTime Timestamp { get; set; } = DateTime.MinValue;
        public ComboState ComboState { get; set; } = ComboState.None;
    }
}
