using UnityEngine;
using System.Threading;
using System;
using System.Linq;
using UnityRawInput;
using System.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using MychIO;
using Cysharp.Threading.Tasks;
using DeviceType = MajdataPlay.Types.DeviceType;
using MychIO.Device;
using System.Collections.Generic;
using MychIO.Event;
using MychIO.Connection;
using static UnityEngine.GraphicsBuffer;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.Antlr3.Runtime;
using MajdataPlay.Collections;
using System.Collections.Concurrent;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static UnityEngine.Rendering.DebugUI.Table;
//using Microsoft.Win32;
//using System.Windows.Forms;
//using Application = UnityEngine.Application;
//using System.Security.Policy;
#nullable enable
namespace MajdataPlay.IO
{
    public partial class InputManager : MonoBehaviour
    {
        public bool displayDebug = false;
        public bool useDummy = false;

        public event EventHandler<InputEventArgs>? OnAnyAreaTrigger;

        TimeSpan _btnDebounceThresholdMs = TimeSpan.Zero;
        TimeSpan _sensorDebounceThresholdMs = TimeSpan.Zero;
        TimeSpan _btnPollingRateMs = TimeSpan.Zero;
        TimeSpan _sensorPollingRateMs = TimeSpan.Zero;

        ConcurrentQueue<InputDeviceReport> _touchPanelInputBuffer = new();
        ConcurrentQueue<InputDeviceReport> _buttonRingInputBuffer = new();

        bool[] _COMReport = Enumerable.Repeat(false,35).ToArray();

        bool C1 = false;
        bool C2 = false;

        bool _isBtnDebounceEnabled = false;
        bool _isSensorDebounceEnabled = false;
        Task? _recvTask = null;
        Mutex _buttonCheckerMutex = new();
        IOManager? _ioManager = null;

        Action _updateIOListener = () => { };

        void Awake()
        {
            MajInstances.InputManager = this;
            DontDestroyOnLoad(this);
            foreach (var (index, child) in transform.ToEnumerable().WithIndex())
            {
                var collider = child.GetComponent<Collider>();
                var type = (SensorType)index;
                _sensors[index] = child.GetComponent<Sensor>();
                _sensors[index].Type = type;
                _instanceID2SensorTypeMappingTable[collider.GetInstanceID()] = type;
                if(type.GetGroup() == SensorGroup.C)
                {
                    var childCollider = child.GetChild(0).GetComponent<Collider>();
                    _instanceID2SensorTypeMappingTable[childCollider.GetInstanceID()] = type;
                }
            }
            foreach(SensorType zone in Enum.GetValues(typeof(SensorType)))
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
        protected bool JitterDetect(SensorType zone, DateTime now,bool isBtn = false)
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
                    _updateIOListener = UpdateInternalIOListener;
                    break;
                case DeviceType.IO4:
                case DeviceType.HID:
                    CheckEnvironment();
                    StartExternalIOManager();
                    _updateIOListener = UpdateExternalIOListener;
                    break;
            }

        }
        internal void OnFixedUpdate()
        {
            _updateIOListener();
        }
        internal void OnUpdate()
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
            COMReceiveAsync();
            RefreshKeyboardStateAsync();
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
                var btnDebounceThresholdMs = btnDebounce ? MajInstances.Setting.Misc.InputDevice.ButtonRing.DebounceThresholdMs : 0;

