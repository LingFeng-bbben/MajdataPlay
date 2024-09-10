using Cysharp.Threading.Tasks;
using MajdataPlay.Types;
using MajSimaiDecode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Utils
{
    public static class SongStorage
    {
        public static SongCollection[] Songs { get; private set; } = new SongCollection[0];
        public static long TotalChartCount { get; private set; } = 0;
        public static ComponentState State { get; private set; } = ComponentState.Idle;
        public static async Task ScanMusicAsync()
        {
            State = ComponentState.Backend;
            if (!Directory.Exists(GameManager.ChartPath))
            {
                Directory.CreateDirectory(GameManager.ChartPath);
                Directory.CreateDirectory(Path.Combine(GameManager.ChartPath,"default"));
                State = ComponentState.Finished;
                return;
            }
            var rootPath = GameManager.ChartPath;
            Songs = await GetCollections(rootPath);
            State = ComponentState.Finished;
        }
        static async ValueTask<SongCollection[]> GetCollections(string rootPath)
        {
            var dirs = new DirectoryInfo(rootPath).GetDirectories();
            List<Task<SongCollection>> tasks = new();
            List<SongCollection> collections = new();
            foreach (var dir in dirs)
            {
                var path = dir.FullName;
                var files = dir.GetFiles();
                var maidataFile = files.FirstOrDefault(o => o.Name is "maidata.txt");
                var trackFile = files.FirstOrDefault(o => o.Name is "track.mp3" or "track.ogg");

                if (maidataFile is not null || trackFile is not null)
                    continue;

                tasks.Add(GetCollection(path));
            }
            await Task.WhenAll(tasks);
            foreach (var task in tasks)
                collections.Add(task.Result);
            return collections.ToArray();
        }
        static async Task<SongCollection> GetCollection(string rootPath)
        {
            var thisDir = new DirectoryInfo(rootPath);
            var dirs = thisDir.GetDirectories(rootPath)
                              .OrderBy(o => o.CreationTime)
                              .ToList();
            var charts = new List<SongDetail>();
            var tasks = new List<Task<SongDetail>>();
            foreach (var songDir in dirs)
            {
                var files = songDir.GetFiles();
                var maidataFile = files.FirstOrDefault(o => o.Name is "maidata.txt");
                var trackFile = files.FirstOrDefault(o => o.Name is "track.mp3" or "track.ogg");

                if (maidataFile is null || trackFile is null)
                    continue;

                var parsingTask = SongDetail.ParseAsync(files);

                tasks.Add(parsingTask);
            }
            await Task.WhenAll(tasks);
            foreach (var task in tasks)
                charts.Add(task.Result);
            return new SongCollection(thisDir.Name, charts.ToArray());
        }
    }
}