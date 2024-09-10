using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
#nullable enable
public class SongDetail
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string?[] Designers { get; set; } = new string[7];
    public string? Description { get; set; }
    public int? ClockCount { get; set; }
    public string[]? Levels { get; set; } = new string[7];
    //public string? Uploader { get; set; }
    //public long? Timestamp { get; set; }

    //public string?[] InnerMaidata { get; set; } = new string[7];
    public string? VideoPath { get; set; }
    public string? TrackPath { get; set; }
    public string? MaidataPath {  get; set; }
    public Sprite? SongCover { get; set; }
    public double First {  get; set; }
    public string Hash { get; set; } = string.Empty;

    public static async Task<SongDetail> ParseAsync(FileInfo[] files)
    {
        var maidataPath = files.FirstOrDefault(o => o.Name is "maidata.txt").Name;
        var trackPath = files.FirstOrDefault(o => o.Name is "track.mp3" or "track.ogg").Name;
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
        Sprite? songCover = null;
        
        

        var maidata = Encoding.UTF8.GetString(maibyte).Split('\n');
        await Task.Run(() =>
        {
            for (int i = 0; i < maidata.Length; i++)
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
                    first = double.Parse(GetValue(maidata[i]));
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
        });
        if (coverFile != null)
            songCover = SpriteLoader.Load(coverFile.FullName);
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
            SongCover = songCover,
            VideoPath = videoPath,
            TrackPath = trackPath,
            MaidataPath = maidataPath
        };
    }
    private static string GetValue(string varline)
    {
        try
        {
            return varline.Substring(varline.FindIndex(o=>o=='=')+1).Replace("\r", "");
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
            if (maidata[i].StartsWith("&inote_" + (selectedDifficulty+1) + "="))
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
}

