using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SongLoader : MonoBehaviour
{
    readonly static string Songdir = Application.dataPath + "/Songs/";
    public static List<SongDetail> ScanMusic()
    {
        List<SongDetail> songList = new List<SongDetail>();
        var dirs = new DirectoryInfo(Songdir).GetDirectories();
        foreach (var dir in dirs)
        {
            var files = dir.GetFiles();
            var maidatafile = files.First(o => o.Name == "maidata.txt");
            SongDetail song = new SongDetail();
            if (maidatafile != null)
            {
                var txtcontent = File.ReadAllText(maidatafile.FullName);
                song = SongDetail.LoadFromMaidata(txtcontent);
            }
            var coverfile = files.First(o => o.Name == "bg.png" || o.Name == "bg.jpg");
            if (coverfile != null)
            {
                song.SongCover = LoadSpriteFromFile(coverfile.FullName);
            }
            var videofile = files.First(o => o.Name == "bg.mp4" || o.Name == "pv.mp4" || o.Name == "mv.mp4");
            if (videofile != null)
            {
                song.VideoPath = videofile.FullName;
            }
            var trackfile = files.First(o => o.Name == "track.mp3" || o.Name == "track.ogg");
            if (trackfile != null)
            {
                song.TrackPath = trackfile.FullName;
            }

            songList.Add(song);
        }
        return songList;
    }

    static Sprite LoadSpriteFromFile(string FilePath)
    {
        Texture2D SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.FullRect);

        return NewSprite;
    }

    static Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }
}
