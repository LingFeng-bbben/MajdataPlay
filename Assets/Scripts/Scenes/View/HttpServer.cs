using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO.Pipes;
using MajdataPlay.Utils;
using System.Threading;
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
        async void StartToListenHttpRequest(CancellationToken token = default)
        {
            await Task.Run(async () =>
            {
                while (_httpServer.IsListening)
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        var context = await _httpServer.GetContextAsync();
                        var req = context.Request;
                        using var rsp = context.Response;
                        var httpMethod = req.HttpMethod;

                        try
                        {
                            using var reqReader = new StreamReader(req.InputStream);
                            using var rspSender = new StreamWriter(rsp.OutputStream);
                            var data = reqReader.ReadToEnd();

                            rsp.StatusCode = 200;
                            await rspSender.WriteLineAsync("Hello!!!");
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
                    catch(OperationCanceledException)
                    {
                        _httpServer.Close();
                        throw;
                    }
                }
            });
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
        private void Update()
        {
            if (MajEnv.Mode == RunningMode.Play)
                Destroy(GameObject);
        }
        void OnDestroy()
        {
            _cts.Cancel();
        }
    }
}
