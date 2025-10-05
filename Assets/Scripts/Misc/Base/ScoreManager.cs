using Cysharp.Threading.Tasks;
using MajdataPlay.Json;
using MajdataPlay.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace MajdataPlay
{
#nullable enable
    internal static class ScoreManager
    {
        static List<MaiScore> _scores = new();

        static bool _isInited = false;

        readonly static JsonSerializer _serializer = JsonSerializer.Create(new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Populate,
            Converters = new List<JsonConverter>
            {
                new JudgeDetailConverter(),
                new JudgeInfoConverter(),
            }
        });

        internal static async UniTask InitAsync()
        {
            if (_isInited)
            {
                return;
            }
            _isInited = true;
            try
            {
                var path = MajEnv.ScoreDBPath;

                if (!File.Exists(path))
                {
                    var json = await Serializer.Json.SerializeAsync(_scores);
                    await File.WriteAllTextAsync(path, json);
                    return;
                }
                using var storageStream = File.OpenRead(path);
                List<MaiScore>? result = await Serializer.Json.DeserializeAsync<List<MaiScore>>(storageStream, _serializer);
                //shoud do some warning here, or all score will be lost and overwirtten
                if (result != null)
                {
                    _scores = result;
                }
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public static MaiScore GetScore(ISongDetail song, ChartLevel level)
        {
            var record = _scores.Find(x => x.Hash == song.Hash && x.ChartLevel == level);
            if (record is null)
            {
                var newRecord = new MaiScore()
                {
                    Hash = song.Hash,
                    PlayCount = 0
                };
                return newRecord;
            }
            return record;
        }
        public static async Task<bool> SaveScore(GameResult result, ChartLevel level)
        {
            try
            {
                var songInfo = result.SongDetail;

                var record = _scores.Find(x => x.Hash == songInfo.Hash && x.ChartLevel == level);
                if (record is null)
                {
                    record = new MaiScore()
                    {
                        ChartLevel = level,
                        Hash = songInfo.Hash,
                        PlayCount = 0
                    };
                    _scores.Add(record);
                }

                record.Acc = result.Acc > record.Acc ? record.Acc.Update(result.Acc) : record.Acc;

                record.DXScore = result.DXScore > record.DXScore ? result.DXScore : record.DXScore;
                record.TotalDXScore = result.TotalDXScore;

                record.JudgeDeatil = result.JudgeRecord;
                record.Fast = result.Fast;
                record.Late = result.Late;
                record.ComboState = result.ComboState > record.ComboState ? result.ComboState : record.ComboState;
                record.Timestamp = DateTime.Now;
                record.PlayCount++;

                using var stream = File.Create(MajEnv.ScoreDBPath);
                await Serializer.Json.SerializeAsync(stream, _scores, _serializer);

                return true;
            }
            catch(Exception ex)
            {
                MajDebug.LogError(ex);
                return false;
            }
        }
    }
}