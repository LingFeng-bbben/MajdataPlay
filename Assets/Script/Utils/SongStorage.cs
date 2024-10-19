using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using MajdataPlay.Extensions;
using MajdataPlay.Net;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Utils
{
    public static class SongStorage
    {
        public static bool IsEmpty => Collections.IsEmpty() || Collections.All(x => x.Count == 0);
        /// <summary>
        /// Current song collection index
        /// </summary>
        public static int CollectionIndex 
        { 
            get => _collectionIndex;
            set => _collectionIndex = value.Clamp(0, Collections.Length - 1); 
        }
        /// <summary>
        /// Current song collection
        /// </summary>
        public static SongCollection WorkingCollection 
        { 
            get
            {
                if (Collections.IsEmpty())
                    return SongCollection.Empty("default");
                return Collections[_collectionIndex];
            }
        }
        /// <summary>
        /// Loaded song collections
        /// </summary>
        public static SongCollection[] Collections { get; private set; } = new SongCollection[0];
        public static SongOrder OrderBy { get; set; } = new();
        public static long TotalChartCount { get; private set; } = 0;

        static int _collectionIndex = 0;
        public static async Task ScanMusicAsync()
        {
            if (!Directory.Exists(GameManager.ChartPath))
            {
                Directory.CreateDirectory(GameManager.ChartPath);
                Directory.CreateDirectory(Path.Combine(GameManager.ChartPath, "default"));
                return;
            }
            var rootPath = GameManager.ChartPath;
            var task = GetCollections(rootPath);
            var songs = await task;
            if (task.IsFaulted)
            {
                var e = task.AsTask().Exception.InnerException;
                Debug.LogException(e);
                throw e;
            }
            else
                Collections = songs;
            TotalChartCount =  await Collections.ToUniTaskAsyncEnumerable().SumAsync(x => x.Count);
            Debug.Log($"Loaded chart count: {TotalChartCount}");
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
            if(MajInstances.Setting.Online.Enable)
                tasks.Add(GetOnlineCollection(MajInstances.Setting.Online.ApiEndpoint));

            var a = Task.WhenAll(tasks);
            await a;
            if (a.IsFaulted)
                throw a.Exception.InnerException;
            foreach (var task in tasks)
            {
                if (task.Result != null)
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
            if (a.IsFaulted)
                throw a.Exception.InnerException;
            foreach (var task in tasks)
                charts.Add(task.Result);
            return new SongCollection(thisDir.Name, charts.ToArray());
        }
        static async Task<SongCollection> GetOnlineCollection(string apiroot)
        {
            var collection = SongCollection.Empty("MajNet");
            if (string.IsNullOrEmpty(apiroot)) 
                return collection;

            var listurl = apiroot + "/SongList";
            Debug.Log("Loading Online Charts from:" + listurl);
            try
            {
                var client = HttpTransporter.ShareClient;
                var liststr = await client.GetStringAsync(listurl);
                var list = JsonSerializer.Deserialize<MajnetSongDetail[]>(liststr);
                if (list is null || list.IsEmpty())
                    return collection;

                var gameList = new List<SongDetail>();
                foreach (var song in list)
                {
                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dateTime = dateTime.AddSeconds(song.Timestamp).ToLocalTime();
                    var songDetail = new SongDetail()
                    {
                        Title = song.Title,
                        Artist = song.Artist,
                        Levels = song.Levels.ToArray(),
                        isOnline = true,
                        MaidataPath = apiroot + "/Maidata/" + song.Id,
                        TrackPath = apiroot + "/Track/" + song.Id,
                        BGPath = apiroot + "/ImageFull/" + song.Id,
                        CoverPath = apiroot + "/Image/" + song.Id,
                        Hash = song.Id.ToString()+"//" + song.Timestamp,
                        AddTime = dateTime
                    };
                    for (int i = 0; i < songDetail.Designers.Count(); i++)
                    {
                        songDetail.Designers[i] = song.Uploader + "@" + song.Designer;
                    }
                    gameList.Add(songDetail);
                }
                Debug.Log("Loaded Online Charts List:" + gameList.Count);
                return new SongCollection("MajNet", gameList.ToArray());
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return collection;
            }
        }
        public static void SortAndFind(string searchKey, SortType sortType)
        {
            OrderBy.Keyword = searchKey;
            OrderBy.SortBy = sortType;
            SortAndFind();
        }
        public static void SortAndFind()
        {
            if (Collections.IsEmpty())
                return;

            var searchKey = OrderBy.Keyword;
            var sortType = OrderBy.SortBy;
            
            if(string.IsNullOrEmpty(searchKey) && sortType == SortType.Default)
            {
                foreach (var collection in Collections)
                    collection.Reset();
                return;
            }
            foreach (var collection in Collections)
                collection.SortAndFilter(OrderBy);
        }
        public static async Task SortAndFindAsync(string searchKey, SortType sortType)
        {
            OrderBy.Keyword = searchKey;
            OrderBy.SortBy = sortType;
            await SortAndFindAsync();
        }
        public static async Task SortAndFindAsync()
        {
            Task[] tasks = new Task[Collections.Length];

            var searchKey = OrderBy.Keyword;
            var sortType = OrderBy.SortBy;

            if (string.IsNullOrEmpty(searchKey) && sortType == SortType.Default)
            {
                foreach (var collection in Collections)
                    collection.Reset();
                return;
            }
            foreach (var (i, collection) in Collections.WithIndex())
                tasks[i] = collection.SortAndFilterAsync(OrderBy);
            await Task.WhenAll(tasks);
        }
    }    
}