                var touchPanelPollingRate = MajInstances.Setting.Misc.InputDevice.TouchPanel.PollingRateMs;
                var touchPanelDebounceThresholdMs = touchPanelDebounce ? MajInstances.Setting.Misc.InputDevice.TouchPanel.DebounceThresholdMs : 0;

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
                    { "ComPortNumber", $"COM{comPortNum}" }
                };

                _ioManager.AddButtonRing(deviceName,
                                         inputSubscriptions: buttonRingCallbacks,
                                         connectionProperties: btnConnProperties);
                _ioManager.AddTouchPanel(AdxTouchPanel.GetDeviceName(),
                                         inputSubscriptions: touchPanelCallbacks,
                                         connectionProperties: touchPanelConnProperties);
                _ioManager.AddLedDevice(AdxLedDevice.GetDeviceName());
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
            }
        }
        void UpdateInternalIOListener()
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
        void UpdateExternalIOListener()
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
        public void BindArea(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var sensor = GetSensor(sType);
            var button = GetButton(sType);
            if (sensor == null || button is null)
                throw new Exception($"{sType} Sensor or Button not found.");

            sensor.AddSubscriber(checker);
            button.AddSubscriber(checker);
        }
        public void UnbindAnyArea(EventHandler<InputEventArgs> checker) => OnAnyAreaTrigger -= checker;
        public void UnbindArea(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var sensor = GetSensor(sType);
            var button = GetButton(sType);
            if (sensor == null || button is null)
                throw new Exception($"{sType} Sensor or Button not found.");

            sensor.RemoveSubscriber(checker);
            button.RemoveSubscriber(checker);
        }
        public bool CheckAreaStatus(SensorType sType, SensorStatus targetStatus)
        {
            return CheckSensorStatus(sType,targetStatus) || CheckButtonStatus(sType, targetStatus);
        }
        public bool CheckSensorStatus(SensorType target, SensorStatus targetStatus)
        {
            var sensor = _sensors[(int)target];
            if (sensor == null)
                throw new Exception($"{target} Sensor or Button not found.");
            return sensor.Status == targetStatus;
        }
        public bool CheckButtonStatus(SensorType target, SensorStatus targetStatus)
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
        public void SetBusy(InputEventArgs args)
        {
            var type = args.Type;
            if (args.IsButton)
            {
                var button = GetButton(type);
                if (button is null)
                    throw new Exception($"{type} Button not found.");

                button.IsJudging = true;
            }
            else
            {
                var sensor = GetSensor(type);
                if (sensor is null)
                    throw new Exception($"{type} Sensor not found.");

                sensor.IsJudging = true;
            }
        }
        public void SetIdle(InputEventArgs args)
        {
            var type = args.Type;
            if (args.IsButton)
            {
                var button = GetButton(type);
                if (button is null)
                    throw new Exception($"{type} Button not found.");

                button.IsJudging = false;
            }
            else
            {
                var sensor = GetSensor(type);
                if (sensor is null)
                    throw new Exception($"{type} Sensor not found.");

                sensor.IsJudging = false;
            }
        }
        public bool IsIdle(InputEventArgs args)
        {
            bool isIdle;
            var type = args.Type;
            if (args.IsButton)
            {
                var button = GetButton(type);
                if (button is null)
                    throw new Exception($"{type} Button not found.");

                isIdle = !button.IsJudging;
            }
            else
            {
                var sensor = GetSensor(type);
                if (sensor is null)
                    throw new Exception($"{type} Sensor not found.");

                isIdle = !sensor.IsJudging;
            }
            return isIdle;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Button? GetButton(SensorType type)
        {
            return type switch
            {
                _ when type < SensorType.A1 => throw new ArgumentOutOfRangeException(),
                _ when type < SensorType.B1 => _buttons[(int)type],
                SensorType.Test => _buttons[8],
                SensorType.P1 => _buttons[9],
                SensorType.Service => _buttons[10],
                SensorType.P2 => _buttons[11],
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sensor GetSensor(SensorType target) => _sensors[(int)target];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sensor[] GetSensors() => _sensors;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sensor[] GetSensors(SensorGroup group) => _sensors.Where(x => x.Group == group).ToArray();
        public void ClearAllSubscriber()
        {
            foreach(var sensor in _sensors.AsSpan())
                sensor.ClearSubscriber();
            foreach(var button in _buttons.AsSpan())
                button.ClearSubscriber();
            OnAnyAreaTrigger = null;
        }
        void PushEvent(InputEventArgs args)
        {
            if (OnAnyAreaTrigger is not null)
                OnAnyAreaTrigger(this, args);
        }
    }
}