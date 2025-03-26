using System;
using WebSocketSharp;

namespace MajdataPlay.Utils
{
    public class RecordHelper : IRecordHelper, IDisposable
    {
        private readonly WebSocket webSocket = new("ws://127.0.0.1:4455");
        public bool Connected = false;
        public bool Recording = false;

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
            // TODO: 根据消息返回来确认是否已连接
            Connected = true;
        }

        private void Disconnect()
        {
            webSocket.Close();
            // TODO: 根据消息返回来确认是否已连接
            Connected = false;
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void StartRecord()
        {
            webSocket.Send(StartRecordMessage);
            // TODO: 根据消息返回来确认是否已开始录制
            Recording = true;
        }

        public void StopRecord()
        {
            webSocket.Send(StopRecordMessage);
            // TODO: 根据消息返回来确认是否已停止录制
            Recording = false;
        }

        private void Authenticate() => webSocket.Send(AuthenticateMessage);
    }
}
