using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using MychIO;
using MychIO.Device;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    public class LightManager : MonoBehaviour
    {
        bool _useDummy = true;
        SpriteRenderer[] _dummyLights = Array.Empty<SpriteRenderer>();
        SerialPort _serial = new SerialPort("COM21", 115200);
        LedDevice[] _ledDevices = new LedDevice[8];
        //Coroutine[] timers = new Coroutine[8];
        //List<byte> _templateAll = new List<byte>() { 0xE0, 0x11, 0x01, 0x08, 0x32, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 };
        //List<byte> _templateSingle = new List<byte>() { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00 };
        readonly byte[] _templateSingle = new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 };
        readonly byte[] _templateUpdate = new byte[]
        {
            0xE0, 0x11, 0x01, 0x01, 0x3C, 0x4F
        };

        private void Awake()
        {
            MajInstances.LightManager = this;
            DontDestroyOnLoad(gameObject);
            _dummyLights = gameObject.GetComponentsInChildren<SpriteRenderer>();
        }
        private void Start()
        {
            for (var i = 0; i < 8; i++)
            {
                _ledDevices[i] = new()
                {
                    Index = i,
                };
            }
            var comPort = MajInstances.Setting.Misc.OutputDevice.Led.COMPort;
            var comPortStr = $"COM{comPort}";
            try
            {
                if (comPort != 21)
                {
                    _serial = new SerialPort(comPortStr, 115200);
                }
                _serial.WriteBufferSize = 16;
                _serial.Open();
                _useDummy = false;
                foreach (var light in _dummyLights)
                {
                    light.forceRenderingOff = true;
                }
            }
            catch
            {
                MajDebug.LogWarning($"Cannot open {comPortStr}, using dummy lights");
                _useDummy = true;
            }
            UpdateLedDeviceAsync();
        }
        private void OnDestroy()
        {
            MajInstanceHelper<LightManager>.Free();
            _serial!.Close();
        }

        byte CalculateCheckSum(List<byte> bytes)
        {
            byte sum = 0;
            for (int i = 1; i < bytes.Count; i++)
            {
                sum += bytes[i];
            }
            return sum;
        }
        byte CalculateCheckSum(Span<byte> bytes)
        {
            byte sum = 0;
            for (int i = 1; i < bytes.Length; i++)
            {
                sum += bytes[i];
            }
            return sum;
        }
        public void SetAllLight(Color lightColor)
        {
            foreach (var device in ArrayHelper.ToEnumerable(_ledDevices))
            {
                device!.SetColor(lightColor);
            }
        }
        public void SetButtonLight(Color lightColor, int button)
        {
            _ledDevices[button].SetColor(lightColor);
        }
        Memory<byte> BuildSetColorPacket(Memory<byte> memory, int index, Color newColor)
        {
            _templateSingle.CopyTo(memory);
            var packet = memory.Span;
            packet[5] = (byte)index;
            packet[6] = (byte)(newColor.r * 255);
            packet[7] = (byte)(newColor.g * 255);
            packet[8] = (byte)(newColor.b * 255);
            packet[9] = CalculateCheckSum(packet.Slice(0, 9));

            return memory;
        }
        LedCommand BuildSetColorCommand(int index, Color newColor)
        {
            Span<byte> data = stackalloc byte[4];
            data[0] = (byte)(LedCommand.SetColor0 + index);
            data[1] = (byte)(newColor.r * 255);
            data[2] = (byte)(newColor.g * 255);
            data[3] = (byte)(newColor.b * 255);

            return MemoryMarshal.Read<LedCommand>(data);
        }
        public void SetButtonLightWithTimeout(Color lightColor, int button, long durationMs = 500)
        {
            _ledDevices[button].SetColor(lightColor, durationMs);
        }
        public void SetButtonLightWithTimeout(Color lightColor, int button, TimeSpan duration)
        {
            _ledDevices[button].SetColor(lightColor, duration);
        }
        async void UpdateLedDeviceAsync()
        {
            UniTask.Void(async () =>
            {
                var token = MajEnv.GlobalCT;
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    foreach (var device in ArrayHelper.ToEnumerable(_ledDevices))
                    {
                        _dummyLights[device!.Index].color = device.Color;
                    }
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                }
            });
            if (_useDummy || !MajInstances.Setting.Misc.OutputDevice.Led.Enable)
                return;

            if (MajInstanceHelper<IOManager>.Instance is null)
            {
                await UpdateInternalLedManagerAsync();
            }
            else
            {
                await UpdateExternalLedManagerAsync();
            }
        }
        async Task UpdateInternalLedManagerAsync()
        {
            await Task.Yield();

            var token = MajEnv.GlobalCT;
            var refreshRate = TimeSpan.FromMilliseconds(MajInstances.Setting.Misc.OutputDevice.Led.RefreshRateMs);
            var stopwatch = new Stopwatch();
            var t1 = stopwatch.Elapsed;

            stopwatch.Start();

            while (true)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    var serialStream = await EnsureSerialStreamIsOpen(_serial);
                    foreach (var device in ArrayHelper.ToEnumerable(_ledDevices))
                    {
                        using (var memoryOwner = MemoryPool<byte>.Shared.Rent(10))
                        {
                            var index = device!.Index;
                            var color = device.Color;
                            var packet = BuildSetColorPacket(memoryOwner.Memory, index, color);
                            //rentedMemory.Span[5] = (byte)index;
                            //rentedMemory.Span[6] = (byte)(color.r * 255);
                            //rentedMemory.Span[7] = (byte)(color.g * 255);
                            //rentedMemory.Span[8] = (byte)(color.b * 255);
                            //rentedMemory.Span[9] = CalculateCheckSum(packet.AsSpan().Slice(0, 9));

                            await serialStream.WriteAsync(packet.Slice(0, 10));
                        }
                    }
                    await serialStream.WriteAsync(_templateUpdate.AsMemory());
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"From Led refresher: \n{e}");
                }
                finally
                {
                    var t2 = stopwatch.Elapsed;
                    var elapsed = t2 - t1;
                    t1 = t2;
                    if (elapsed < refreshRate)
                        await Task.Delay(refreshRate - elapsed, token);
                }
            }
        }
        async Task UpdateExternalLedManagerAsync()
        {
            await Task.Yield();

            var ioManager = MajInstanceHelper<IOManager>.Instance!;
            var token = MajEnv.GlobalCT;
            var commands = new LedCommand[9];
            var refreshRate = TimeSpan.FromMilliseconds(MajInstances.Setting.Misc.OutputDevice.Led.RefreshRateMs);
            var stopwatch = new Stopwatch();
            var t1 = stopwatch.Elapsed;

            stopwatch.Start();
            commands[8] = LedCommand.Update;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    for (var i = 0; i < 8; i++)
                    {
                        var device = _ledDevices[i];
                        var index = device.Index;
                        var color = device.Color;
                        var command = BuildSetColorCommand(index, color);

                        commands[i] = command;
                    }
                    await ioManager.WriteToDevice(DeviceClassification.LedDevice, commands);
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"From Led refresher: \n{e}");
                }
                finally
                {
                    var t2 = stopwatch.Elapsed;
                    var elapsed = t2 - t1;
                    t1 = t2;
                    if (elapsed < refreshRate)
                        await Task.Delay(refreshRate - elapsed, token);
                }
            }
        }
        async ValueTask<Stream> EnsureSerialStreamIsOpen(SerialPort serialSession)
        {
            if (serialSession.IsOpen)
            {
                return serialSession.BaseStream;
            }
            else
            {
                await Task.Yield();

                serialSession.Open();

                return serialSession.BaseStream;
            }
        }
    }
}