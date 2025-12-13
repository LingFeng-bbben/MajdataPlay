using System;
using System.Threading.Tasks;
using MajdataPlay.Utils;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using HidSharp;
using System.IO;
using UnityEngine;
using MajdataPlay.Numerics;
using MajdataPlay.Settings;

//using Microsoft.Win32;
//using System.Windows.Forms;
//using Application = UnityEngine.Application;
//using System.Security.Policy;
#nullable enable
namespace MajdataPlay.IO
{
    internal static unsafe partial class InputManager
    {
        static class TouchPanel
        {
            public static bool IsConnected { get; private set; } = false;

            static SpinLock _syncLock = new();
            static Task _touchPanelUpdateLoop = Task.CompletedTask;

            readonly static bool[] _sensorStates = new bool[35];
            readonly static bool[] _sensorRealTimeStates = new bool[35];
            readonly static bool[] _isSensorHadOn = new bool[35];
            readonly static bool[] _isSensorHadOff = new bool[35];

            readonly static bool[] _isSensorHadOnInternal = new bool[35];
            readonly static bool[] _isSensorHadOffInternal = new bool[35];

            #region Public Methods
            public static void Init()
            {
                if (!_touchPanelUpdateLoop.IsCompleted)
                    return;
                switch (_deviceManufacturer)
                {
                    case DeviceManufacturerOption.Yuan:
                    case DeviceManufacturerOption.General:
                        _touchPanelUpdateLoop = Task.Factory.StartNew(SerialPortUpdateLoop, TaskCreationOptions.LongRunning);
                        break;
                    case DeviceManufacturerOption.Dao:
                        _touchPanelUpdateLoop = Task.Factory.StartNew(SlaveThreadUpdateLoop, TaskCreationOptions.LongRunning);
                        break;
                    default:
                        MajDebug.LogWarning($"Not supported touch panel manufacturer: {MajEnv.Settings.IO.Manufacturer}");
                        break;
                }
            }
            /// <summary>
            /// Update the touchpanel state of the this frame
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void OnPreUpdate()
            {
                ref var @lock = ref _syncLock;
                var isLocked = false;
                try
                {
                    @lock.Enter(ref isLocked);
                    var hadOn = _isSensorHadOn.AsSpan();
                    var hadOff = _isSensorHadOff.AsSpan();
                    var states = _sensorStates.AsSpan();
                    var hadOnInternal = _isSensorHadOnInternal.AsSpan();
                    var hadOffInternal = _isSensorHadOffInternal.AsSpan();
                    var realTimeStates = _sensorRealTimeStates.AsSpan();

                    hadOnInternal.CopyTo(hadOn);
                    hadOffInternal.CopyTo(hadOff);
                    realTimeStates.CopyTo(states);

                    hadOnInternal.Clear();
                    hadOffInternal.Clear();
                }
                finally
                {
                    if(isLocked)
                    {
                        @lock.Exit();
                    }
                }
            }
            /// <summary>
            /// See also <seealso cref="IsHadOn(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsHadOn(SensorArea area)
            {
                if(area < SensorArea.C)
                {
                    return _isSensorHadOn[(int)area];
                }
                else if(area == SensorArea.C)
                {
                    return _isSensorHadOn[16] || _isSensorHadOn[17];
                }
                else if(area <= SensorArea.E8)
                {
                    return _isSensorHadOn[(int)area + 1];
                }
                return false;
            }
            /// <summary>
            /// See also <seealso cref="IsOn(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsOn(SensorArea area)
            {
                if (area < SensorArea.C)
                {
                    return _sensorStates[(int)area];
                }
                else if (area == SensorArea.C)
                {
                    return _sensorStates[16] || _sensorStates[17];
                }
                else if (area <= SensorArea.E8)
                {
                    return _sensorStates[(int)area + 1];
                }
                return false;
            }
            /// <summary>
            /// See also <seealso cref="IsHadOff(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsHadOff(SensorArea area)
            {
                if (area < SensorArea.C)
                {
                    return _isSensorHadOff[(int)area];
                }
                else if (area == SensorArea.C)
                {
                    return _isSensorHadOff[16] && _isSensorHadOff[17];
                }
                else if (area <= SensorArea.E8)
                {
                    return _isSensorHadOff[(int)area + 1];
                }
                return false;
            }
            /// <summary>
            /// See also <seealso cref="IsOff(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsOff(SensorArea area)
            {
                return !IsOn(area);
            }
            /// <summary>
            /// See also <seealso cref="IsCurrentlyOn(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsCurrentlyOn(SensorArea area)
            {
                if (area < SensorArea.C)
                {
                    return _sensorRealTimeStates[(int)area];
                }
                else if (area == SensorArea.C)
                {
                    return _sensorRealTimeStates[16] || _sensorRealTimeStates[17];
                }
                else if (area <= SensorArea.E8)
                {
                    return _sensorRealTimeStates[(int)area + 1];
                }
                return false;
            }
            /// <summary>
            /// See also <seealso cref="IsCurrentlyOff(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsCurrentlyOff(SensorArea area)
            {
                return !IsCurrentlyOn(area);
            }



