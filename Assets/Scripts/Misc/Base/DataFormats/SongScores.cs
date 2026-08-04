using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay;
internal readonly struct SongScores
{
    public required MaiScore Easy { get; init; }
    public required MaiScore Basic { get; init; }
    public required MaiScore Advance { get; init; }
    public required MaiScore Expert { get; init; }
    public required MaiScore Master { get; init; }
    public required MaiScore ReMaster { get; init; }
    public required MaiScore UTAGE { get; init; }

    public static SongScores Create(ISongDetail songDetail)
    {
        return Create(songDetail.Hash);
    }
    public static SongScores Create(string hash)
    {
        if(string.IsNullOrEmpty(hash))
        {
            throw new ArgumentNullException(nameof(hash));
        }
        return new SongScores()
        {
            Easy = new MaiScore()
            {
                Hash = hash,
                ChartLevel = ChartLevel.Easy,
                PlayCount = 0
            },
            Basic = new MaiScore()
            {
                Hash = hash,
                ChartLevel = ChartLevel.Basic,
                PlayCount = 0
            },
            Advance = new MaiScore()
            {
                Hash = hash,
                ChartLevel = ChartLevel.Advance,
                PlayCount = 0
            },
            Expert = new MaiScore()
            {
                Hash = hash,
                ChartLevel = ChartLevel.Expert,
                PlayCount = 0
            },
            Master = new MaiScore()
            {
                Hash = hash,
                ChartLevel = ChartLevel.Master,
                PlayCount = 0
            },
            ReMaster = new MaiScore()
            {
                Hash = hash,
                ChartLevel = ChartLevel.ReMaster,
                PlayCount = 0
            },
            UTAGE = new MaiScore()
            {
                Hash = hash,
                ChartLevel = ChartLevel.UTAGE,
                PlayCount = 0
            }
        };
    }
}
