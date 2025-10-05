using MajdataPlay.Json;
using MajdataPlay.Scenes.Game.Notes;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Collections
{
    [JsonConverter(typeof(JudgeInfoConverter))]
    public class JudgeInfo : IReadOnlyDictionary<JudgeGrade, int>
    {
        [JsonIgnore]
        public static JudgeInfo Empty { get; } = new JudgeInfo(new Dictionary<JudgeGrade, int>()
        {
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat3rd, 0 },
            {JudgeGrade.FastGreat2nd, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect3rd, 0 },
            {JudgeGrade.FastPerfect2nd, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect2nd, 0 },
            {JudgeGrade.LatePerfect3rd, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat2nd, 0 },
            {JudgeGrade.LateGreat3rd, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        });
        IReadOnlyDictionary<JudgeGrade, int> db;
        public JudgeInfo(IReadOnlyDictionary<JudgeGrade, int> source)
        {
            db = source;
        }

        public int this[JudgeGrade key] => db[key];

        public IEnumerable<JudgeGrade> Keys => db.Keys;

        public IEnumerable<int> Values => db.Values;

        public int Count => db.Count;

        public bool ContainsKey(JudgeGrade key) => db.ContainsKey(key);
        public IEnumerator<KeyValuePair<JudgeGrade, int>> GetEnumerator() => db.GetEnumerator();
        public bool TryGetValue(JudgeGrade key, out int value) => db.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)db).GetEnumerator();

        public static JudgeInfo operator +(JudgeInfo left, JudgeInfo right)
        {
            var newRecord = left.ToDictionary(
                kv => kv.Key,
                kv => left[kv.Key] + right[kv.Key]
                );

            return new JudgeInfo(newRecord);
        }
        
    }
}
