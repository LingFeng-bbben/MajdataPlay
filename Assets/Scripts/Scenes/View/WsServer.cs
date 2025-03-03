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
using System.Net.WebSockets;
using System.Text;

#nullable enable
namespace MajdataPlay.View
{
    internal class WsServer: MajComponent
    {
        WebSocketServer _webSocket;
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
            _webSocket = new WebSocketServer("ws://127.0.0.1:8083");
            _webSocket.AddWebSocketService<MajdataWsService>("/majdata");
            _webSocket.Start();
        }

        void OnDestroy()
        {
            _webSocket.Stop();
        }
    }
    public class MajdataWsService : WebSocketBehavior
    {
        ViewManager _viewManager = Majdata<ViewManager>.Instance!;

        readonly CancellationTokenSource _cts = new();
        readonly static JsonSerializerOptions JSON_READER_OPTIONS = new()
        {
            Converters =
            {
                new JsonStringEnumConverter()
            },
        };
        public MajdataWsService()
        {
            Task.Factory.StartNew(() =>
            {
                var stream = new MemoryStream();
                while (true)
                {
                    _cts.Cancel();
                    try
                    {
                        if (Sessions is null)
                            continue;
                        var rsp = new MajWsResponseBase()
                        {
                            responseType = MajWsResponseType.Ok,
                            responseData = ViewManager.Summary
                        };
                        stream.SetLength(0);
                        JsonSerializer.Serialize(stream, rsp, JSON_READER_OPTIONS);
                        stream.Position = 0;
                        Sessions.Broadcast(stream, (int)stream.Length);
                    }
                    catch (Exception e)
                    {
                        MajDebug.LogException(e);
                    }
                    finally
                    {
                        Thread.Sleep(16);
                    }
                }

            }, TaskCreationOptions.LongRunning);
        }
        ~MajdataWsService()
        {
            _cts.Cancel();
        }
        protected override async void OnMessage(MessageEventArgs e)
        {
            try
            {
                if(!Serializer.Json.TryDeserialize<MajWsRequestBase?>(e.Data,out var r, JSON_READER_OPTIONS) || 
                    r is null)
                {
                    Error("Wrong Fromat");
                    return;
                }
                var req = (MajWsRequestBase)r; 
                var payloadjson = req.requestData?.ToString() ?? string.Empty;
                switch (req.requestType)
                {
                    case MajWsRequestType.Load:
                        {
                            var isValid = Serializer.Json.TryDeserialize<MajWsRequestLoad?>(payloadjson, out var p) |
                                          Serializer.Json.TryDeserialize<MajWsRequestLoadBinary?>(payloadjson, out var pBinary);
                            isValid = isValid && (p is not null || pBinary is not null);
                            if (!isValid) 
                            {
                                Error("Wrong Fromat");
                                return; 
                            }
                            if(p is not null)
                            {
                                var payload = (MajWsRequestLoad)p;
                                await _viewManager.LoadAssests(payload.TrackPath, payload.ImagePath, payload.VideoPath);
                                Response();
                            }
                            else if(pBinary is not null)
                            {
                                var payload = (MajWsRequestLoadBinary)pBinary;
                                await _viewManager.LoadAssests(payload.Track, payload.Image, payload.Video);
                                Response();
                            }
                        }
                        break;
                    case MajWsRequestType.Play:
                        {
                            if (!Serializer.Json.TryDeserialize<MajWsRequestPlay?>(payloadjson, out var p) || p is null)
                            {
                                Error("Wrong Fromat");
                                return;
                            }
                            var payload = (MajWsRequestPlay)p;
                            _viewManager.Offset = (float)payload.Offset;
                            Response();
                            await _viewManager.ParseAndLoadChartAsync(payload.StartAt, payload.SimaiFumen);
                            Response();
                            await _viewManager.PlayAsync();
                            Response(MajWsResponseType.PlayStarted);
                        }
                        break;
                    case MajWsRequestType.Resume:
                        {
                            await _viewManager.PlayAsync();
                            Response(MajWsResponseType.PlayResumed);
                        }
                        break;
                    case MajWsRequestType.Pause:
                        {
                            await _viewManager.PauseAsync();
                            Response();
                        }
                        break;
                    case MajWsRequestType.Stop:
                        {
                            await _viewManager.StopAsync();
                            Response();
                        }
                        break;
                    //TODO: Status
                    case MajWsRequestType.State:
                        {
                            Response(MajWsResponseType.Ok, ViewManager.Summary);
                        }
                        break;
                    default:
                        Error("Not Supported");
                        break;
                }
            }
            catch(Exception ex)
            {
                Error(ex);
                MajDebug.LogException(ex);
            }
        }
        void Error<T>(T exception) where T : Exception
        {
            Response(MajWsResponseType.Error, exception.ToString());
        }
        void Error(string errMsg)
        {
            Response(MajWsResponseType.Error, errMsg);
        }
        void Response(MajWsResponseType type = MajWsResponseType.Ok, object? data = null) 
        {
            var rsp = new MajWsResponseBase()
            {
                responseType = type,
                responseData = data
            };
            var json = JsonSerializer.Serialize( rsp, JSON_READER_OPTIONS);
            Send(json);
        }
    }
}
