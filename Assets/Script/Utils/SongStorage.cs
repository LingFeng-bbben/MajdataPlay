using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using MajdataPlay.Collections;
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
        public static async Task ScanMusicAsync(IProgress<ChartScanProgress> progressReporter)
        {
            if (!Directory.Exists(MajEnv.ChartPath))
            {
                Directory.CreateDirectory(MajEnv.ChartPath);
                Directory.CreateDirectory(Path.Combine(MajEnv.ChartPath, "default"));
                return;
            }
            var rootPath = MajEnv.ChartPath;
            var task = GetCollections(rootPath,progressReporter);
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
        static async ValueTask<SongCollection[]> GetCollections(string rootPath, IProgress<ChartScanProgress> progressReporter)
        {
            var dirs = new DirectoryInfo(rootPath).GetDirectories();
            List<Task<SongCollection>> tasks = new();
            List<SongCollection> collections = new();

            //Local Charts
            foreach (var dir in dirs)
            {
                var path = dir.FullName;
                var files = dir.GetFiles();
                var maidataFile = files.FirstOrDefault(o => o.Name.ToLower() is "maidata.txt");
                var trackFile = files.FirstOrDefault(o => o.Name.ToLower() is "track.mp3" or "track.ogg");

                if (maidataFile is not null || trackFile is not null)
                    continue;

                tasks.Add(GetCollection(path));
            }
            
            var a = Task.WhenAll(tasks);
            await a;

            if (a.IsFaulted)
                throw a.Exception.InnerException;
            foreach (var task in tasks)
            {
                if (task.Result != null)
                    collections.Add(task.Result);
            }
            //Online Charts
            if (MajInstances.Setting.Online.Enable)
            {
                foreach (var item in MajInstances.Setting.Online.ApiEndpoints)
                {
                    progressReporter.Report(new ChartScanProgress()
                    {
                        StorageType = ChartStorageLocation.Online,
                        Message = item.Name
                    });
                    var result = await GetOnlineCollection(item);
                    if (!result.IsEmpty)
                        collections.Add(result);
                }
            }
            //Add all songs to "All" folder
            var allcharts = new List<SongDetail>();
            foreach (var collection in collections)
            {
                foreach (var item in collection)
                {
                    allcharts.Add(item);
                }
            }
            collections.Add(new SongCollection("All", allcharts.ToArray()));
            Debug.Log("Load Dans");
            var danFiles = new DirectoryInfo(rootPath).GetFiles("*.json");
            foreach (var file in danFiles)
            {
                var json = File.ReadAllText(file.FullName);
                var dan = Serializer.Json.Deserialize<DanInfo>(json, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = false
                });
                if(dan is null)
                {
                    Debug.LogError("Failed to load dan file:" + file.FullName);
                    continue;
                }
                List<SongDetail> danSongs = new();
                foreach (var hash in dan.SongHashs)
                {
                    var songDetail = allcharts.FirstOrDefault(x => x.Hash == hash);
                    if (songDetail is not null)
                        danSongs.Add(songDetail);
                    else
                    {
                        Debug.LogError("Cannot find the song with hash:" + hash);
                        if (dan.IsPlayList)
                        {
                            continue;
                        }
                        danSongs.Clear();
                        break;
                    }
                }
                if(danSongs.Count == 0)
                {
                    Debug.LogError("Failed to load dan, songs are empty or unable to find:" + dan.Name);
                    continue;
                }
                collections.Add(new SongCollection(dan.Name, danSongs.ToArray())
                {
                    Type = dan.IsPlayList ? ChartStorageType.List : ChartStorageType.Dan,
                    DanInfo = dan.IsPlayList ? null : dan
                });
                Debug.Log("Loaded Dan:" + dan.Name);
            }
            return collections.ToArray();
        }
        static async Task<SongCollection> GetCollection(string rootPath)
        {
            return await Task.Run(async () =>
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
            });
        }
        static async Task<SongCollection> GetOnlineCollection(ApiEndpoint api)
        {
            var name = api.Name;
            var collection = SongCollection.Empty(name);
            var apiroot = api.Url;
            if (string.IsNullOrEmpty(apiroot)) 
                return collection;

            var listurl = apiroot + "/maichart/list";
            Debug.Log("Loading Online Charts from:" + listurl);
            try
            {
                var client = HttpTransporter.ShareClient;
                var liststr = await client.GetStringAsync(listurl);
                var list = JsonSerializer.Deserialize<MajnetSongDetail[]>(liststr, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (list is null || list.IsEmpty())
                    return collection;

                var gameList = new List<SongDetail>();
                foreach (var song in list)
                {
                    SongDetail songDetail = SongDetail.ParseOnline(api, song);
                    gameList.Add(songDetail);
                }
                Debug.Log("Loaded Online Charts List:" + gameList.Count);
                return new SongCollection(name, gameList.ToArray())
                {
                    Location = ChartStorageLocation.Online
                };
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