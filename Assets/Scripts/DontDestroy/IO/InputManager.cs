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
using UnityRawInput;
//using Microsoft.Win32;
//using System.Windows.Forms;
//using Application = UnityEngine.Application;
//using System.Security.Policy;
#nullable enable
namespace MajdataPlay.IO
{
    public unsafe partial class InputManager : MonoBehaviour
    {
        public bool displayDebug = false;
        public static bool useDummy = false;

        public static event EventHandler<InputEventArgs>? OnAnyAreaTrigger;

        static TimeSpan _btnDebounceThresholdMs = TimeSpan.Zero;
        static TimeSpan _sensorDebounceThresholdMs = TimeSpan.Zero;
        static TimeSpan _btnPollingRateMs = TimeSpan.Zero;
        static TimeSpan _sensorPollingRateMs = TimeSpan.Zero;

        readonly static ConcurrentQueue<InputDeviceReport> _touchPanelInputBuffer = new();
        readonly static ConcurrentQueue<InputDeviceReport> _buttonRingInputBuffer = new();

        readonly static ReadOnlyMemory<RawKey> _bindingKeys = new RawKey[12]
        {
            RawKey.W,
            RawKey.E,
            RawKey.D,
            RawKey.C,
            RawKey.X,
            RawKey.Z,
            RawKey.A,
            RawKey.Q,
            RawKey.Numpad9,
            RawKey.Multiply,
            RawKey.Numpad7,
            RawKey.Numpad3,
        };
        readonly static ReadOnlyMemory<Button> _buttons = new Button[12]
        {
            new Button(RawKey.W,SensorArea.A1),
            new Button(RawKey.E,SensorArea.A2),
            new Button(RawKey.D,SensorArea.A3),
            new Button(RawKey.C,SensorArea.A4),
            new Button(RawKey.X,SensorArea.A5),
            new Button(RawKey.Z,SensorArea.A6),
            new Button(RawKey.A,SensorArea.A7),
            new Button(RawKey.Q,SensorArea.A8),
            new Button(RawKey.Numpad9,SensorArea.Test),
            new Button(RawKey.Multiply,SensorArea.P1),
            new Button(RawKey.Numpad7,SensorArea.Service),
            new Button(RawKey.Numpad3,SensorArea.P2),
        };
        readonly static Dictionary<SensorArea, DateTime> _btnLastTriggerTimes = new();
        readonly static Memory<bool> _buttonStates = new bool[12];

        static ReadOnlyMemory<Sensor> _sensors = ReadOnlyMemory<Sensor>.Empty;
        static readonly Dictionary<SensorArea, DateTime> _sensorLastTriggerTimes = new();
        readonly static Memory<bool> _sensorStates = new bool[35];

        static bool _isBtnDebounceEnabled = false;
        static bool _isSensorDebounceEnabled = false;

        static Task _serialPortUpdateTask = Task.CompletedTask;
        static Task _buttonRingUpdateTask = Task.CompletedTask;

        static IOManager? _ioManager = null;

        static delegate*<void> _updateIOListenerPtr = &DefaultIOListener;

        void Awake()
        {
            MajInstances.InputManager = this;
            DontDestroyOnLoad(this);
            var sensors = new Sensor[33];
            foreach (var (index, child) in transform.ToEnumerable().WithIndex())
            {
                var collider = child.GetComponent<Collider>();
                var area = (SensorArea)index;
                sensors[index] = child.GetComponent<Sensor>();
                sensors[index].Area = area;
                _instanceID2SensorTypeMappingTable[collider.GetInstanceID()] = area;
                if(area.GetGroup() == SensorGroup.C)
                {
                    var childCollider = child.GetChild(0).GetComponent<Collider>();
                    _instanceID2SensorTypeMappingTable[childCollider.GetInstanceID()] = area;
                }
            }
            _sensors = sensors;
            foreach(SensorArea zone in Enum.GetValues(typeof(SensorArea)))
            {
                if (((int)zone).InRange(0, 7))
                {
                    _btnLastTriggerTimes[zone] = DateTime.MinValue;
                }
                _sensorLastTriggerTimes[zone] = DateTime.MinValue;
            }
        }
        /// <summary>
        /// Used to check whether the device activation is caused by abnormal jitter
        /// </summary>
        /// <param name="zone"></param>
        /// <returns>
        /// If the trigger interval is lower than the debounce threshold, returns <see cref="bool">true</see>, otherwise <see cref="bool">false</see>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool JitterDetect(SensorArea zone, DateTime now,bool isBtn = false)
        {
            DateTime lastTriggerTime;
            TimeSpan debounceTime;
            if(isBtn)
            {
                _btnLastTriggerTimes.TryGetValue(zone, out lastTriggerTime);
                debounceTime = _btnDebounceThresholdMs;
            }
            else
            {
                _sensorLastTriggerTimes.TryGetValue(zone, out lastTriggerTime);
                debounceTime = _sensorDebounceThresholdMs;
            }
            var diff = now - lastTriggerTime;
            if (diff < debounceTime)
            {
                MajDebug.Log($"[Debounce] Received {(isBtn?"button":"sensor")} response\nZone: {zone}\nInterval: {diff.Milliseconds}ms");
                return true;
            }
            return false;
        }
        void CheckEnvironment(bool forceQuit = true)
        {
            //// MSVC 2015-2019
            //var registryKey = @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64";
            //using var key = Registry.LocalMachine.OpenSubKey(registryKey);
            //if(key is null)
            //{
            //    //var msg = "IO4 and HID input methods depend on the MSVC runtime library, but MajdataPlay did not find the MSVC runtime library on your computer. Please click \"OK\" to jump to download and install.";
            //    var msg = Localization.GetLocalizedText(MajText.MISSING_MSVC_CONTENT);
            //    if (string.IsNullOrEmpty(msg))
            //        msg = "MSVCRT not found\r\nClick \"OK\" to download";
            //    var title = "Missing MSVC";
            //    if (forceQuit)
            //    {
            //        MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        Application.OpenURL("https://aka.ms/vs/17/release/vc_redist.x64.exe");
            //        Application.Quit();
            //    }
            //    else
            //        MajDebug.LogWarning("Missing environment: MSVC runtime library not found.");
            //}
        }
        void Start()
        {
            switch(MajInstances.Setting.Misc.InputDevice.ButtonRing.Type)
            {
                case DeviceType.Keyboard:
                    CheckEnvironment(false);
                    StartInternalIOManager();
                    _updateIOListenerPtr = &UpdateInternalIOListener;
                    break;
                case DeviceType.IO4:
                case DeviceType.HID:
                    CheckEnvironment();
                    StartExternalIOManager();
                    _updateIOListenerPtr = &UpdateExternalIOListener;
                    break;
            }

        }
        internal void OnFixedUpdate()
        {
            //_updateIOListener();
        }
        internal void OnUpdate()
        {
            _updateIOListenerPtr();
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
            MajInstanceHelper<IOManager>.Instance = _ioManager;
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
                buttonRingCallbacks[zone] = (zone, state) =>
                {
                    _buttonRingInputBuffer.Enqueue(new()
                    {
                        Index = GetIndexByButtonRingZone(zone),
                        State = state == InputState.On ? SensorStatus.On : SensorStatus.Off,
                        Timestamp = DateTime.Now
                    });
                };
            }

