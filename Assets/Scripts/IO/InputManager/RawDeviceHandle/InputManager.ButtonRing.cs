using System;
using System.Threading.Tasks;
using MajdataPlay.Utils;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Diagnostics;
using HidSharp;
using System.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Settings;

//using Microsoft.Win32;
//using System.Windows.Forms;
//using Application = UnityEngine.Application;
//using System.Security.Policy;
#nullable enable
namespace MajdataPlay.IO
{
    using Unsafe = System.Runtime.CompilerServices.Unsafe;
    internal static unsafe partial class InputManager
    {
        static class ButtonRing
        {
            public static bool IsConnected { get; private set; } = false;

            static SpinLock _syncLock = new();
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
                {
                    return;
                }
                var manufacturer = _deviceManufacturer;
                if (manufacturer == DeviceManufacturerOption.General)
                {
                    switch (_buttonRingDevice)
                    {
                        case ButtonRingDeviceOption.Keyboard:
                            _buttonRingUpdateLoop = Task.Factory.StartNew(KeyboardUpdateLoop, TaskCreationOptions.LongRunning);
                            break;
                        case ButtonRingDeviceOption.HID:
                            _buttonRingUpdateLoop = Task.Factory.StartNew(HIDUpdateLoop, TaskCreationOptions.LongRunning);
                            break;
                        default:
                            MajDebug.LogWarning($"ButtonRing: Not supported button ring device: {_buttonRingDevice}");
                            break;
                    }
                }
                else if (manufacturer is DeviceManufacturerOption.Yuan or DeviceManufacturerOption.Dao)
                {
                    _buttonRingUpdateLoop = Task.Factory.StartNew(HIDUpdateLoop, TaskCreationOptions.LongRunning);
                }
                else
                {
                    MajDebug.LogWarning($"ButtonRing: Not supported button ring manufacturer: {manufacturer}");
                }
            }
            /// <summary>
            /// Update the button ring state of the this frame
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static void OnPreUpdate()
            {
                ref var @lock = ref _syncLock;
                var isLocked = false;
                try
                {
                    @lock.Enter(ref isLocked);
                    var isBtnHadOn = _isBtnHadOn.AsSpan();
                    var isBtnHadOff = _isBtnHadOff.AsSpan();
                    var buttonStates = _buttonStates.AsSpan();
                    var isBtnHadOnInternal = _isBtnHadOnInternal.AsSpan();
                    var isBtnHadOffInternal = _isBtnHadOffInternal.AsSpan();
                    var buttonRealTimeStates = _buttonRealTimeStates.AsSpan();

                    isBtnHadOnInternal.CopyTo(isBtnHadOn);
                    isBtnHadOffInternal.CopyTo(isBtnHadOff);
                    buttonRealTimeStates.CopyTo(buttonStates);

                    isBtnHadOnInternal.Clear();
                    isBtnHadOffInternal.Clear();
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
            public static bool IsHadOn(ButtonZone area)
            {
                return IsHadOn(GetIndexFromArea(area));
            }
            /// <summary>
            /// See also <seealso cref="IsOn(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsOn(ButtonZone area)
            {
                return IsOn(GetIndexFromArea(area));
            }
            /// <summary>
            /// See also <seealso cref="IsHadOff(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsHadOff(ButtonZone area)
            {
                return IsHadOff(GetIndexFromArea(area));
            }
            /// <summary>
            /// See also <seealso cref="IsOff(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsOff(ButtonZone area)
            {
                return IsOff(GetIndexFromArea(area));
            }
            /// <summary>
            /// See also <seealso cref="IsCurrentlyOn(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsCurrentlyOn(ButtonZone area)
            {
                return IsCurrentlyOn(GetIndexFromArea(area));
            }
            /// <summary>
            /// See also <seealso cref="IsCurrentlyOff(int)"/>
            /// </summary>
            /// <param name="area"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsCurrentlyOff(ButtonZone area)
            {
                return IsCurrentlyOff(GetIndexFromArea(area));
            }
            #endregion

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static int GetIndexFromArea(ButtonZone area)
            {
                if(area < ButtonZone.A1 || area > ButtonZone.P2)
                {
                    ThrowHelper.OutOfRange(nameof(area));
                }
                return (int)area;
            }
            static void KeyboardUpdateLoop()
            {
                var currentThread = Thread.CurrentThread;
                var token = MajEnv.GlobalCT;
                var pollingRate = _btnPollingRateMs;
                var stopwatch = new Stopwatch();
                var t1 = stopwatch.Elapsed;
                var gameButtons = _buttons.Span.Slice(0, 8);
                var fnButtons = _buttons.Span.Slice(8);
                var fnBuffer = _buttonRealTimeStates.AsSpan(8);
                ref var @lock = ref _syncLock;

                currentThread.Name = "IO/B Thread";
                currentThread.IsBackground = true;
                currentThread.Priority = MajEnv.THREAD_PRIORITY_IO;
                stopwatch.Start();
                try
                {
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            var now = MajTimeline.UnscaledTime;

                            for (var i = 0; i < gameButtons.Length; i++)
                            {
                                var button = gameButtons[i];
                                var keyCode = button.BindingKey;
                                var state = KeyboardHelper.IsKeyDown(keyCode);
                                _buttonRealTimeStates[i] = state;
                            }
                            fnBuffer.Clear();
                            UpdateKeyboardFn(fnButtons, fnBuffer);
                            IsConnected = true;
                            
                            var isLocked = false;
                            try
                            {
                                @lock.Enter(ref isLocked);
                                var states = _buttonRealTimeStates.AsSpan();
                                var hadOn = _isBtnHadOnInternal.AsSpan();
                                var hadOff = _isBtnHadOffInternal.AsSpan();

                                for (int i = 0; i < 12; i++)
                                {
                                    var state = states[i];
                                    hadOn[i] |= state;
                                    hadOff[i] |= !state;
                                }
                            }
                            finally
                            {
                                if(isLocked)
                                {
                                    @lock.Exit();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            IsConnected = false;
                            MajDebug.LogError($"From Keyboard listener: \n{e}");
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
                    IsConnected = false;
                }
            }
            static void HIDUpdateLoop()
            {
                ref var @lock = ref _syncLock;
                var hidOptions = _buttonRingHidConnInfo;
                var currentThread = Thread.CurrentThread;
                var token = MajEnv.GlobalCT;
                var pollingRate = _btnPollingRateMs;
                var fnButtons = _buttons.Span.Slice(8);
                var fnBuffer = _buttonRealTimeStates.AsSpan(8);
                var stopwatch = new Stopwatch();
                var t1 = stopwatch.Elapsed;
                var pid = hidOptions.ProductId;
                var vid = hidOptions.VendorId;
                var manufacturer = _deviceManufacturer;
                var deviceType = _buttonRingDevice;
                var deviceName = string.IsNullOrEmpty(hidOptions.DeviceName) ? GetHIDDeviceName(deviceType, manufacturer) : hidOptions.DeviceName;
                var hidConfig = new OpenConfiguration();
                var filter = new DeviceFilter()
                {
                    DeviceName = deviceName,
                    ProductId = pid,
                    VendorId = vid,
                };

                hidConfig.SetOption(OpenOption.Exclusive, hidOptions.Exclusice);
                hidConfig.SetOption(OpenOption.Priority, hidOptions.OpenPriority);
                currentThread.Name = "IO/B Thread";
                currentThread.IsBackground = true;
                currentThread.Priority = MajEnv.THREAD_PRIORITY_IO;

                HidDevice? device = null;
                HidStream? hidStream = null;

                if (!HidManager.TryGetDevices(filter, out var devices))
                {
                    MajDebug.LogWarning("ButtonRing: hid device not found");
                    return;
                }
                foreach(var d in devices)
                {
                    if (d.TryOpen(hidConfig, out hidStream))
                    {
                        device = d;
                        break;
                    }
                }
                if(hidStream is null || device is null)
                {
                    MajDebug.LogError($"ButtonRing: cannot open hid devices:\n{string.Join('\n', devices)}");
                    return;
                }

                try
                {
                    Memory<byte> memory = new byte[device.GetMaxInputReportLength()];
                    _ioThreadSync.ReadBufferMemory = memory;
                    _ioThreadSync.Notify();
                    Span<byte> buffer = memory.Span;
                    IsConnected = true;
                    MajDebug.LogInfo($"ButtonRing: Connected\nDevice: {device}");
                    stopwatch.Start();
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            var now = MajTimeline.UnscaledTime;
                            hidStream.Read(buffer);
                            switch(manufacturer)
                            {
                                case DeviceManufacturerOption.General:
                                    GeneralHIDDevice.Parse(buffer, _buttonRealTimeStates);
                                    break;
                                case DeviceManufacturerOption.Yuan:
                                    AdxHIDDevice.Parse(buffer, _buttonRealTimeStates);
                                    break;
                                case DeviceManufacturerOption.Dao:
                                    _ioThreadSync.Notify();
                                    DaoHIDDevice.Parse(buffer, _buttonRealTimeStates);
                                    _ioThreadSync.WaitNotify();
                                    break;
                            }
                            UpdateKeyboardFn(fnButtons, fnBuffer);
                            IsConnected = true;
                            var isLocked = false;
                            try
                            {
                                @lock.Enter(ref isLocked);
                                var states = _buttonRealTimeStates.AsSpan();
                                var hadOn = _isBtnHadOnInternal.AsSpan();
                                var hadOff = _isBtnHadOffInternal.AsSpan();

                                for (int i = 0; i < 12; i++)
                                {
                                    var state = states[i];
                                    hadOn[i] |= state;
                                    hadOff[i] |= !state;
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
                        catch(IOException ioE)
                        {
                            IsConnected = false;
                            MajDebug.LogError($"ButtonRing: \n{ioE}");
                        }
                        catch (Exception e)
                        {
                            MajDebug.LogError($"ButtonRing: \n{e}");
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
                                {
                                    Thread.Sleep(pollingRate - elapsed);
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
            static void UpdateKeyboardFn(in ReadOnlySpan<Button> fnButtons, in Span<bool> buffer)
            {
                for (var i = 0; i < fnButtons.Length; i++)
                {
                    var button = fnButtons[i];
                    var keyCode = button.BindingKey;
                    var state = KeyboardHelper.IsKeyDown(keyCode);
                    buffer[i] |= state;
                }
            }
            static string GetHIDDeviceName(ButtonRingDeviceOption deviceType, DeviceManufacturerOption manufacturer)
            {
                switch(deviceType)
                {
                    case ButtonRingDeviceOption.HID:
                        if(manufacturer == DeviceManufacturerOption.General)
                        {
                            return string.Empty;
                        }
                        else if(manufacturer == DeviceManufacturerOption.Yuan)
                        {
                            //return "MusicGame Composite USB";
                            return string.Empty;
                        }
                        else if (manufacturer == DeviceManufacturerOption.Dao)
                        {
                            //return "SkyStar Maimoller";
                            return string.Empty;
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    default:
                        throw new NotSupportedException();
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
            static class GeneralHIDDevice
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

                const int IO4_BA1_1P_INDEX = 28;
                const int IO4_BA2_1P_INDEX = 28;
                const int IO4_BA3_1P_INDEX = 28;
                const int IO4_BA4_1P_INDEX = 29;
                const int IO4_BA5_1P_INDEX = 29;
                const int IO4_BA6_1P_INDEX = 29;
                const int IO4_BA7_1P_INDEX = 29;
                const int IO4_BA8_1P_INDEX = 29;

                const int IO4_BA1_2P_INDEX = 30;
                const int IO4_BA2_2P_INDEX = 30;
                const int IO4_BA3_2P_INDEX = 30;
                const int IO4_BA4_2P_INDEX = 31;
                const int IO4_BA5_2P_INDEX = 31;
                const int IO4_BA6_2P_INDEX = 31;
                const int IO4_BA7_2P_INDEX = 31;
                const int IO4_BA8_2P_INDEX = 31;

                const int IO4_TEST_INDEX = 29;
                const int IO4_SELECT_P1_INDEX = 28;
                const int IO4_SERVICE_INDEX = 25;
                const int IO4_SELECT_P2_INDEX = 28;
                public static void Parse(ReadOnlySpan<byte> reportData,Span<bool> buffer)
                {
                    reportData = reportData.Slice(1); // skip report id
                    switch (_playerIndex)
                    {
                        case 1:
                            buffer[0] = (~reportData[IO4_BA1_1P_INDEX] & IO4_BA1_OFFSET) != 0;
                            buffer[1] = (~reportData[IO4_BA2_1P_INDEX] & IO4_BA2_OFFSET) != 0;
                            buffer[2] = (~reportData[IO4_BA3_1P_INDEX] & IO4_BA3_OFFSET) != 0;
                            buffer[3] = (~reportData[IO4_BA4_1P_INDEX] & IO4_BA4_OFFSET) != 0;
                            buffer[4] = (~reportData[IO4_BA5_1P_INDEX] & IO4_BA5_OFFSET) != 0;
                            buffer[5] = (~reportData[IO4_BA6_1P_INDEX] & IO4_BA6_OFFSET) != 0;
                            buffer[6] = (~reportData[IO4_BA7_1P_INDEX] & IO4_BA7_OFFSET) != 0;
                            buffer[7] = (~reportData[IO4_BA8_1P_INDEX] & IO4_BA8_OFFSET) != 0;
                            break;
                        case 2:
                            buffer[0] = (~reportData[IO4_BA1_2P_INDEX] & IO4_BA1_OFFSET) != 0;
                            buffer[1] = (~reportData[IO4_BA2_2P_INDEX] & IO4_BA2_OFFSET) != 0;
                            buffer[2] = (~reportData[IO4_BA3_2P_INDEX] & IO4_BA3_OFFSET) != 0;
                            buffer[3] = (~reportData[IO4_BA4_2P_INDEX] & IO4_BA4_OFFSET) != 0;
                            buffer[4] = (~reportData[IO4_BA5_2P_INDEX] & IO4_BA5_OFFSET) != 0;
                            buffer[5] = (~reportData[IO4_BA6_2P_INDEX] & IO4_BA6_OFFSET) != 0;
                            buffer[6] = (~reportData[IO4_BA7_2P_INDEX] & IO4_BA7_OFFSET) != 0;
                            buffer[7] = (~reportData[IO4_BA8_2P_INDEX] & IO4_BA8_OFFSET) != 0;
                            break;
                    }
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
                    buffer[9] = (sideBtnData & (1 << 3)) != 0;  // SELECT P1
                    buffer[8] = (sideBtnData & (1 << 2)) != 0;  // TEST
                    buffer[11] = (sideBtnData & (1 << 1)) != 0; // SELECT P2
                    buffer[10] = (sideBtnData & (1 << 0)) != 0; // SERVICE
                }
            }
        }
    }
}