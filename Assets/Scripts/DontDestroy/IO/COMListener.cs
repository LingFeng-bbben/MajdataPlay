using MajdataPlay.Extensions;
using MajdataPlay.Types;
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
    public partial class InputManager : MonoBehaviour
    {
        void StartUpdatingTouchPanelState()
        {
            if (!_serialPortUpdateTask.IsCompleted)
                return;
            _serialPortUpdateTask = Task.Factory.StartNew(() =>
            {
                var token = MajEnv.GlobalCT;
                var pollingRate = _sensorPollingRateMs;
                var comPort = $"COM{MajInstances.Setting.Misc.InputDevice.TouchPanel.COMPort}";
                var stopwatch = new Stopwatch();
                var t1 = stopwatch.Elapsed;
                var sharedMemoryPool = MemoryPool<byte>.Shared;
                using var serial = new SerialPort(comPort, MajInstances.Setting.Misc.InputDevice.TouchPanel.BaudRate);

                stopwatch.Start();

                try
                {
                    EnsureTouchPanelSerialStreamIsOpen(serial);
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            var serialStream = EnsureTouchPanelSerialStreamIsOpen(serial);
                            var bytesToRead = serial.BytesToRead;

                            using var bufferOwner = sharedMemoryPool.Rent(bytesToRead);
                            var buffer = bufferOwner.Memory;
                            serialStream.Read(buffer.Span);

                            TouchPannelPacketHandle(buffer.Slice(0, bytesToRead));
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
                catch (IOException)
                {
                    MajDebug.LogWarning($"Cannot open {comPort}, using Mouse as fallback.");
                    useDummy = true;
                }
            });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void TouchPannelPacketHandle(ReadOnlyMemory<byte> packet)
        {
            if (packet.IsEmpty)
                return;
            var now = DateTime.Now;
            var packetSpan = packet.Span;
            Span<int> startIndexs = stackalloc int[packet.Length];
            int x = -1;
            for (var y = 0; y < packetSpan.Length; y++)
            {
                var @byte = packetSpan[y];
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
                        var state = (packetBody[i] & 0x01 << j) > 0;

                        _touchPanelInputBuffer.Enqueue(new ()
                        {
                            Index = k++,
                            State = state ? SensorStatus.On : SensorStatus.Off,
                            Timestamp = now,
                        });
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ReadOnlySpan<byte> GetPacketBody(ReadOnlyMemory<byte> packet, int start)
        {
            var endIndex = -1;
            var packetSpan = packet.Span;
            for (var i = start; i < packetSpan.Length; i++)
            {
                var @byte = packetSpan[i];
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
            return packetSpan[(start + 1)..endIndex];
        }
        async ValueTask<Stream> EnsureTouchPanelSerialStreamIsOpenAsync(SerialPort serialSession)
        {
            if(serialSession.IsOpen)
            {
                return serialSession.BaseStream;
            }
            else
            {
                MajDebug.Log($"TouchPannel was not connected,trying to connect to TouchPannel via {serialSession.PortName}...");
                serialSession.Open();
                var encoding = Encoding.ASCII;
                var serialStream = serialSession.BaseStream;
                var isSensitivityOverride = MajEnv.UserSetting.Misc.InputDevice.TouchPanel.SensitivityOverride;
                var sens = MajEnv.UserSetting.Misc.InputDevice.TouchPanel.Sensitivity;
                var index = MajEnv.UserSetting.Misc.InputDevice.TouchPanel.Index == 1 ? 'L' : 'R'; 
                //see https://github.com/Sucareto/Mai2Touch/tree/main/Mai2Touch

                await serialStream.WriteAsync(encoding.GetBytes("{RSET}"));
                await serialStream.WriteAsync(encoding.GetBytes("{HALT}"));

                //send ratio
                for (byte a = 0x41; a <= 0x62; a++)
                {
                    await serialStream.WriteAsync(encoding.GetBytes($"{{{index}{(char)a}r2}}"));
                }
                if(isSensitivityOverride)
                {
                    try
                    {
                        for (byte a = 0x41; a <= 0x62; a++)
                        {
                            using var cts = new CancellationTokenSource();
                            cts.CancelAfter(3000);

                            var value = GetSensitivityValue(a, sens);
                            await serialStream.WriteAsync(encoding.GetBytes($"{{{index}{(char)a}k{(char)value}}}"), cts.Token);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        MajDebug.LogWarning($"TouchPanel does not support sensitivity override: \"Write timeout\"");
                    }
                    catch (Exception e)
                    {
                        MajDebug.LogError($"Failed to override sensitivity: \n{e}");
                    }
                }

                await serialStream.WriteAsync(encoding.GetBytes("{STAT}"));
                serialSession.DiscardInBuffer();

                MajDebug.Log("TouchPannel connected");
                return serialStream;
            }
        }
        Stream EnsureTouchPanelSerialStreamIsOpen(SerialPort serialSession) => EnsureTouchPanelSerialStreamIsOpenAsync(serialSession).Result;
        byte GetSensitivityValue(byte sensor,int sens)
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
