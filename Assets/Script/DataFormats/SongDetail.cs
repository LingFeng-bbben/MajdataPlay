using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SongDetail
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string? Designer { get; set; }
    public string? Description { get; set; }
    public string[]? Levels { get; set; } = new string[6];
    //public string? Uploader { get; set; }
    //public long? Timestamp { get; set; }

    public string?[] InnerMaidata { get; set; } = new string[6];
    public string? VideoPath { get; set; }
    public string? TrackPath {  get; set; }
    public Sprite SongCover { get; set; }

    public double First {  get; set; }


/*    public SongDetail(string _id, string _title, string _artist, string _designer, IEnumerable<string> _levels, string _description = "")
    {
        Id = _id;
        Title = _title;
        Artist = _artist;
        Designer = _designer;
        Description = _description;
        Levels = _levels.ToArray();
    }*/
    public SongDetail() { }
    public static SongDetail LoadFromMaidata(string maidatas)
    {
        var maidata = maidatas.Split('\n');
        var detail = new SongDetail();
        var levels = new string[7];
        for (int i = 0; i < maidata.Length; i++)
        {
            if (maidata[i].StartsWith("&title="))
                detail.Title = GetValue(maidata[i]);
            else if (maidata[i].StartsWith("&artist="))
                detail.Artist = GetValue(maidata[i]);
            else if (maidata[i].StartsWith("&des="))
                detail.Designer = GetValue(maidata[i]);
            else if (maidata[i].StartsWith("&freemsg="))
                detail.Description = GetValue(maidata[i]);
            else if (maidata[i].StartsWith("&first="))
                detail.First = double.Parse( GetValue(maidata[i]));
            else if (maidata[i].StartsWith("&lv_") || maidata[i].StartsWith("&inote_"))
            {
                for (int j = 1; j < 8 && i < maidata.Length; j++)
                {
                    if (maidata[i].StartsWith("&lv_" + j + "="))
                        levels[j - 1] = GetValue(maidata[i]);
                    if (maidata[i].StartsWith("&inote_" + j + "="))
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

                        detail.InnerMaidata[j - 1] = TheNote;
                    }
                }
            }
        }
        detail.Levels = levels;
        return detail;
    }
    static private string GetValue(string varline)
    {
        try
        {
            return varline.Split('=')[1].Trim().Replace("\r", "");
        }
        catch { return null; }
    }
}