            foreach (TouchPanelZone zone in Enum.GetValues(typeof(TouchPanelZone)))
            {
                touchPanelCallbacks[zone] = (zone, state) =>
                {
                    _touchPanelInputBuffer.Enqueue(new()
                    {
                        Index = (int)zone,
                        State = state == InputState.On ? SensorStatus.On : SensorStatus.Off,
                        Timestamp = DateTime.Now
                    });
                };
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
                    { "BaudRate", MajInstances.Setting.Misc.InputDevice.TouchPanel.BaudRate }
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
                if (useDummy)
                    UpdateMousePosition();
                else
                    UpdateSensorState();
                UpdateButtonState();
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
        void ExternalIOEventHandler(IOEventType eventType,DeviceClassification deviceType,string msg)
        {
            var executionQueue = IOManager.ExecutionQueue;
            var logContent = $"From external IOManager:\nEventType: {eventType}\nDeviceType: {deviceType}\nMsg: {msg.Trim()}";
            switch (eventType)
            {
                case IOEventType.Attach:
                case IOEventType.Debug:
                    executionQueue.Enqueue(() => MajDebug.Log(logContent));
                    break;
                case IOEventType.ConnectionError:
                case IOEventType.SerialDeviceReadError:
                case IOEventType.HidDeviceReadError:
                case IOEventType.ReconnectionError:
                case IOEventType.InvalidDevicePropertyError:
                    executionQueue.Enqueue(() => MajDebug.LogError(logContent));
                    break;
                case IOEventType.Detach:
                    executionQueue.Enqueue(() => MajDebug.LogWarning(logContent));
                    break;
            }
        }
        public void BindAnyArea(EventHandler<InputEventArgs> checker) => OnAnyAreaTrigger += checker;
        public void BindArea(EventHandler<InputEventArgs> checker, SensorArea sType)
        {
            var sensor = GetSensor(sType);
            var button = GetButton(sType);
            if (sensor == null || button is null)
                throw new Exception($"{sType} Sensor or Button not found.");

            sensor.AddSubscriber(checker);
            button.AddSubscriber(checker);
        }
        public void UnbindAnyArea(EventHandler<InputEventArgs> checker) => OnAnyAreaTrigger -= checker;
        public void UnbindArea(EventHandler<InputEventArgs> checker, SensorArea sType)
        {
            var sensor = GetSensor(sType);
            var button = GetButton(sType);
            if (sensor is null || button is null)
                throw new Exception($"{sType} Sensor or Button not found.");

            sensor.RemoveSubscriber(checker);
            button.RemoveSubscriber(checker);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAreaStatus(SensorArea sType, SensorStatus targetStatus)
        {
            return CheckSensorStatus(sType,targetStatus) || CheckButtonStatus(sType, targetStatus);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckSensorStatus(SensorArea target, SensorStatus targetStatus)
        {
            var sensor = _sensors.Span[(int)target];
            if (sensor is null)
                throw new Exception($"{target} Sensor or Button not found.");
            return sensor.Status == targetStatus;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckButtonStatus(SensorArea target, SensorStatus targetStatus)
        {
            var keyRange = new Range<int>(0, 7, ContainsType.Closed);
            var specialRange = new Range<int>(33, 36, ContainsType.Closed);
            if (!(keyRange.InRange((int)target) || specialRange.InRange((int)target)))
                throw new ArgumentOutOfRangeException("Button index cannot greater than A8");
            var button = GetButton(target);

            if (button is null)
                throw new Exception($"{target} Button not found.");

            return button.Status == targetStatus;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Button? GetButton(SensorArea type)
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
        public Sensor GetSensor(SensorArea target) => _sensors.Span[(int)target];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Sensor> GetSensors() => _sensors;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearAllSubscriber()
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
    }
}