            /// <summary>
            /// Determines whether the sensor at the given index was ever ON
            /// during the interval between the two most recent OnPreUpdate calls.
            /// </summary>
            /// <param name="index">
            /// Zero‑based sensor index (valid range 0-33).
            /// </param>
            /// <returns>
            /// True if the sensor was ON at any point during that interval; otherwise, false.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsHadOn(int index)
            {
                if (!index.InRange(0, 33))
                    return false;

                return _isSensorHadOn[index];
            }
            /// <summary>
            /// Determines whether the sensor at the given index is ON in the this frame.
            /// </summary>
            /// <param name="index">
            /// Zero‑based sensor index (valid range 0-33).
            /// </param>
            /// <returns>
            /// True if the sensor state is ON in this frame; otherwise, false.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsOn(int index)
            {
                if (!index.InRange(0, 33))
                    return false;

                return _sensorStates[index];
            }
            /// <summary>
            /// Determines whether the sensor at the given index was ever OFF
            /// during the interval between the two most recent OnPreUpdate calls.
            /// </summary>
            /// <param name="index">
            /// Zero‑based sensor index (valid range 0-33).
            /// </param>
            /// <returns>
            /// True if the sensor was OFF at any point during that interval; otherwise, false.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsHadOff(int index)
            {
                if (!index.InRange(0, 33))
                    return false;

                return _isSensorHadOff[index];
            }
            /// <summary>
            /// Determines whether the sensor at the given index is OFF in the this frame.
            /// </summary>
            /// <param name="index">
            /// Zero‑based sensor index (valid range 0-33).
            /// </param>
            /// <returns>
            /// True if the sensor state is OFF in this frame; otherwise, false.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsOff(int index)
            {
                return !IsOn(index);
            }
            /// <summary>
            /// Retrieves the real‑time state of the sensor at the given index
            /// as read from the IO thread, indicating whether it is currently ON.
            /// </summary>
            /// <param name="index">
            /// Zero‑based sensor index (valid range 0-33).
            /// </param>
            /// <returns>
            /// True if the sensor is ON according to the latest IO thread reading; otherwise, false.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsCurrentlyOn(int index)
            {
                if (!index.InRange(0, 33))
                    return false;

                return _sensorRealTimeStates[index];
            }
            /// <summary>
            /// Retrieves the real‑time state of the sensor at the given index
            /// as read from the IO thread, indicating whether it is currently OFF.
            /// </summary>
            /// <param name="index">
            /// Zero‑based sensor index (valid range 0-33).
            /// </param>
            /// <returns>
            /// True if the sensor is OFF according to the latest IO thread reading; otherwise, false.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsCurrentlyOff(int index)
            {
                return !IsCurrentlyOn(index);
            }
#endregion

