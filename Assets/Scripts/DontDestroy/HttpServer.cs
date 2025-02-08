using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO.Pipes;

namespace MajdataPlay
{
    internal class HttpServer: MajComponent
    {
        HttpListener _httpServer = new();
        int _httpPort = 8013;
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(GameObject);
            var rd = new Random();
            while (IsPortInUse(_httpPort))
            {
                _httpPort = rd.Next(1000, 65535);
            }
            _httpServer.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _httpServer.Prefixes.Add($"http://localhost:{_httpPort}/");
            _httpServer.Start();
            StartToListenHttpRequest();
            StartToListenPipe();
        }
        async void StartToListenHttpRequest()
        {
            await Task.Run(async () =>
            {
                while (_httpServer.IsListening)
                {
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
                    catch(JsonException)
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
        async void StartToListenPipe()
        {
            while (true)
            {
                var pipeServer = new NamedPipeServerStream("MajdataPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await pipeServer.WaitForConnectionAsync();
                PipeRequestHandleAsync(pipeServer);
            }
        }
        async void PipeRequestHandleAsync(NamedPipeServerStream pipeStream)
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
    }
}
