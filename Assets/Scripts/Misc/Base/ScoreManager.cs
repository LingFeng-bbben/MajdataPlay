using Cysharp.Threading.Tasks;
using MajdataPlay.Collections;
using MajdataPlay.Json;
using MajdataPlay.Scenes.Game;
using MajdataPlay.Utils;
using Newtonsoft.Json;
using SQLite;
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

        static Dictionary<string, SongScores>? _onlineBuckets = null;

        readonly static Dictionary<string, SongScores> _buckets = new();

        static SQLiteAsyncConnection? _db;

        readonly static JsonSerializerSettings _jsonReadSettings = new()
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Converters = new List<JsonConverter>
            {
                new JudgeDetailConverter(),
                new JudgeInfoConverter(),
            }
        };

        internal static async UniTask InitAsync()
        {
            if (_isInited)
            {
                return;
            }
            _isInited = true;
            try
            {
                var dbPath = MajEnv.ScoreDBPath;
                _db = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);

                await _db.CreateTableAsync<MaiScoreDB>();

                // Migrate from legacy JSON file
                var legacyPath = MajEnv.LegacyScoreDBPath;
                if (File.Exists(legacyPath))
                {
                    var migrated = await MigrateFromJsonAsync(legacyPath);
                    if (migrated)
                    {
                        try
                        {
                            File.Delete(legacyPath);
                            MajDebug.LogInfo("Migrated scores from legacy JSON to SQLite, old file deleted.");
                        }
                        catch (Exception ex)
                        {
                            MajDebug.LogError($"Failed to delete legacy score file: {ex.Message}");
                        }
                    }
                }

                // Load from SQLite
                var rows = await _db.QueryAsync<MaiScoreDB>("SELECT * FROM MaiScores WHERE PlayCount > 0");
                var grouped = rows.Select(x => x.ToMaiScore()).GroupBy(x => x.Hash);
                foreach (var group in grouped)
                {
                    var hash = group.Key;
                    if (string.IsNullOrEmpty(hash))
                    {
                        continue;
                    }
                    var dict = group.ToDictionary(x => x.ChartLevel, x => x);

                    var scores = new SongScores()
                    {
                        Easy = GetOrCreate(dict, hash, ChartLevel.Easy),
                        Basic = GetOrCreate(dict, hash, ChartLevel.Basic),
                        Advance = GetOrCreate(dict, hash, ChartLevel.Advance),
                        Expert = GetOrCreate(dict, hash, ChartLevel.Expert),
                        Master = GetOrCreate(dict, hash, ChartLevel.Master),
                        ReMaster = GetOrCreate(dict, hash, ChartLevel.ReMaster),
                        UTAGE = GetOrCreate(dict, hash, ChartLevel.UTAGE),
                    };
                    _buckets.Add(hash, scores);
                }
            }
            catch(Exception e)
            {
                MajDebug.LogError(e);
            }
            finally
            {
                await UniTask.Yield();
            }
        }

        static MaiScore GetOrCreate(Dictionary<ChartLevel, MaiScore> dict, string hash, ChartLevel level)
        {
            if (dict.TryGetValue(level, out var score))
            {
                return score;
            }
            return new MaiScore()
            {
                Hash = hash,
                ChartLevel = level,
                PlayCount = 0,
            };
        }

        static async Task<bool> MigrateFromJsonAsync(string legacyPath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(legacyPath);
                var scores = JsonConvert.DeserializeObject<List<MaiScore>>(json, _jsonReadSettings);
                if (scores is null || scores.Count == 0)
                {
                    return true; // nothing to migrate
                }

                var dbRows = new List<MaiScoreDB>(scores.Count);
                foreach (var score in scores)
                {
                    if (string.IsNullOrEmpty(score.Hash) || score.PlayCount == 0)
                    {
                        continue;
                    }
                    dbRows.Add(MaiScoreDB.FromMaiScore(score));
                }

                if (dbRows.Count > 0 && _db is not null)
                {
                    await _db.InsertAllAsync(dbRows, runInTransaction: true);
                }

                return true;
            }
            catch (Exception ex)
            {
                MajDebug.LogError($"Failed to migrate legacy scores: {ex.Message}");
                return false;
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

                record.JudgeDetail = result.JudgeRecord;
                record.Fast = result.Fast;
                record.Late = result.Late;
                record.ComboState = result.ComboState > record.ComboState ? result.ComboState : record.ComboState;
                record.Timestamp = DateTime.Now;
                record.PlayCount++;

                if (_db is not null)
                {
                    var dbRow = MaiScoreDB.FromMaiScore(record);
                    await _db.InsertOrReplaceAsync(dbRow);
                }

                return true;
            }
            catch (Exception ex)
            {
                MajDebug.LogError(ex);
                return false;
            }
        }
        public static void LoadOnlineScores(ReadOnlySpan<MajNetAccountSongScore> scores)
        {
            ref var @lock = ref _lock;
            var isLocked = false;
            try
            {
                @lock.Enter(ref isLocked);
                _onlineBuckets = new();
                if (scores.IsEmpty)
                {
                    return;
                }
                for (var i = 0; i < scores.Length; i++)
                {
                    var score = scores[i];
                    if (!_onlineBuckets.TryGetValue(score.Hash, out var scoreRecord))
                    {
                        scoreRecord = SongScores.Create(score.Hash);
                        _onlineBuckets.Add(score.Hash, scoreRecord);
                    }
                    var maiScore = default(MaiScore);
                    switch (score.ChartLevel)
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
                _onlineBuckets = null;
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
                var buckets = _buckets;
                if (isOnline && _onlineBuckets is not null)
                {
                    buckets = _onlineBuckets;
                }
                if (!buckets.TryGetValue(hash, out var records))
                {
                    records = SongScores.Create(hash);
                    buckets.Add(hash, records);
                }
                return records;
            }
            finally
            {
                if (isLocked)
                {
                    @lock.Exit();
                }
            }
        }
        [Table("MaiScores")]
        public class MaiScoreDB
        {
            [PrimaryKey]
            public string Key { get; set; } = string.Empty;

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
                Converters = new List<JsonConverter>
                {
                    new JudgeDetailConverter(),
                    new JudgeInfoConverter(),
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
                    JudgeDetail = judgeDetail ?? JudgeDetail.Empty,
                    Timestamp = new DateTime(TimestampTicks, DateTimeKind.Local),
                    ComboState = (ComboState)ComboStateInt,
                };
            }

            public static MaiScoreDB FromMaiScore(MaiScore score)
            {
                string? judgeJson = null;
                if (score.JudgeDetail is not null)
                {
                    try
                    {
                        judgeJson = JsonConvert.SerializeObject(score.JudgeDetail, _jsonSettings);
                    }
                    catch
                    {
                        judgeJson = null;
                    }
                }

                var hash = score.Hash ?? string.Empty;

                return new MaiScoreDB()
                {
                    Key = MakeKey(hash, score.ChartLevel),
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
}
