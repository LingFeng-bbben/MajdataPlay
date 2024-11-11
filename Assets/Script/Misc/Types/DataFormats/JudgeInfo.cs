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

        public static JudgeInfo operator +(JudgeInfo left,JudgeInfo right)
        {
            var newRecord = left.ToDictionary(
                kv => kv.Key,
                kv => left[kv.Key] + right[kv.Key]
                );

            return new JudgeInfo(newRecord);
        }
        public static JudgeInfo Empty => new JudgeInfo(new Dictionary<JudgeType,int>()
        {
            {JudgeType.TooFast, 0 },
            {JudgeType.FastGood, 0 },
            {JudgeType.FastGreat2, 0 },
            {JudgeType.FastGreat1, 0 },
            {JudgeType.FastGreat, 0 },
            {JudgeType.FastPerfect2, 0 },
            {JudgeType.FastPerfect1, 0 },
            {JudgeType.Perfect, 0 },
            {JudgeType.LatePerfect1, 0 },
            {JudgeType.LatePerfect2, 0 },
            {JudgeType.LateGreat, 0 },
            {JudgeType.LateGreat1, 0 },
            {JudgeType.LateGreat2, 0 },
            {JudgeType.LateGood, 0 },
            {JudgeType.Miss, 0 },
        });
    }
}
