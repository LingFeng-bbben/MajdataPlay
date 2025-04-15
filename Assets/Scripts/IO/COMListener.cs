using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    internal static partial class InputManager
    {
        static void StartUpdatingTouchPanelState()
        {
            if (!_serialPortUpdateTask.IsCompleted)
                return;
            _serialPortUpdateTask = Task.Factory.StartNew(() =>
            {
                var token = MajEnv.GlobalCT;
                var pollingRate = _sensorPollingRateMs;
                var comPort = $"COM{MajInstances.Settings.Misc.InputDevice.TouchPanel.COMPort}";
                var stopwatch = new Stopwatch();
                var t1 = stopwatch.Elapsed;
                using var serial = new SerialPort(comPort, MajInstances.Settings.Misc.InputDevice.TouchPanel.BaudRate);

                serial.ReadTimeout = 2000;
                serial.WriteTimeout = 2000;
                Thread.CurrentThread.Priority = System.Threading.ThreadPriority.RealTime;
                stopwatch.Start();

                try
                {
                    EnsureTouchPanelSerialStreamIsOpen(serial);
                    IsTouchPanelConnected = true;
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            if (!EnsureTouchPanelSerialStreamIsOpen(serial)) 
                                continue;
                            ReadFromSerialPort(serial);
                        }
                        catch(TimeoutException)
                        {
                            MajDebug.LogError($"From SerialPort listener: Read timeout");
                        }
                        catch (Exception e)
                        {
                            MajDebug.LogError($"From SerialPort listener: \n{e}");
                        }
                        finally
                        {
                            var t2 = stopwatch.Elapsed;
                            var elapsed = t2 - t1;
                            t1 = t2;
                            if (elapsed < pollingRate)
                                Thread.Sleep(pollingRate - elapsed);
                        }
                    }
                }
                catch (IOException ioE)
                {
                    MajDebug.LogWarning($"Cannot open {comPort}, using Mouse as fallback.\n{ioE}");
                    _useDummy = true;
                }
            }, TaskCreationOptions.LongRunning);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ReadFromSerialPort(SerialPort serial)
        {
            var bytes2Read = serial.BytesToRead;
            if (bytes2Read == 0)
                return;
            Span<byte> buffer = stackalloc byte[bytes2Read]; 
            //the SerialPort.BaseStream will be eaten by serialport's own buffer so we dont do that
            var read = serial.Read(buffer);
            TouchPannelPacketHandle(buffer);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void TouchPannelPacketHandle(ReadOnlySpan<byte> packet)
        {
            if (packet.IsEmpty)
                return;
            var now = MajTimeline.UnscaledTime;
            Span<int> startIndexs = stackalloc int[packet.Length];
            int x = -1;
            for (var y = 0; y < packet.Length; y++)
            {
                var @byte = packet[y];
                if(@byte == '(')
                {
                    startIndexs[++x] = y;
                }
            }
            if (x == -1)
                return;
            startIndexs = startIndexs.Slice(0, x + 1);
            foreach (var startIndex in startIndexs)
            {
                var packetBody = GetPacketBody(packet, startIndex);

                if (packetBody.IsEmpty)
                    continue;
                else if (packetBody.Length != 7)
                    continue;

                int k = 0;
                for (int i = 0; i < 7; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        var rawState = packetBody[i] & 1UL << j;
                        var state = rawState > 0 ? SensorStatus.On : SensorStatus.Off;

                        TouchPanel.OnTouchPanelStateChanged(k, state);
                        _touchPanelInputBuffer.Enqueue(new()
                        {
                            Index = k++,
                            State = state,
                            Timestamp = now,
                        });
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ReadOnlySpan<byte> GetPacketBody(ReadOnlySpan<byte> packet, int start)
        {
            var endIndex = -1;
            for (var i = start; i < packet.Length; i++)
            {
                var @byte = packet[i];
                if (@byte == ')')
                {
                    endIndex = i;
                    break;
                }
            }
            if(endIndex == -1)
            {
                return ReadOnlySpan<byte>.Empty;
            }
            return packet[(start + 1)..endIndex];
        }
        static bool EnsureTouchPanelSerialStreamIsOpen(SerialPort serialSession)
        {
            if (serialSession.IsOpen)
            {
                return true;
            }
            else
            {
                MajDebug.Log($"TouchPannel was not connected,trying to connect to TouchPannel via {serialSession.PortName}...");
                serialSession.Open();
                var encoding = Encoding.ASCII;
                var serialStream = serialSession.BaseStream;
                var sens = MajEnv.UserSettings.Misc.InputDevice.TouchPanel.Sensitivity;
                var index = MajEnv.UserSettings.Misc.InputDevice.TouchPanel.Index == 1 ? 'L' : 'R';
                //see https://github.com/Sucareto/Mai2Touch/tree/main/Mai2Touch

                serialStream.Write(encoding.GetBytes("{RSET}"));
                serialStream.Write(encoding.GetBytes("{HALT}"));

                //send ratio
                for (byte a = 0x41; a <= 0x62; a++)
                {
                    serialStream.Write(encoding.GetBytes($"{{{index}{(char)a}r2}}"));
                }
                try
                {
                    for (byte a = 0x41; a <= 0x62; a++)
                    {
                        var value = GetSensitivityValue(a, sens);
                        
                        serialStream.Write(encoding.GetBytes($"{{{index}{(char)a}k{(char)value}}}"));
                    }
                }
                catch (TimeoutException)
                {
                    MajDebug.LogWarning($"TouchPanel does not support sensitivity override: Write timeout");
                    return false;
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"Failed to override sensitivity: \n{e}");
                    return false;
                }
                serialStream.Write(encoding.GetBytes("{STAT}"));
                serialSession.DiscardInBuffer();

                MajDebug.Log("TouchPannel connected");
                return true;
            }
        }
        static byte GetSensitivityValue(byte sensor,int sens)
        {
            if (sensor > 0x62 || sensor < 0x41)
                return 0x28;
            if(sensor < 0x49)
            {
                return sens switch
                {
                    -5 => 0x5A,
                    -4 => 0x50,
                    -3 => 0x46,
                    -2 => 0x3C,
                    -1 => 0x32,
                    1  => 0x1E,
                    2  => 0x1A,
                    3  => 0x17,
                    4  => 0x14,
                    5  => 0x0A,
                    _  => 0x28
                };
            }
            else
            {
                return sens switch
                {
                    -5 => 0x46,
                    -4 => 0x3C,
                    -3 => 0x32,
                    -2 => 0x28,
                    -1 => 0x1E,
                    1  => 0x14,
                    2  => 0x0F,
                    3  => 0x0A,
                    4  => 0x05,
                    5  => 0x01,
                    _  => 0x01
                };
            }
        }
    }
}
