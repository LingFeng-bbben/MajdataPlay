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
using Unity.Collections;
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

        public static Sprite LoadFromFile(string filePath, bool markNonReadable = true)
        {
            return LoadFromFileWithBorder(filePath, Vector4.zero, markNonReadable);
        }
        public static Sprite LoadFromFileWithBorder(string filePath, Vector4 border, bool markNonReadable = true)
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            }
            try
            {
                using var buffer = new NativeArray<byte>((int)fileInfo.Length, Allocator.Temp);
                using var fileStream = fileInfo.OpenRead();
                fileStream.Read(buffer.AsSpan());

                return LoadFromMemoryWithBorder(buffer.AsReadOnlySpan(), border, markNonReadable);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load sprite from file: {filePath}\nException: {e}");
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            }
        }
        public static Sprite LoadFromMemory(ReadOnlySpan<byte> data, bool markNonReadable = true)
        {
            return LoadFromMemoryWithBorder(data, Vector4.zero, markNonReadable);
        }
        public static Sprite LoadFromMemoryWithBorder(ReadOnlySpan<byte> data, Vector4 border, bool markNonReadable = true)
        {
            try
            {
                var texture = Decode(data, markNonReadable);

                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 1,
                    SpriteMeshType.FullRect, border);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load sprite from memory\nException: {e}");
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            }
        }


        public static Task<Sprite> LoadFromFileAsync(string filePath, bool markNonReadable = true, CancellationToken token = default)
        {
            return LoadFromFileWithBorderAsync(filePath, Vector4.zero, markNonReadable, token);
        }
        public static async Task<Sprite> LoadFromFileWithBorderAsync(string filePath, 
                                                                     Vector4 border, 
                                                                     bool markNonReadable = true, 
                                                                     CancellationToken token = default)
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            }
            try
            {
                //using var buffer = new NativeArray<byte>((int)fileInfo.Length, Allocator.Temp);
                var buffer = new byte[(int)fileInfo.Length];
                using var fileStream = fileInfo.OpenRead();

                await fileStream.ReadAsync(buffer, token);

                return await LoadFromMemoryWithBorderAsync(buffer, border, markNonReadable, token);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load sprite from file: {filePath}\nException: {e}");
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            }
        }
        public static Task<Sprite> LoadFromMemoryAsync(ReadOnlyMemory<byte> buffer, 
                                                       bool markNonReadable = true, 
                                                       CancellationToken token = default)
        {
            return LoadFromMemoryWithBorderAsync(buffer, Vector4.zero, markNonReadable, token);
        }
        public static async Task<Sprite> LoadFromMemoryWithBorderAsync(ReadOnlyMemory<byte> dataMemory, 
                                                                       Vector4 border, 
                                                                       bool markNonReadable = true, 
                                                                       CancellationToken token = default)
        {
            try
            {
                var texture = await DecodeAsync(dataMemory, markNonReadable);

                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 1,
                    SpriteMeshType.FullRect, border);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load sprite from memory\nException: {e}");
                return Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));
            }
        }





        async static Task<Texture2D> DecodeAsync(ReadOnlyMemory<byte> dataMemory, bool markNonReadable)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                var bitmap = SKBitmap.Decode(dataMemory.Span);

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