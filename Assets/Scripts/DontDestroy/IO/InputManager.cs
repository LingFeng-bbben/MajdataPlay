using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using MychIO;
using DeviceType = MajdataPlay.Types.DeviceType;
using MychIO.Device;
using System.Collections.Generic;
using MychIO.Event;
using System.Runtime.CompilerServices;
using MajdataPlay.Collections;
using System.Collections.Concurrent;
using System.Security.Policy;
//using Microsoft.Win32;
//using System.Windows.Forms;
//using Application = UnityEngine.Application;
//using System.Security.Policy;
#nullable enable
namespace MajdataPlay.IO
{
    internal unsafe partial class InputManager : MonoBehaviour
    {
        public bool IsTouchPanelConnected { get; private set; } = false;
        public ReadOnlyMemory<SensorStatus> ButtonStatusInThisFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _btnStatusInThisFrame;
        }
        public ReadOnlyMemory<SensorStatus> ButtonStatusInPreviousFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _btnStatusInPreviousFrame;
        }
        public ReadOnlyMemory<SensorStatus> SensorStatusInThisFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sensorStatusInThisFrame;
        }
        public ReadOnlyMemory<SensorStatus> SensorStatusInPreviousFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sensorStatusInPreviousFrame;
        }

        public static event EventHandler<InputEventArgs>? OnAnyAreaTrigger;

        static TimeSpan _btnDebounceThresholdMs = TimeSpan.Zero;
        static TimeSpan _sensorDebounceThresholdMs = TimeSpan.Zero;
        static TimeSpan _btnPollingRateMs = TimeSpan.Zero;
        static TimeSpan _sensorPollingRateMs = TimeSpan.Zero;

        readonly static Memory<SensorStatus> _latestBtnStateLogger = new SensorStatus[12];
        //The serial port will report the status of 35 zones, but there are actually only 34 zones.
        readonly static Memory<SensorStatus> _latestSensorStateLogger = new SensorStatus[35];
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
        readonly static Memory<SensorRenderer> _sensorRenderers = new SensorRenderer[34];
        //The serial port will report the status of 35 zones, but there are actually only 34 zones.
        readonly static Memory<bool> _sensorStates = new bool[35];
        readonly static SensorStatus[] _sensorStatusInPreviousFrame = new SensorStatus[33];
        readonly static SensorStatus[] _sensorStatusInThisFrame = new SensorStatus[33];

        static bool _useDummy = false;
        static bool _isBtnDebounceEnabled = false;
        static bool _isSensorDebounceEnabled = false;
        static bool _isSensorRendererEnabled = false;

        static Task _serialPortUpdateTask = Task.CompletedTask;
        static Task _buttonRingUpdateTask = Task.CompletedTask;

        static IOManager? _ioManager = null;

        static delegate*<void> _updateIOListenerPtr = &DefaultIOListener;

        void Awake()
        {
            MajInstances.InputManager = this;
            DontDestroyOnLoad(this);
            Input.multiTouchEnabled = true;
            var sensorRenderers = _sensorRenderers.Span;
            foreach (var (index, child) in transform.ToEnumerable().WithIndex())
            {
                var collider = child.GetComponent<MeshCollider>();
                var renderer = child.GetComponent<MeshRenderer>();
                var filter = child.GetComponent<MeshFilter>();
                sensorRenderers[index] = new SensorRenderer(index, filter, renderer, collider, child.gameObject);
                _instanceID2SensorIndexMappingTable[collider.GetInstanceID()] = index;
            }
            for (var i = 0; i < 33; i++)
            {
                if (i.InRange(0, 7))
                {
                    _btnLastTriggerTimes[i] = TimeSpan.Zero;
                }
                _sensorLastTriggerTimes[i] = TimeSpan.Zero;
            }
        }
        void Start()
        {
            _isSensorRendererEnabled = MajInstances.Setting.Debug.DisplaySensor;
            
            switch(MajInstances.Setting.Misc.InputDevice.ButtonRing.Type)
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

        }
        void OnTouchPanelConnected()
        {
            if (!_isSensorRendererEnabled)
            {
                foreach (var renderer in _sensorRenderers.Span)
                {
                    renderer.Destroy();
                }
            }
        }
        internal void OnFixedUpdate()
        {
            //_updateIOListener();
        }
        internal void OnPreUpdate()
        {
            _updateIOListenerPtr();
            if(_isSensorRendererEnabled)
            {
                var sensorRenderers = _sensorRenderers.Span;
                foreach (var (i, state) in _sensorStates.Span.WithIndex())
                {
                    if (i == 34)
                        continue;
#if UNITY_EDITOR
                    sensorRenderers[i].Color = state ? new Color(0, 0, 0, 0.4f) : new Color(0, 0, 0, 0.1f);
#else
                    sensorRenderers[i].Color = state ? new Color(0, 0, 0, 0.3f) : new Color(0, 0, 0, 0f);
#endif
                }
            }
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
        void StartInternalIOManager()
        {
            _btnDebounceThresholdMs = TimeSpan.FromMilliseconds(MajInstances.Setting.Misc.InputDevice.ButtonRing.DebounceThresholdMs);
            _btnPollingRateMs = TimeSpan.FromMilliseconds(MajInstances.Setting.Misc.InputDevice.ButtonRing.PollingRateMs);
            _sensorDebounceThresholdMs = TimeSpan.FromMilliseconds(MajInstances.Setting.Misc.InputDevice.TouchPanel.DebounceThresholdMs);
            _sensorPollingRateMs = TimeSpan.FromMilliseconds(MajInstances.Setting.Misc.InputDevice.TouchPanel.PollingRateMs);
            //RawInput.Start();
            //RawInput.OnKeyDown += OnRawKeyDown;
            //RawInput.OnKeyUp += OnRawKeyUp;
            _isBtnDebounceEnabled = MajInstances.Setting.Misc.InputDevice.ButtonRing.Debounce;
            _isSensorDebounceEnabled = MajInstances.Setting.Misc.InputDevice.TouchPanel.Debounce;
            StartUpdatingTouchPanelState();
            StartUpdatingKeyboardState();
        }
        public void StartExternalIOManager()
        {
            if(_ioManager is null)
                _ioManager = new();
            Majdata<IOManager>.Instance = _ioManager;
            var useHID = MajInstances.Setting.Misc.InputDevice.ButtonRing.Type is DeviceType.HID;
            var executionQueue = MajEnv.ExecutionQueue;
            var buttonRingCallbacks = new Dictionary<ButtonRingZone, Action<ButtonRingZone, InputState>>();
            var touchPanelCallbacks = new Dictionary<TouchPanelZone, Action<TouchPanelZone, InputState>>();

            _btnDebounceThresholdMs = TimeSpan.FromMilliseconds(MajInstances.Setting.Misc.InputDevice.ButtonRing.DebounceThresholdMs);
            _sensorDebounceThresholdMs = TimeSpan.FromMilliseconds(MajInstances.Setting.Misc.InputDevice.TouchPanel.DebounceThresholdMs);
            _isBtnDebounceEnabled = MajInstances.Setting.Misc.InputDevice.ButtonRing.Debounce;
            _isSensorDebounceEnabled = MajInstances.Setting.Misc.InputDevice.TouchPanel.Debounce;

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
            _ioManager.AddDeviceErrorHandler(new DeviceErrorHandler(_ioManager, 4));

            try
            {
                var deviceName = useHID ? AdxHIDButtonRing.GetDeviceName() : AdxIO4ButtonRing.GetDeviceName();
                var btnDebounce = MajInstances.Setting.Misc.InputDevice.ButtonRing.Debounce;
                var touchPanelDebounce = MajInstances.Setting.Misc.InputDevice.TouchPanel.Debounce;

                var btnProductId = MajInstances.Setting.Misc.InputDevice.ButtonRing.ProductId;
                var btnVendorId = MajInstances.Setting.Misc.InputDevice.ButtonRing.VendorId;
                var comPortNum = MajInstances.Setting.Misc.InputDevice.TouchPanel.COMPort;

                var btnPollingRate = MajInstances.Setting.Misc.InputDevice.ButtonRing.PollingRateMs;
                //var btnDebounceThresholdMs = btnDebounce ? MajInstances.Setting.Misc.InputDevice.ButtonRing.DebounceThresholdMs : 0;
                var btnDebounceThresholdMs = 0;

                var touchPanelPollingRate = MajInstances.Setting.Misc.InputDevice.TouchPanel.PollingRateMs;
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
                    { "BaudRate", MajInstances.Setting.Misc.InputDevice.TouchPanel.BaudRate },
                    { "SensitivityOverride", true },
                    { "Sensitivity", MajInstances.Setting.Misc.InputDevice.TouchPanel.Sensitivity }
                };
                var ledConnProperties = new Dictionary<string, dynamic>()
                {
                    { "ComPortNumber", $"COM{MajInstances.Setting.Misc.OutputDevice.Led.COMPort}" },
                    { "BaudRate", MajInstances.Setting.Misc.OutputDevice.Led.BaudRate }
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
                var extraButtonFromTouch = new bool[8];
                if (_useDummy)
                    extraButtonFromTouch = UpdateMousePosition();
                else
                    UpdateSensorState();
                UpdateButtonState(extraButtonFromTouch);
                while (executionQueue.TryDequeue(out var eventAction))
                    eventAction();
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSensorClickedInThisFrame(SensorArea target)
        {
            ThrowIfSensorIndexOutOfRange(target);
            var index = (int)target;

            return _sensorStatusInPreviousFrame[index] == SensorStatus.Off &&
                   _sensorStatusInThisFrame[index] == SensorStatus.On;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Button? GetButton(SensorArea type)
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
        public static ReadOnlyMemory<Button> GetButtons()
        {
            return _buttons;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Sensor GetSensor(SensorArea target)
        {
            return _sensors.Span[(int)target];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Sensor> GetSensors()
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

            _buttonRingInputBuffer.Enqueue(new()
            {
                Index = i,
                State = majState,
                Timestamp = MajTimeline.UnscaledTime
            });
        }
    }
    class SensorRenderer
    {
        public int Index { get; init; }
        public MeshFilter MeshFilter { get; init; }
        public MeshRenderer MeshRenderer { get; init; }
        public MeshCollider MeshCollider { get; init; }
        public GameObject GameObject { get; init; }
        public Color Color 
        {
            get => _material.color;
            set => _material.color = value; 
        }
        Material _material;
        public SensorRenderer(int index, MeshFilter meshFilter, MeshRenderer meshRenderer, MeshCollider meshCollider, GameObject gameObject)
        {
            Index = index;
            MeshFilter = meshFilter;
            MeshRenderer = meshRenderer;
            MeshCollider = meshCollider;
            _material = new Material(Shader.Find("Sprites/Default"));
            MeshRenderer.material = _material;
            GameObject = gameObject;
            Color = new Color(0, 0, 0, 0f);
        }
        public void Destroy()
        {
            GameObject.Destroy(GameObject);
            GameObject.Destroy(_material);
        }
    }
}