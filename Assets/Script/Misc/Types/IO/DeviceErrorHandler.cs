using MajdataPlay.Utils;
using MychIO;
using MychIO.Device;
using MychIO.Event;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    public class DeviceErrorHandler: IDeviceErrorHandler
    {
        public int MaxRetryCount { get; init; } = 4;

        int _retryCount = 0;
        IOManager _ioManager;

        static readonly Dictionary<DeviceClassification, bool> _deviceHandleState = new();
        static readonly ConcurrentQueue<Action> _executionQueue = IOManager.ExecutionQueue;
        public DeviceErrorHandler(IOManager ioManager) : this(ioManager, 4) { }
        public DeviceErrorHandler(IOManager ioManager,int maxRetryCount)
        {
            _ioManager = ioManager;
            MaxRetryCount = maxRetryCount;
        }
        public void Handle(IOEventType eventType, DeviceClassification deviceType, string msg)
        {
            if (_ioManager is null)
            {
                Error("External IOManager was never initialized");
                return;
            }
            else if (_deviceHandleState.TryGetValue(deviceType, out bool isHandling) && isHandling)
                return;
            else if (_retryCount == MaxRetryCount)
                return;
            switch(eventType)
            {
                case IOEventType.Attach:
                case IOEventType.Debug:
                case IOEventType.InvalidDevicePropertyError:
                    return;
            }
            _deviceHandleState[deviceType] = true;
            if(_retryCount == MaxRetryCount - 1)
            {
                lock(_ioManager)
                {
                    if (_retryCount == MaxRetryCount)
                        return;
                    MajInstances.InputManager.StartExternalIOManager();
                    _retryCount++;
                }
            }
            else
            {
                var isConn = true;
                if(!_ioManager.IsReading(deviceType))
                {
                    Warning($"Device of \"{deviceType}\" has not started reading, trying to start a read loop...");
                    if (!_ioManager.StartReading(deviceType))
                        Error($"Unable to start read loop for \"{deviceType}\"");
                    isConn = false;
                }
                if(!_ioManager.IsConnected(deviceType))
                {
                    Warning($"Device of \"{deviceType}\" has not connected, trying to connect the device...");
                    if (!_ioManager.ReConnect(deviceType))
                        Error($"Unable to connect to device: \"{deviceType}\"");
                    isConn = false;
                }
                if (!isConn)
                    _retryCount++;
                else
                    Debug.LogWarning("[DeviceErrHandler] Received a error event but the device connection is OK");
            }
            _deviceHandleState[deviceType] = false;
        }
        void Log<T>(T msg)
        {
            _executionQueue.Enqueue(() => Debug.Log(msg));
        }
        void Warning<T>(T msg)
        {
            _executionQueue.Enqueue(() => Debug.LogWarning(msg));
        }
        void Error<T>(T msg)
        {
            _executionQueue.Enqueue(() => Debug.LogError(msg));
        }
    }
}
