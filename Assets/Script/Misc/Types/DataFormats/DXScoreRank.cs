namespace MajdataPlay.Types
{
    public readonly ref struct DXScoreRank
    {
        public int Rank { get; init; }
        public long Upper { get; init; }
        public long Lower { get; init; }
        public DXScoreRank(long dxScore,long totalDXScore)
        {
            var percent = dxScore / (double)totalDXScore;
            Rank = percent switch
            {
                > 0.97 => 5,
                > 0.95 => 4,
                > 0.93 => 3,
                > 0.90 => 2,
                > 0.85 => 1,
                _ => 0
            };
            switch(Rank)
            {
                case 5:
                    Upper = totalDXScore * 1;
                    Lower = (long)(totalDXScore * 0.97);
                    break;
                case 4:
                    Upper = (long)(totalDXScore * 0.97);
                    Lower = (long)(totalDXScore * 0.95);
                    break;
                case 3:
                    Upper = (long)(totalDXScore * 0.95);
                    Lower = (long)(totalDXScore * 0.93);
                    break;
                case 2:
                    Upper = (long)(totalDXScore * 0.93);
                    Lower = (long)(totalDXScore * 0.90);
                    break;
                case 1:
                    Upper = (long)(totalDXScore * 0.90);
                    Lower = (long)(totalDXScore * 0.85);
                    break;
                default:
                    Upper = (long)(totalDXScore * 0.85);
                    Lower = 0;
                    break;
                    
            }
        }
    }
}
