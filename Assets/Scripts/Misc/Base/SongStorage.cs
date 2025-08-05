using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using MajdataPlay.Settings;
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
using System.Threading;
using System.Threading.Tasks;
using MajdataPlay.Buffers;
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
                {
                    return EMPTY_SONG_COLLECTION;
                }
                return Collections[_collectionIndex];
            }
        }
        /// <summary>
        /// Loaded song collections
        /// </summary>
        public static SongCollection[] Collections { get; private set; } = Array.Empty<SongCollection>();
        public static SongOrder OrderBy { get; set; } = new();
        public static long TotalChartCount
        { 
            get
            {
                return _totalChartCount;
            }
        }

        static int _collectionIndex = 0;
        static long _totalChartCount = 0;
        static long _parsedChartCount = 0;


        readonly static List<ISongDetail> _allCharts = new(8192);
        readonly static HashSet<string> _storageFav = new();
        static DanInfo? _userFavorites = null;
        static MyFavoriteSongCollection _myFavorite;

        static bool _isInited = false;

        readonly static SongCollection EMPTY_SONG_COLLECTION = SongCollection.Empty("default");
        readonly static string MY_FAVORITE_FILENAME = "MyFavorites.json";
        readonly static string MY_FAVORITE_EXPORT_PATH = Path.Combine(MajEnv.ChartPath, MY_FAVORITE_FILENAME);
        readonly static string MY_FAVORITE_STORAGE_PATH = Path.Combine(MajEnv.CachePath, "Runtime", MY_FAVORITE_FILENAME);

        internal static async Task InitAsync(IProgress<string>? progressReporter = null)
        {
            try
            {
                await Task.Run(async () =>
                {
                    if (File.Exists(MY_FAVORITE_EXPORT_PATH))
                    {
                        bool result;
                        (result, _userFavorites) = await Serializer.Json.TryDeserializeAsync<DanInfo>(File.OpenRead(MY_FAVORITE_EXPORT_PATH));
                        if (!result)
                        {
                            var path = Path.Combine(MY_FAVORITE_EXPORT_PATH, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.bak");
                            File.Copy(MY_FAVORITE_EXPORT_PATH, path);
                            MajDebug.LogError($"Failed to load favorites\nPath: {MY_FAVORITE_EXPORT_PATH}");
                        }
                    }
                    if (File.Exists(MY_FAVORITE_STORAGE_PATH))
                    {

                        var (result, storageFav) = await Serializer.Json.TryDeserializeAsync<HashSet<string>>(File.OpenRead(MY_FAVORITE_STORAGE_PATH));
                        if (!result)
                        {
                            var path = Path.Combine(MY_FAVORITE_STORAGE_PATH, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.bak");
                            File.Copy(MY_FAVORITE_STORAGE_PATH, path);
                            MajDebug.LogError($"Failed to load favorites\nPath: {MY_FAVORITE_STORAGE_PATH}");
                        }
                        else if(storageFav is not null)
                        {
                            foreach(var hash in storageFav)
                            {
                                if (string.IsNullOrEmpty(hash))
                                {
                                    continue;
                                }
                                _storageFav.Add(hash);
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
                    MajDebug.Log($"Loaded chart count: {TotalChartCount}");
                    _isInited = true;
                });
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
                throw;
            }
            finally
            {
                MajEnv.OnApplicationQuit += OnApplicationQuit;
            }
        }
        internal static async Task RefreshAsync(IProgress<string>? progressReporter = null)
        {
            if(!_isInited)
            {
                return;
            }
            await UniTask.SwitchToThreadPool();
            using var chartListBackup = new RentedList<ISongDetail>(_allCharts);
            try
            {
                _allCharts.Clear();
                _parsedChartCount = 0;
                _totalChartCount = 0;
                
                var collections = await GetCollections(MajEnv.ChartPath, progressReporter);
                await UniTask.Delay(100);
                progressReporter?.Report($"{"MAJTEXT_CLEAN_UP".i18n()}");
                await UniTask.Delay(100);
                foreach (var songDetail in chartListBackup)
                {
                    switch(songDetail)
                    {
                        case OnlineSongDetail online:
                            online.Dispose();
                            break;
                        case SongDetail local:
                            local.Dispose();
                            break;
                    }
                }
                Collections = collections;
                MajDebug.Log($"Loaded chart count: {TotalChartCount}");
            }
            catch(Exception e)
            {
                _allCharts.Clear();
                _allCharts.AddRange(chartListBackup);
                MajDebug.LogException(e);
                throw;
            }
        }
        static async Task<SongCollection[]> GetCollections(string rootPath, IProgress<string>? progressReporter)
        {
            var dirs = new DirectoryInfo(rootPath).GetDirectories();
            var tasks = new List<Task<SongCollection>>(dirs.Length);
            var collections = new List<SongCollection>(dirs.Length);

            //Local Charts
            Parallel.For(0, dirs.Length, i =>
            {
                var dir = dirs[i];
                var path = dir.FullName;
                var files = dir.GetFiles();
                var maidataFile = files.FirstOrDefault(o => o.Name.ToLower() is "maidata.txt");
                var trackFile = files.FirstOrDefault(o => o.Name.ToLower() is "track.mp3" or "track.ogg");

                if (maidataFile is not null || trackFile is not null)
                {
                    return;
                }

                tasks.Add(GetCollection(path));
            });

            var allTasks = Task.WhenAll(tasks);

            while (!allTasks.IsCompleted)
            {
                var percent = 0f;
                if (_totalChartCount != 0)
                {
                    percent = _parsedChartCount / (float)_totalChartCount;
                }
                progressReporter?.Report($"{"MAJTEXT_SCANNING_CHARTS".i18n()}...({percent * 100:F2}%)");
                await Task.Delay(33);
            }
            progressReporter?.Report($"{"MAJTEXT_SCANNING_CHARTS".i18n()}...(100.00%)");

            foreach (var task in tasks)
            {
                if (task.Result != null)
                {
                    collections.Add(task.Result);
                }
            }
            collections = collections.OrderBy(x => x.Name).ToList();
            await Task.Delay(1000);
            //Online Charts
            if (MajInstances.Settings.Online.Enable)
            {
                foreach (var item in MajInstances.Settings.Online.ApiEndpoints.OrderBy(x => x.Name).GroupBy(x => x.Url))
                {
                    var api = item.FirstOrDefault();
                    if (api is null)
                    {
                        continue;
                    }
                    if (string.IsNullOrEmpty(api.Name))
                    {
                        continue;
                    }
                    progressReporter?.Report(ZString.Format(Localization.GetLocalizedText("MAJTEXT_SCANNING_CHARTS_FROM_{0}"), api.Name));
                    var result = await GetOnlineCollection(api, progressReporter);
                    if (!result.IsEmpty)
                    {
                        collections.Add(result);
                    }
                }
            }
            //Add all songs to "All" folder
            foreach (var collection in collections)
            {
                foreach (var item in collection)
                {
                    _allCharts.Add(item);
                }
            }
            collections.Add(new SongCollection("All", _allCharts.ToArray()));
            MajDebug.Log("MyFavorite");
            if (_userFavorites is not null)
            {
                foreach (var hash in _userFavorites.SongHashs)
                {
                    _storageFav.Add(hash);
                }
            }
            var hashSet = _storageFav;
            var favoriteSongs = _allCharts.Where(x => hashSet.Any(y => y == x.Hash))
                                          .GroupBy(x => x.Hash)
                                          .Select(x => x.FirstOrDefault())
                                          .Where(x => x is not null)
                                          .OrderBy(x => hashSet.ToList().IndexOf(x.Hash))
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
                    loadDanTasks[i] = GetDanCollection(_allCharts, dan);
                }
            }
            if (loadDanTasks.Length != 0)
            {
                await Task.WhenAll(loadDanTasks);

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
        static async Task<SongCollection> GetCollection(string rootPath)
        {
            await UniTask.SwitchToThreadPool();
            var thisDir = new DirectoryInfo(rootPath);
            var dirs = thisDir.GetDirectories()
                              .OrderBy(o => o.CreationTime)
                              .ToList();
            if (dirs.Count == 0)
            {
                return SongCollection.Empty(thisDir.Name);
            }
            var charts = new List<SongDetail>();
            var tasks = new List<Task<SongDetail>>();
            foreach (var songDir in dirs)
            {
                var files = songDir.GetFiles();
                var maidataFile = files.FirstOrDefault(o => o.Name is "maidata.txt");
                var trackFile = files.FirstOrDefault(o => o.Name is "track.mp3" or "track.ogg");

                if (maidataFile is null || trackFile is null)
                {
                    continue;
                }

                var parsingTask = Task.Run(async () =>
                {
                    var chart = await SongDetail.ParseAsync(songDir.FullName);
                    Interlocked.Increment(ref _parsedChartCount);
                    return chart;
                });
                Interlocked.Increment(ref _totalChartCount);
                tasks.Add(parsingTask);
            }
            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                if (task.IsFaulted)
                {
                    MajDebug.LogException(task.Exception);
                    Interlocked.Decrement(ref _totalChartCount);
                    continue;
                }
                charts.Add(task.Result);
            }
            return new SongCollection(thisDir.Name, charts.ToArray());
        }
        static async Task<SongCollection> GetOnlineCollection(ApiEndpoint api, IProgress<string>? progressReporter)
        {
            var name = api.Name;
            var collection = SongCollection.Empty(name);
            var apiroot = api.Url;
            if (string.IsNullOrEmpty(apiroot))
            {
                return collection;
            }

            var listurl = apiroot + "/maichart/list";
            MajDebug.Log("Loading Online Charts from:" + listurl);
            try
            {
                var client = MajEnv.SharedHttpClient;
                var rspStream = Stream.Null;
                for (var i = 0; i <= MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                {
                    try
                    {
                        if(i != 0)
                        {
                            progressReporter?.Report(ZString.Format("Scanning Charts From {0}".i18n(), api.Name)+ $" ({i}/{MajEnv.HTTP_REQUEST_MAX_RETRY})");
                        }
                        rspStream = await client.GetStreamAsync(listurl);
                        break;
                    }
                    catch
                    {
                        if (i == MajEnv.HTTP_REQUEST_MAX_RETRY)
                        {
                            progressReporter?.Report(ZString.Format("Failed to fetch list from {0}".i18n(), api.Name));
                            await Task.Delay(2000);
                            throw new OperationCanceledException();
                        }
                    }
                }
                var list = await JsonSerializer.DeserializeAsync<MajnetSongDetail[]>(rspStream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (list is null || list.IsEmpty())
                {
                    return collection;
                }

                var gameList = new ISongDetail[list.Length];
                Parallel.For(0, list.Length, i =>
                {
                    var song = list[i];
                    var songDetail = new OnlineSongDetail(api, song);
                    gameList[i] = songDetail;
                });

                MajDebug.Log("Loaded Online Charts List:" + gameList.Length);
                Interlocked.Add(ref _totalChartCount, list.Length);
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
                var songHashs = danInfo.SongHashs.ToList();
                var targetCharts = allCharts.Where(x => songHashs.Any(y => y == x.Hash))
                                            .OrderByDescending(x => x.IsOnline)
                                            .GroupBy(x => x.Hash)
                                            .Select(x => x.FirstOrDefault())
                                            .Where(x => x is not null)
                                            .OrderBy(x => songHashs.IndexOf(x.Hash))
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
            try
            {
                if (!_isInited)
                {
                    return;
                }
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
            }
            finally
            {
                MajEnv.OnApplicationQuit -= OnApplicationQuit;
            }
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