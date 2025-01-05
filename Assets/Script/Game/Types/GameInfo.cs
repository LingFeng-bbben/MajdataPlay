using MajdataPlay.Extensions;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Game.Types
{
    internal class GameInfo
    {
        public GameMode Mode { get; init; }
        public SongDetail? Current { get; private set; } = null;
        public ChartLevel CurrentLevel { get; private set; } = ChartLevel.Easy;
        public bool IsDanMode => Mode == GameMode.Dan;

        public SongDetail[] Charts => _chartQueue;
        public ChartLevel[] Levels => _levels;

        // Dan Mode
        public int CurrentHP { get; set; } = 0;
        public int MaxHP { get; set; } = 0;
        public int HPRecover { get; set; } = 0;
        public bool IsForceGameover => DanInfo?.IsForceGameover ?? false; 
        public DanInfo? DanInfo { get; init; } = null;
        // TO-DO: Practice Mode
        public GameResult[] Results { get; init; }

        int _index = 0;
        int _playedCount = 0;
        SongDetail[] _chartQueue;
        ChartLevel[] _levels;
        public GameInfo(GameMode mode, SongDetail[] chartQueue,ChartLevel[] levels)
        {
            Mode = mode;
            if(chartQueue is null)
            {
                _chartQueue = Array.Empty<SongDetail>();
                Results = Array.Empty<GameResult>();
                _levels = Array.Empty<ChartLevel>();
            }
            else
            {
                var count = 0;
                for (var i = 0; i < chartQueue.Length; i++)
                {
                    if (chartQueue[i] is not null)
                        count++;
                }
                _levels = levels;
                Results = new GameResult[count];
                _chartQueue = chartQueue;
                Current = chartQueue[0];
                CurrentLevel = levels[0];
            }
        }
        bool MoveNext()
        {
            if (_index >= _chartQueue.Length)
                return false;

            _index++;
            for (; _index < _chartQueue.Length; _index++)
            {
                
                if (_chartQueue[_index] is null)
                {
                    continue;
                }
                else
                {
                    Current = _chartQueue[_index];
                    CurrentLevel = _levels[_index];
                    return true;
                }
            }

            return false;
        }
        public void OnNoteJudged(in JudgeGrade grade)
        {
            if (!IsDanMode || DanInfo is null)
                return;
            var damage = DanInfo.Damages[grade];
            CurrentHP += damage;
            CurrentHP = CurrentHP.Clamp(0, MaxHP);
        }
        public void RecordResult(in GameResult result)
        {
            if (_playedCount == Results.Length)
                return;
            Results[_playedCount++] = result;
        }
        public GameResult? GetLastResult()
        {
            if (_playedCount == 0)
                return null;
            else if (Results.Length == 0)
                return null;
            else if (_playedCount >= Results.Length)
            {
                return Results[Results.Length - 1];
            }
            return Results[_playedCount - 1];
        }
        public bool NextRound()
        {
            var canMoveNext = MoveNext();
            if (!canMoveNext)
                return false;
            switch(Mode)
            {
                case GameMode.Dan:
                    CurrentHP += HPRecover;
                    CurrentHP = CurrentHP.Clamp(0, MaxHP);
                    break;
            }
            return true;
        }
    }
}
