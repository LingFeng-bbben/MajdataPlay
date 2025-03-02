using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace MajdataPlay.View.Services
{
    internal sealed class PlayerController: WebSocketBehavior
    {
        protected override async void OnMessage(MessageEventArgs e)
        {
            var reqPath = Context.RequestUri.LocalPath;
            switch (reqPath)
            {
                case var _ when reqPath.StartsWith("/api/play"):
                    break;
                case var _ when reqPath.StartsWith("/api/pause"):
                    break;
                case var _ when reqPath.StartsWith("/api/resume"):
                    break;
                case var _ when reqPath.StartsWith("/api/stop"):
                    break;
                case var _ when reqPath.StartsWith("/api/timeline"):
                case var _ when reqPath.StartsWith("/api/state"):
                    {
                        var stream = new MemoryStream();
                        await Serializer.Json.SerializeAsync(stream, ViewManager.Summary);
                        SendAsync(stream, (int)stream.Length, null);
                    }
                    break;
            }
        }
    }
}
