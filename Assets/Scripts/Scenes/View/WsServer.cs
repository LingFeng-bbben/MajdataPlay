using MajdataPlay.Scenes.View.Types;
using MajdataPlay.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

#nullable enable
namespace MajdataPlay.Scenes.View
{
    internal class WsServer: MajComponent
    {
        WebSocketServer _webSocket;
        int _httpPort = 8013;
        readonly CancellationTokenSource _cts = new();
        ViewManager _viewManager;
        protected override void Awake()
        {
            if (Majdata<WsServer>.Instance is not null) { 
                Destroy(this.gameObject);
                return;
            }
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
            if (_webSocket is not null)
            {
                _webSocket.RemoveWebSocketService("/majdata");
                _webSocket.Stop();
            }
        }
    }
    public class MajdataWsService : WebSocketBehavior, IDisposable
    {
        ViewManager? _viewManager => Majdata<ViewManager>.Instance;

        readonly CancellationTokenSource _cts = new();
        readonly static JsonSerializerSettings JSON_READER_OPTIONS = new()
        {
            Converters =
            {
                new StringEnumConverter()
            }
        };
        public MajdataWsService()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    _cts.Cancel();
                    try
                    {
                        if (Sessions is null)
                            continue;
                        string json = GetSummaryJson();
                        Sessions.Broadcast(json);
                    }
                    catch (Exception e)
                    {
                        if (e is InvalidOperationException)
                        {
                            _cts.Cancel();
                            break;
                        }
                        MajDebug.LogException(e);
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }

            }, TaskCreationOptions.LongRunning);
        }

        private static string GetSummaryJson()
        {
            var rsp = new MajWsResponseBase()
            {
                responseType = MajWsResponseType.Heartbeat,
                responseData = ViewManager.Summary
            };
            var json = Serializer.Json.Serialize(rsp, JSON_READER_OPTIONS);
            return json;
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
        protected override async void OnMessage(MessageEventArgs e)
        {
            if (_viewManager is null) return;
            try
            {
                var json = string.Empty;
                if (e.IsText)
                {
                    json = e.Data;
                }
                else if(e.IsBinary)
                {
                    json = Encoding.UTF8.GetString(e.RawData);
                }
                else
                {
                    return;
                }
                if(!Serializer.Json.TryDeserialize<MajWsRequestBase?>(json,out var r, JSON_READER_OPTIONS) || 
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
                                Response(MajWsResponseType.LoadOk, ViewManager.Summary);
                            }
                            /*else if(pBinary is not null)
                            {
                                var payload = (MajWsRequestLoadBinary)pBinary;
                                await _viewManager.LoadAssests(payload.Track, payload.Image, payload.Video);
                                Response(MajWsResponseType.Ok, ViewManager.Summary);
                            }*/
                        }
                        break;
/*                    case MajWsRequestType.Parse:
                        {
                            if (!Serializer.Json.TryDeserialize<MajWsRequestParse?>(payloadjson, out var p) || p is null)
                            {
                                Error("Wrong Fromat");
                                return;
                            }
                            var payload = (MajWsRequestParse)p;
                            _viewManager.Offset = (float)payload.Offset;
                            await _viewManager.ParseAndLoadChartAsync(payload.StartAt, payload.SimaiFumen);
                            Response(MajWsResponseType.Ok, ViewManager.Summary);
                        }
                        break;*/
                    case MajWsRequestType.Play:
                        {
                            if (!Serializer.Json.TryDeserialize<MajWsRequestPlay?>(payloadjson, out var p) || p is null)
                            {
                                Error("Wrong Fromat");
                                return;
                            }
                            var payload = (MajWsRequestPlay)p;
                            _viewManager.Offset = (float)payload.Offset;
                            await _viewManager.ParseAndLoadChartAsync(payload.StartAt, payload.SimaiFumen);
                            await _viewManager.PlayAsync(payload.Speed);
                            Response(MajWsResponseType.PlayStarted, ViewManager.Summary);
                        }
                        break;
                    case MajWsRequestType.Resume:
                        {
                            await _viewManager.PlayAsync();
                            Response(MajWsResponseType.PlayResumed, ViewManager.Summary);
                        }
                        break;
                    case MajWsRequestType.Pause:
                        {
                            await _viewManager.PauseAsync();
                            Response(MajWsResponseType.PlayPaused, ViewManager.Summary);
                        }
                        break;
                    case MajWsRequestType.Stop:
                        {
                            await _viewManager.StopAsync();
                            Response(MajWsResponseType.PlayStopped, ViewManager.Summary);
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
            var json = Serializer.Json.Serialize(rsp, JSON_READER_OPTIONS);
            Send(json);
        }
    }
}
