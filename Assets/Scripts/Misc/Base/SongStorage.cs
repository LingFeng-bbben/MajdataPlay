using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Net;
using MajdataPlay.Numerics;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay
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


        static HashSet<string> _storageFav = new();
        static DanInfo? _userFavorites = null;
        static MyFavoriteSongCollection _myFavorite;

        readonly static string MY_FAVORITE_FILENAME = "MyFavorites.json";
        readonly static string MY_FAVORITE_EXPORT_PATH = Path.Combine(MajEnv.ChartPath, MY_FAVORITE_FILENAME);
        readonly static string MY_FAVORITE_STORAGE_PATH = Path.Combine(MajEnv.CachePath, "Runtime", MY_FAVORITE_FILENAME);

        internal static async Task InitAsync(IProgress<ChartScanProgress>? progressReporter = null)
        {
            try
            {
                if (File.Exists(MY_FAVORITE_EXPORT_PATH))
                {
                    bool result;
                    HashSet<string>? storageFav;
                    if (File.Exists(MY_FAVORITE_EXPORT_PATH))
                    {
                        (result, _userFavorites) = await Serializer.Json.TryDeserializeAsync<DanInfo>(File.OpenRead(MY_FAVORITE_EXPORT_PATH));
                        if (!result)
                        {
                            MajDebug.LogError($"Failed to load favorites\nPath: {MY_FAVORITE_EXPORT_PATH}");
                        }
                    }
                    if (File.Exists(MY_FAVORITE_STORAGE_PATH))
                    {
                        (result, storageFav) = await Serializer.Json.TryDeserializeAsync<HashSet<string>>(File.OpenRead(MY_FAVORITE_STORAGE_PATH));
                        if (!result)
                        {
                            MajDebug.LogError($"Failed to load favorites\nPath: {MY_FAVORITE_EXPORT_PATH}");
                        }
                        else
                        {
                            _storageFav = storageFav ?? _storageFav;
                        }
                    }
                }

                if (!Directory.Exists(MajEnv.ChartPath))
                {
                    Directory.CreateDirectory(MajEnv.ChartPath);
                    Directory.CreateDirectory(Path.Combine(MajEnv.ChartPath, "default"));
                    return;
                }
                var rootPath = MajEnv.ChartPath;
                var songs = await GetCollections(rootPath, progressReporter);

                Collections = songs;
                TotalChartCount = await Collections.ToUniTaskAsyncEnumerable().SumAsync(x => x.Count);
                MajDebug.Log($"Loaded chart count: {TotalChartCount}");
                MajEnv.OnApplicationQuit += OnApplicationQuit;
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
                throw;
            }
        }
        static async Task<SongCollection[]> GetCollections(string rootPath, IProgress<ChartScanProgress>? progressReporter)
        {
            try
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

                await Task.WhenAll(tasks);

                foreach (var task in tasks)
                {
                    if (task.Result != null)
                        collections.Add(task.Result);
                }
                //Online Charts
                if (MajInstances.Settings.Online.Enable)
                {
                    foreach (var item in MajInstances.Settings.Online.ApiEndpoints.GroupBy(x => x.Url))
                    {
                        var api = item.FirstOrDefault();
                        if (api is null)
                            continue;
                        if (string.IsNullOrEmpty(api.Name))
                            continue;
                        progressReporter?.Report(new ChartScanProgress()
                        {
                            StorageType = ChartStorageLocation.Online,
                            Message = api.Name
                        });
                        var result = await GetOnlineCollection(api);
                        if (!result.IsEmpty)
                        {
                            collections.Add(result);
                        }
                    }
                }
                //Add all songs to "All" folder
                var allcharts = new List<ISongDetail>();
                foreach (var collection in collections)
                {
                    foreach (var item in collection)
                    {
                        allcharts.Add(item);
                    }
                }
                collections.Add(new SongCollection("All", allcharts.ToArray()));
                MajDebug.Log("MyFavorite");
                if (_userFavorites is not null)
                {
                    foreach (var hash in _userFavorites.SongHashs)
                    {
                        _storageFav.Add(hash);
                    }
                }
                var hashSet = _storageFav;
                var favoriteSongs = allcharts.Where(x => hashSet.Any(y => y == x.Hash))
                                             .OrderByDescending(x => x.IsOnline)
                                             .GroupBy(x => x.Hash)
                                             .Select(x => x.FirstOrDefault())
                                             .Where(x => x is not null)
                                             .ToList();
                MajDebug.Log(favoriteSongs.Count);
                _myFavorite = new(favoriteSongs, new HashSet<string>(_storageFav));
                //The collections and _myFavorite share a same ref of original List<T>
                collections.Add(_myFavorite);
                MajDebug.Log("Load Dans");
                var danFiles = new DirectoryInfo(rootPath).GetFiles("*.json");
                var loadDanTasks = new Task<SongCollection?>[danFiles.Length];
                for (var i = 0; i < loadDanTasks.Length; i++)
                {
                    if (i >= danFiles.Length)
                    {
                        loadDanTasks[i] = Task.FromResult<SongCollection?>(null);
                        continue;
                    }
                    var file = danFiles[i];
                    if (file.Name == MY_FAVORITE_FILENAME)
                    {
                        loadDanTasks[i] = Task.FromResult<SongCollection?>(null);
                        continue;
                    }
                    var jsonStream = File.OpenRead(file.FullName);
                    var (result, dan) = await Serializer.Json.TryDeserializeAsync<DanInfo>(jsonStream, new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = false
                    });
                    if (result && dan is not null)
                    {
                        loadDanTasks[i] = GetDanCollection(allcharts, dan);
                    }
                }
                if (loadDanTasks.Length != 0)
                {
                    var allTask = Task.WhenAll(loadDanTasks);
                    while (!allTask.IsCompleted)
                        await Task.Yield();
                    foreach (var task in loadDanTasks)
                    {
                        if (task.IsFaulted)
                        {
                            MajDebug.LogError(task.Exception);
                            continue;
                        }
                        var collection = task.Result;
                        if (collection is null)
                        {
                            continue;
                        }
                        collections.Add(collection);
                        MajDebug.Log("Loaded Dan:" + collection.DanInfo?.Name ?? collection.Name);
                    }
                }
                return collections.ToArray();
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
                throw;
            }
        }
        static async Task<SongCollection> GetCollection(string rootPath)
        {
            return await Task.Run(async () =>
            {
                try
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

                        var parsingTask = SongDetail.ParseAsync(songDir.FullName);

                        tasks.Add(parsingTask);
                    }
                    var allTask = Task.WhenAll(tasks);

                    while (!allTask.IsCompleted)
                    {
                        await Task.Yield();
                    }
                    foreach (var task in tasks)
                    {
                        if (task.IsFaulted)
                        {
                            MajDebug.LogException(task.Exception);
                            continue;
                        }
                        charts.Add(task.Result);
                    }
                    return new SongCollection(thisDir.Name, charts.ToArray());
                }
                catch (Exception e)
                {
                    MajDebug.LogException(e);
                    throw;
                }
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
            MajDebug.Log("Loading Online Charts from:" + listurl);
            try
            {
                var client = MajEnv.SharedHttpClient;
                var rspStream = await client.GetStreamAsync(listurl);
                var list = await JsonSerializer.DeserializeAsync<MajnetSongDetail[]>(rspStream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (list is null || list.IsEmpty())
                    return collection;

                var gameList = new List<ISongDetail>();
                foreach (var song in list)
                {
                    var songDetail = new OnlineSongDetail(api, song);
                    gameList.Add(songDetail);
                }
                MajDebug.Log("Loaded Online Charts List:" + gameList.Count);
                var cacheFolder = Path.Combine(MajEnv.CachePath, $"Net/{name}");
                if (!Directory.Exists(cacheFolder))
                {
                    Directory.CreateDirectory(cacheFolder);
                }
                return new SongCollection(name, gameList.ToArray())
                {
                    Location = ChartStorageLocation.Online
                };
            }
            catch (OperationCanceledException)
            {
                var cachePath = Path.Combine(MajEnv.CachePath, "Net", name);
                if (!Directory.Exists(cachePath))
                {
                    return collection;
                }
                var c = await GetCollection(cachePath);
                MajDebug.Log("Loaded Cached Online Charts List:" + c.Count);
                return c;
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
                return collection;
            }
        }
        static async Task<SongCollection?> GetDanCollection(IEnumerable<ISongDetail> allCharts, DanInfo danInfo)
        {
            return await Task.Run(() =>
            {
                var songHashs = danInfo.SongHashs;
                var targetCharts = allCharts.Where(x => songHashs.Any(y => y == x.Hash))
                                            .OrderByDescending(x => x.IsOnline)
                                            .GroupBy(x => x.Hash)
                                            .Select(x => x.FirstOrDefault())
                                            .Where(x => x is not null)
                                            .ToArray();
                if (targetCharts.Length == 0)
                {
                    MajDebug.LogError("Failed to load dan, songs are empty or unable to find:" + danInfo.Name);
                    return default;
                }
                return new SongCollection(danInfo.Name, targetCharts)
                {
                    Type = danInfo.IsPlayList ? ChartStorageType.PlayList : ChartStorageType.Dan,
                    DanInfo = danInfo.IsPlayList ? null : danInfo
                };
            });
        }
        static void OnApplicationQuit()
        {
            var hashSet = _myFavorite.ExportHashSet();
            File.WriteAllText(MY_FAVORITE_STORAGE_PATH, Serializer.Json.Serialize(hashSet));
            File.WriteAllText(MY_FAVORITE_EXPORT_PATH,
                              Serializer.Json.Serialize(new DanInfo()
                              {
                                  Name = "My Favorites",
                                  SongHashs = hashSet.ToArray(),
                                  IsPlayList = true
                              }
                ));
            MajEnv.OnApplicationQuit -= OnApplicationQuit;
        }
        public static void AddToMyFavorites(ISongDetail songDetail)
        {
            _myFavorite.Add(songDetail);
            RefreshMyFavStorage();
        }
        public static bool IsInMyFavorites(ISongDetail songDetail)
        {
            return _myFavorite.Any(o => o.Hash == songDetail.Hash);
        }
        public static void RemoveFromMyFavorites(ISongDetail songDetail)
        {
            _myFavorite.Remove(songDetail);
            RefreshMyFavStorage();
        }
        public static void RemoveFromMyFavorites(string hashBase64Str)
        {
            _myFavorite.Remove(hashBase64Str);
            RefreshMyFavStorage();
        }
        static void RefreshMyFavStorage()
        {
            File.WriteAllText(MY_FAVORITE_STORAGE_PATH, Serializer.Json.Serialize(_myFavorite.ExportHashSet()));
        }
    }
}