using Cysharp.Threading.Tasks;
using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Net;
using MajdataPlay.Utils;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Types
{
#nullable enable
    public class SongDetail
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string?[] Designers { get; set; } = new string[7];
        public string? Description { get; set; }
        public int? ClockCount { get; set; }
        public string[] Levels { get; set; } = new string[7];
        public string? VideoPath { get; init; }
        public string? TrackPath { get; init; }
        public string? MaidataPath { get; init; }
        public string? CoverPath { get; init; }
        public string? BGPath { get; init; }
        private Sprite? SongCover;
        public double First { get; set; }
        public string Hash { get; set; } = string.Empty;
        public string OnlineId { get; set; } = "";
        public ApiEndpoint? ApiEndpoint { get; set; }
        public DateTime AddTime { get; set; }
        public bool IsOnline { get; set; } = false;

        public static async Task<SongDetail> ParseAsync(FileInfo[] files)
        {
            var maidataFile = files.FirstOrDefault(o => o.Name is "maidata.txt");
            var maidataPath = maidataFile.FullName;
            var trackPath = files.FirstOrDefault(o => o.Name is "track.mp3" or "track.ogg").FullName;
            var videoFile = files.FirstOrDefault(o => o.Name is "bg.mp4" or "pv.mp4" or "mv.mp4");
            var coverFile = files.FirstOrDefault(o => o.Name is "bg.png" or "bg.jpg");
            var videoPath = string.Empty;
            var maibyte = await File.ReadAllBytesAsync(maidataPath);
            var hash = await GetHashAsync(maibyte);

            string title = string.Empty;
            string artist = string.Empty;
            string description = string.Empty;
            int clockCount = 4;
            double first = 0;
            string[] designers = new string[7];
            string[] levels = new string[7];
            string? coverPath = null;

            var maidata = Encoding.UTF8.GetString(maibyte).Split('\n');
            await Task.Run(() =>
            {

                for (int i = 0; i < maidata.Length; i++)
                {
                    try
                    {
                        if (maidata[i].StartsWith("&title="))
                            title = GetValue(maidata[i]);
                        else if (maidata[i].StartsWith("&artist="))
                            artist = GetValue(maidata[i]);
                        else if (maidata[i].StartsWith("&des="))
                        {
                            for (int k = 0; k < designers.Length; k++)
                            {
                                designers[k] = GetValue(maidata[i]);
                            }
                        }
                        else if (maidata[i].StartsWith("&freemsg="))
                            description = GetValue(maidata[i]);
                        else if (maidata[i].StartsWith("&clock_count="))
                            clockCount = int.Parse(GetValue(maidata[i]));
                        else if (maidata[i].StartsWith("&first="))
                        {
                            if (double.TryParse(GetValue(maidata[i]), out double result))
                            {
                                first = result;
                            }
                            else
                            {
                                first = 0;
                            }
                        }
                        else if (maidata[i].StartsWith("&lv_") || maidata[i].StartsWith("&des_"))
                        {
                            for (int j = 1; j < 8 && i < maidata.Length; j++)
                            {
                                if (maidata[i].StartsWith("&lv_" + j + "="))
                                    levels[j - 1] = GetValue(maidata[i]);
                                else if (maidata[i].StartsWith("&des_" + j + "="))
                                    designers[j - 1] = GetValue(maidata[i]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MajDebug.LogError("Failed to Load " + maidataPath + "at line " + i);
                        MajDebug.LogError(ex.Message);
                        throw ex;
                    }
                }

            });
            if (coverFile != null)
                coverPath = coverFile.FullName;
            if (videoFile != null)
                videoPath = videoFile.FullName;
            return new SongDetail()
            {
                Title = title,
                Artist = artist,
                Designers = designers,
                Description = description,
                ClockCount = clockCount,
                Levels = levels,
                First = first,
                Hash = hash,
                VideoPath = videoPath,
                TrackPath = trackPath,
                CoverPath = coverPath,
                MaidataPath = maidataPath,
                AddTime = maidataFile.LastWriteTime
            };
        }

        public static SongDetail ParseOnline(ApiEndpoint api, MajnetSongDetail song)
        {
            var apiroot = api.Url + "/maichart/";
            var songDetail = new SongDetail()
            {
                Title = song.Title,
                Artist = song.Artist,
                Levels = song.Levels.ToArray(),
                IsOnline = true,
                MaidataPath = apiroot + song.Id + "/chart",
                TrackPath = apiroot + song.Id + "/track",
                BGPath = apiroot + song.Id + "/image?fullimage=true",
                VideoPath = apiroot + song.Id + "/video",
                CoverPath = apiroot + song.Id + "/image",
                Hash = song.Hash,
                ApiEndpoint = api,
                OnlineId = song.Id,
                AddTime = song.Timestamp
            };
            for (int i = 0; i < songDetail.Designers.Count(); i++)
            {
                songDetail.Designers[i] = song.Uploader + "@" + song.Designer;
            }

            return songDetail;
        }

        private static string GetValue(string varline)
        {
            try
            {
                return varline.Substring(varline.FindIndex(o => o == '=') + 1).Replace("\r", "");
            }
            catch
            {
                return string.Empty;
            }
        }
        private static async ValueTask<string> GetHashAsync(byte[] file)
        {
            return await Task.Run(() =>
            {
                var hashComputer = MD5.Create();
                var chartHash = hashComputer.ComputeHash(file);

                return Convert.ToBase64String(chartHash);
            });
        }

        byte[] _maidatabytecache = new byte[0];
        public async UniTask<string> GetInnerMaidata(int selectedDifficulty)
        {
            //TODO: should check hash change here
            var maidatabyte = new byte[0];
            if (IsOnline)
            {
                if (_maidatabytecache.Length == 0)
                {
                    maidatabyte = await HttpTransporter.ShareClient.GetByteArrayAsync(MaidataPath);
                    _maidatabytecache = maidatabyte;
                }
                else
                {
                    maidatabyte = _maidatabytecache;
                }
            }
            else
            {
                maidatabyte = await File.ReadAllBytesAsync(MaidataPath);
                var hash = await GetHashAsync(maidatabyte);
                if (hash != Hash)
                {
                    MajDebug.LogError("Chart Hash Mismatch");
                    Application.Quit();
                }
            }
            
            var maidata = Encoding.UTF8.GetString(maidatabyte).Split('\n');
            for (int i = 0; i < maidata.Length; i++)
            {
                if (maidata[i].StartsWith("&inote_" + (selectedDifficulty + 1) + "="))
                {
                    var TheNote = "";
                    //first line behind =
                    TheNote += GetValue(maidata[i]) + "\n";
                    i++;
                    //read the lines
                    for (; i < maidata.Length; i++)
                    {
                        //end when eof or another command
                        if (i < maidata.Length)
                            if (maidata[i].StartsWith("&"))
                                break;
                        TheNote += maidata[i] + "\n";
                    }

                    return TheNote;
                }
            }
            return string.Empty;
        }

        private bool _spriteLoadLock = false;
        public async Task<Sprite> GetSpriteAsync(CancellationToken ct = default)
        {
            while (_spriteLoadLock) await Task.Delay(200);
            _spriteLoadLock = true;
            if (SongCover != null)
            {
                MajDebug.Log("Memory Cache Hit");
                _spriteLoadLock = false;
                return SongCover;
            }
            if (string.IsNullOrEmpty(CoverPath))
            {
                SongCover = await SpriteLoader.LoadAsync(Application.streamingAssetsPath + "/dummy.jpg");
                _spriteLoadLock = false;
                return SongCover;
            }
            if (IsOnline)
            {
                MajDebug.Log("Try load cover online" + CoverPath);
                SongCover = await SpriteLoader.LoadAsync(new Uri(CoverPath), ct);
                _spriteLoadLock = false;
                return SongCover;
            }
            else
            {
                SongCover = await SpriteLoader.LoadAsync(CoverPath, ct);
                _spriteLoadLock = false;
                return SongCover;
            }

        }

        //Dump an online chart to local, return a new SongDetail points to local song
        public async UniTask<SongDetail> DumpToLocal(CancellationTokenSource _cts)
        {
            if(!this.IsOnline) throw new Exception("Not an online song");
            var _songDetail = this;
            var chartFolder = Path.Combine(MajEnv.ChartPath, $"MajnetPlayed/{_songDetail.Hash.Replace('/', '_')}");
            Directory.CreateDirectory(chartFolder);
            var dirInfo = new DirectoryInfo(chartFolder);
            var trackPath = Path.Combine(chartFolder, "track.mp3");
            var chartPath = Path.Combine(chartFolder, "maidata.txt");
            var bgPath = Path.Combine(chartFolder, "bg.png");
            var videoPath = Path.Combine(chartFolder, "bg.mp4");
            var trackUri = _songDetail.TrackPath;
            var chartUri = _songDetail.MaidataPath;
            var bgUri = _songDetail.BGPath;
            var videoUri = _songDetail.VideoPath;
            var token = _cts.Token;

            if (trackUri is null or "")
                throw new AudioTrackNotFoundException(trackPath);
            if (chartUri is null or "")
                throw new ChartNotFoundException(_songDetail);

            MajInstances.LightManager.SetAllLight(Color.blue);
            MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading")}...");
            await UniTask.Yield();
            if (!File.Exists(trackPath))
            {
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Audio Track")}...");
                await UniTask.Yield();
                await DownloadFile(trackUri, trackPath, _cts);
            }
            token.ThrowIfCancellationRequested();
            if (!File.Exists(chartPath))
            {
                MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Maidata")}...");
                await UniTask.Yield();
                await DownloadFile(chartUri, chartPath, _cts);
            }
            token.ThrowIfCancellationRequested();
            SongDetail song;
            token.ThrowIfCancellationRequested();
            if (!File.Exists(bgPath))
            {
                try
                {
                    MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Picture")}...");
                    await UniTask.Yield();
                    await DownloadFile(bgUri, bgPath, _cts);
                }
                catch
                {
                    MajDebug.Log("No video for this song");
                    File.Delete(videoPath);
                    videoPath = "";
                    MajInstances.SceneSwitcher.SetLoadingText("");
                }
            }
            token.ThrowIfCancellationRequested();
            if (!File.Exists(videoPath) && videoUri is not null)
            {
                try
                {
                    MajInstances.SceneSwitcher.SetLoadingText($"{Localization.GetLocalizedText("Downloading Video")}...");
                    await UniTask.Yield();
                    await DownloadFile(videoUri, videoPath, _cts);
                }
                catch
                {
                    MajDebug.Log("No video for this song");
                    File.Delete(videoPath);
                    videoPath = "";
                    MajInstances.SceneSwitcher.SetLoadingText("");
                }
            }
            token.ThrowIfCancellationRequested();
            song = await ParseAsync(dirInfo.GetFiles());
            song.Hash = _songDetail.Hash;
            song.OnlineId = _songDetail.OnlineId;
            song.ApiEndpoint = _songDetail.ApiEndpoint;
            MajInstances.SceneSwitcher.SetLoadingText("");
            await UniTask.Yield();
            return song;
        }


        static async UniTask DownloadFile(string uri, string savePath, CancellationTokenSource _cts)
        {
            var task = HttpTransporter.ShareClient.GetByteArrayAsync(uri);
            var token = _cts.Token;
            while (!task.IsCompleted)
            {
                token.ThrowIfCancellationRequested();
                await UniTask.Yield();
            }
            if (task.IsCanceled)
            {
                throw new Exception("Download failed");
            }
            File.WriteAllBytes(savePath, task.Result);
            return;
        }
    }
}