using MajdataPlay.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
    public string Hash { get; set; }

    
    public SongDetail(string maidataPath,string trackPath)
    {
        var maibyte = File.ReadAllBytes(maidataPath);
        this.Hash = GetHash(maibyte);
        this.TrackPath = trackPath;
        this.MaidataPath = maidataPath;
        var maidata = Encoding.UTF8.GetString(maibyte).Split('\n');
        for (int i = 0; i < maidata.Length; i++)
        {
            if (maidata[i].StartsWith("&title="))
                this.Title = GetValue(maidata[i]);
            else if (maidata[i].StartsWith("&artist="))
                this.Artist = GetValue(maidata[i]);
            else if (maidata[i].StartsWith("&des="))
            {
                for(int k=0;k<this.Designers.Length;k++)
                {
                    this.Designers[k] = GetValue(maidata[i]);
                }
            }
            else if (maidata[i].StartsWith("&freemsg="))
                this.Description = GetValue(maidata[i]);
            else if (maidata[i].StartsWith("&clock_count="))
                this.ClockCount = int.Parse(GetValue(maidata[i]));
            else if (maidata[i].StartsWith("&first="))
                this.First = double.Parse(GetValue(maidata[i]));
            else if (maidata[i].StartsWith("&lv_") ||  maidata[i].StartsWith("&des_"))
            {
                for (int j = 1; j < 8 && i < maidata.Length; j++)
                {
                    if (maidata[i].StartsWith("&lv_" + j + "="))
                        this.Levels[j - 1] = GetValue(maidata[i]);
                    else if (maidata[i].StartsWith("&des_" + j + "="))
                        this.Designers[j - 1] = GetValue(maidata[i]);
                }
            }
        }
    }
    static private string? GetValue(string varline)
    {
        try
        {
            return varline.Substring(varline.FindIndex(o=>o=='=')+1).Replace("\r", "");
        }
        catch { return null; }
    }
    private static string GetHash(byte[] file)
    {
        var hashComputer = MD5.Create();
        var chartHash = hashComputer.ComputeHash(file);

        return Convert.ToBase64String(chartHash);
    }

    public string LoadInnerMaidata(int selectedDifficulty)
    {
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
        return "";
    }
}

