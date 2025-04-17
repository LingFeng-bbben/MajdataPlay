using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using MychIO;
using DeviceType = MajdataPlay.IO.DeviceType;
using MychIO.Device;
using System.Collections.Generic;
using MychIO.Event;
using System.Runtime.CompilerServices;
using MajdataPlay.Collections;
using System.Collections.Concurrent;
using System.Security.Policy;
using System.Threading;
using System.Diagnostics;
using HidSharp;
//using Microsoft.Win32;
//using System.Windows.Forms;
//using Application = UnityEngine.Application;
//using System.Security.Policy;
#nullable enable
namespace MajdataPlay.IO
{
    internal static unsafe partial class InputManager
    {
        public static bool IsTouchPanelConnected { get; private set; } = false;
        public static float FingerRadius
        {
            get
            {
                return MajEnv.UserSettings.Misc.InputDevice.TouchPanel.TouchSimulationRadius;
            }
        }
        public static ReadOnlySpan<SensorStatus> ButtonStatusInThisFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _btnStatusInThisFrame;
        }
        public static ReadOnlySpan<SensorStatus> ButtonStatusInPreviousFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _btnStatusInPreviousFrame;
        }
        public static ReadOnlySpan<SensorStatus> SensorStatusInThisFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sensorStatusInThisFrame;
        }
        public static ReadOnlySpan<SensorStatus> SensorStatusInPreviousFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sensorStatusInPreviousFrame;
        }
        public static ReadOnlySpan<bool> TouchPanelRawData
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _sensorStates.Span;
            }
        }

        public static event EventHandler<InputEventArgs>? OnAnyAreaTrigger;

        readonly static TimeSpan _btnDebounceThresholdMs = TimeSpan.Zero;
        readonly static TimeSpan _sensorDebounceThresholdMs = TimeSpan.Zero;
        readonly static TimeSpan _btnPollingRateMs = TimeSpan.Zero;
        readonly static TimeSpan _sensorPollingRateMs = TimeSpan.Zero;

        readonly static ConcurrentQueue<InputDeviceReport> _touchPanelInputBuffer = new();
        readonly static ConcurrentQueue<InputDeviceReport> _buttonRingInputBuffer = new();

        readonly static ReadOnlyMemory<KeyCode> _bindingKeys = new KeyCode[12]
        {
            KeyCode.B1,
            KeyCode.B2,
            KeyCode.B3,
            KeyCode.B4,
            KeyCode.B5,
            KeyCode.B6,
            KeyCode.B7,
            KeyCode.B8,
            KeyCode.Test,
            KeyCode.SelectP1,
            KeyCode.Service,
            KeyCode.SelectP2
        };
        readonly static ReadOnlyMemory<Button> _buttons = new Button[12]
        {
            new Button(KeyCode.B1,SensorArea.A1),
            new Button(KeyCode.B2,SensorArea.A2),
            new Button(KeyCode.B3,SensorArea.A3),
            new Button(KeyCode.B4,SensorArea.A4),
            new Button(KeyCode.B5,SensorArea.A5),
            new Button(KeyCode.B6,SensorArea.A6),
            new Button(KeyCode.B7,SensorArea.A7),
            new Button(KeyCode.B8,SensorArea.A8),
            new Button(KeyCode.Test,SensorArea.Test),
            new Button(KeyCode.SelectP1,SensorArea.P1),
            new Button(KeyCode.Service,SensorArea.Service),
            new Button(KeyCode.SelectP2,SensorArea.P2),
        };
        readonly static TimeSpan[] _btnLastTriggerTimes = new TimeSpan[8];
        readonly static SensorStatus[] _btnStatusInPreviousFrame = new SensorStatus[12];
        readonly static SensorStatus[] _btnStatusInThisFrame = new SensorStatus[12];

        readonly static ReadOnlyMemory<Sensor> _sensors = new Sensor[33]
        {
            new Sensor()
            {
                Area = SensorArea.A1,
            },
            new Sensor()
            {
                Area = SensorArea.A2,
            },
            new Sensor()
            {
                Area = SensorArea.A3,
            },
            new Sensor()
            {
                Area = SensorArea.A4,
            },
            new Sensor()
            {
                Area = SensorArea.A5,
            },
            new Sensor()
            {
                Area = SensorArea.A6,
            },
            new Sensor()
            {
                Area = SensorArea.A7,
            },
            new Sensor()
            {
                Area = SensorArea.A8,
            },
            new Sensor()
            {
                Area = SensorArea.B1,
            },
            new Sensor()
            {
                Area = SensorArea.B2,
            },
            new Sensor()
            {
                Area = SensorArea.B3,
            },
            new Sensor()
            {
                Area = SensorArea.B4,
            },
            new Sensor()
            {
                Area = SensorArea.B5,
            },
            new Sensor()
            {
                Area = SensorArea.B6,
            },
            new Sensor()
            {
                Area = SensorArea.B7,
            },
            new Sensor()
            {
                Area = SensorArea.B8,
            },
            new Sensor()
            {
                Area = SensorArea.C,
            },
            new Sensor()
            {
                Area = SensorArea.D1,
            },
            new Sensor()
            {
                Area = SensorArea.D2,
            },
            new Sensor()
            {
                Area = SensorArea.D3,
            },
            new Sensor()
            {
                Area = SensorArea.D4,
            },
            new Sensor()
            {
                Area = SensorArea.D5,
            },
            new Sensor()
            {
                Area = SensorArea.D6,
            },
            new Sensor()
            {
                Area = SensorArea.D7,
            },
            new Sensor()
            {
                Area = SensorArea.D8,
            },
            new Sensor()
            {
                Area = SensorArea.E1,
            },
            new Sensor()
            {
                Area = SensorArea.E2,
            },
            new Sensor()
            {
                Area = SensorArea.E3,
            },
            new Sensor()
            {
                Area = SensorArea.E4,
            },
            new Sensor()
            {
                Area = SensorArea.E5,
            },
            new Sensor()
            {
                Area = SensorArea.E6,
            },
            new Sensor()
            {
                Area = SensorArea.E7,
            },
            new Sensor()
            {
                Area = SensorArea.E8,
            },
        };
        readonly static TimeSpan[] _sensorLastTriggerTimes = new TimeSpan[33];
        //The serial port will report the status of 35 zones, but there are actually only 34 zones.
        readonly static Memory<bool> _sensorStates = new bool[35];
        readonly static SensorStatus[] _sensorStatusInPreviousFrame = new SensorStatus[33];
        readonly static SensorStatus[] _sensorStatusInThisFrame = new SensorStatus[33];

        static bool _useDummy = false;
        readonly static bool _isBtnDebounceEnabled = false;
        readonly static bool _isSensorDebounceEnabled = false;
        readonly static bool _isSensorRendererEnabled = false;

        static Task _serialPortUpdateTask = Task.CompletedTask;
        static Task _buttonRingUpdateTask = Task.CompletedTask;

        static IOManager? _ioManager = null;

        static delegate*<void> _updateIOListenerPtr = &DefaultIOListener;
        static IReadOnlyDictionary<int, int> _instanceID2SensorIndexMappingTable = new Dictionary<int, int>();
        static InputManager()
        {
            _isSensorRendererEnabled = MajEnv.UserSettings.Debug.DisplaySensor;
            _btnDebounceThresholdMs = TimeSpan.FromMilliseconds(MajInstances.Settings.Misc.InputDevice.ButtonRing.DebounceThresholdMs);
            _btnPollingRateMs = TimeSpan.FromMilliseconds(MajInstances.Settings.Misc.InputDevice.ButtonRing.PollingRateMs);
            _sensorDebounceThresholdMs = TimeSpan.FromMilliseconds(MajInstances.Settings.Misc.InputDevice.TouchPanel.DebounceThresholdMs);
            _sensorPollingRateMs = TimeSpan.FromMilliseconds(MajInstances.Settings.Misc.InputDevice.TouchPanel.PollingRateMs);
            _isBtnDebounceEnabled = MajInstances.Settings.Misc.InputDevice.ButtonRing.Debounce;
            _isSensorDebounceEnabled = MajInstances.Settings.Misc.InputDevice.TouchPanel.Debounce;
            for (var i = 0; i < 33; i++)
            {
                if (i.InRange(0, 7))
                {
                    _btnLastTriggerTimes[i] = TimeSpan.Zero;
                }
                _sensorLastTriggerTimes[i] = TimeSpan.Zero;
            }
            switch (MajInstances.Settings.Misc.InputDevice.ButtonRing.Type)
            {
                case DeviceType.Keyboard:
                    StartInternalIOManager();
                    _updateIOListenerPtr = &UpdateInternalIOListener;
                    break;
                case DeviceType.IO4:
                case DeviceType.HID:
                    StartExternalIOManager();
                    _updateIOListenerPtr = &UpdateExternalIOListener;
                    break;
            }
            MajEnv.OnApplicationQuit += OnApplicationQuit;
        }
        internal static void Init(IReadOnlyDictionary<int, int> instanceID2SensorIndexMappingTable)
        {
            Input.multiTouchEnabled = true;
            _instanceID2SensorIndexMappingTable = instanceID2SensorIndexMappingTable;
        }
        internal static void OnFixedUpdate()
        {
            //_updateIOListener();
        }
        internal static void OnPreUpdate()
        {
            _updateIOListenerPtr();
            var buttons = _buttons.Span;
            var sensors = _sensors.Span;
            for (var i = 0; i < 12; i++)
            {
                var btn = buttons[i];
                _btnStatusInPreviousFrame[i] = _btnStatusInThisFrame[i];
                _btnStatusInThisFrame[i] = btn.State;
            }
            for (var i = 0; i < 33; i++)
            {
                var sen = sensors[i];
                _sensorStatusInPreviousFrame[i] = _sensorStatusInThisFrame[i];
                _sensorStatusInThisFrame[i] = sen.State;
            }
        }
        static void DefaultIOListener()
        {

        }
        static void StartInternalIOManager()
        {
            StartUpdatingTouchPanelState();
            StartUpdatingKeyboardState();
        }
        static void StartExternalIOManager()
        {
            if(_ioManager is null)
                _ioManager = new();
            Majdata<IOManager>.Instance = _ioManager;
            var useHID = MajInstances.Settings.Misc.InputDevice.ButtonRing.Type is DeviceType.HID;
            var executionQueue = MajEnv.ExecutionQueue;
            var buttonRingCallbacks = new Dictionary<ButtonRingZone, Action<ButtonRingZone, InputState>>();
            var touchPanelCallbacks = new Dictionary<TouchPanelZone, Action<TouchPanelZone, InputState>>();

            foreach (ButtonRingZone zone in Enum.GetValues(typeof(ButtonRingZone)))
            {
                buttonRingCallbacks[zone] = OnButtonRingStateChanged;
            }

            foreach (TouchPanelZone zone in Enum.GetValues(typeof(TouchPanelZone)))
            {
                touchPanelCallbacks[zone] = OnTouchPanelStateChanged;
            }

            
            _ioManager.Destroy();
            _ioManager.SubscribeToAllEvents(ExternalIOEventHandler);
            _ioManager.AddDeviceErrorHandler(new DeviceErrorHandler(_ioManager, StartExternalIOManager, 4));

            try
            {
                var deviceName = useHID ? AdxHIDButtonRing.GetDeviceName() : AdxIO4ButtonRing.GetDeviceName();
                var btnDebounce = MajInstances.Settings.Misc.InputDevice.ButtonRing.Debounce;
                var touchPanelDebounce = MajInstances.Settings.Misc.InputDevice.TouchPanel.Debounce;

                var btnProductId = MajInstances.Settings.Misc.InputDevice.ButtonRing.ProductId;
                var btnVendorId = MajInstances.Settings.Misc.InputDevice.ButtonRing.VendorId;
                var comPortNum = MajInstances.Settings.Misc.InputDevice.TouchPanel.COMPort;

                var btnPollingRate = MajInstances.Settings.Misc.InputDevice.ButtonRing.PollingRateMs;
                //var btnDebounceThresholdMs = btnDebounce ? MajInstances.Setting.Misc.InputDevice.ButtonRing.DebounceThresholdMs : 0;
                var btnDebounceThresholdMs = 0;

                var touchPanelPollingRate = MajInstances.Settings.Misc.InputDevice.TouchPanel.PollingRateMs;
                //var touchPanelDebounceThresholdMs = touchPanelDebounce ? MajInstances.Setting.Misc.InputDevice.TouchPanel.DebounceThresholdMs : 0;
                var touchPanelDebounceThresholdMs = 0;

                var btnConnProperties = new Dictionary<string, dynamic>()
                {
                    { "PollingRateMs", btnPollingRate },
                    { "DebounceTimeMs", btnDebounceThresholdMs },
                    { "ProductId", btnProductId },
                    { "VendorId", btnVendorId },
                };
                var touchPanelConnProperties = new Dictionary<string, dynamic>()
                {
                    { "PollingRateMs", touchPanelPollingRate },
                    { "DebounceTimeMs", touchPanelDebounceThresholdMs },
                    { "ComPortNumber", $"COM{comPortNum}" },
                    { "BaudRate", MajInstances.Settings.Misc.InputDevice.TouchPanel.BaudRate },
                    { "SensitivityOverride", true },
                    { "Sensitivity", MajInstances.Settings.Misc.InputDevice.TouchPanel.Sensitivity }
                };
                var ledConnProperties = new Dictionary<string, dynamic>()
                {
                    { "ComPortNumber", $"COM{MajInstances.Settings.Misc.OutputDevice.Led.COMPort}" },
                    { "BaudRate", MajInstances.Settings.Misc.OutputDevice.Led.BaudRate }
                };

                _ioManager.AddButtonRing(deviceName,
                                         inputSubscriptions: buttonRingCallbacks,
                                         connectionProperties: btnConnProperties);
                _ioManager.AddTouchPanel(AdxTouchPanel.GetDeviceName(),
                                         inputSubscriptions: touchPanelCallbacks,
                                         connectionProperties: touchPanelConnProperties);
                _ioManager.AddLedDevice(AdxLedDevice.GetDeviceName(),
                                        connectionProperties: ledConnProperties);
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void UpdateInternalIOListener()
        {
            try
            {
                var executionQueue = MajEnv.ExecutionQueue;

                if (_useDummy)
                {
                    UpdateMousePosition();
                }
                else
                {
                    UpdateSensorState();
                }
                UpdateButtonState();
                while (executionQueue.TryDequeue(out var eventAction))
                {
                    eventAction();
                }
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void UpdateExternalIOListener()
        {
            var executionQueue = MajEnv.ExecutionQueue;
            try
            {
                while (executionQueue.TryDequeue(out var eventAction))
                    eventAction();
                UpdateSensorState();
                UpdateButtonState();
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
            }
        }
        static void ExternalIOEventHandler(IOEventType eventType,DeviceClassification deviceType,string msg)
        {
            var logContent = $"From external IOManager:\nEventType: {eventType}\nDeviceType: {deviceType}\nMsg: {msg.Trim()}";
            switch (eventType)
            {
                case IOEventType.Attach:
                case IOEventType.Debug:
                    MajDebug.Log(logContent);
                    break;
                case IOEventType.ConnectionError:
                case IOEventType.SerialDeviceReadError:
                case IOEventType.HidDeviceReadError:
                case IOEventType.ReconnectionError:
                case IOEventType.InvalidDevicePropertyError:
                    MajDebug.LogError(logContent);
                    break;
                case IOEventType.Detach:
                    MajDebug.LogWarning(logContent);
                    break;
            }
        }
        public static void BindAnyArea(EventHandler<InputEventArgs> checker) => OnAnyAreaTrigger += checker;
        public static void BindArea(EventHandler<InputEventArgs> checker, SensorArea sType)
        {
            var sensor = GetSensor(sType);
            var button = GetButton(sType);
            if (sensor == null || button is null)
                throw new Exception($"{sType} Sensor or Button not found.");

            sensor.AddSubscriber(checker);
            button.AddSubscriber(checker);
        }
        public static void UnbindAnyArea(EventHandler<InputEventArgs> checker) => OnAnyAreaTrigger -= checker;
        public static void UnbindArea(EventHandler<InputEventArgs> checker, SensorArea sType)
        {
            var sensor = GetSensor(sType);
            var button = GetButton(sType);
            if (sensor is null || button is null)
                throw new Exception($"{sType} Sensor or Button not found.");

            sensor.RemoveSubscriber(checker);
            button.RemoveSubscriber(checker);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckAreaStatus(SensorArea sType, SensorStatus targetStatus)
        {
            return CheckSensorStatus(sType,targetStatus) || CheckButtonStatus(sType, targetStatus);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckSensorStatus(SensorArea target, SensorStatus targetStatus)
        {
            ThrowIfSensorIndexOutOfRange(target);

            var sensor = _sensors.Span[(int)target];
            if (sensor is null)
                throw new Exception($"{target} Sensor or Button not found.");

            return sensor.State == targetStatus;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckButtonStatus(SensorArea target, SensorStatus targetStatus)
        {
            ThrowIfButtonIndexOutOfRange(target);
            var button = GetButton(target);

            if (button is null)
                throw new Exception($"{target} Button not found.");

            return button.State == targetStatus;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckButtonStatusInThisFrame(SensorArea target, SensorStatus targetStatus)
        {
            ThrowIfButtonIndexOutOfRange(target);
            var index = GetButtonIndex(target);

            return _btnStatusInThisFrame[index] == targetStatus;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckButtonStatusInPreviousFrame(SensorArea target, SensorStatus targetStatus)
        {
            ThrowIfButtonIndexOutOfRange(target);
            var index = GetButtonIndex(target);

            return _btnStatusInPreviousFrame[index] == targetStatus;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SensorStatus GetButtonStatusInThisFrame(SensorArea target)
        {
            ThrowIfButtonIndexOutOfRange(target);
            var index = GetButtonIndex(target);

            return _btnStatusInThisFrame[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SensorStatus GetButtonStatusInPreviousFrame(SensorArea target)
        {
            ThrowIfButtonIndexOutOfRange(target);
            var index = GetButtonIndex(target);

            return _btnStatusInPreviousFrame[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsButtonClickedInThisFrame(SensorArea target)
        {
            ThrowIfButtonIndexOutOfRange(target);
            var index = GetButtonIndex(target);

            return _btnStatusInPreviousFrame[index] == SensorStatus.Off &&
                   _btnStatusInThisFrame[index] == SensorStatus.On;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckSensorStatusInThisFrame(SensorArea target, SensorStatus targetStatus)
        {
            ThrowIfSensorIndexOutOfRange(target);
            var index = (int)target;

            return _sensorStatusInThisFrame[index] == targetStatus;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckSensorStatusInPreviousFrame(SensorArea target, SensorStatus targetStatus)
        {
            ThrowIfSensorIndexOutOfRange(target);
            var index = (int)target;

            return _sensorStatusInPreviousFrame[index] == targetStatus;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SensorStatus GetSensorStatusInThisFrame(SensorArea target)
        {
            ThrowIfSensorIndexOutOfRange(target);
            var index = (int)target;

            return _sensorStatusInThisFrame[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SensorStatus GetSensorStatusInPreviousFrame(SensorArea target)
        {
            ThrowIfSensorIndexOutOfRange(target);
            var index = (int)target;

            return _sensorStatusInPreviousFrame[index];
        }
        public static SensorArea GetSensorAreaFromInstanceID(int instanceID)
        {
            if(_instanceID2SensorIndexMappingTable.TryGetValue(instanceID,out var index))
            {
                switch(index)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                        return (SensorArea)index;
                    case 17:
                        return SensorArea.C;
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                    case 27:
                    case 28:
                    case 29:
                    case 30:
                    case 31:
                    case 32:
                    case 33:
                        return (SensorArea)(index - 1);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            throw new InvalidOperationException();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSensorClickedInThisFrame(SensorArea target)
        {
            ThrowIfSensorIndexOutOfRange(target);
            var index = (int)target;

            return _sensorStatusInPreviousFrame[index] == SensorStatus.Off &&
                   _sensorStatusInThisFrame[index] == SensorStatus.On;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Button? GetButton(SensorArea type)
        {
            var buttons = _buttons.Span;
            return type switch
            {
                _ when type < SensorArea.A1 => throw new ArgumentOutOfRangeException(),
                _ when type < SensorArea.B1 => buttons[(int)type],
                SensorArea.Test => buttons[8],
                SensorArea.P1 => buttons[9],
                SensorArea.Service => buttons[10],
                SensorArea.P2 => buttons[11],
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ReadOnlyMemory<Button> GetButtons()
        {
            return _buttons;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Sensor GetSensor(SensorArea target)
        {
            return _sensors.Span[(int)target];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ReadOnlyMemory<Sensor> GetSensors()
        {
            return _sensors;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearAllSubscriber()
        {
            foreach(var sensor in _sensors.Span)
                sensor.ClearSubscriber();
            foreach(var button in _buttons.Span)
                button.ClearSubscriber();
            OnAnyAreaTrigger = null;
        }
        static void OnApplicationQuit()
        {
            _ioManager?.Dispose();
            MajEnv.OnApplicationQuit -= OnApplicationQuit;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void PushEvent(InputEventArgs args)
        {
            if (OnAnyAreaTrigger is not null)
                OnAnyAreaTrigger(null, args);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlyMemory<bool> GetTouchPanelRawData() => _sensorStates;
        /// <summary>
        /// Used to check whether the device activation is caused by abnormal jitter
        /// </summary>
        /// <param name="zone"></param>
        /// <returns>
        /// If the trigger interval is lower than the debounce threshold, returns <see cref="bool">true</see>, otherwise <see cref="bool">false</see>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool JitterDetect(SensorArea zone, TimeSpan now, bool isBtn = false)
        {
            var index = (int)zone;
            if(!index.InRange(0,32))
            {
                return false;
            }
            TimeSpan lastTriggerTime;
            TimeSpan debounceTime;
            if (isBtn)
            {
                if (index > 32 || index > 7)
                {
                    return false;
                }
                lastTriggerTime = _btnLastTriggerTimes[index];
                debounceTime = _btnDebounceThresholdMs;
            }
            else
            {
                lastTriggerTime = _sensorLastTriggerTimes[index];
                debounceTime = _sensorDebounceThresholdMs;
            }
            var diff = now - lastTriggerTime;
            if (diff < debounceTime)
            {
                MajDebug.Log($"[Debounce] Received {(isBtn ? "button" : "sensor")} response\nZone: {zone}\nInterval: {diff.Milliseconds}ms");
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ThrowIfButtonIndexOutOfRange(SensorArea target)
        {
            var keyRange = new Range<int>(0, 7, ContainsType.Closed);
            var specialRange = new Range<int>(33, 36, ContainsType.Closed);
            if (!(keyRange.InRange((int)target) || specialRange.InRange((int)target)))
                throw new ArgumentOutOfRangeException("Button index cannot greater than A8");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ThrowIfSensorIndexOutOfRange(SensorArea area)
        {
            if (area < SensorArea.A1 || area > SensorArea.E8)
                throw new ArgumentOutOfRangeException();
        }
        static int GetButtonIndex(SensorArea area)
        {
            switch(area)
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
                case SensorArea.P1:
                case SensorArea.Service:
                case SensorArea.P2:
                    return (int)area - 25;
                default:
                    throw new ArgumentOutOfRangeException("Button index cannot greater than A8");
            }
        }

        static void OnTouchPanelStateChanged(TouchPanelZone zone, InputState state)
        {
            var i = (int)zone;
            var majState = state == InputState.On ? SensorStatus.On : SensorStatus.Off;

            TouchPanel.OnTouchPanelStateChanged(i, majState);
            _touchPanelInputBuffer.Enqueue(new()
            {
                Index = i,
                State = majState,
                Timestamp = MajTimeline.UnscaledTime
            });
        }
        static void OnButtonRingStateChanged(ButtonRingZone zone, InputState state)
        {
            var majState = state == InputState.On ? SensorStatus.On : SensorStatus.Off;
            var i = GetIndexByButtonRingZone(zone);

            //ButtonRing.OnButtonRingStateChanged(i, majState);
            _buttonRingInputBuffer.Enqueue(new()
            {
                Index = i,
                State = majState,
                Timestamp = MajTimeline.UnscaledTime
            });
        }
        static class KeyboardHelper
        {
            public static bool IsKeyDown(KeyCode keyCode)
            {
#if UNITY_STANDALONE_WIN
                var result = Win32API.GetAsyncKeyState((int)ToWinKeyCode(keyCode));
                return (result & 0x8000) != 0;
#else
            return Input.GetKey(ToUnityKeyCode(keyCode));
#endif
            }
            public static bool IsKeyUp(KeyCode keyCode)
            {
                return !IsKeyDown(keyCode);
            }
            static Win32API.RawKey ToWinKeyCode(KeyCode keyCode)
            {
                return keyCode switch
                {
                    KeyCode.B1 => Win32API.RawKey.W,
                    KeyCode.B2 => Win32API.RawKey.E,
                    KeyCode.B3 => Win32API.RawKey.D,
                    KeyCode.B4 => Win32API.RawKey.C,
                    KeyCode.B5 => Win32API.RawKey.X,
                    KeyCode.B6 => Win32API.RawKey.Z,
                    KeyCode.B7 => Win32API.RawKey.A,
                    KeyCode.B8 => Win32API.RawKey.Q,
                    KeyCode.Test => Win32API.RawKey.Numpad9,
                    KeyCode.SelectP1 => Win32API.RawKey.Multiply,
                    KeyCode.Service => Win32API.RawKey.Numpad7,
                    KeyCode.SelectP2 => Win32API.RawKey.Numpad3,
                    _ => throw new ArgumentOutOfRangeException(nameof(keyCode)),
                };
            }
            static UnityEngine.KeyCode ToUnityKeyCode(KeyCode keyCode)
            {
                return keyCode switch
                {
                    KeyCode.B1 => UnityEngine.KeyCode.W,
                    KeyCode.B2 => UnityEngine.KeyCode.E,
                    KeyCode.B3 => UnityEngine.KeyCode.D,
                    KeyCode.B4 => UnityEngine.KeyCode.C,
                    KeyCode.B5 => UnityEngine.KeyCode.X,
                    KeyCode.B6 => UnityEngine.KeyCode.Z,
                    KeyCode.B7 => UnityEngine.KeyCode.A,
                    KeyCode.B8 => UnityEngine.KeyCode.Q,
                    KeyCode.Test => UnityEngine.KeyCode.Keypad9,
                    KeyCode.SelectP1 => UnityEngine.KeyCode.KeypadMultiply,
                    KeyCode.Service => UnityEngine.KeyCode.Keypad7,
                    KeyCode.SelectP2 => UnityEngine.KeyCode.Keypad3,
                    _ => throw new ArgumentOutOfRangeException(nameof(keyCode)),
                };
            }
        }
        static class ButtonRing
        {
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
                if (!_buttonRingUpdateTask.IsCompleted)
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
            public static bool IsHadOn(SensorArea area)
            {
                if (area > SensorArea.A8 || area < SensorArea.A1)
                    return false;

                return _isBtnHadOn[(int)area];
            }
            public static bool IsOn(SensorArea area)
            {
                if (area > SensorArea.A8 || area < SensorArea.A1)
                    return false;

                return _buttonStates[(int)area];
            }
            public static bool IsHadOff(SensorArea area)
            {
                if (area > SensorArea.A8 || area < SensorArea.A1)
                    return true;

                return _isBtnHadOff[(int)area];
            }
            public static bool IsOff(SensorArea area)
            {
                return !IsOn(area);
            }
            public static bool IsCurrentlyOn(SensorArea area)
            {
                if (area > SensorArea.A8 || area < SensorArea.A1)
                    return false;

                return _buttonRealTimeStates[(int)area];
            }
            public static bool IsCurrentlyOff(SensorArea area)
            {
                return !IsCurrentlyOn(area);
            }
            #endregion

            static void KeyboardUpdateLoop()
            {
                var token = MajEnv.GlobalCT;
                var pollingRate = _btnPollingRateMs;
                var stopwatch = new Stopwatch();
                var t1 = stopwatch.Elapsed;
                var buttons = _buttons.Span;
                Span<bool> buffer = stackalloc bool[12];

                Thread.CurrentThread.Priority = System.Threading.ThreadPriority.AboveNormal;
                stopwatch.Start();
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
                            buffer[i] = state;
                        }
                        lock(_buttonRingUpdateLoop)
                        {
                            for (var i = 0; i < 12; i++)
                            {
                                var state = buffer[i];
                                _buttonRealTimeStates[i] = state;
                                _isBtnHadOnInternal[i] |= state;
                                _isBtnHadOffInternal[i] |= !state;

                                buffer[i] = default;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MajDebug.LogError($"From KeyBoard listener: \n{e}");
                    }
                    finally
                    {
                        if(pollingRate.TotalMilliseconds > 0)
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
            static void HIDUpdateLoop()
            {
                var token = MajEnv.GlobalCT;
                var pollingRate = _btnPollingRateMs;
                var stopwatch = new Stopwatch();
                var t1 = stopwatch.Elapsed;
                var buttons = _buttons.Span;
                var pid = MajEnv.UserSettings.Misc.InputDevice.ButtonRing.ProductId;
                var vid = MajEnv.UserSettings.Misc.InputDevice.ButtonRing.VendorId;
                var deviceType = MajEnv.UserSettings.Misc.InputDevice.ButtonRing.Type;
                var devices = DeviceList.Local.GetHidDevices();
                HidDevice? device = null;
                HidStream? hidStream = null;

                foreach(var d in devices)
                {
                    if (d.ProductID == pid && d.VendorID == vid)
                    {
                        device = d;
                        break;
                    }
                }
                if(device is null)
                {
                    MajDebug.LogWarning("Hid device not found");
                    return;
                }
                else if(!device.TryOpen(out hidStream))
                {
                    MajDebug.LogError($"cannot open hid device:\n{device}");
                    return;
                }

                using (hidStream)
                {
                    Span<byte> readBuffer = stackalloc byte[device.GetMaxInputReportLength()];
                    Span<bool> buffer = stackalloc bool[12];

                    Thread.CurrentThread.Priority = System.Threading.ThreadPriority.AboveNormal;
                    stopwatch.Start();
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            var now = MajTimeline.UnscaledTime;
                            hidStream.Read(readBuffer);
                            switch(deviceType)
                            {
                                case DeviceType.HID:
                                    AdxHIDDevice.Parse(readBuffer, buffer);
                                    break;
                                case DeviceType.IO4:
                                    AdxIO4Device.Parse(readBuffer, buffer);
                                    break;
                                default:
                                    continue;
                            }
                            lock (_buttonRingUpdateLoop)
                            {
                                for (var i = 0; i < 12; i++)
                                {
                                    var state = buffer[i];
                                    _buttonRealTimeStates[i] = state;
                                    _isBtnHadOnInternal[i] |= state;
                                    _isBtnHadOffInternal[i] |= !state;

                                    buffer[i] = default;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MajDebug.LogError($"From HID listener: \n{e}");
                        }
                        finally
                        {
                            readBuffer.Clear();
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
                    buffer[8] = (~reportData[IO4_TEST_INDEX] & IO4_TEST_OFFSET) != 0;
                    buffer[9] = (~reportData[IO4_SELECT_P1_INDEX] & IO4_SELECT_P1_OFFSET) != 0;
                    buffer[10] = (~reportData[IO4_SERVICE_INDEX] & IO4_SERVICE_OFFSET) != 0;
                    buffer[11] = (~reportData[IO4_SELECT_P2_INDEX] & IO4_SELECT_P2_OFFSET) != 0;
                }
            }

            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //public static bool IsButtonReleased(SensorArea button)
            //{
            //    var i = GetButtonIndexFromArea(button);
            //    return _buttonRealTimeStates[i] == SensorStatus.Off;
            //}
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //public static bool IsButtonPressed(SensorArea button)
            //{
            //    var i = GetButtonIndexFromArea(button);
            //    return _buttonRealTimeStates[i] == SensorStatus.On;
            //}
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //public static void OnButtonRingStateChanged(int buttonIndex, SensorStatus state)
            //{
            //    if(!buttonIndex.InRange(0,11))
            //    {
            //        return;
            //    }
            //    _buttonRealTimeStates[buttonIndex] = state;
            //}
            //static int GetButtonIndexFromArea(SensorArea area)
            //{
            //    switch(area)
            //    {
            //        case SensorArea.A1:
            //        case SensorArea.A2:
            //        case SensorArea.A3:
            //        case SensorArea.A4:
            //        case SensorArea.A5:
            //        case SensorArea.A6:
            //        case SensorArea.A7:
            //        case SensorArea.A8:
            //            return (int)area;
            //        case SensorArea.Test:
            //            return 8;
            //        case SensorArea.P1:
            //            return 9;
            //        case SensorArea.Service:
            //            return 10;
            //        case SensorArea.P2:
            //            return 11;
            //        default:
            //            throw new ArgumentOutOfRangeException(nameof(area));
            //    }
            //}
        }
        static class TouchPanel
        {
            public static ReadOnlySpan<SensorStatus> SensorStateLogger
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    return _sensorStates;
                }
            }
            readonly static SensorStatus[] _sensorStates = new SensorStatus[34];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsSensorRelased(SensorArea area)
            {
                var i = (int)area;
                return _sensorStates[i] == SensorStatus.Off;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsSensorPressed(SensorArea area)
            {
                var i = (int)area;
                return _sensorStates[i] == SensorStatus.On;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void OnTouchPanelStateChanged(int sensorIndex, SensorStatus state)
            {
                if (!sensorIndex.InRange(0, 33))
                {
                    return;
                }
                _sensorStates[sensorIndex] = state;
            }
        }
        class Button : IEventPublisher<EventHandler<InputEventArgs>>
        {
            public KeyCode BindingKey
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set;
            }
            public SensorArea Area
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set;
            }
            /// <summary>
            /// Update by InputManager.PreUpdate
            /// </summary>
            public SensorStatus State
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set;
            }

            event EventHandler<InputEventArgs>? OnStatusChanged;
            public Button(KeyCode bindingKey, SensorArea type)
            {
                BindingKey = bindingKey;
                Area = type;
                State = SensorStatus.Off;
                OnStatusChanged = null;
            }
            public void AddSubscriber(EventHandler<InputEventArgs> handler)
            {
                OnStatusChanged += handler;
            }
            public void RemoveSubscriber(EventHandler<InputEventArgs> handler)
            {
                if (OnStatusChanged is not null)
                    OnStatusChanged -= handler;
            }
            public void PushEvent(in InputEventArgs args)
            {
                if (OnStatusChanged is not null)
                    OnStatusChanged(this, args);
            }
            public void ClearSubscriber() => OnStatusChanged = null;
        }
        class Sensor : IEventPublisher<EventHandler<InputEventArgs>>
        {
            /// <summary>
            /// Update by InputManager.PreUpdate
            /// </summary>
            public SensorStatus State
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set;
            } = SensorStatus.Off;
            public SensorArea Area
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set;
            }
            public SensorGroup Group
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var i = (int)Area;
                    if (i <= 7)
                        return SensorGroup.A;
                    else if (i <= 15)
                        return SensorGroup.B;
                    else if (i <= 16)
                        return SensorGroup.C;
                    else if (i <= 24)
                        return SensorGroup.D;
                    else
                        return SensorGroup.E;
                }
            }
            event EventHandler<InputEventArgs>? OnStatusChanged;//oStatus nStatus

            public void AddSubscriber(EventHandler<InputEventArgs> handler)
            {
                OnStatusChanged += handler;
            }
            public void RemoveSubscriber(EventHandler<InputEventArgs> handler)
            {
                if (OnStatusChanged is not null)
                    OnStatusChanged -= handler;
            }
            public void PushEvent(in InputEventArgs args)
            {
                if (OnStatusChanged is not null)
                    OnStatusChanged(this, args);
            }
            public void ClearSubscriber() => OnStatusChanged = null;
        }
    }
}