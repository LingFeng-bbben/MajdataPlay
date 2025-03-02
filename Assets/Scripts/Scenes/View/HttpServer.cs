using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO.Pipes;
using MajdataPlay.Utils;
using System.Threading;
using System.Net.Http;
using System.Text;

#nullable enable
namespace MajdataPlay.View
{
    internal class HttpServer: MajComponent
    {
        HttpListener _httpServer = new();
        int _httpPort = 8013;
        readonly CancellationTokenSource _cts = new();
        ViewManager _viewManager;
        protected override void Awake()
        {
            base.Awake();
            Majdata<HttpServer>.Instance = this;
            DontDestroyOnLoad(GameObject);
            var rd = new Random();
            while (IsPortInUse(_httpPort))
            {
                _httpPort = rd.Next(1000, 65535);
            }
            _httpServer.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _httpServer.Prefixes.Add($"http://localhost:{_httpPort}/");
            _httpServer.Start();
            StartToListenHttpRequest(_cts.Token);
            StartToListenPipe(_cts.Token);
        }
        void Start()
        {
            _viewManager = Majdata<ViewManager>.Instance!;
        }
        void StartToListenHttpRequest(CancellationToken token = default)
        {
            Task.Factory.StartNew(() =>
            {
                while (_httpServer.IsListening)
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        var context = _httpServer.GetContext();

                        HttpRequestHandleAsync(context.Request, context.Response);
                    }
                    catch(OperationCanceledException)
                    {
                        _httpServer.Close();
                        throw;
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
        void HttpRequestHandleAsync(HttpListenerRequest req,HttpListenerResponse rsp)
        {
            Task.Run(async () =>
            {
                using (rsp)
                {
                    try
                    {
                        var httpMethod = req.HttpMethod;
                        switch (httpMethod)
                        {
                            case "GET":
                                await GetRequestHandleAsync(req, rsp);
                                break;
                            case "POST":
                                await PostRequestHandleAsync(req, rsp);
                                break;
                            default:
                                rsp.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                                break;
                        }
                    }
                    catch (JsonException)
                    {
                        rsp.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                    catch
                    {
                        rsp.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }
            });
        }
        async Task GetRequestHandleAsync(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            switch (req.RawUrl)
            {
                case "/api/timestramp":
                case "/api/state":
                    await Serializer.Json.SerializeAsync(rsp.OutputStream, ViewManager.Summary);
                    rsp.StatusCode = 200;
                    break;
                case "/api/pause":
                    await _viewManager.PauseAsync();
                    rsp.StatusCode = 200;
                    break;
                case "/api/resume":
                    await _viewManager.PlayAsync();
                    rsp.StatusCode = 200;
                    break;
                case "/api/stop":
                    await _viewManager.StopAsync();
                    rsp.StatusCode = 200;
                    break;
                case "/api/reset":
                    await _viewManager.ResetAsync();
                    rsp.StatusCode = 200;
                    break;
                default:
                    using (var writer = new StreamWriter(rsp.OutputStream))
                    {
                        await writer.WriteLineAsync("Hello!!!");
                    }
                    rsp.StatusCode = 200;
                    break;
            }
        }
        async Task PostRequestHandleAsync(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            switch (req.RawUrl)
            {
                case "/api/play":
                case "/api/maidata":
                case "/api/load":
                    break;
                default:
                    var str = await new StreamReader(req.InputStream).ReadToEndAsync();
                    using (var writer = new StreamWriter(rsp.OutputStream))
                    {
                        await writer.WriteLineAsync("Hello!!!");
                    }
                    rsp.StatusCode = 200;
                    break;
            }
        }

        async Task LoadRequestHandleAsync(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            using var reader = new StreamReader(req.InputStream);
            var content = await reader.ReadToEndAsync();
        }
        async void StartToListenPipe(CancellationToken token = default)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                var pipeServer = new NamedPipeServerStream("MajdataPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await pipeServer.WaitForConnectionAsync();
                PipeRequestHandleAsync(pipeServer, token);
            }
        }
        async void PipeRequestHandleAsync(NamedPipeServerStream pipeStream, CancellationToken token = default)
        {
            await Task.Run(async () =>
            {
                using (pipeStream)
                {
                    using var reader = new StreamReader(pipeStream);
                    using var writer = new StreamWriter(pipeStream);
                    writer.AutoFlush = true;
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        var req = (await reader.ReadLineAsync()).Split(";");
                        if (req.Length == 0 || req[0] != "MajdataHello")
                            continue;
                        writer.WriteLine(_httpPort);
                    }
                }
            });
        }
        static bool IsPortInUse(int port)
        {
            try
            {
                var tester = new TcpListener(IPAddress.Any, port);
                tester.Start();
                tester.Stop();
                return false;
            }
            catch
            {
                return true;
            }
        }
        void OnDestroy()
        {
            _cts.Cancel();
        }
    }
}
