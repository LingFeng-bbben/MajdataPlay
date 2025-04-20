using System;
using System.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Diagnostics;
using HidSharp;
using System.IO;
//using Microsoft.Win32;
//using System.Windows.Forms;
//using Application = UnityEngine.Application;
//using System.Security.Policy;
#nullable enable
namespace MajdataPlay.IO
{
    internal static unsafe partial class InputManager
    {
        static class ButtonRing
        {
            public static bool IsConnected { get; private set; } = false;

            static Task _buttonRingUpdateLoop = Task.CompletedTask;

            readonly static bool[] _buttonStates = new bool[12];
            readonly static bool[] _buttonRealTimeStates = new bool[12];
            readonly static bool[] _isBtnHadOn = new bool[12];
            readonly static bool[] _isBtnHadOff = new bool[12];

            readonly static bool[] _isBtnHadOnInternal = new bool[12];
            readonly static bool[] _isBtnHadOffInternal = new bool[12];
            #region Public Methods
            public static void Init()
            {
                if (!_buttonRingUpdateLoop.IsCompleted)
                    return;
                switch(MajEnv.UserSettings.Misc.InputDevice.ButtonRing.Type)
                {
                    case DeviceType.Keyboard:
                        _buttonRingUpdateLoop = Task.Factory.StartNew(KeyboardUpdateLoop, TaskCreationOptions.LongRunning);
                        break;
                    case DeviceType.HID:
                    case DeviceType.IO4:
                        _buttonRingUpdateLoop = Task.Factory.StartNew(HIDUpdateLoop, TaskCreationOptions.LongRunning);
                        break;
                }
            }
            /// <summary>
            /// Update the button ring state of the this frame
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void OnPreUpdate()
            {
                var buttonStates = _buttonStates.AsSpan();
                var isBtnHadOn = _isBtnHadOn.AsSpan();
                var isBtnHadOff = _isBtnHadOff.AsSpan();
                var isBtnHadOnInternal = _isBtnHadOnInternal.AsSpan();
                var isBtnHadOffInternal = _isBtnHadOffInternal.AsSpan();
                var buttonRealTimeStates = _buttonRealTimeStates.AsSpan();

                lock (_buttonRingUpdateLoop)
                {
                    for (var i = 0; i < 12; i++)
                    {
                        isBtnHadOn[i] = isBtnHadOnInternal[i];
                        isBtnHadOff[i] = isBtnHadOffInternal[i];
                        buttonStates[i] = buttonRealTimeStates[i];

                        isBtnHadOnInternal[i] = default;
                        isBtnHadOffInternal[i] = default;
                    }
                }
            }
            /// <summary>
            /// Determines whether the button at the given index was ever ON
            /// during the interval between the two most recent OnPreUpdate calls.
            /// </summary>
            /// <param name="index">
            /// Zero‑based button index (valid range 0–11).
            /// </param>
            /// <returns>
            /// True if the button was ON at any point during that interval; otherwise, false.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsHadOn(int index)
            {
                if (!index.InRange(0, 11))
                    return false;

                return _isBtnHadOn[index];
            }
            /// <summary>
            /// Determines whether the button at the given index is ON in the this frame.
            /// </summary>
            /// <param name="index">
            /// Zero‑based button index (valid range 0–11).
            /// </param>
            /// <returns>
            /// True if the button state is ON in this frame; otherwise, false.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsOn(int index)
            {
                if (!index.InRange(0, 11))
                    return false;

                return _buttonStates[index];
            }
            /// <summary>
            /// Determines whether the button at the given index was ever OFF
            /// during the interval between the two most recent OnPreUpdate calls.
            /// </summary>
            /// <param name="index">
            /// Zero‑based button index (valid range 0–11).
            /// </param>
            /// <returns>
            /// True if the button was OFF at any point during that interval; otherwise, false.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsHadOff(int index)
            {
                if (!index.InRange(0, 11))
                    return false;

                return _isBtnHadOff[index];
            }
            /// <summary>
            /// Determines whether the button at the given index is OFF in the this frame.
            /// </summary>
            /// <param name="index">
            /// Zero‑based button index (valid range 0–11).
            /// </param>
            /// <returns>
            /// True if the button state is OFF in this frame; otherwise, false.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsOff(int index)
            {
                return !IsOn(index);
            }
            /// <summary>
            /// Retrieves the real‑time state of the button at the given index
            /// as read from the IO thread, indicating whether it is currently ON.
            /// </summary>
            /// <param name="index">
            /// Zero‑based button index (valid range 0–11).
            /// </param>
            /// <returns>
            /// True if the button is ON according to the latest IO thread reading; otherwise, false.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsCurrentlyOn(int index)
            {
                if (!index.InRange(0, 11))
                    return false;

                return _buttonRealTimeStates[index];
            }
            /// <summary>
            /// Retrieves the real‑time state of the button at the given index
            /// as read from the IO thread, indicating whether it is currently OFF.
            /// </summary>
            /// <param name="index">
            /// Zero‑based button index (valid range 0–11).
            /// </param>
            /// <returns>
            /// True if the button is OFF according to the latest IO thread reading; otherwise, false.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsCurrentlyOff(int index)
            {
                return !IsCurrentlyOn(index);
            }


