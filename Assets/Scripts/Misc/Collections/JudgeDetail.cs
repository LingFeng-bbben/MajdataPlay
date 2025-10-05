using MajdataPlay.Json;
using MajdataPlay.Scenes.Game;
using MajdataPlay.Scenes.Game.Notes;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace MajdataPlay.Collections
{
    [JsonConverter(typeof(JudgeDetailConverter))]
    public class JudgeDetail : IReadOnlyDictionary<ScoreNoteType, JudgeInfo>
    {
        [JsonIgnore]
        public static JudgeDetail Empty { get; } = new JudgeDetail(new Dictionary<ScoreNoteType, JudgeInfo>()
        {
            {ScoreNoteType.Tap,JudgeInfo.Empty },
            {ScoreNoteType.Hold,JudgeInfo.Empty },
            {ScoreNoteType.Slide,JudgeInfo.Empty },
            {ScoreNoteType.Touch,JudgeInfo.Empty },
            {ScoreNoteType.Break,JudgeInfo.Empty },
        });
        public JudgeInfo TotalJudgeInfo
        {
            get
            {
                return total;
            }
        }
        [JsonIgnore]
        public int Count
        {
            get
            {
                return db.Count;
            }
        }

        IReadOnlyDictionary<ScoreNoteType, JudgeInfo> db;
        JudgeInfo total = JudgeInfo.Empty;
        public JudgeDetail(IReadOnlyDictionary<ScoreNoteType, JudgeInfo> source)
        {
            db = source;
            foreach (var kv in source)
            {
                total += kv.Value;
            }
        }

        public JudgeInfo this[ScoreNoteType key] => db[key];

        public IEnumerable<ScoreNoteType> Keys => db.Keys;

        public IEnumerable<JudgeInfo> Values => db.Values;
        public bool ContainsKey(ScoreNoteType key) => db.ContainsKey(key);
        public IEnumerator<KeyValuePair<ScoreNoteType, JudgeInfo>> GetEnumerator() => db.GetEnumerator();
        public bool TryGetValue(ScoreNoteType key, out JudgeInfo value) => db.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)db).GetEnumerator();

        public static UnpackJudgeInfo UnpackJudgeRecord(JudgeInfo judgeInfo)
        {
            long cPerfect = 0;
            long perfect = 0;
            long great = 0;
            long good = 0;
            long miss = 0;

            long fast = 0;
            long late = 0;

            foreach (var kv in judgeInfo)
            {
                if (!kv.Key.IsMissOrTooFast())
                {
                    if (kv.Key > JudgeGrade.Perfect)
                        fast += kv.Value;
                    else if (kv.Key is not JudgeGrade.Perfect)
                        late += kv.Value;
                }
                switch (kv.Key)
                {
                    case JudgeGrade.TooFast:
                    case JudgeGrade.Miss:
                        miss += kv.Value;
                        break;
                    case JudgeGrade.FastGood:
                    case JudgeGrade.LateGood:
                        good += kv.Value;
                        break;
                    case JudgeGrade.LateGreat3rd:
                    case JudgeGrade.LateGreat2nd:
                    case JudgeGrade.LateGreat:
                    case JudgeGrade.FastGreat:
                    case JudgeGrade.FastGreat2nd:
                    case JudgeGrade.FastGreat3rd:
                        great += kv.Value;
                        break;
                    case JudgeGrade.LatePerfect3rd:
                    case JudgeGrade.LatePerfect2nd:
                    case JudgeGrade.FastPerfect2nd:
                    case JudgeGrade.FastPerfect3rd:
                        perfect += kv.Value;
                        break;
                    case JudgeGrade.Perfect:
                        cPerfect += kv.Value;
                        break;
                }
            }
            return new UnpackJudgeInfo()
            {
                CriticalPerfect = cPerfect,
                Perfect = perfect,
                Great = great,
                Good = good,
                Miss = miss,
                Fast = fast,
                Late = late,
            };

        }
    }
}
