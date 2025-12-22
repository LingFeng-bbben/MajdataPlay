using Cysharp.Threading.Tasks;
using MajdataPlay.Drawing;
using SkiaSharp;
using SkiaSharp.Unity;
using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Utils
{
    public static class SpriteLoader
    {
        public static Sprite EmptySprite
        {
            get
            {
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            }
        }

        //readonly static Sprite _emptySprite = Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
        public static Sprite Load(string path)
        {
            if (!File.Exists(path))
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            var bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(0, 0);
            texture.LoadImage(bytes);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public static async Task<Sprite> LoadAsync(string path, CancellationToken ct = default)
        {
            try
            {
                await UniTask.SwitchToThreadPool();
                if (!File.Exists(path))
                {
                    await UniTask.SwitchToMainThread();
                    return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
                }
                var bytes = await File.ReadAllBytesAsync(path, ct);
                ct.ThrowIfCancellationRequested();
                var texture = await ImageDecodeAsync(bytes);
                await UniTask.SwitchToMainThread();
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            finally
            {
                await UniTask.SwitchToThreadPool();
            }
        }
        public static async Task<Sprite> LoadAsync(string path, Vector4 border, CancellationToken ct = default)
        {
            try
            {
                await UniTask.SwitchToThreadPool();
                if (!File.Exists(path))
                {
                    await UniTask.SwitchToMainThread();
                    return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
                }
                var bytes = await File.ReadAllBytesAsync(path, ct);
                ct.ThrowIfCancellationRequested();
                var texture = await ImageDecodeAsync(bytes);
                await UniTask.SwitchToMainThread();
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 1,
                    SpriteMeshType.FullRect, border);
            }
            finally
            {
                await UniTask.SwitchToThreadPool();
            }
        }
        public static async Task<Sprite> LoadAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default)
        {
            try
            {
                await UniTask.SwitchToThreadPool();
                ct.ThrowIfCancellationRequested();
                var texture = await ImageDecodeAsync(buffer);
                await UniTask.SwitchToMainThread();
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            catch
            {
                await UniTask.SwitchToMainThread();
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            }
            finally
            {
                await UniTask.SwitchToThreadPool();
            }
        }

        public static async Task<Sprite> LoadAsync(Uri uri, CancellationToken ct = default)
        {
            try
            {
                await UniTask.SwitchToThreadPool();
                Directory.CreateDirectory(MajEnv.CachePath);
                var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(uri.OriginalString));
                var cachefile = Path.Combine(MajEnv.CachePath, b64);
                byte[] bytes = Array.Empty<byte>();
                if (!File.Exists(cachefile))
                {
                    var client = MajEnv.SharedHttpClient;
                    for (int i = 0; i < MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                    {
                        try
                        {
                            bytes = await client.GetByteArrayAsync(uri);
                            break;
                        }
                        catch (Exception e)
                        {
                            await Task.Delay(500);
                        }
                    }

                    await File.WriteAllBytesAsync(cachefile, bytes, ct);
                }
                else
                {
                    MajDebug.LogInfo("Local Cache Hit");
                    bytes = await File.ReadAllBytesAsync(cachefile, ct);
                }
                ct.ThrowIfCancellationRequested();
                var texture = await ImageDecodeAsync(bytes);
                await UniTask.SwitchToMainThread();
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            finally
            {
                await UniTask.SwitchToThreadPool();
            }
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
        async static UniTask<Texture2D> ImageDecodeAsync(ReadOnlyMemory<byte> data)
        {
            using var bitmap = await Task.Run(() =>
            {
                return SKBitmap.Decode(data.Span);
            });
            return await bitmap.ToTexture2DAsync();
        }
    }
}