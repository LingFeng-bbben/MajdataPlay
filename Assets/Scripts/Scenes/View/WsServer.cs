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
        protected override async void OnMessage(MessageEventArgs e)
        {
            try
            {
                var _viewManager = Majdata<ViewManager>.Instance!;
                var req = JsonSerializer.Deserialize<MajWsRequestBase>(e.Data);
                if (req is null) { Send(Response(MajWsResponseType.Error, "Wrong Format")); ; return; }
                var payloadjson = req.requestData.ToString();
                switch (req.requestType)
                {
                    case MajWsRequestType.Load:
                        
                        var payload = JsonSerializer.Deserialize<MajWsRequestLoad>(payloadjson);
                        if (payload is null) { Send(Response(MajWsResponseType.Error, "Wrong Fromat")); ; return; }
                        //TODO: Check Exist
                        var trackbyte = await File.ReadAllBytesAsync(payload.TrackPath);
                        var bgbyte = await File.ReadAllBytesAsync(payload.ImagePath);
                        var videobyte = await File.ReadAllBytesAsync(payload.VideoPath);
                        await _viewManager.LoadAssests(trackbyte, bgbyte, videobyte);
                        Send(Response());
                        break;
                    case MajWsRequestType.Play:
                        var payload1 = JsonSerializer.Deserialize<MajWsRequestPlay>(payloadjson);
                        if (payload1 is null) { Send(Response(MajWsResponseType.Error, "Wrong Fromat")); ; return; }
                        //we need offset here
                        await _viewManager.ParseAndLoadChartAsync(payload1.StartAt, payload1.SimaiFumen);
                        await _viewManager.PlayAsync();
                        Send(Response(MajWsResponseType.PlayStarted));
                        break;
                    case MajWsRequestType.Pause:
                        await _viewManager.PauseAsync();
                        Send(Response());
                        break;
                    case MajWsRequestType.Stop:
                        await _viewManager.StopAsync();
                        Send(Response());
                        break;
                    //TODO: Status
                    default:
                        Send(Response(MajWsResponseType.Error,"Not Supported"));
                        break;
                }
            }catch(Exception ex)
            {
                Send(Response(MajWsResponseType.Error, ex.ToString()));
                MajDebug.LogException(ex);
            }
        }

        string Response(MajWsResponseType type = MajWsResponseType.Ok, string message = "") {
            var resp = JsonSerializer.Serialize<MajWsResponseBase>(
                new MajWsResponseBase()
                {
                    responseType = type,
                    responseData = message
                });
            return resp;
        }
    }

}
