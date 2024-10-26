using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MychIO.Device;
using MychIO.Event;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.Build;
using UnityEngine;

namespace MychIO.Connection.HidDevice
{
    public class HidDeviceConnection : Connection
    {

        // Hold callbacks to prevent garbage collection
        private GCHandle _dataCallbackHandle;
        private GCHandle _eventCallbackHandle;

        // Holds C++ plugin object reference
        private IntPtr _pluginHandle;

        public HidDeviceConnection(IDevice device, IConnectionProperties connectionProperties, IOManager manager) :
         base(device, connectionProperties, manager)
        {
            if (connectionProperties is not HidDeviceProperties)
            {
                manager.handleEvent(
                    IOEventType.ConnectionError,
                        _device.GetClassification(),
                        "Invalid property object passed to HidConnection"
                );
            }

            if (1 != UnityHidApiPlugin.PluginLoaded())
            {
                manager.handleEvent(
                    IOEventType.ConnectionError,
                        _device.GetClassification(),
                        "Error loading UnityHidApiPlugin plugin"
                );
            }

            HidDeviceProperties properties = (HidDeviceProperties)connectionProperties;
            _pluginHandle = UnityHidApiPlugin.Initialize(
                properties.VendorId,
                properties.ProductId,
                properties.BufferSize,
                properties.LeftBytesToTruncate,
                properties.BytesToRead
            );
            if (_pluginHandle == IntPtr.Zero)
            {
                manager.handleEvent(
                    IOEventType.ConnectionError,
                    _device.GetClassification(),
                    "Error Initializing HID Connection plugin, please try again"
                );
                UnityHidApiPlugin.ReloadPlugin();
            }
        }

        private void OnDestroy()
        {
            Task.Run(async () =>
            {
                if (IsReading())
                {
                    await Disconnect();
                }
                if (_dataCallbackHandle.IsAllocated)
                {
                    _dataCallbackHandle.Free();
                }
                if (_eventCallbackHandle.IsAllocated)
                {
                    _eventCallbackHandle.Free();
                }
                if (_pluginHandle != IntPtr.Zero)
                {
                    UnityHidApiPlugin.Dispose(_pluginHandle);
                }
            }).Wait();
        }

        public override bool CanConnect(IConnection connectionProperties)
        {
            // It is possible to have multiple devices with the same vendorId and ProductId
            // if in the future multiplayer on the same machine is supported 
            // device path should be used instead requiring a rework of how the plugin is implemented 
            // e.g. add support for device path as a connectionProperty then overload the plugin constructor 
            return !(connectionProperties is HidDeviceProperties) ||
            (
             ((HidDeviceProperties)connectionProperties).VendorId !=
              ((HidDeviceProperties)_connectionProperties).VendorId &&
             ((HidDeviceProperties)connectionProperties).ProductId !=
              ((HidDeviceProperties)_connectionProperties).ProductId
            );
        }

        public override Task Connect()
        {

            if (IsConnected())
            {
                // TODO: Set event here
                return Task.CompletedTask;
            }

            var eventReceivedCallback = new UnityHidApiPlugin.EventCallbackDelegate(
                (string message) =>
                {
                    _manager.handleEvent(IOEventType.ConnectionError, _device.GetClassification(), _device.GetType().ToString() + " Error: " + message);
                }
            );

            if (UnityHidApiPlugin.Connect(_pluginHandle, eventReceivedCallback))
            {
                _manager.handleEvent(IOEventType.ConnectionError, _device.GetClassification(), _device.GetType().ToString() + " Failed to Connect");
            }

            var dataReceivedCallback = new UnityHidApiPlugin.DataCallbackDelegate(_device.ReadData);

            // prevent garbage collection of callbacks
            _dataCallbackHandle = GCHandle.Alloc(dataReceivedCallback);
            _eventCallbackHandle = GCHandle.Alloc(eventReceivedCallback);
            Read();

            _manager.handleEvent(IOEventType.Attach, _device.GetClassification(), _device.GetType().ToString() + " Device is running properly");

            return Task.CompletedTask;

        }

        public override async Task Disconnect()
        {
            if (IsConnected())
            {
                await _device.OnDisconnectWrite();
            }
            UnityHidApiPlugin.Disconnect(_pluginHandle);
        }

        // Is Connected means its reading currently
        public override bool IsConnected()
        {
            return UnityHidApiPlugin.IsConnected(_pluginHandle);
        }

        // currently no need to write to HID devices so not implemented
        public override Task Write(byte[] bytes)
        {
            return Task.CompletedTask;
        }

        public override bool IsReading()
        {
            return UnityHidApiPlugin.IsReading(_pluginHandle);
        }

        public override void Read()
        {
            if (!IsReading() && null != _pluginHandle)
            {
                var dataCallback = (UnityHidApiPlugin.DataCallbackDelegate)_dataCallbackHandle.Target;
                var eventCallback = (UnityHidApiPlugin.EventCallbackDelegate)_eventCallbackHandle.Target;
                UnityHidApiPlugin.Read(_pluginHandle, dataCallback, eventCallback);
            }

            if (!UnityHidApiPlugin.IsReading(_pluginHandle))
            {
                _manager.handleEvent(IOEventType.ConnectionError, _device.GetClassification(), _device.GetType().ToString() + " Error: failed to start reading from device");
            }
        }

        public override void StopReading()
        {
            if (IsReading())
            {
                UnityHidApiPlugin.StopReading(_pluginHandle);
            }
        }
    }
}