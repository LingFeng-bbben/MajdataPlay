using MajdataPlay.Extensions;
using MajdataPlay.Types;
using System.Collections;
using System.Collections.Generic;

namespace MajdataPlay.Collections
{
    public class JudgeDetail : IReadOnlyDictionary<ScoreNoteType, JudgeInfo>
    {
        public JudgeInfo TotalJudgeInfo => total;
        IReadOnlyDictionary<ScoreNoteType, JudgeInfo> db;
        JudgeInfo total = JudgeInfo.Empty;
        public JudgeDetail(IReadOnlyDictionary<ScoreNoteType, JudgeInfo> source)
        {
            db = source;
            foreach (var kv in source)
                total += kv.Value;
        }

        public JudgeInfo this[ScoreNoteType key] => db[key];

        public IEnumerable<ScoreNoteType> Keys => db.Keys;

        public IEnumerable<JudgeInfo> Values => db.Values;

        public int Count => db.Count;

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
                    if (kv.Key > JudgeType.Perfect)
                        fast += kv.Value;
                    else if (kv.Key is not JudgeType.Perfect)
                        late += kv.Value;
                }
                switch (kv.Key)
                {
                    case JudgeType.TooFast:
                    case JudgeType.Miss:
                        miss += kv.Value;
                        break;
                    case JudgeType.FastGood:
                    case JudgeType.LateGood:
                        good += kv.Value;
                        break;
                    case JudgeType.LateGreat2:
                    case JudgeType.LateGreat1:
                    case JudgeType.LateGreat:
                    case JudgeType.FastGreat:
                    case JudgeType.FastGreat1:
                    case JudgeType.FastGreat2:
                        great += kv.Value;
                        break;
                    case JudgeType.LatePerfect2:
                    case JudgeType.LatePerfect1:
                    case JudgeType.FastPerfect1:
                    case JudgeType.FastPerfect2:
                        perfect += kv.Value;
                        break;
                    case JudgeType.Perfect:
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
