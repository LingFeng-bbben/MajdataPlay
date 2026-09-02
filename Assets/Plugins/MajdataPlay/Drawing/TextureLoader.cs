using Cysharp.Threading.Tasks;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Drawing
{
    public static class TextureLoader
    {
        public static Texture2D LoadFromMemory(ReadOnlySpan<byte> data, bool markNonReadable)
        {
            var bitmap = SKBitmap.Decode(data);

            return bitmap.ToTexture2D(markNonReadable);
        }
        public static async Task<Texture2D> LoadFromMemoryAsync(ReadOnlyMemory<byte> dataMemory, bool markNonReadable)
        {
            await UniTask.SwitchToThreadPool();
            var bitmap = SKBitmap.Decode(dataMemory.Span);

            await UniTask.SwitchToMainThread();
            var texture = bitmap.ToTexture2D(markNonReadable);
            return texture;
        }        
    }
}
