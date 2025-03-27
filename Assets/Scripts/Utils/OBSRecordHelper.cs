using MajdataPlay.Types;
using System;
using WebSocketSharp;

namespace MajdataPlay.Utils
{
    public class OBSRecordHelper : IRecordHelper, IDisposable
    {
        private WebSocket webSocket = new("ws://127.0.0.1:4455");
        public bool Connected { get; set; } = false;
        public bool Recording { get; set; } = false;
        private bool disposed = false;

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

        public OBSRecordHelper() => Init();

        public void Init()
        {
            webSocket.OnMessage += OnMessageReceived;
            Connect();
            Authenticate();
        }

        private void Connect() => webSocket.Connect();

        private void Disconnect()
        {
            webSocket.Close();
            Connected = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                Disconnect();
                webSocket = null;
            }

            disposed = true;
        }

        ~OBSRecordHelper() => Dispose(false);
        public void StartRecord() => webSocket.Send(StartRecordMessage);
        public void StopRecord() => webSocket.Send(StopRecordMessage);
        private void Authenticate() => webSocket.Send(AuthenticateMessage);


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
                        Connected = true;
                        break;
                    }
                    case 7: // RequestResponse
                    {
                        if (message.d.requestType == "StartRecord")
                        {
                            if (message.d.requestStatus.result)
                            {
                                Recording = true;
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
                                Recording = false;
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
