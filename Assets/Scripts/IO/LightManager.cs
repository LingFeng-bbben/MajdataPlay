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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    internal static class LightManager
    {
        public static bool IsEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _isEnabled;
            }
        }
        public static bool IsConnected
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            private set;
        }
        public static ReadOnlyMemory<Color> LedColors
        {
            get
            {
                return _ledColors;
            }
        }
        
        readonly static SerialPort _serial = new SerialPort("COM21", 9600);
        readonly static Memory<Color> _ledColors = new Color[8];
        readonly static ReadOnlyMemory<LedDevice> _ledDevices = Array.Empty<LedDevice>();
        readonly static ReadOnlyMemory<byte> _templateSingle = new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 };
        readonly static ReadOnlyMemory<byte> _templateUpdate = new byte[]
        {
            0xE0, 0x11, 0x01, 0x01, 0x3C, 0x4F
        };
        readonly static bool _isEnabled = true;
        static Task _updateLoop = Task.CompletedTask;

        static LightManager()
        {
            _isEnabled = MajInstances.Settings.Misc.OutputDevice.Led.Enable;
            var ledDevices = new LedDevice[8];
            for (var i = 0; i < 8; i++)
            {
                ledDevices[i] = new()
                {
                    Index = i,
                };
            }
            if (!_isEnabled)
            {
                for (var i = 0; i < 8; i++)
                {
                    ledDevices[i].SetColor(Color.black);
                }
            }
            _ledDevices = ledDevices;
            var comPort = MajInstances.Settings.Misc.OutputDevice.Led.COMPort;
            var comPortStr = $"COM{comPort}";
            var baudRate = MajInstances.Settings.Misc.OutputDevice.Led.BaudRate;
            try
            {
                if(comPort != 21 || baudRate != 9600)
                {
                    _serial = new SerialPort(comPortStr, baudRate);
                }
                _serial.WriteTimeout = 2000;
                _serial.WriteBufferSize = 16;
                _serial.Open();
                IsConnected = true;
                StartLedDeviceUpdateLoop();
            }
            catch
            {
                MajDebug.LogWarning($"Cannot open {comPortStr}, using dummy lights");
                IsConnected = false;
            }
        }

        internal static void OnPreUpdate()
        {
            var ledDevices = _ledDevices.Span;
            var ledColors = _ledColors.Span;
            for (var i = 0; i < 8; i++)
            {
                ledColors[i] = ledDevices[i].Color;
            }
        }
        public static void SetAllLight(Color lightColor)
        {
            if (!_isEnabled)
                return;
            foreach (var device in _ledDevices.Span)
            {
                device!.SetColor(lightColor);
            }
        }
        public static void SetButtonLight(Color lightColor, int button)
        {
            if (!_isEnabled)
                return;
            _ledDevices.Span[button].SetColor(lightColor);
        }
        public static void SetButtonLightWithTimeout(Color lightColor, int button, long durationMs = 500)
        {
            if (!_isEnabled)
                return;
            _ledDevices.Span[button].SetColor(lightColor, durationMs);
        }
        public static void SetButtonLightWithTimeout(Color lightColor, int button, TimeSpan duration)
        {
            if (!_isEnabled)
                return;
            _ledDevices.Span[button].SetColor(lightColor, duration);
        }
        static void StartLedDeviceUpdateLoop()
        {
            if(!_updateLoop.IsCompleted)
            {
                return;
            }
            if (Majdata<IOManager>.Instance is null)
            {
                _updateLoop = Task.Factory.StartNew(UpdateInternalLedManager, TaskCreationOptions.LongRunning);
            }
            else
            {
                _updateLoop = Task.Factory.StartNew(UpdateInternalLedManager, TaskCreationOptions.LongRunning);
            }
        }
        static void UpdateInternalLedManager()
        {
            var token = MajEnv.GlobalCT;
            var refreshRate = TimeSpan.FromMilliseconds(MajInstances.Settings.Misc.OutputDevice.Led.RefreshRateMs);
            var stopwatch = new Stopwatch();
            var t1 = stopwatch.Elapsed;
            var ledDevices = _ledDevices.Span;
            var templateUpdate = _templateUpdate.Span;
            var needUpdate = false;

            Span<byte> buffer = stackalloc byte[10];
            Span<LedReport> latestReports = stackalloc LedReport[8]
            {
                new LedReport()
                {
                    Index = 0,
                    Color = Color.white,
                },
                new LedReport()
                {
                    Index = 1,
                    Color = Color.white,
                },
                new LedReport()
                {
                    Index = 2,
                    Color = Color.white,
                },
                new LedReport()
                {
                    Index = 3,
                    Color = Color.white,
                },
                new LedReport()
                {
                    Index = 4,
                    Color = Color.white,
                },
                new LedReport()
                {
                    Index = 5,
                    Color = Color.white,
                },
                new LedReport()
                {
                    Index = 6,
                    Color = Color.white,
                },
                new LedReport()
                {
                    Index = 7,
                    Color = Color.white,
                }
            };

            stopwatch.Start();
            using (_serial)
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        needUpdate = false;
                        EnsureSerialPortIsOpen(_serial);
                        for (var i = 0; i < 8; i++)
                        {
                            var device = ledDevices[i];
                            var color = device.Color;
                            ref var latestReport = ref latestReports[i];
                            if(latestReport.Color == color)
                            {
                                continue;
                            }
                            var packet = BuildSetColorPacket(buffer, i, color);
                            latestReport = new()
                            {
                                Index = i,
                                Color = color,
                            };
                            needUpdate = true;
                            _serial.Write(packet.Slice(0, 10));
                        }
                        if(needUpdate)
                        {
                            _serial.Write(templateUpdate);
                        }
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
                        {
                            Thread.Sleep(refreshRate - elapsed);
                        }
                    }
                }
            }
        }
        static void UpdateExternalLedManager()
        {
            var ioManager = Majdata<IOManager>.Instance!;
            var token = MajEnv.GlobalCT;
            var commands = new LedCommand[9];
            var refreshRate = TimeSpan.FromMilliseconds(MajInstances.Settings.Misc.OutputDevice.Led.RefreshRateMs);
            var stopwatch = new Stopwatch();
            var t1 = stopwatch.Elapsed;
            var ledDevices = _ledDevices.Span;
            var needUpdate = false;

            Span<LedReport> latestReports = stackalloc LedReport[8]
            {
                new LedReport()
                {
                    Index = 0,
                    Color = Color.white,
                },
                new LedReport()
                {
                    Index = 1,
                    Color = Color.white,
                },
                new LedReport()
                {
                    Index = 2,
                    Color = Color.white,
                },
                new LedReport()
                {
                    Index = 3,
                    Color = Color.white,
                },
                new LedReport()
                {
                    Index = 4,
                    Color = Color.white,
                },
                new LedReport()
                {
                    Index = 5,
                    Color = Color.white,
                },
                new LedReport()
                {
                    Index = 6,
                    Color = Color.white,
                },
                new LedReport()
                {
                    Index = 7,
                    Color = Color.white,
                }
            };

            stopwatch.Start();
            commands[8] = LedCommand.Update;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    needUpdate = false;
                    for (var i = 0; i < 8; i++)
                    {
                        var device = ledDevices[i];
                        var color = device.Color;
                        ref var latestReport = ref latestReports[i];
                        if (latestReport.Color != color)
                        {
                            needUpdate = true;
                        }
                        var command = BuildSetColorCommand(i, color);
                        latestReport = new()
                        {
                            Index = i,
                            Color = color,
                        };
                        commands[i] = command;
                    }
                    if(needUpdate)
                    {
                        ioManager.WriteToDeviceAsync(DeviceClassification.LedDevice, commands).Wait();
                    }
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
                    {
                        Thread.Sleep(refreshRate - elapsed);
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Memory<byte> BuildSetColorPacket(Memory<byte> memory, int index, Color newColor)
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Span<byte> BuildSetColorPacket(Span<byte> packet, int index, Color newColor)
        {
            _templateSingle.Span.CopyTo(packet);
            packet[5] = (byte)index;
            packet[6] = (byte)(newColor.r * 255);
            packet[7] = (byte)(newColor.g * 255);
            packet[8] = (byte)(newColor.b * 255);
            packet[9] = CalculateCheckSum(packet.Slice(0, 9));

            return packet;
        }
        static LedCommand BuildSetColorCommand(int index, Color newColor)
        {
            Span<byte> data = stackalloc byte[4];
            data[0] = (byte)(LedCommand.SetColorBA1 + index);
            data[1] = (byte)(newColor.r * 255);
            data[2] = (byte)(newColor.g * 255);
            data[3] = (byte)(newColor.b * 255);

            return MemoryMarshal.Read<LedCommand>(data);
        }
        static byte CalculateCheckSum(List<byte> bytes)
        {
            byte sum = 0;
            for (int i = 1; i < bytes.Count; i++)
            {
                sum += bytes[i];
            }
            return sum;
        }
        static byte CalculateCheckSum(Span<byte> bytes)
        {
            byte sum = 0;
            for (int i = 1; i < bytes.Length; i++)
            {
                sum += bytes[i];
            }
            return sum;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void EnsureSerialPortIsOpen(SerialPort serialSession)
        {
            if (serialSession.IsOpen)
            {
                return;
            }
            else
            {
                serialSession.Open();
            }
        }
        class LedDevice
        {
            public int Index { get; init; } = 0;
            public Color Color
            {
                get
                {
                    if (_expTime is null)
                        return _color;

                    var now = DateTime.Now;
                    var expTime = (DateTime)_expTime;
                    if (now > expTime)
                        return _color;
                    else
                        return _immediateColor;
                }
            }

            DateTime? _expTime = null;
            Color _color = Color.white;
            Color _immediateColor = Color.white;

            public void SetColor(Color newColor)
            {
                _color = newColor;
                _expTime = null;
            }
            public void SetColor(Color newColor, long durationMs)
            {
                SetColor(newColor, TimeSpan.FromMilliseconds(durationMs));
            }
            public void SetColor(Color newColor, TimeSpan duration)
            {
                var now = DateTime.Now;
                var exp = now + duration;
                _immediateColor = newColor;
                _expTime = exp;
            }
        }
        readonly struct LedReport
        {
            public int Index { get; init; }
            public Color Color { get; init; }
        }
    }
}