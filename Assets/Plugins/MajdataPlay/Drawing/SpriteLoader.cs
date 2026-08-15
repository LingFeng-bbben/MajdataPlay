using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.Drawing;
using SkiaSharp;
using SkiaSharp.Unity;
using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace MajdataPlay.Drawing
{
    public static class SpriteLoader
    {
        static Sprite? _emptySprite;

        public static Sprite EmptySprite
        {
            get
            {
                if (_emptySprite != null)
                {
                    return _emptySprite;
                }

                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                {
                    name = "EmptySpriteTexture",
                    hideFlags = HideFlags.HideAndDontSave
                };
                texture.SetPixel(0, 0, Color.clear);
                texture.Apply();

                _emptySprite = Sprite.Create(texture,
                                             new Rect(0, 0, 1, 1),
                                             new Vector2(0.5f, 0.5f),
                                             100,
                                             0,
                                             SpriteMeshType.FullRect);
                _emptySprite.name = "EmptySprite";
                _emptySprite.hideFlags = HideFlags.HideAndDontSave;
                return _emptySprite;
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
                return EmptySprite;
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
                return EmptySprite;
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
                //var texture = new Texture2D(0, 0);
                //texture.LoadImage(data, markNonReadable);

                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 1,
                    SpriteMeshType.FullRect, border);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load sprite from memory\nException: {e}");
                return EmptySprite;
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
                await UniTask.SwitchToMainThread();
                return EmptySprite;
            }
            try
            {
                
                var length = (int)fileInfo.Length;
                using var buffer = new NativeArray<byte>(length, Allocator.Persistent);
                var bufferMemory = buffer.AsMemory();
                using var fileStream = fileInfo.OpenRead();

                await fileStream.ReadAsync(bufferMemory, token);

                return await LoadFromMemoryWithBorderAsync(bufferMemory, border, markNonReadable, token);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load sprite from file: {filePath}\nException: {e}");
                await UniTask.SwitchToMainThread();
                return EmptySprite;
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
                await UniTask.SwitchToMainThread();

                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 1,
                    SpriteMeshType.FullRect, border);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load sprite from memory\nException: {e}");
                await UniTask.SwitchToMainThread();

                return EmptySprite;
            }
        }





        async static Task<Texture2D> DecodeAsync(ReadOnlyMemory<byte> dataMemory, bool markNonReadable)
        {
            await UniTask.SwitchToThreadPool();
            var bitmap = SKBitmap.Decode(dataMemory.Span);

            await UniTask.SwitchToMainThread();
            var texture = bitmap.ToTexture2D(markNonReadable);
            return texture;
        }
        static Texture2D Decode(ReadOnlySpan<byte> data, bool markNonReadable)
        {
            var bitmap = SKBitmap.Decode(data);
            
            return bitmap.ToTexture2D(markNonReadable);
        }
    }
}
