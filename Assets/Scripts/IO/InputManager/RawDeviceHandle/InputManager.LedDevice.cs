using HidSharp;
using MajdataPlay.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MajdataPlay.Settings;
using UnityEngine;
using MajdataPlay.Numerics;
#nullable enable
namespace MajdataPlay.IO
{
    internal static unsafe partial class InputManager
    {
        static class LedDevice
        {
            public static bool IsConnected
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                private set;
            }

            static Task _ledDeviceUpdateLoop = Task.CompletedTask;

            readonly static bool _isThrottlerEnabled = false;
            readonly static bool _isEnabled = true;

            static float _brightness = 1.0f;
            static LedDevice()
            {
#if !UNITY_STANDALONE_WIN
                _isEnabled = false;
#else
                _isEnabled = MajInstances.Settings.IO.OutputDevice.Led.Enable;
#endif

                _isThrottlerEnabled = MajInstances.Settings.IO.OutputDevice.Led.Throttler;

                if (MajInstances.Settings.IO.OutputDevice.Led.RefreshRateMs <= 16)
                {
                    MajInstances.Settings.IO.OutputDevice.Led.RefreshRateMs = 16;
                }
            }
            public static void Init()
            {
#if !UNITY_STANDALONE_WIN
                return;
#endif
                _brightness = MajInstances.Settings.IO.OutputDevice.Led.Brightness.Clamp(0, 1f);
                try
                {
                    if (!_ledDeviceUpdateLoop.IsCompleted || !_isEnabled)
                    {
                        return;
                    }
                    var manufacturer = _deviceManufacturer;
                    switch (manufacturer)
                    {
                        case DeviceManufacturerOption.General:
                        case DeviceManufacturerOption.Yuan:
                            _ledDeviceUpdateLoop = Task.Factory.StartNew(SerialPortUpdateLoop, TaskCreationOptions.LongRunning);
                            break;
                        case DeviceManufacturerOption.Dao:
                            _ledDeviceUpdateLoop = Task.Factory.StartNew(HIDUpdateLoop, TaskCreationOptions.LongRunning);
                            break;
                        default:
                            MajDebug.LogWarning($"Led: Not supported led device manufacturer: {manufacturer}");
                            break;
                    }
                }
                catch
                {
                    //MajDebug.LogWarning($"Cannot open {comPortStr}, using dummy lights");
                    IsConnected = false;
                }
            }
            static void SerialPortUpdateLoop()
            {
                var currentThread = Thread.CurrentThread;
                var serialPortOptions = _ledDeviceSerialConnInfo;
                var token = MajEnv.GlobalCT;
                var refreshRate = TimeSpan.FromMilliseconds(MajInstances.Settings.IO.OutputDevice.Led.RefreshRateMs);
                var stopwatch = new Stopwatch();
                var t1 = stopwatch.Elapsed;
                var ledColors = LedRing.LedColors;
                var updatePacket = GeneralSerialLedDevice.BuildUpdatePacket();
                using var serial = new SerialPort($"COM{serialPortOptions.Port}", serialPortOptions.BaudRate);

                serial.WriteTimeout = 2000;
                serial.WriteBufferSize = 16;
                currentThread.Name = "IO/L Thread";
                currentThread.IsBackground = true;
                currentThread.Priority = MajEnv.THREAD_PRIORITY_IO;
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

                if (!EnsureSerialPortIsOpen(serial))
                {
                    MajDebug.LogWarning($"Led: Cannot open COM{serialPortOptions.Port}, using dummy lights");
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
                            var color = ledColors[i];
                            ref var latestReport = ref latestReports[i];
                            if (latestReport.Color == color && _isThrottlerEnabled)
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
                        MajDebug.LogError($"Led: \n{e}");
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
                var ledOptions = MajEnv.Settings.IO.OutputDevice.Led;
                var hidOptions = _ledDeviceHidConnInfo;
                var currentThread = Thread.CurrentThread;
                var token = MajEnv.GlobalCT;
                var refreshRate = TimeSpan.FromMilliseconds(ledOptions.RefreshRateMs);
                var stopwatch = new Stopwatch();
                var ledColors = LedRing.LedColors;
                var t1 = stopwatch.Elapsed;
                var pid = hidOptions.ProductId;
                var vid = hidOptions.VendorId;
                var deviceName = string.IsNullOrEmpty(hidOptions.DeviceName) ? string.Empty : hidOptions.DeviceName;
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
                currentThread.Priority = MajEnv.THREAD_PRIORITY_IO;

                HidDevice? device = null;
                HidStream? hidStream = null;

                if (!HidManager.TryGetDevices(filter, out var devices))
                {
                    MajDebug.LogWarning("Led: hid device not found");
                    return;
                }
                foreach (var d in devices)
                {
                    if (d.TryOpen(hidConfig, out hidStream))
                    {
                        device = d;
                        break;
                    }
                }
                if (hidStream is null || device is null)
                {
                    MajDebug.LogError($"Led: cannot open hid devices:\n{string.Join('\n', devices)}");
                    return;
                }
                try
                {
                    var outputReportId = device.GetReportDescriptor()
                                               .OutputReports
                                               .FirstOrDefault()
                                               ?.ReportID ?? 0;
                    Span<byte> buffer = stackalloc byte[device.GetMaxOutputReportLength()];
                    buffer[0] = outputReportId;
                    IsConnected = true;
                    MajDebug.LogInfo($"Led: Connected\nDevice: {device}");
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
                                var color = ledColors[i];
                                ref var latestReport = ref latestReports[i];
                                if (latestReport.Color == color && _isThrottlerEnabled)
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
                                var reportBuffer = DaoHIDLedDevice.BuildUpdatePacket(buffer, ledColors);
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
                            MajDebug.LogError($"Led: \n{ioE}");
                        }
                        catch (Exception e)
                        {
                            MajDebug.LogError($"Led: \n{e}");
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
                                {
                                    Thread.Sleep(refreshRate - elapsed);
                                }
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
                    packet[6] = (byte)(newColor.r * 255 * _brightness);
                    packet[7] = (byte)(newColor.g * 255 * _brightness);
                    packet[8] = (byte)(newColor.b * 255 * _brightness);
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
                public static ReadOnlySpan<byte> BuildUpdatePacket(Span<byte> rawBuffer, ReadOnlySpan<Color> ledColors)
                {
                    var buffer = rawBuffer.Slice(1);
                    for (int i = 0,li = 0; li < ledColors.Length;)
                    {
                        var color = ledColors[li++];
                        var r = (byte)(color.r * 255 * _brightness);
                        var g = (byte)(color.g * 255 * _brightness);
                        var b = (byte)(color.b * 255 * _brightness);

                        buffer[i++] = r;
                        buffer[i++] = g;
                        buffer[i++] = b;
                    }
                    return rawBuffer;
                }
            }
            readonly struct LedReport
            {
                public int Index { get; init; }
                public Color Color { get; init; }
            }
        }
    }
}
