using System;
using WebSocketSharp;

namespace MajdataPlay.Utils
{
    public class RecordHelper : IRecordHelper, IDisposable
    {
        private WebSocket webSocket = new("ws://127.0.0.1:4455");

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

        public RecordHelper()
        {
            Connect();
            Authenticate();
        }

        private void Connect()
        {
            webSocket.Connect();
        }

        private void Disconnect()
        {
            webSocket.Close();
        }

        public void Dispose()
        {

        }

        public void StartRecord()
        {
            webSocket.Send(StartRecordMessage);
            Disconnect();
        }

        public void StopRecord()
        {
            webSocket.Send(StopRecordMessage);
            Disconnect();
        }

        private void Authenticate()
        {
            webSocket.Send(AuthenticateMessage);
        }
    }
}
