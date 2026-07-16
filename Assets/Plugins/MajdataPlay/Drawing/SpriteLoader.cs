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

namespace MajdataPlay.Drawing
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

        public static Sprite LoadFromFile(string path, bool markNonReadable = true)
        {
            if (!File.Exists(path))
            {
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            }
            try
            {
                var bytes = File.ReadAllBytes(path);
                var texture = Decode(bytes);

                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load sprite from path: {path}\nException: {e}");
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            }
        }
        public static Sprite LoadFromFileWithBorder(string path, Vector4 border, bool markNonReadable = true)
        {
            if (!File.Exists(path))
            {
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            }
            try
            {
                var bytes = File.ReadAllBytes(path);
                var texture = new Texture2D(0, 0);
                texture.LoadImage(bytes, markNonReadable);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 1,
                    SpriteMeshType.FullRect, border);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load sprite from path: {path}\nException: {e}");
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            }
        }


        public static async Task<Sprite> LoadFromFileAsync(string path, CancellationToken ct = default)
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
                var texture = await DecodeAsync(bytes);
                await UniTask.SwitchToMainThread();
                texture.filterMode = FilterMode.Trilinear;
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            catch (Exception e)
            {
                await UniTask.SwitchToMainThread();
                Debug.LogError($"Failed to load sprite from path: {path}\nException: {e}");
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            }
        }
        public static async Task<Sprite> LoadFromMemoryAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default)
        {
            try
            {
                await UniTask.SwitchToThreadPool();
                ct.ThrowIfCancellationRequested();
                var texture = await DecodeAsync(buffer);
                await UniTask.SwitchToMainThread();
                texture.filterMode = FilterMode.Trilinear;
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            catch (Exception e)
            {
                await UniTask.SwitchToMainThread();
                Debug.LogError($"Failed to load sprite from memory\nException: {e}");                
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            }
        }

        




        async static Task<Texture2D> DecodeAsync(ReadOnlyMemory<byte> data, bool markNonReadable)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var bitmap = SKBitmap.Decode(data.Span);

                await UniTask.SwitchToMainThread();
                var texture = bitmap.ToTexture2D(markNonReadable);
                return texture;
            }
        }
        static Texture2D Decode(ReadOnlySpan<byte> data, bool markNonReadable)
        {
            var bitmap = SKBitmap.Decode(data);
            
            return bitmap.ToTexture2D(markNonReadable);
        }
    }
}