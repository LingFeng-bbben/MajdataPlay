using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO.Pipes;
using MajdataPlay.Utils;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using MajdataPlay.View.Types;
using System.Diagnostics;
using System.Text.Json.Serialization;

#nullable enable
namespace MajdataPlay.View
{
    internal class WsServer: MajComponent
    {
        WebSocketServer webSocket;
        int _httpPort = 8013;
        readonly CancellationTokenSource _cts = new();
        ViewManager _viewManager;
        protected override void Awake()
        {
            base.Awake();
            Majdata<WsServer>.Instance = this;
            DontDestroyOnLoad(GameObject);
        }

        void Start()
        {
            _viewManager = Majdata<ViewManager>.Instance!;
            webSocket = new WebSocketServer("ws://127.0.0.1:8083");
            webSocket.AddWebSocketService<MajdataWsService>("/majdata");
            webSocket.Start();
        }

        void OnDestroy()
        {
            webSocket.Stop();
        }
    }

    public class MajdataWsService : WebSocketBehavior
    {
        ViewManager _viewManager = Majdata<ViewManager>.Instance!;
        readonly static JsonSerializerOptions JSON_READER_OPTIONS = new()
        {
            Converters =
            {
                new JsonStringEnumConverter()
            },
        };
        protected override async void OnMessage(MessageEventArgs e)
        {
            try
            {
                
                if(!Serializer.Json.TryDeserialize<MajWsRequestBase?>(e.Data,out var r, JSON_READER_OPTIONS) || 
                    r is null)
                {
                    await ErrorAsync("Wrong Fromat");
                    return;
                }
                var req = (MajWsRequestBase)r; 
                var payloadjson = req.requestData?.ToString() ?? string.Empty;
                switch (req.requestType)
                {
                    case MajWsRequestType.Load:
                        {
                            if (!Serializer.Json.TryDeserialize<MajWsRequestLoad?>(payloadjson,out var p) || p is null) 
                            {
                                await ErrorAsync("Wrong Fromat");
                                return; 
                            }
                            var payload = (MajWsRequestLoad)p;
                            //TODO: Check Exist
                            var trackbyte = await File.ReadAllBytesAsync(payload.TrackPath);
                            var bgbyte = await File.ReadAllBytesAsync(payload.ImagePath);
                            var videobyte = await File.ReadAllBytesAsync(payload.VideoPath);
                            await _viewManager.LoadAssests(trackbyte, bgbyte, videobyte);
                            await ResponseAsync();
                        }
                        break;
                    case MajWsRequestType.Play:
                        {
                            if (!Serializer.Json.TryDeserialize<MajWsRequestPlay?>(payloadjson, out var p) || p is null)
                            {
                                await ErrorAsync("Wrong Fromat");
                                return;
                            }
                            var payload = (MajWsRequestPlay)p;
                            //we need offset here
                            await _viewManager.ParseAndLoadChartAsync(payload.StartAt, payload.SimaiFumen);
                            await _viewManager.PlayAsync();
                            await ResponseAsync(MajWsResponseType.PlayStarted);
                        }
                        break;
                    case MajWsRequestType.Pause:
                        {
                            await _viewManager.PauseAsync();
                            await ResponseAsync();
                        }
                        break;
                    case MajWsRequestType.Stop:
                        {
                            await _viewManager.StopAsync();
                            await ResponseAsync();
                        }
                        break;
                    //TODO: Status
                    case MajWsRequestType.Status:
                        {
                            await ResponseAsync(MajWsResponseType.Ok, ViewManager.Summary);
                        }
                        break;
                    default:
                        await ErrorAsync("Not Supported");
                        break;
                }
            }
            catch(Exception ex)
            {
                await ErrorAsync(ex);
                MajDebug.LogException(ex);
            }
        }
        async Task ErrorAsync<T>(T exception) where T : Exception
        {
            await ResponseAsync(MajWsResponseType.Error, exception.ToString());
        }
        async Task ErrorAsync(string errMsg)
        {
            await ResponseAsync(MajWsResponseType.Error, errMsg);
        }
        async Task ResponseAsync(MajWsResponseType type = MajWsResponseType.Ok, object? data = null) 
        {
            var stream = new MemoryStream();
            var rsp = new MajWsResponseBase()
            {
                responseType = type,
                responseData = data
            };
            await JsonSerializer.SerializeAsync(stream,rsp,JSON_READER_OPTIONS);
            SendAsync(stream, (int)stream.Length, null);
        }
    }

}
