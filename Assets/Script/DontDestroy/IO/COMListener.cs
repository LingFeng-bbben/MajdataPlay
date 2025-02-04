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

                //see https://github.com/Sucareto/Mai2Touch/tree/main/Mai2Touch

                await serialStream.WriteAsync(encoding.GetBytes("{RSET}"));
                await serialStream.WriteAsync(encoding.GetBytes("{HALT}"));

                //send ratio
                for (byte a = 0x41; a <= 0x62; a++)
                {
                    await serialStream.WriteAsync(encoding.GetBytes("{L" + (char)a + "r2}"));
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
    }
}
