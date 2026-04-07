using Cysharp.Threading.Tasks;
using MajdataPlay.Json;
using MajdataPlay.Net;
using MajdataPlay.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MajdataPlay
{
#nullable enable
    internal static class ScoreManager
    {
        static bool _isInited = false;

        static SpinLock _lock = new();

        static readonly Dictionary<string, Dictionary<string, SongScores>> _onlineBucketsByEndpoint = new();

        readonly static Dictionary<string, SongScores> _buckets = new();

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
                    var songScores = _buckets.Select(x => x.Value);
                    var scores = Array.Empty<MaiScore>()
                                      .Concat(songScores.Select(x => x.Easy))
                                      .Concat(songScores.Select(x => x.Basic))
                                      .Concat(songScores.Select(x => x.Advance))
                                      .Concat(songScores.Select(x => x.Expert))
                                      .Concat(songScores.Select(x => x.Master))
                                      .Concat(songScores.Select(x => x.ReMaster))
                                      .Concat(songScores.Select(x => x.UTAGE))
                                      .ToArray();
                    var json = await Serializer.Json.SerializeAsync(scores);
                    await File.WriteAllTextAsync(path, json);
                    return;
                }
                using var storageStream = File.OpenRead(path);
                List<MaiScore>? result = await Serializer.Json.DeserializeAsync<List<MaiScore>>(storageStream, _serializer);
                //shoud do some warning here, or all score will be lost and overwirtten
                var grouped = result.GroupBy(x => x.Hash);
                var dicts = grouped.Select(x => (x.Key ,x.ToDictionary(x => x.ChartLevel, x => x)));
                foreach(var (hash, dict) in dicts)
                {
                    if(string.IsNullOrEmpty(hash))
                    {
                        continue;
                    }
                    if(!dict.TryGetValue(ChartLevel.Easy,out var easy))
                    {
                        easy = new MaiScore()
                        {
                            Hash = hash,
                            PlayCount = 0
                        };
                    }
                    if (!dict.TryGetValue(ChartLevel.Basic, out var basic))
                    {
                        basic = new MaiScore()
                        {
                            Hash = hash,
                            PlayCount = 0
                        };
                    }
                    if (!dict.TryGetValue(ChartLevel.Advance, out var advance))
                    {
                        advance = new MaiScore()
                        {
                            Hash = hash,
                            PlayCount = 0
                        };
                    }
                    if (!dict.TryGetValue(ChartLevel.Expert, out var expert))
                    {
                        expert = new MaiScore()
                        {
                            Hash = hash,
                            PlayCount = 0
                        };
                    }
                    if (!dict.TryGetValue(ChartLevel.Master, out var master))
                    {
                        master = new MaiScore()
                        {
                            Hash = hash,
                            PlayCount = 0
                        };
                    }
                    if (!dict.TryGetValue(ChartLevel.ReMaster, out var remas))
                    {
                        remas = new MaiScore()
                        {
                            Hash = hash,
                            PlayCount = 0
                        };
                    }
                    if (!dict.TryGetValue(ChartLevel.UTAGE, out var utage))
                    {
                        utage = new MaiScore()
                        {
                            Hash = hash,
                            PlayCount = 0
                        };
                    }

                    var scores = new SongScores()
                    {
                        Easy = easy,
                        Basic = basic,
                        Advance = advance,
                        Expert = expert,
                        Master = master,
                        ReMaster = remas,
                        UTAGE = utage
                    };
                    _buckets.Add(hash!, scores);
                }
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public static MaiScore GetScore(ISongDetail song, ChartLevel level)
        {
            var hash = song.Hash;
            var records = CheckAndGetSongScores(hash, song.IsOnline);
            switch (level)
            {
                case ChartLevel.Easy:
                    return records.Easy;
                case ChartLevel.Basic:
                    return records.Basic;
                case ChartLevel.Advance:
                    return records.Advance;
                case ChartLevel.Expert:
                    return records.Expert;
                case ChartLevel.Master:
                    return records.Master;
                case ChartLevel.ReMaster:
                    return records.ReMaster;
                case ChartLevel.UTAGE:
                    return records.UTAGE;
                default:
                    throw new ArgumentOutOfRangeException("sb");
            }
        }
        public static SongScores GetSongScores(ISongDetail song)
        {
            var hash = song.Hash;

            return CheckAndGetSongScores(hash, song.IsOnline);
        }
        public static async Task<bool> SaveScore(GameResult result, ChartLevel level)
        {
            try
            {
                var songInfo = result.SongDetail;
                var hash = songInfo.Hash;
                var records = CheckAndGetSongScores(hash, result.SongDetail.IsOnline);
                var record = level switch
                {
                    ChartLevel.Easy => records.Easy,
                    ChartLevel.Basic => records.Basic,
                    ChartLevel.Advance => records.Advance,
                    ChartLevel.Expert => records.Expert,
                    ChartLevel.Master => records.Master,
                    ChartLevel.ReMaster => records.ReMaster,
                    ChartLevel.UTAGE => records.UTAGE,
                    _ => throw new ArgumentOutOfRangeException(nameof(level))
                };

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
                var songScores = _buckets.Select(x => x.Value);
                var scores = Array.Empty<MaiScore>()
                                  .Concat(songScores.Select(x => x.Easy))
                                  .Concat(songScores.Select(x => x.Basic))
                                  .Concat(songScores.Select(x => x.Advance))
                                  .Concat(songScores.Select(x => x.Expert))
                                  .Concat(songScores.Select(x => x.Master))
                                  .Concat(songScores.Select(x => x.ReMaster))
                                  .Concat(songScores.Select(x => x.UTAGE))
                                  .Where(x => x.PlayCount != 0)
                                  .ToArray();
                await Serializer.Json.SerializeAsync(stream, scores, _serializer);

                return true;
            }
            catch(Exception ex)
            {
                MajDebug.LogError(ex);
                return false;
            }
        }
        public static void LoadOnlineScores(ReadOnlySpan<MajNetAccountSongScore> scores, string endpointName = "")
        {
            ref var @lock = ref _lock;
            var isLocked = false;
            try
            {
                @lock.Enter(ref isLocked);
                if (!_onlineBucketsByEndpoint.TryGetValue(endpointName, out var bucket))
                {
                    bucket = new();
                    _onlineBucketsByEndpoint[endpointName] = bucket;
                }
                bucket.Clear();
                if (scores.IsEmpty)
                {
                    return;
                }
                for (var i = 0; i < scores.Length; i++)
                {
                    var score = scores[i];
                    if(!bucket.TryGetValue(score.Hash, out var scoreRecord))
                    {
                        scoreRecord = SongScores.Create(score.Hash);
                        bucket.Add(score.Hash, scoreRecord);
                    }
                    var maiScore = default(MaiScore);
                    switch(score.ChartLevel)
                    {
                        case ChartLevel.Easy:
                            maiScore = scoreRecord.Easy;
                            break;
                        case ChartLevel.Basic:
                            maiScore = scoreRecord.Basic;
                            break;
                        case ChartLevel.Advance:
                            maiScore = scoreRecord.Advance;
                            break;
                        case ChartLevel.Expert:
                            maiScore = scoreRecord.Expert;
                            break;
                        case ChartLevel.Master:
                            maiScore = scoreRecord.Master;
                            break;
                        case ChartLevel.ReMaster:
                            maiScore = scoreRecord.ReMaster;
                            break;
                        case ChartLevel.UTAGE:
                            maiScore = scoreRecord.UTAGE;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(score.ChartLevel), score.ChartLevel, null);
                    }
                    maiScore.Acc = score.Acc;
                    maiScore.DXScore = score.DXScore;
                    maiScore.ComboState = score.ComboState;
                    maiScore.Timestamp = score.Timestamp;
                    maiScore.PlayCount = 1;
                }
            }
            finally
            {
                if (isLocked)
                {
                    @lock.Exit();
                }
            }
        }
        public static void UnloadOnlineScores()
        {
            ref var @lock = ref _lock;
            var isLocked = false;
            try
            {
                @lock.Enter(ref isLocked);
                _onlineBucketsByEndpoint.Clear();
            }
            finally
            {
                if (isLocked)
                {
                    @lock.Exit();
                }
            }
        }
        public static void UnloadOnlineScores(string endpointName)
        {
            if (string.IsNullOrEmpty(endpointName))
            {
                UnloadOnlineScores();
                return;
            }
            ref var @lock = ref _lock;
            var isLocked = false;
            try
            {
                @lock.Enter(ref isLocked);
                _onlineBucketsByEndpoint.Remove(endpointName);
            }
            finally
            {
                if (isLocked)
                {
                    @lock.Exit();
                }
            }
        }
        static SongScores CheckAndGetSongScores(string hash, bool isOnline)
        {
            ref var @lock = ref _lock;
            var isLocked = false;
            try
            {
                @lock.Enter(ref isLocked);
                if (isOnline && _onlineBucketsByEndpoint.Count > 0)
                {
                    foreach (var (_, bucket) in _onlineBucketsByEndpoint)
                    {
                        if (bucket.TryGetValue(hash, out var records))
                        {
                            return records;
                        }
                    }
                    var firstBucket = _onlineBucketsByEndpoint.Values.First();
                    var newRecords = SongScores.Create(hash);
                    firstBucket.Add(hash, newRecords);
                    return newRecords;
                }
                if (!_buckets.TryGetValue(hash, out var localRecords))
                {
                    localRecords = SongScores.Create(hash);
                    _buckets.Add(hash, localRecords);
                }
                return localRecords;
            }
            finally
            {
                if(isLocked)
                {
                    @lock.Exit();
                }
            }
        }
    }
}
