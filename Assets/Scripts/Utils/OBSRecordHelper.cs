using MajdataPlay.Types;
using System;
using WebSocketSharp;

namespace MajdataPlay.Utils
{
    public class OBSRecordHelper : IRecordHelper, IDisposable
    {
        private readonly WebSocket webSocket = new("ws://127.0.0.1:4455");
        public bool Connected { get; set; } = false;
        public bool Recording { get; set; } = false;

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
            try
            {
                webSocket.OnMessage += OnMessageReceived;
                Connect();
                Authenticate();
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
            }
        }
        private void Connect()
        {
            webSocket.Connect();
        }

        private void Disconnect()
        {
            try
            {
                webSocket.Close();
                Connected = false;
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void StartRecord()
        { 
            webSocket.Send(StartRecordMessage);
        }

        public void StopRecord()
        {
            webSocket.Send(StopRecordMessage);
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                var message = Serializer.Json.Deserialize<ReceivedMessage>(e.Data);
                switch (message.Op)
                {
                    case 2: // Identified
                    {
                        Connected = true;
                        break;
                    }
                    case 7: // RequestResponse
                    {
                        if (message.D.RequestType == "StartRecord")
                        {
                            if (message.D.RequestStatus.Result)
                            {
                                Recording = true;
                            }
                            else
                            {
                                MajDebug.Log("Start Record Failed.");
                            }
                        }

                        if (message.D.RequestType == "StopRecord")
                        {
                            if (message.D.RequestStatus.Result)
                            {
                                Recording = false;
                                MajDebug.Log("Record Saved To " + message.D.ResponseData.OutputPath);
                            }
                            else
                            {
                                MajDebug.Log("Stop Record Failed.");
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

        private void Authenticate() => webSocket.Send(AuthenticateMessage);

        #region WebSocketMessageClass
        public class RequestStatus
        {
            public int Code { get; set; }
            public bool Result { get; set; }
        }

        public class ResponseData
        {
            public string OutputPath { get; set; }
        }

        public class EventData
        {
            public bool OutputActive { get; set; }
            public string OutputPath { get; set; }
            public string OutputState { get; set; }
        }

        public class Event
        {
            public EventData EventData { get; set; }
            public int EventIntent { get; set; }
            public string EventType { get; set; }
        }

        public class DData
        {
            public string ObsWebSocketVersion { get; set; }
            public int RpcVersion { get; set; }
            public int NegotiatedRpcVersion { get; set; }
            public string RequestId { get; set; }
            public RequestStatus RequestStatus { get; set; }
            public string RequestType { get; set; }
            public EventData EventData { get; set; }
            public string OutputPath { get; set; }
            public Event Event { get; set; }
            public ResponseData ResponseData { get; set; }
        }

        public class ReceivedMessage
        {
            public DData D { get; set; }
            public int Op { get; set; }
        }
        #endregion
    }
}