            static void SerialPortUpdateLoop()
            {
                const int RECONNECT_INTERVAL = 1000;

                ref var @lock = ref _syncLock;
                var serialPortOptions = _touchPanelSerialConnInfo;
                var currentThread = Thread.CurrentThread;
                var token = MajEnv.GlobalCT;
                var pollingRate = _sensorPollingRateMs;
                var comPort = $"COM{serialPortOptions.Port}";
                var stopwatch = new Stopwatch();
                var t1 = stopwatch.Elapsed;
                var isReconnecting = false;

                currentThread.Name = "IO/T Thread";
                currentThread.IsBackground = true;
                currentThread.Priority = MajEnv.THREAD_PRIORITY_IO;
                stopwatch.Start();

                var serial = new SerialPort(comPort, serialPortOptions.BaudRate);
                serial.ReadTimeout = 2000;
                serial.WriteTimeout = 2000;
            SERIAL_START:
                try
                {
                    if (!EnsureSerialStreamIsOpen(serial))
                    {
                        if (!isReconnecting)
                        {
                            MajDebug.LogWarning($"TouchPanel: Cannot open {comPort}, using Mouse as fallback.");
                            return;
                        }
                        else
                        {
                            MajDebug.LogError($"TouchPanel: Cannot open {comPort}");
                            Thread.Sleep(RECONNECT_INTERVAL);
                            goto SERIAL_START;
                        }
                    }
                    IsConnected = true;
                    isReconnecting = true;
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            if (!EnsureSerialStreamIsOpen(serial))
                            {
                                IsConnected = false;
                                continue;
                            }
                            ReadFromSerialPort(serial, _sensorRealTimeStates);
                            IsConnected = true;
                            var isLocked = false;
                            try
                            {
                                @lock.Enter(ref isLocked);
                                var sensorRealTimeStates = _sensorRealTimeStates.AsSpan();
                                var isSensorHadOnInternal = _isSensorHadOnInternal.AsSpan();
                                var isSensorHadOffInternal = _isSensorHadOffInternal.AsSpan();

                                for (var i = 0; i < 35; i++)
                                {
                                    var state = sensorRealTimeStates[i];
                                    isSensorHadOnInternal[i] |= state;
                                    isSensorHadOffInternal[i] |= !state;
                                }
                            }
                            finally
                            {
                                if (isLocked)
                                {
                                    @lock.Exit();
                                }
                            }
                        }
                        catch (IOException e)
                        {
                            IsConnected = false;
                            MajDebug.LogError($"TouchPanel: \n{e}");
                            MajDebug.LogInfo($"TouchPanel: Trying to reconnect to {comPort}");
                            Thread.Sleep(RECONNECT_INTERVAL);
                            goto SERIAL_START;
                        }
                        catch (TimeoutException)
                        {
                            IsConnected = false;
                            MajDebug.LogError($"TouchPanel: Read timeout");
                        }
                        catch (Exception e)
                        {
                            IsConnected = false;
                            MajDebug.LogError($"TouchPanel: \n{e}");
                        }
                        finally
                        {
                            if (pollingRate.TotalMilliseconds > 0)
                            {
                                var t2 = stopwatch.Elapsed;
                                var elapsed = t2 - t1;
                                t1 = t2;
                                if (elapsed < pollingRate)
                                {
                                    Thread.Sleep(pollingRate - elapsed);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    _useDummy = true;
                    IsConnected = false;
                    serial.Dispose();
                }
            }
            //static void HIDUpdateLoop()
            //{
            //    var touchPanelOptions = MajEnv.UserSettings.IO.InputDevice.TouchPanel;
            //    var hidOptions = touchPanelOptions.HidOptions;
            //    var currentThread = Thread.CurrentThread;
            //    var token = MajEnv.GlobalCT;
            //    var pollingRate = _sensorPollingRateMs;
            //    var stopwatch = new Stopwatch();
            //    var t1 = stopwatch.Elapsed;
            //    var pid = hidOptions.ProductId;
            //    var vid = hidOptions.VendorId;
            //    var manufacturer = hidOptions.Manufacturer;
            //    var deviceType = MajEnv.UserSettings.IO.InputDevice.TouchPanel.Type;
            //    var deviceName = string.IsNullOrEmpty(hidOptions.DeviceName) ? GetHIDDeviceName(deviceType, manufacturer) : hidOptions.DeviceName;
            //    var hidConfig = new OpenConfiguration();
            //    var filter = new DeviceFilter()
            //    {
            //        DeviceName = deviceName,
            //        ProductId = pid,
            //        VendorId = vid,
            //    };

                
            //    currentThread.Name = "IO/T Thread";
            //    currentThread.IsBackground = true;
            //    currentThread.Priority = MajEnv.UserSettings.Debug.IOThreadPriority;

            //    hidConfig.SetOption(OpenOption.Exclusive, hidOptions.Exclusice);
            //    hidConfig.SetOption(OpenOption.Priority, hidOptions.OpenPriority);
            //    HidDevice? device = null;
            //    HidStream? hidStream = null;

                
            //    if (!HidManager.TryGetDevice(filter,out device))
            //    {
            //        MajDebug.LogWarning("TouchPanel: hid device not found");
            //        return;
            //    }
            //    else if (!device.TryOpen(hidConfig, out hidStream))
            //    {
            //        MajDebug.LogError($"TouchPanel: cannot open hid device:\n{device}");
            //        return;
            //    }

            //    try
            //    {
            //        Span<byte> buffer = stackalloc byte[device.GetMaxInputReportLength()];
            //        IsConnected = true;
            //        MajDebug.Log($"TouchPanel connected\nDevice: {device}");
            //        stopwatch.Start();
            //        while (true)
            //        {
            //            token.ThrowIfCancellationRequested();
            //            try
            //            {
            //                var now = MajTimeline.UnscaledTime;
            //                hidStream.Read(buffer);
            //                DaoHIDTouchPanel.Parse(buffer, _sensorRealTimeStates);
            //                IsConnected = true;
            //                lock (_touchPanelUpdateLoop)
            //                {
            //                    for (var i = 0; i < 35; i++)
            //                    {
            //                        var state = _sensorRealTimeStates[i];
            //                        _isSensorHadOnInternal[i] |= state;
            //                        _isSensorHadOffInternal[i] |= !state;
            //                    }
            //                }
            //            }
            //            catch (OperationCanceledException)
            //            {
            //                break;
            //            }
            //            catch (IOException ioE)
            //            {
            //                IsConnected = false;
            //                MajDebug.LogError($"TouchPanel: from HID listener: \n{ioE}");
            //            }
            //            catch (Exception e)
            //            {
            //                MajDebug.LogError($"TouchPanel: from HID listener: \n{e}");
            //            }
            //            finally
            //            {
            //                buffer.Clear();
            //                if (pollingRate.TotalMilliseconds > 0)
            //                {
            //                    var t2 = stopwatch.Elapsed;
            //                    var elapsed = t2 - t1;
            //                    t1 = t2;
            //                    if (elapsed < pollingRate)
            //                        Thread.Sleep(pollingRate - elapsed);
            //                }
            //            }
            //        }
            //    }
            //    finally
            //    {
            //        hidStream.Dispose();
            //        IsConnected = false;
            //    }
            //}
            static void SlaveThreadUpdateLoop()
            {
                ref var @lock = ref _syncLock;
                var currentThread = Thread.CurrentThread;
                var token = MajEnv.GlobalCT;

                currentThread.Name = "IO/T Thread";
                currentThread.IsBackground = true;
                currentThread.Priority = MajEnv.THREAD_PRIORITY_IO;

                try
                {
                    _ioThreadSync.WaitNotify();
                    ReadOnlySpan<byte> buffer = _ioThreadSync.ReadBuffer;
                    IsConnected = true;
                    MajDebug.LogInfo($"TouchPanel: TouchPanel slave thread has started");
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            _ioThreadSync.WaitNotify();
                            DaoHIDTouchPanel.Parse(buffer, _sensorRealTimeStates);
                            _ioThreadSync.Notify();
                            var isLocked = false;
                            try
                            {
                                @lock.Enter(ref isLocked);
                                var sensorRealTimeStates = _sensorRealTimeStates.AsSpan();
                                var isSensorHadOnInternal = _isSensorHadOnInternal.AsSpan();
                                var isSensorHadOffInternal = _isSensorHadOffInternal.AsSpan();

                                for (var i = 0; i < 35; i++)
                                {
                                    var state = sensorRealTimeStates[i];
                                    isSensorHadOnInternal[i] |= state;
                                    isSensorHadOffInternal[i] |= !state;
                                }
                            }
                            finally
                            {
                                if (isLocked)
                                {
                                    @lock.Exit();
                                }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (IOException ioE)
                        {
                            IsConnected = false;
                            MajDebug.LogError($"TouchPanel: \n{ioE}");
                        }
                        catch (Exception e)
                        {
                            MajDebug.LogError($"TouchPanel: \n{e}");
                        }
                    }
                }
                finally
                {
                    IsConnected = false;
                }
            }
            [MethodImpl(MethodImplOptions.NoInlining)]
            static void ReadFromSerialPort(SerialPort serial, Span<bool> buffer)
            {
                var bytes2Read = serial.BytesToRead;
                if (bytes2Read == 0)
                    return;
                Span<byte> dataBuffer = stackalloc byte[bytes2Read];
                //the SerialPort.BaseStream will be eaten by serialport's own buffer so we dont do that
                var read = serial.Read(dataBuffer);
                GeneralSerialTouchPanel.Parse(dataBuffer, buffer);
            }
            static bool EnsureSerialStreamIsOpen(SerialPort serialSession)
            {
                try
                {
                    if (serialSession.IsOpen)
                    {
                        return true;
                    }
                    else
                    {
                        MajDebug.LogInfo($"TouchPanel: Trying to connect to TouchPannel via {serialSession.PortName}...");
                        serialSession.Open();
                        var encoding = Encoding.ASCII;
                        var sensConfig = MajEnv.Settings.IO.InputDevice.TouchPanel.Sensitivities;
                        var index = _playerIndex == 1 ? 'L' : 'R';
                        //see also https://github.com/Sucareto/Mai2Touch/tree/main/Mai2Touch

                        serialSession.Write(encoding.GetBytes("{RSET}"));
                        serialSession.Write(encoding.GetBytes("{HALT}"));

                        //send ratio
                        for (byte a = 0x41; a <= 0x62; a++)
                        {
                            serialSession.Write(encoding.GetBytes($"{{{index}{(char)a}r2}}"));
                        }
                        try
                        {
                            var sens = (sensConfig.A, sensConfig.B, sensConfig.C, sensConfig.D, sensConfig.E);
                            MajDebug.LogInfo($"TouchPanel: Sensitivities:\nA:{sens.A}\nB:{sens.B}\nC:{sens.C}\nD:{sens.D}\nE:{sens.E}");
                            for (byte a = 0x41; a <= 0x62; a++)
                            {
                                var value = GetSensitivityValue(a, sens);

                                serialSession.Write(encoding.GetBytes($"{{{index}{(char)a}k{(char)value}}}"));
                            }
                        }
                        catch (TimeoutException)
                        {
                            MajDebug.LogWarning($"TouchPanel: TouchPanel does not support sensitivity override: Write timeout");
                            return false;
                        }
                        catch (Exception e)
                        {
                            MajDebug.LogError($"TouchPanel: Failed to override sensitivity: \n{e}");
                            return false;
                        }
                        serialSession.Write(encoding.GetBytes("{STAT}"));
                        serialSession.DiscardInBuffer();

                        MajDebug.LogInfo("TouchPanel: Connected");
                        return true;
                    }
                }
                catch (Exception e)
                {
                    MajDebug.LogException(e);
                    return false;
                }
            }
            static byte GetSensitivityValue(byte sensor, (short A, short B, short C, short D, short E) sens)
            {
                const int A1 = 0x41;
                const int A2 = 0x42;
                const int A3 = 0x43;
                const int A4 = 0x44;
                const int A5 = 0x45;
                const int A6 = 0x46;
                const int A7 = 0x47;
                const int A8 = 0x48;

                const int B1 = 0x49;
                const int B2 = 0x4A;
                const int B3 = 0x4B;
                const int B4 = 0x4C;
                const int B5 = 0x4D;
                const int B6 = 0x4E;
                const int B7 = 0x4F;
                const int B8 = 0x50;

                const int C1 = 0x51;
                const int C2 = 0x52;

                const int D1 = 0x53;
                const int D2 = 0x54;
                const int D3 = 0x55;
                const int D4 = 0x56;
                const int D5 = 0x57;
                const int D6 = 0x58;
                const int D7 = 0x59;
                const int D8 = 0x5A;

                const int E1 = 0x5B;
                const int E2 = 0x5C;
                const int E3 = 0x5D;
                const int E4 = 0x5E;
                const int E5 = 0x5F;
                const int E6 = 0x60;
                const int E7 = 0x61;
                const int E8 = 0x62;

                var (A, B, C, D, E) = sens;
                var s = 0;
                
                switch(sensor)
                {
                    case A1:
                    case A2:
                    case A3:
                    case A4:
                    case A5:
                    case A6:
                    case A7:
                    case A8:
                        s = A;
                        goto SENS_A_RETURN;
                    case B1:
                    case B2:
                    case B3:
                    case B4:
                    case B5:
                    case B6:
                    case B7:
                    case B8:
                        s = B;
                        goto SENS_B_C_D_E_RETURN;
                    case C1:
                    case C2:
                        s = C;
                        goto SENS_B_C_D_E_RETURN;
                    case D1:
                    case D2:
                    case D3:
                    case D4:
                    case D5:
                    case D6:
                    case D7:
                    case D8:
                        s = D;
                        goto SENS_B_C_D_E_RETURN;
                    case E1:
                    case E2:
                    case E3:
                    case E4:
                    case E5:
                    case E6:
                    case E7:
                    case E8:
                        s = E;
                        goto SENS_B_C_D_E_RETURN;
                    SENS_A_RETURN:
                        return s switch
                        {
                            -5 => 0x5A, // -5
                            -4 => 0x50, // -4
                            -3 => 0x46, // -3
                            -2 => 0x3C, // -2
                            -1 => 0x32, // -1
                            1 => 0x1E,  // +1
                            2 => 0x1A,  // +2
                            3 => 0x17,  // +3
                            4 => 0x14,  // +4
                            5 => 0x0A,  // +5
                            _ => 0x28   // 0
                        };
                    SENS_B_C_D_E_RETURN:
                        return s switch
                        {
                            -5 => 0x46, // -5
                            -4 => 0x3C, // -4
                            -3 => 0x32, // -3
                            -2 => 0x28, // -2
                            -1 => 0x1E, // -1
                            1 => 0x0F,  // +1
                            2 => 0x0A,  // +2
                            3 => 0x05,  // +3
                            4 => 0x01,  // +4
                            5 => 0x01,  // +5
                            _ => 0x14   // 0
                        };
                    default:
                        return 0x28;
                }
            }
            static class GeneralSerialTouchPanel
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Parse(ReadOnlySpan<byte> packet, Span<bool> buffer)
                {
                    if (packet.IsEmpty)
                        return;
                    var now = MajTimeline.UnscaledTime;
                    Span<int> startIndexs = stackalloc int[packet.Length];
                    int x = -1;
                    for (var y = 0; y < packet.Length; y++)
                    {
                        var @byte = packet[y];
                        if (@byte == '(')
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
                                var state = rawState > 0;
                                buffer[k] = state;
                                k++;
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
                    if (endIndex == -1)
                    {
                        return ReadOnlySpan<byte>.Empty;
                    }
                    return packet[(start + 1)..endIndex];
                }
                
            }
            static class DaoHIDTouchPanel
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void Parse(ReadOnlySpan<byte> reportData, Span<bool> buffer)
                {
                    reportData = reportData.Slice(1); //skip report id
                    var A = reportData[0];
                    var B = reportData[1];
                    var C = reportData[2];
                    var D = reportData[3];
                    var E = reportData[4];

                    for (var i = 0; i < 8; i++)
                    {
                        var bit = 1 << i;
                        buffer[i] = (A & bit) != 0;
                        buffer[i + 8] = (B & bit) != 0;
                        buffer[i + 18] = (D & bit) != 0;
                        buffer[i + 26] = (E & bit) != 0;
                    }
                    buffer[16] = (C & (1 << 0)) != 0; //C1
                    buffer[17] = (C & (1 << 1)) != 0; //C2
                }
            }
        }
    }
}