using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
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
        public bool isOnline { get; set; } = false;

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
                        Debug.LogError("Failed to Load " + maidataPath + "at line " + i);
                        Debug.LogError(ex.Message);
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
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(song.Timestamp).ToLocalTime();
            var apiroot = api.Url;
            var songDetail = new SongDetail()
            {
                Title = song.Title,
                Artist = song.Artist,
                Levels = song.Levels.ToArray(),
                isOnline = true,
                MaidataPath = apiroot + "/Maidata/" + song.Id,
                TrackPath = apiroot + "/Track/" + song.Id,
                BGPath = apiroot + "/ImageFull/" + song.Id,
                VideoPath = apiroot + "/Video/" + song.Id,
                CoverPath = apiroot + "/Image/" + song.Id,
                Hash = song.Id.ToString() + "-" + song.Timestamp,
                ApiEndpoint = api,
                OnlineId = song.Id,
                AddTime = dateTime
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

        public string LoadInnerMaidata(int selectedDifficulty)
        {
            //TODO: should check hash change here
            var maidata = File.ReadAllLines(MaidataPath);
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

        public async Task<Sprite> GetSpriteAsync()
        {
            if (SongCover != null)
            {
                Debug.Log("Memory Cache Hit");
                return SongCover;
            }
            if (string.IsNullOrEmpty(CoverPath))
            {
                SongCover = await SpriteLoader.LoadAsync(Application.streamingAssetsPath + "/dummy.jpg");
                return SongCover;
            }
            if (isOnline)
            {
                Debug.Log("Try load cover online" + CoverPath);
                SongCover = await SpriteLoader.LoadOnlineAsync(CoverPath);
                return SongCover;
            }
            else
            {
                SongCover = await SpriteLoader.LoadAsync(CoverPath);
                return SongCover;
            }

        }

        //TODO: add callback for progress
        public async Task<SongDetail> DumpToLocal()
        {
            if (!isOnline) return this;
            var dir = GameManager.ChartPath + "/MajnetPlayed/" + Hash;
            Directory.CreateDirectory(dir);
            var client = new HttpClient(new HttpClientHandler() { Proxy = WebRequest.GetSystemWebProxy(), UseProxy = true });

            var trackp = dir + "/track.mp3";
            if (!File.Exists(trackp))
            {
                var track = await client.GetByteArrayAsync(TrackPath);
                File.WriteAllBytes(trackp, track);
            }

            var maidatap = dir + "/maidata.txt";
            if (!File.Exists(maidatap))
            {
                var maidata = await client.GetByteArrayAsync(MaidataPath);
                File.WriteAllBytes(maidatap, maidata);
            }

            var bgp = dir + "/bg.png";
            if (!File.Exists(bgp))
            {
                var bg = await client.GetByteArrayAsync(BGPath);
                File.WriteAllBytes(bgp, bg);
            }

            var info = new DirectoryInfo(dir);
            var song = await ParseAsync(info.GetFiles());
            song.Hash = Hash;
            return song;
        }

    }
}