            /// <summary>
            /// See also <seealso cref="IsHadOn(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsHadOn(SensorArea area)
            {
                return IsHadOn(GetIndexFromArea(area));
            }
            /// <summary>
            /// See also <seealso cref="IsOn(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsOn(SensorArea area)
            {
                return IsOn(GetIndexFromArea(area));
            }
            /// <summary>
            /// See also <seealso cref="IsHadOff(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsHadOff(SensorArea area)
            {
                return IsHadOff(GetIndexFromArea(area));
            }
            /// <summary>
            /// See also <seealso cref="IsOff(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsOff(SensorArea area)
            {
                return IsOff(GetIndexFromArea(area));
            }
            /// <summary>
            /// See also <seealso cref="IsCurrentlyOn(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsCurrentlyOn(SensorArea area)
            {
                return IsCurrentlyOn(GetIndexFromArea(area));
            }
            /// <summary>
            /// See also <seealso cref="IsCurrentlyOff(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsCurrentlyOff(SensorArea area)
            {
                return IsCurrentlyOff(GetIndexFromArea(area));
            }
            #endregion

            static int GetIndexFromArea(SensorArea area)
            {
                switch (area)
                {
                    case SensorArea.A1:
                    case SensorArea.A2:
                    case SensorArea.A3:
                    case SensorArea.A4:
                    case SensorArea.A5:
                    case SensorArea.A6:
                    case SensorArea.A7:
                    case SensorArea.A8:
                        return (int)area;
                    case SensorArea.Test:
                        return 8;
                    case SensorArea.P1:
                        return 9;
                    case SensorArea.Service:
                        return 10;
                    case SensorArea.P2:
                        return 11;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            static void KeyboardUpdateLoop()
            {
                var currentThread = Thread.CurrentThread;
                var token = MajEnv.GlobalCT;
                var pollingRate = _btnPollingRateMs;
                var stopwatch = new Stopwatch();
                var t1 = stopwatch.Elapsed;
                var buttons = _buttons.Span;

                currentThread.Name = "IO/B Thread";
                currentThread.IsBackground = true;
                currentThread.Priority = MajEnv.UserSettings.Debug.IOThreadPriority;
                stopwatch.Start();
                try
                {
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            var now = MajTimeline.UnscaledTime;

                            for (var i = 0; i < buttons.Length; i++)
                            {
                                var button = buttons[i];
                                var keyCode = button.BindingKey;
                                var state = KeyboardHelper.IsKeyDown(keyCode);
                                _buttonRealTimeStates[i] = state;
                            }
                            IsConnected = true;
                            lock (_buttonRingUpdateLoop)
                            {
                                for (var i = 0; i < 12; i++)
                                {
                                    var state = _buttonRealTimeStates[i];
                                    _isBtnHadOnInternal[i] |= state;
                                    _isBtnHadOffInternal[i] |= !state;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            IsConnected = false;
                            MajDebug.LogError($"From KeyBoard listener: \n{e}");
                        }
                        finally
                        {
                            if (pollingRate.TotalMilliseconds > 0)
                            {
                                var t2 = stopwatch.Elapsed;
                                var elapsed = t2 - t1;
                                t1 = t2;
                                if (elapsed < pollingRate)
                                    Thread.Sleep(pollingRate - elapsed);
                            }
                        }
                    }
                }
                finally
                {
                    IsConnected = false;
                }
            }
            static void HIDUpdateLoop()
            {
                var hidOptions = MajEnv.UserSettings.Misc.InputDevice.ButtonRing.HidOptions;
                var currentThread = Thread.CurrentThread;
                var token = MajEnv.GlobalCT;
                var pollingRate = _btnPollingRateMs;
                var stopwatch = new Stopwatch();
                var t1 = stopwatch.Elapsed;
                var pid = hidOptions.ProductId;
                var vid = hidOptions.VendorId;
                var manufacturer = hidOptions.Manufacturer;
                var deviceType = MajEnv.UserSettings.Misc.InputDevice.ButtonRing.Type;
                var devices = DeviceList.Local.GetHidDevices();
                var deviceName = GetHIDDeviceName(deviceType, manufacturer);
                var hidConfig = new OpenConfiguration();

                hidConfig.SetOption(OpenOption.Exclusive, hidOptions.Exclusice);
                hidConfig.SetOption(OpenOption.Priority, hidOptions.OpenPriority);
                currentThread.Name = "IO/B Thread";
                currentThread.IsBackground = true;
                currentThread.Priority = MajEnv.UserSettings.Debug.IOThreadPriority;
                HidDevice? device = null;
                HidStream? hidStream = null;

                foreach(var d in devices)
                {
                    if (d.ProductID == pid && d.VendorID == vid)
                    {
                        var isMatch = false;
                        if(!string.IsNullOrEmpty(deviceName))
                        {
                            if($"{d.GetManufacturer()} {d.GetProductName()}" == deviceName)
                            {
                                isMatch = true;
                            }
                        }
                        else
                        {
                            isMatch = true;
                        }
                        if(isMatch)
                        {
                            device = d;
                            break;
                        }
                    }
                }
                if(device is null)
                {
                    MajDebug.LogWarning("Hid device not found");
                    return;
                }
                else if(!device.TryOpen(hidConfig, out hidStream))
                {
                    MajDebug.LogError($"cannot open hid device:\n{device}");
                    return;
                }

                try
                {
                    Span<byte> buffer = stackalloc byte[device.GetMaxInputReportLength()];
                    IsConnected = true;
                    stopwatch.Start();
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            var now = MajTimeline.UnscaledTime;
                            hidStream.Read(buffer);
                            switch (deviceType)
                            {
                                case DeviceType.HID:
                                    if (manufacturer == DeviceManufacturer.Adx)
                                    {
                                        AdxHIDDevice.Parse(buffer, _buttonRealTimeStates);
                                    }
                                    else
                                    {
                                        DaoHIDDevice.Parse(buffer, _buttonRealTimeStates);
                                    }
                                    break;
                                case DeviceType.IO4:
                                    AdxIO4Device.Parse(buffer, _buttonRealTimeStates);
                                    break;
                                default:
                                    continue;
                            }
                            IsConnected = true;
                            lock (_buttonRingUpdateLoop)
                            {
                                for (var i = 0; i < 12; i++)
                                {
                                    var state = _buttonRealTimeStates[i];
                                    _isBtnHadOnInternal[i] |= state;
                                    _isBtnHadOffInternal[i] |= !state;
                                }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch(IOException ioE)
                        {
                            IsConnected = false;
                            MajDebug.LogError($"From HID listener: \n{ioE}");
                        }
                        catch (Exception e)
                        {
                            MajDebug.LogError($"From HID listener: \n{e}");
                        }
                        finally
                        {
                            buffer.Clear();
                            if (pollingRate.TotalMilliseconds > 0)
                            {
                                var t2 = stopwatch.Elapsed;
                                var elapsed = t2 - t1;
                                t1 = t2;
                                if (elapsed < pollingRate)
                                    Thread.Sleep(pollingRate - elapsed);
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

            static string GetHIDDeviceName(DeviceType deviceType, DeviceManufacturer manufacturer)
            {
                switch(deviceType)
                {
                    case DeviceType.HID:
                        if(manufacturer == DeviceManufacturer.Adx)
                        {
                            return "MusicGame Composite USB";
                        }
                        else if(manufacturer == DeviceManufacturer.Dao)
                        {
                            return "SkyStar SkyStar";
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    case DeviceType.IO4:
                        return string.Empty;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(deviceType));
                }
            }
            static class AdxHIDDevice
            {
                const int HID_BA1_INDEX = 4;
                const int HID_BA2_INDEX = 3;
                const int HID_BA3_INDEX = 2;
                const int HID_BA4_INDEX = 1;
                const int HID_BA5_INDEX = 8;
                const int HID_BA6_INDEX = 7;
                const int HID_BA7_INDEX = 6;
                const int HID_BA8_INDEX = 5;
                const int HID_TEST_INDEX = 10;
                const int HID_SELECT_P1_INDEX = 9;
                const int HID_SERVICE_INDEX = 12;
                const int HID_SELECT_P2_INDEX = 11;
                public static void Parse(ReadOnlySpan<byte> reportData, Span<bool> buffer)
                {
                    reportData = reportData.Slice(1); // skip report id
                    for (var i = 1; i < 13; i++)
                    {
                        switch(i)
                        {
                            case HID_BA1_INDEX:
                                buffer[0] = reportData[i] == 1;
                                break;
                            case HID_BA2_INDEX:
                                buffer[1] = reportData[i] == 1;
                                break;
                            case HID_BA3_INDEX:
                                buffer[2] = reportData[i] == 1;
                                break;
                            case HID_BA4_INDEX:
                                buffer[3] = reportData[i] == 1;
                                break;
                            case HID_BA5_INDEX:
                                buffer[4] = reportData[i] == 1;
                                break;
                            case HID_BA6_INDEX:
                                buffer[5] = reportData[i] == 1;
                                break;
                            case HID_BA7_INDEX:
                                buffer[6] = reportData[i] == 1;
                                break;
                            case HID_BA8_INDEX:
                                buffer[7] = reportData[i] == 1;
                                break;
                            case HID_TEST_INDEX:
                                buffer[8] = reportData[i] == 1;
                                break;
                            case HID_SELECT_P1_INDEX:
                                buffer[9] = reportData[i] == 1;
                                break;
                            case HID_SERVICE_INDEX:
                                buffer[10] = reportData[i] == 1;
                                break;
                            case HID_SELECT_P2_INDEX:
                                buffer[11] = reportData[i] == 1;
                                break;
                        }
                    }
                }
            }
            static class AdxIO4Device
            {
                const int IO4_BA1_OFFSET = 0b00000100;
                const int IO4_BA2_OFFSET = 0b00001000;
                const int IO4_BA3_OFFSET = 0b00000001;
                const int IO4_BA4_OFFSET = 0b10000000;
                const int IO4_BA5_OFFSET = 0b01000000;
                const int IO4_BA6_OFFSET = 0b00100000;
                const int IO4_BA7_OFFSET = 0b00010000;
                const int IO4_BA8_OFFSET = 0b00001000;
                const int IO4_TEST_OFFSET = 0b00000010;
                const int IO4_SELECT_P1_OFFSET = 0b00000010;
                const int IO4_SERVICE_OFFSET = 0b00000001;
                const int IO4_SELECT_P2_OFFSET = 0b01000000;

                const int IO4_BA1_INDEX = 28;
                const int IO4_BA2_INDEX = 28;
                const int IO4_BA3_INDEX = 28;
                const int IO4_BA4_INDEX = 29;
                const int IO4_BA5_INDEX = 29;
                const int IO4_BA6_INDEX = 29;
                const int IO4_BA7_INDEX = 29;
                const int IO4_BA8_INDEX = 29;
                const int IO4_TEST_INDEX = 29;
                const int IO4_SELECT_P1_INDEX = 28;
                const int IO4_SERVICE_INDEX = 25;
                const int IO4_SELECT_P2_INDEX = 28;
                public static void Parse(ReadOnlySpan<byte> reportData,Span<bool> buffer)
                {
                    reportData = reportData.Slice(1); // skip report id
                    buffer[0] = (~reportData[IO4_BA1_INDEX] & IO4_BA1_OFFSET) != 0;
                    buffer[1] = (~reportData[IO4_BA2_INDEX] & IO4_BA2_OFFSET) != 0;
                    buffer[2] = (~reportData[IO4_BA3_INDEX] & IO4_BA3_OFFSET) != 0;
                    buffer[3] = (~reportData[IO4_BA4_INDEX] & IO4_BA4_OFFSET) != 0;
                    buffer[4] = (~reportData[IO4_BA5_INDEX] & IO4_BA5_OFFSET) != 0;
                    buffer[5] = (~reportData[IO4_BA6_INDEX] & IO4_BA6_OFFSET) != 0;
                    buffer[6] = (~reportData[IO4_BA7_INDEX] & IO4_BA7_OFFSET) != 0;
                    buffer[7] = (~reportData[IO4_BA8_INDEX] & IO4_BA8_OFFSET) != 0;
                    buffer[8] = (reportData[IO4_TEST_INDEX] & IO4_TEST_OFFSET) != 0;
                    buffer[9] = (reportData[IO4_SELECT_P1_INDEX] & IO4_SELECT_P1_OFFSET) != 0;
                    buffer[10] = (reportData[IO4_SERVICE_INDEX] & IO4_SERVICE_OFFSET) != 0;
                    buffer[11] = (reportData[IO4_SELECT_P2_INDEX] & IO4_SELECT_P2_OFFSET) != 0;
                }
            }
            static class DaoHIDDevice
            {
                public static void Parse(ReadOnlySpan<byte> reportData, Span<bool> buffer)
                {
                    const int BUTTON_INDEX = 5;
                    const int SIDE_BUTTON_INDEX = 6;

                    reportData = reportData.Slice(1);// skip report id

                    var btnData = reportData[BUTTON_INDEX];
                    var sideBtnData = reportData[SIDE_BUTTON_INDEX];

                    for (var i = 0; i < 8; i++)
                    {
                        var bit = 1 << i;
                        buffer[i] = (btnData & bit) != 0;
                    }
                    buffer[8] = (sideBtnData & (1 << 3)) != 0;  // TEST
                    buffer[9] = (sideBtnData & (1 << 7)) != 0;  // SELECT P1
                    buffer[10] = (sideBtnData & (1 << 0)) != 0; // SERVICE
                    buffer[11] = (sideBtnData & (1 << 1)) != 0; // SELECT P2
                }
            }
        }
    }
}