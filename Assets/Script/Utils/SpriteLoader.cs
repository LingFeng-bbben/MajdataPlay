using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Utils
{
    public static class SpriteLoader
    {
        public static Sprite Load(string path)
        {
            if (!File.Exists(path))
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            var bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(0, 0);
            texture.LoadImage(bytes);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public static async Task<Sprite> LoadAsync(string path)
        {
            if (!File.Exists(path))
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            var bytes = await File.ReadAllBytesAsync(path);
            var texture = new Texture2D(0, 0);
            texture.LoadImage(bytes);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public static async Task<Sprite> LoadOnlineAsync(string Url)
        {
            Directory.CreateDirectory(GameManager.CachePath);
            var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(Url));
            var cachefile = GameManager.CachePath + "/" + b64;
            byte[] bytes;
            if (!File.Exists(cachefile))
            {
                var client = new HttpClient(new HttpClientHandler() { Proxy = WebRequest.GetSystemWebProxy(), UseProxy = true });
                bytes = await client.GetByteArrayAsync(Url);
                await File.WriteAllBytesAsync(cachefile, bytes);
            }
            else
            {
                Debug.Log("Local Cache Hit");
                bytes = await File.ReadAllBytesAsync(cachefile);
            }
            var texture = new Texture2D(0, 0);
            texture.LoadImage(bytes);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public static Sprite Load(string path, Vector4 border)
        {
            if (!File.Exists(path))
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            var bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(0, 0);
            texture.LoadImage(bytes);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 1,
                SpriteMeshType.FullRect, border);
        }
    }
}