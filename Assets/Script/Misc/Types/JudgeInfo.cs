using MajdataPlay.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Types
{
    public class JudgeInfo : IReadOnlyDictionary<JudgeType,int>
    {
        IReadOnlyDictionary<JudgeType, int> db;
        public JudgeInfo(IReadOnlyDictionary<JudgeType, int> source) 
        {
            db = source;
        }

        public int this[JudgeType key] => db[key];

        public IEnumerable<JudgeType> Keys => db.Keys;

        public IEnumerable<int> Values => db.Values;

        public int Count => db.Count;

        public bool ContainsKey(JudgeType key) => db.ContainsKey(key);
        public IEnumerator<KeyValuePair<JudgeType, int>> GetEnumerator() => db.GetEnumerator();
        public bool TryGetValue(JudgeType key, out int value) => db.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)db).GetEnumerator();
    }
}
