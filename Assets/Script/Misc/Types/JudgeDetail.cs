using System.Collections;
using System.Collections.Generic;

namespace MajdataPlay.Types
{
    public class JudgeDetail : IReadOnlyDictionary<ScoreNoteType,JudgeInfo>
    {
        IReadOnlyDictionary<ScoreNoteType, JudgeInfo> db;
        public JudgeDetail(IReadOnlyDictionary<ScoreNoteType, JudgeInfo> source)
        {
            db = source;
        }

        public JudgeInfo this[ScoreNoteType key] => db[key];

        public IEnumerable<ScoreNoteType> Keys => db.Keys;

        public IEnumerable<JudgeInfo> Values => db.Values;

        public int Count => db.Count;

        public bool ContainsKey(ScoreNoteType key) => db.ContainsKey(key);
        public IEnumerator<KeyValuePair<ScoreNoteType, JudgeInfo>> GetEnumerator() => db.GetEnumerator();
        public bool TryGetValue(ScoreNoteType key, out JudgeInfo value) => db.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)db).GetEnumerator();
    }
}
