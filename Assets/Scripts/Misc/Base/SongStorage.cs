using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using MajdataPlay.Buffers;
using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Net;
using MajdataPlay.Numerics;
using MajdataPlay.Settings;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.Networking;
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
        public static SongOrder OrderBy 
        {
            get => MajEnv.RuntimeConfig.List.OrderBy;
        }
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
        static string MY_FAVORITE_EXPORT_PATH = string.Empty;
        static string MY_FAVORITE_STORAGE_PATH = string.Empty;

        internal static async Task InitAsync(IProgress<string>? progressReporter = null)
        {
            if (string.IsNullOrEmpty(MY_FAVORITE_EXPORT_PATH))
            {
                MY_FAVORITE_EXPORT_PATH = Path.Combine(MajEnv.ChartPath, MY_FAVORITE_FILENAME);
            }
            if(string.IsNullOrEmpty(MY_FAVORITE_STORAGE_PATH))
            {
                MY_FAVORITE_STORAGE_PATH = Path.Combine(MajEnv.CachePath, "Runtime", MY_FAVORITE_FILENAME);
            }
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
                    MajDebug.LogInfo($"Loaded chart count: {TotalChartCount}");
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
                MajEnv.OnSave += OnSave;
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
                var listConfig = MajEnv.RuntimeConfig.List;
                var selectedDiff = listConfig.SelectedDiff;
                var selectedIndex = listConfig.SelectedSongIndex;
                var selectedDir = listConfig.SelectedDir;

                var collections = await GetCollections(MajEnv.ChartPath, progressReporter);
                await Task.Delay(100);
                progressReporter?.Report($"{"MAJTEXT_CLEANING_UP".i18n()}");
                await Task.Delay(100);

                var tasks = new Task[chartListBackup.Count];
                var tasksI = -1;
                Parallel.For(0, chartListBackup.Count, i =>
                {
                    var songDetail = chartListBackup[i];
                    switch (songDetail)
                    {
                        case OnlineSongDetail online:
                            tasks[Interlocked.Increment(ref tasksI)] = online.DisposeAsync().AsTask();
                            break;
                        case SongDetail local:
                            tasks[Interlocked.Increment(ref tasksI)] = local.DisposeAsync().AsTask();
                            break;
                    }
                });
                var waitAllTask = Task.WhenAll(tasks);
                await using(UniTask.ReturnToCurrentSynchronizationContext())
                {
                    while(!waitAllTask.IsCompleted)
                    {
                        await UniTask.Yield();
                    }
                }
                tasks = null;
                waitAllTask = null;
                Collections = collections;
                MajDebug.LogInfo($"Loaded chart count: {TotalChartCount}");
                GC.Collect();

                CollectionIndex = selectedDir;
                var selectedCollection = WorkingCollection;

                if (selectedCollection.IsEmpty)
                {
                    return;
                }
                else if (selectedIndex >= selectedCollection.Count)
                {
                    selectedCollection.Index = 0;
                }
                else
                {
                    selectedCollection.Index = selectedIndex;
                }
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
                if (task.IsFaulted)
                {
                    MajDebug.LogException(task.Exception);
                    continue;
                }
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
            MajDebug.LogInfo("MyFavorite");
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
            MajDebug.LogInfo(favoriteSongs.Count);
            _myFavorite = new(favoriteSongs, new HashSet<string>(_storageFav));
            //The collections and _myFavorite share a same ref of original List<T>
            collections.Add(_myFavorite);
            MajDebug.LogInfo("Load Dans");
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
                var (result, dan) = await Serializer.Json.TryDeserializeAsync<DanInfo>(jsonStream);
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
                    MajDebug.LogInfo("Loaded Dan:" + collection.DanInfo?.Name ?? collection.Name);
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
            var flagDirPath = System.IO.Path.Combine(rootPath, ".MajdataPlay");

            if (!Directory.Exists(flagDirPath))
            {
                var info = Directory.CreateDirectory(flagDirPath);
                info.Attributes |= FileAttributes.Hidden;
            }
            if (dirs.Count == 0)
            {
                return SongCollection.Empty(rootPath, thisDir.Name);
            }
            using var charts = new RentedList<SongDetail>();
            using var tasks = new RentedList<Task<SongDetail>>();
            
            foreach (var songDir in dirs)
            {
                if((songDir.Attributes & FileAttributes.Hidden) != 0)
                {
                    continue;
                }
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
            return new SongCollection(rootPath, thisDir.Name, charts.ToArray());
        }
        static async Task<SongCollection> GetOnlineCollection(ApiEndpoint api, IProgress<string>? progressReporter)
        {
            var name = api.Name;
            var cachePath = Path.Combine(MajEnv.CachePath, "Net", name);
            if(!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }
            var collection = SongCollection.Empty(cachePath, name);
            var apiroot = api.Url;

            if (string.IsNullOrEmpty(apiroot))
            {
                return collection;
            }

            var listurl = apiroot + "/maichart/list";
            MajDebug.LogInfo("Loading Online Charts from:" + listurl);
            try
            {
                var client = MajEnv.SharedHttpClient;
                var rspText = string.Empty;
                for (var i = 0; i <= MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                {
                    try
                    {
                        if (i != 0)
                        {
                            progressReporter?.Report(
                                ZString.Format("MAJTEXT_SCANNING_CHARTS_FROM_{0}".i18n(), api.Name) +
                                $" ({i}/{MajEnv.HTTP_REQUEST_MAX_RETRY})");
                        }
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
                        await UniTask.SwitchToMainThread();
                        var getReq = UnityWebRequest.Get(listurl);
                        getReq.timeout = MajEnv.HTTP_TIMEOUT_MS / 1000;
                        getReq.SetRequestHeader("User-Agent", MajEnv.HTTP_USER_AGENT);
                        var asyncOperation = getReq.SendWebRequest();
                        while (!asyncOperation.isDone)
                        {
                            await UniTask.Yield();
                        }

                        getReq.EnsureSuccessStatusCode();
                        rspText = getReq.downloadHandler.text;
#else
                        rspText = await client.GetStringAsync(listurl);
#endif
                        break;
                    }
                    catch (Exception e)
                    {
                        if (i == MajEnv.HTTP_REQUEST_MAX_RETRY)
                        {
                            progressReporter?.Report(ZString.Format("Failed to fetch list from {0}".i18n(), api.Name));
                            MajDebug.LogException(e);
                            await Task.Delay(2000);
                            throw new OperationCanceledException();
                        }
                    }
                    finally
                    {
                        await UniTask.SwitchToThreadPool();
                    }
                }
                var list = await Serializer.Json.DeserializeAsync<MajnetSongDetail[]>(rspText);
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

                MajDebug.LogInfo("Loaded Online Charts List:" + gameList.Length);
                Interlocked.Add(ref _totalChartCount, list.Length);
                var cacheFolder = Path.Combine(MajEnv.CachePath, $"Net/{name}");
                if (!Directory.Exists(cacheFolder))
                {
                    Directory.CreateDirectory(cacheFolder);
                }
                return new SongCollection(cachePath, name, gameList.ToArray())
                {
                    Location = ChartStorageLocation.Online
                };
            }
            catch (OperationCanceledException)
            {
                if (!Directory.Exists(cachePath))
                {
                    return collection;
                }
                var c = await GetCollection(cachePath);
                MajDebug.LogInfo("Loaded Cached Online Charts List:" + c.Count);
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
        static void OnSave()
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
            catch (Exception e)
            {
                MajDebug.LogException(e);
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