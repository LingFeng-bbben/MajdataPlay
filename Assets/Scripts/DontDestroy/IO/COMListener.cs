using MajdataPlay.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    public partial class InputManager : MonoBehaviour
    {
        async void COMReceiveAsync()
        {
            await Task.Yield();

            var token = MajEnv.GlobalCT;
            var pollingRate = _sensorPollingRateMs;
            var comPort = $"COM{MajInstances.Setting.Misc.InputDevice.TouchPanel.COMPort}";
            var stopwatch = new Stopwatch();
            var t1 = stopwatch.Elapsed;
            var reportData = new byte[9];
            var sharedMemoryPool = MemoryPool<byte>.Shared;
            using var serial = new SerialPort(comPort, 9600);

            stopwatch.Start();

            try
            {
                await EnsureTouchPanelSerialStreamIsOpen(serial);
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        var serialStream = await EnsureTouchPanelSerialStreamIsOpen(serial);
                        var bytesToRead = serial.BytesToRead;

                        if (bytesToRead % 9 != 0)
                        {
                            using var bufferOwner = sharedMemoryPool.Rent(bytesToRead);
                            var buffer = bufferOwner.Memory;
                            await serialStream.ReadAsync(buffer);

                            for (var x = 0; x < bytesToRead % 9; x++)
                            {
                                var slicedBuffer = buffer.Slice(x * 9, 9);
                                if (slicedBuffer.Length != 9)
                                    break;
                                slicedBuffer.CopyTo(reportData);
                                if (reportData[0] == '(')
                                {
                                    int k = 0;
                                    for (int i = 1; i < 8; i++)
                                    {
                                        //print(buf[i].ToString("X2"));
                                        for (int j = 0; j < 5; j++)
                                        {
                                            _COMReport[k] = (reportData[i] & 0x01 << j) > 0;
                                            k++;
                                        }
                                    }
                                }
                            }
                            serial.DiscardInBuffer();
                        }
                        else if (bytesToRead != 0)
                        {
                            serial.DiscardInBuffer();
                        }
                    }
                    catch(Exception e)
                    {
                        MajDebug.LogError($"From SerialPort listener: \n{e}");
                    }
                    finally
                    {
                        var t2 = stopwatch.Elapsed;
                        var elapsed = t2 - t1;
                        t1 = t2;
                        if (elapsed < pollingRate)
                            await Task.Delay(pollingRate - elapsed, token);
                    }
                }
            }
            catch(IOException)
            {
                MajDebug.LogWarning($"Cannot open {comPort}, using Mouse as fallback.");
                useDummy = true;
            }
        }
        async ValueTask<Stream> EnsureTouchPanelSerialStreamIsOpen(SerialPort serialSession)
        {
            if(serialSession.IsOpen)
            {
                return serialSession.BaseStream;
            }
            else
            {
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
                //send sensitivity
                //adx have another method to set sens, so we dont do it here
                /*for (byte a = 0x41; a <= 0x62; a++)
                {
                    serial.Write("{L" + (char)a + "k"+sens+"}");
                }*/
                await serialStream.WriteAsync(encoding.GetBytes("{STAT}"));
                serialSession.DiscardInBuffer();

                return serialStream;
            }
        }
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
