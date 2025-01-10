using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
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
                Debug.LogWarning($"Cannot open {comPortStr}, using dummy lights");
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
            foreach(var device in ArrayHelper.ToEnumerable(_ledDevices))
            {
                device!.SetColor(lightColor);
            }
        }

        public void SetButtonLight(Color lightColor, int button)
        {
            //if (_useDummy)
            //{
            //    _dummyLights[button].color = lightColor;
            //}
            //else
            //{
            //    SetButtonLightSerial(lightColor, button);
            //}
            _ledDevices[button].SetColor(lightColor);
        }
        Memory<byte> BuildSetColorPacket(Memory<byte> memory,int index, Color newColor)
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
        public void SetButtonLightWithTimeout(Color lightColor, int button, long durationMs = 300)
        {
            _ledDevices[button].SetColor(lightColor, durationMs);
        }
        public void SetButtonLightWithTimeout(Color lightColor, int button,TimeSpan duration)
        {
            _ledDevices[button].SetColor(lightColor,duration);
        }
        async void UpdateLedDeviceAsync()
        {
            UniTask.Void(async () =>
            {
                var token = GameManager.GlobalCT;
                while(!token.IsCancellationRequested)
                {
                    foreach (var device in ArrayHelper.ToEnumerable(_ledDevices))
                    {
                        _dummyLights[device!.Index].color = device.Color;
                    }
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                }
            });
            if (_useDummy || !MajInstances.Setting.Misc.OutputDevice.Led.Enable)
                return;
            await Task.Run(async () =>
            {
                var token = GameManager.GlobalCT;
                var refreshRateMs = MajInstances.Setting.Misc.OutputDevice.Led.RefreshRateMs;
                while (!token.IsCancellationRequested)
                {
                    var startAt = MajTimeline.UnscaledTime;
                    try
                    {
                        var serialStream = _serial.BaseStream;
                        foreach (var device in ArrayHelper.ToEnumerable(_ledDevices))
                        {
                            using(var memoryOwner = MemoryPool<byte>.Shared.Rent(10))
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
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                    var endAt = MajTimeline.UnscaledTime;
                    var elapsedTime = endAt - startAt;
                    var waitTime = elapsedTime.TotalMilliseconds > refreshRateMs ? 0 : refreshRateMs - elapsedTime.TotalMilliseconds;
                    await Task.Delay(TimeSpan.FromMilliseconds(waitTime));
                }
            });
        }
    }
}