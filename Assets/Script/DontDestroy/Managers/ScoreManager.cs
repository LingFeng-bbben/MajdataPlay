using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay
{
#nullable enable
    public class ScoreManager : MonoBehaviour
    {
        List<MaiScore> scores = new();
        Task? backendTask = null;

        void Awake()
        {
            MajInstances.ScoreManager = this;
        }
        void Start()
        {
            DontDestroyOnLoad(this);
            var path = GameManager.ScoreDBPath;
            var option = new JsonSerializerOptions();
            option.Converters.Add(new JudgeDetailConverter());
            option.Converters.Add(new JudgeInfoConverter());

            if (!File.Exists(path))
            {
                var json = Serializer.Json.Serialize(scores);
                File.WriteAllText(path, json);
                return;
            }
            var content = File.ReadAllText(path);
            List<MaiScore>? result;

            if (!Serializer.Json.TryDeserialize(content, out result, option) || result is null)
                scores = new();
            else
                scores = result;
        }
        public MaiScore GetScore(SongDetail song, ChartLevel level)
        {
            var record = scores.Find(x => x.Hash == song.Hash && x.ChartLevel == level);
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
        public bool SaveScore(in GameResult result, ChartLevel level)
        {
            try
            {
                var songInfo = result.SongInfo;

                var record = scores.Find(x => x.Hash == songInfo.Hash && x.ChartLevel == level);
                if (record is null)
                {
                    record = new MaiScore()
                    {
                        ChartLevel = level,
                        Hash = songInfo.Hash,
                        PlayCount = 0
                    };
                    scores.Add(record);
                }

                record.Acc = result.Acc > record.Acc ? record.Acc.Update(result.Acc) : record.Acc;

                record.DXScore = result.DXScore;
                record.TotalDXScore = result.TotalDXScore;

                record.JudgeDeatil = result.JudgeRecord;
                record.Fast = result.Fast;
                record.Late = result.Late;
                record.ComboState = result.ComboState;
                record.Timestamp = DateTime.Now;
                record.PlayCount++;

                if (backendTask is null || backendTask.IsCompleted)
                    backendTask = SavingBackend();
                else
                    Task.Run(() =>
                    {
                        backendTask.Wait();
                        backendTask = SavingBackend();
                    });

                return true;
            }
            catch
            {
                return false;
            }
        }
        async Task SavingBackend()
        {
            using var stream = File.Create(GameManager.ScoreDBPath);
            await Serializer.Json.SerializeAsync(stream, scores);
        }
    }
}