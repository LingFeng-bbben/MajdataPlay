using Cysharp.Threading.Tasks;
using MajdataPlay.Types;
using MajSimaiDecode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
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
            var task = GetCollections(rootPath);
            var songs = await task;
            if (task.IsFaulted)
            {
                State = ComponentState.Failed;
                throw task.AsTask().Exception.InnerException;
            }
            else
            {
                Songs = songs;
                State = ComponentState.Finished;
            }
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
            //TODO:Add this to setting
            tasks.Add(GetOnlineCollection(GameManager.Instance.Setting.Online.ApiEndpoint));
            var a = Task.WhenAll(tasks);
            await a;
            if (a.IsFaulted)
                throw a.Exception.InnerException;
            foreach (var task in tasks)
            {
                if(task.Result!=null)
                    collections.Add(task.Result);
            }
            return collections.ToArray();
        }
        static async Task<SongCollection> GetCollection(string rootPath)
        {
            var thisDir = new DirectoryInfo(rootPath);
            var dirs = thisDir.GetDirectories()
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
            var a = Task.WhenAll(tasks);
            await a;
            if(a.IsFaulted)
                throw a.Exception.InnerException;
            foreach (var task in tasks)
                charts.Add(task.Result);
            return new SongCollection(thisDir.Name, charts.ToArray());
        }

        static async Task<SongCollection> GetOnlineCollection(string apiroot)
        {
            if (apiroot is null or "") return null;

            var listurl = apiroot + "/SongList";
            Debug.Log("Loading Online Charts from:" + listurl);
            try
            {
                var client = new HttpClient(new HttpClientHandler() { Proxy = WebRequest.GetSystemWebProxy(), UseProxy = true});
                var liststr = await client.GetStringAsync(listurl);
                var list = JsonSerializer.Deserialize<MajnetSongDetail[]>(liststr);
                var gameList = new List<SongDetail>();
                foreach (var song in list)
                {
                    var songDetail = new SongDetail()
                    {
                        Title = song.Title,
                        Artist = song.Artist,
                        Levels = song.Levels.ToArray(),
                        isOnline = true,
                        MaidataPath = apiroot + "/Maidata/" + song.Id,
                        TrackPath = apiroot + "/Track/" + song.Id,
                        //TODO: add logic for imagefull
                        CoverPath = apiroot + "/Image/" + song.Id,
                        Hash = song.Id.ToString(),

                    };
                    for (int i = 0; i < songDetail.Designers.Count(); i++)
                    {
                        songDetail.Designers[i] = song.Uploader + "@" + song.Designer;
                    }
                    gameList.Add(songDetail);
                }
                Debug.Log("Loaded Online Charts List:" + gameList.Count);
                return new SongCollection("MajNet", gameList.ToArray());
            }catch(Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }
    }
}