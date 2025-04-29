using HidSharp;
using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
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
        
        readonly static Memory<Color> _ledColors = new Color[8];
        readonly static ReadOnlyMemory<LedDevice> _ledDevices = Array.Empty<LedDevice>();
        readonly static bool _isEnabled = true;
        static Task _ledDeviceUpdateLoop = Task.CompletedTask;

        static LightManager()
        {
            _isEnabled = MajInstances.Settings.IO.OutputDevice.Led.Enable;
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

            if (MajInstances.Settings.IO.OutputDevice.Led.RefreshRateMs <= 100) {
                MajInstances.Settings.IO.OutputDevice.Led.RefreshRateMs = 100;
            }

            _ledDevices = ledDevices;

            try
            {
                if (!_ledDeviceUpdateLoop.IsCompleted)
                {
                    return;
                }
                var manufacturer = MajEnv.UserSettings.IO.Manufacturer;
                switch (manufacturer)
                {
                    case DeviceManufacturer.General:
                    case DeviceManufacturer.Yuan:
                        _ledDeviceUpdateLoop = Task.Factory.StartNew(SerialPortUpdateLoop, TaskCreationOptions.LongRunning);
                        break;
                    case DeviceManufacturer.Dao:
                        _ledDeviceUpdateLoop = Task.Factory.StartNew(HIDUpdateLoop, TaskCreationOptions.LongRunning);
                        break;
                    default:
                        MajDebug.LogWarning($"Not supported led device manufacturer: {manufacturer}");
                        break;
                }
            }
            catch
            {
                //MajDebug.LogWarning($"Cannot open {comPortStr}, using dummy lights");
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
        static void SerialPortUpdateLoop()
        {
            var currentThread = Thread.CurrentThread;
            var ledOptions = MajEnv.UserSettings.IO.OutputDevice.Led;
            var serialPortOptions = ledOptions.SerialPortOptions;
            var token = MajEnv.GlobalCT;
            var refreshRate = TimeSpan.FromMilliseconds(MajInstances.Settings.IO.OutputDevice.Led.RefreshRateMs);
            var stopwatch = new Stopwatch();
            var t1 = stopwatch.Elapsed;
            var ledDevices = _ledDevices.Span;
            var updatePacket = GeneralSerialLedDevice.BuildUpdatePacket();
            using var serial = new SerialPort($"COM{serialPortOptions.Port}", serialPortOptions.BaudRate);

            serial.WriteTimeout = 2000;
            serial.WriteBufferSize = 16;
            currentThread.Name = "IO/L Thread";
            currentThread.IsBackground = true;
            currentThread.Priority = MajEnv.UserSettings.Debug.IOThreadPriority;
            Span<byte> buffer = stackalloc byte[10];
            Span<LedReport> latestReports = stackalloc LedReport[8]
            {
                new LedReport()
                {
                    Index = 0,
                    Color = Color.black,
                },
                new LedReport()
                {
                    Index = 1,
                    Color = Color.black,
                },
                new LedReport()
                {
                    Index = 2,
                    Color = Color.black,
                },
                new LedReport()
                {
                    Index = 3,
                    Color = Color.black,
                },
                new LedReport()
                {
                    Index = 4,
                    Color = Color.black,
                },
                new LedReport()
                {
                    Index = 5,
                    Color = Color.black,
                },
                new LedReport()
                {
                    Index = 6,
                    Color = Color.black,
                },
                new LedReport()
                {
                    Index = 7,
                    Color = Color.black,
                }
            };

            stopwatch.Start();
            
            if(!EnsureSerialPortIsOpen(serial))
            {
                MajDebug.LogWarning($"Cannot open COM{serialPortOptions.Port}, using dummy lights");
                return;
            }
            while (true)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    var needUpdate = false;
                    EnsureSerialPortIsOpen(serial);
                    for (var i = 0; i < 8; i++)
                    {
                        var device = ledDevices[i];
                        var color = device.Color;
                        ref var latestReport = ref latestReports[i];
                        if (latestReport.Color == color)
                        {
                            continue;
                        }
                        var packet = GeneralSerialLedDevice.BuildSetColorPacket(buffer, i, color);
                        latestReport = new()
                        {
                            Index = i,
                            Color = color,
                        };
                        needUpdate = true;
                        serial.Write(packet);
                    }
                    if (needUpdate)
                    {
                        serial.Write(updatePacket);
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
        static void HIDUpdateLoop()
        {
            var ledOptions = MajEnv.UserSettings.IO.OutputDevice.Led;
            var hidOptions = ledOptions.HidOptions;
            var currentThread = Thread.CurrentThread;
            var token = MajEnv.GlobalCT;
            var refreshRate = TimeSpan.FromMilliseconds(ledOptions.RefreshRateMs);
            var stopwatch = new Stopwatch();
            var ledDevices = _ledDevices.Span;
            var t1 = stopwatch.Elapsed;
            var pid = hidOptions.ProductId;
            var vid = hidOptions.VendorId;
            var deviceName = string.IsNullOrEmpty(hidOptions.DeviceName) ? "SkyStar Maimoller" : hidOptions.DeviceName;
            var hidConfig = new OpenConfiguration();
            var filter = new DeviceFilter()
            {
                DeviceName = deviceName,
                ProductId = pid,
                VendorId = vid,
            };
            Span<LedReport> latestReports = stackalloc LedReport[8]
            {
                new LedReport()
                {
                    Index = 0,
                    Color = Color.black,
                },
                new LedReport()
                {
                    Index = 1,
                    Color = Color.black,
                },
                new LedReport()
                {
                    Index = 2,
                    Color = Color.black,
                },
                new LedReport()
                {
                    Index = 3,
                    Color = Color.black,
                },
                new LedReport()
                {
                    Index = 4,
                    Color = Color.black,
                },
                new LedReport()
                {
                    Index = 5,
                    Color = Color.black,
                },
                new LedReport()
                {
                    Index = 6,
                    Color = Color.black,
                },
                new LedReport()
                {
                    Index = 7,
                    Color = Color.black,
                }
            };

            hidConfig.SetOption(OpenOption.Exclusive, hidOptions.Exclusice);
            hidConfig.SetOption(OpenOption.Priority, hidOptions.OpenPriority);
            currentThread.Name = "IO/L Thread";
            currentThread.IsBackground = true;
            currentThread.Priority = MajEnv.UserSettings.Debug.IOThreadPriority;


            if (!HidManager.TryGetDevice(filter, out var hidDevice))
            {
                MajDebug.LogWarning("Led: hid device not found");
                return;
            }
            if (!hidDevice.TryOpen(hidConfig, out var hidStream))
            {
                MajDebug.LogError($"Led: cannot open hid device:\n{hidDevice}");
                return;
            }
            try
            {
                var outputReportId = hidDevice.GetReportDescriptor()
                                              .OutputReports
                                              .FirstOrDefault()
                                              ?.ReportID ?? 0;
                Span<byte> buffer = stackalloc byte[hidDevice.GetMaxOutputReportLength()];
                buffer[0] = outputReportId;
                IsConnected = true;
                MajDebug.Log($"Led device connected\nDevice: {hidDevice}");
                stopwatch.Start();
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        var now = MajTimeline.UnscaledTime;
                        var needUpdate = false;
                        for (var i = 0; i < 8; i++)
                        {
                            var device = ledDevices[i];
                            var color = device.Color;
                            ref var latestReport = ref latestReports[i];
                            if (latestReport.Color == color)
                            {
                                continue;
                            }
                            latestReport = new()
                            {
                                Index = i,
                                Color = color,
                            };
                            needUpdate = true;
                        }
                        if (needUpdate)
                        {
                            var reportBuffer = DaoHIDLedDevice.BuildUpdatePacket(buffer, ledDevices);
                            hidStream.Write(reportBuffer);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (IOException ioE)
                    {
                        IsConnected = false;
                        MajDebug.LogError($"Led: from HID listener: \n{ioE}");
                    }
                    catch (Exception e)
                    {
                        MajDebug.LogError($"Led: from HID listener: \n{e}");
                    }
                    finally
                    {
                        buffer.Clear();
                        buffer[0] = outputReportId;
                        if (refreshRate.TotalMilliseconds > 0)
                        {
                            var t2 = stopwatch.Elapsed;
                            var elapsed = t2 - t1;
                            t1 = t2;
                            if (elapsed < refreshRate)
                                Thread.Sleep(refreshRate - elapsed);
                        }
                    }
                }
            }
            finally
            {
                hidStream.Dispose();
                IsConnected = false;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool EnsureSerialPortIsOpen(SerialPort serialSession)
        {
            try
            {
                if (serialSession.IsOpen)
                {
                    return true;
                }
                else
                {
                    serialSession.Open();
                    return true;
                }
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
                return false;
            }

        }
        static class GeneralSerialLedDevice
        {
            readonly static ReadOnlyMemory<byte> _templateSingle = new byte[] 
            { 
                0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 
            };
            readonly static ReadOnlyMemory<byte> _templateUpdate = new byte[]
            {
                0xE0, 0x11, 0x01, 0x01, 0x3C, 0x4F
            };
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ReadOnlySpan<byte> BuildSetColorPacket(Span<byte> packet, int index, Color newColor)
            {
                _templateSingle.Span.CopyTo(packet);
                packet[5] = (byte)index;
                packet[6] = (byte)(newColor.r * 255);
                packet[7] = (byte)(newColor.g * 255);
                packet[8] = (byte)(newColor.b * 255);
                packet[9] = CalculateCheckSum(packet.Slice(0, 9));

                return packet.Slice(0, 10);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ReadOnlySpan<byte> BuildUpdatePacket()
            {
                return _templateUpdate.Span;
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
        }
        static class DaoHIDLedDevice
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ReadOnlySpan<byte> BuildUpdatePacket(Span<byte> rawBuffer, ReadOnlySpan<LedDevice> ledDevices)
            {
                var buffer = rawBuffer.Slice(1);
                for (int i = 0; i < ledDevices.Length; i++)
                {
                    var device = ledDevices[i];
                    var color = device.Color;
                    var r = (byte)(color.r * 255);
                    var g = (byte)(color.g * 255);
                    var b = (byte)(color.b * 255);

                    buffer[i] = r;
                    buffer[i + 1] = g;
                    buffer[i + 2] = b;
                }
                return rawBuffer;
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