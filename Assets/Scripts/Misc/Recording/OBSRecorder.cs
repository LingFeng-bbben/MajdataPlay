using MajdataPlay.Utils;
using System;
using System.Threading.Tasks;
using WebSocketSharp;

namespace MajdataPlay.Recording
{
    public class OBSRecorder : IRecorder
    {
        private WebSocket _webSocket = new("ws://127.0.0.1:4455");
        private bool _disposed = false;
        public bool IsConnected { get; set; } = false;
        public bool IsRecording { get; set; } = false;

        private const string StartRecordMessage = @"{
                    ""op"": 6,
                    ""d"": {
                        ""requestType"": ""StartRecord"",
                        ""requestId"": ""start_recording""
                    }
                }";

        private const string StopRecordMessage = @"{
                    ""op"": 6,
                    ""d"": {
                        ""requestType"": ""StopRecord"",
                        ""requestId"": ""stop_recording""
                    }
                }";

        private const string AuthenticateMessage = @"{
                    ""op"": 1,
                    ""d"": {
                        ""rpcVersion"": 1
                    }
                }";

        public OBSRecorder() => Init();

        public void Init()
        {
            _webSocket.OnMessage += OnMessageReceived;
            Connect();
            Authenticate();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Disconnect();
                _webSocket = null;
            }

            _disposed = true;
        }

        ~OBSRecorder() => Dispose(false);

        private void Connect() => _webSocket.Connect();

        private void Disconnect()
        {
            _webSocket.Close();
            IsConnected = false;
        }

        public void StartRecord() => _webSocket.Send(StartRecordMessage);
        public async Task StartRecordAsync()
        {
            await Task.Run(StartRecord);
        }
        public void StopRecord() => _webSocket.Send(StopRecordMessage);
        public async Task StopRecordAsync()
        {
            await Task.Run(StopRecord);
        }
        private void Authenticate() => _webSocket.Send(AuthenticateMessage);
        public void OnLateUpdate()
        {

        }
        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                var message = Serializer.Json.Deserialize<ReceivedMessage>(e.Data);
                MajDebug.Log("[OBS] Received: " + e.Data);
                MajDebug.Log(message);
                switch (message.op)
                {
                    case 2: // Identified
                        {
                            IsConnected = true;
                            break;
                        }
                    case 7: // RequestResponse
                        {
                            if (message.d.requestType == "StartRecord")
                            {
                                if (message.d.requestStatus.result)
                                {
                                    IsRecording = true;
                                }
                                else
                                {
                                    MajDebug.Log("[OBS] Start Record Failed.");
                                }
                            }

                            if (message.d.requestType == "StopRecord")
                            {
                                if (message.d.requestStatus.result)
                                {
                                    IsRecording = false;
                                    MajDebug.Log("[OBS] Record Saved To " + message.d.responseData.outputPath);
                                }
                                else
                                {
                                    MajDebug.Log("[OBS] Stop Record Failed.");
                                }
                            }

                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MajDebug.LogException(ex);
            }
        }

        #region WebSocketMessageClass
        public class RequestStatus
        {
            public int code { get; set; }
            public bool result { get; set; }
        }

        public class ResponseData
        {
            public string outputPath { get; set; }
        }

        public class EventData
        {
            public bool outputActive { get; set; }
            public string outputPath { get; set; }
            public string outputState { get; set; }
        }

        public class Event
        {
            public EventData eventData { get; set; }
            public int eventIntent { get; set; }
            public string eventType { get; set; }
        }

        public class DData
        {
            public string obsWebSocketVersion { get; set; }
            public int rpcVersion { get; set; }
            public int negotiatedRpcVersion { get; set; }
            public string requestId { get; set; }
            public RequestStatus requestStatus { get; set; }
            public string requestType { get; set; }
            public EventData eventData { get; set; }
            public ResponseData responseData { get; set; }
            public int eventIntent { get; set; }
            public string eventType { get; set; }
        }

        public class ReceivedMessage
        {
            public DData d { get; set; }
            public int op { get; set; }
            public override string ToString() => Serializer.Json.Serialize(this);
        }
        #endregion
    }
}
