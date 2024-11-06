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

        bool[] _COMReport = Enumerable.Repeat(false,35).ToArray();
        Task? _recvTask = null;
        Mutex _buttonCheckerMutex = new();
        IOManager? _ioManager = null;
        CancellationTokenSource _cancelSource = new();

        void Awake()
        {
            MajInstances.InputManager = this;
            DontDestroyOnLoad(this);
            foreach (var (index, child) in transform.ToEnumerable().WithIndex())
            {
                _sensors[index] = child.GetComponent<Sensor>();
                _sensors[index].Type = (SensorType)index;
            }
            
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
            //        Debug.LogWarning("Missing environment: MSVC runtime library not found.");
            //}
        }
        void Start()
        {
            switch(MajInstances.Setting.Misc.InputDevice.ButtonRing.Type)
            {
                case DeviceType.Keyboard:
                    CheckEnvironment(false);
                    StartInternalIOManager();
                    StartInternalIOListener();
                    break;
                case DeviceType.IO4:
                case DeviceType.HID:
                    CheckEnvironment();
                    StartExternalIOManager();
                    StartExternalIOListener();
                    break;
            }

        }
        void StartInternalIOManager()
        {
            RawInput.Start();
            RawInput.OnKeyDown += OnRawKeyDown;
            RawInput.OnKeyUp += OnRawKeyUp;
            try
            {
                COMReceiveAsync(_cancelSource.Token);
            }
            catch
            {
                Debug.LogWarning("Cannot open COM3, using Mouse as fallback.");
                useDummy = true;
            }
        }
        public void StartExternalIOManager()
        {
            if(_ioManager is null)
                _ioManager = new();
            var useHID = MajInstances.Setting.Misc.InputDevice.ButtonRing.Type is DeviceType.HID;
            var executionQueue = GameManager.ExecutionQueue;
            var buttonRingCallbacks = new Dictionary<ButtonRingZone, Action<ButtonRingZone, InputState>>();
            var touchPanelCallbacks = new Dictionary<TouchPanelZone, Action<TouchPanelZone, InputState>>();

            foreach (ButtonRingZone zone in Enum.GetValues(typeof(ButtonRingZone)))
                buttonRingCallbacks[zone] = (zone, state) => executionQueue.Enqueue(() => OnKeyStateChanged(zone, state));

            foreach (TouchPanelZone zone in Enum.GetValues(typeof(TouchPanelZone)))
                touchPanelCallbacks[zone] = (zone, state) => _COMReport[(int)zone] = state is InputState.On;

            
            _ioManager.Destroy();
            _ioManager.SubscribeToAllEvents(ExternalIOEventHandler);
            _ioManager.AddDeviceErrorHandler(new DeviceErrorHandler(_ioManager, 4));

            try
            {
                var deviceName = useHID ? AdxHIDButtonRing.GetDeviceName() : AdxIO4ButtonRing.GetDeviceName();
                var btnPollingRate = MajInstances.Setting.Misc.InputDevice.ButtonRing.PollingRateMs;
                var btnDebounceThresholdMs = MajInstances.Setting.Misc.InputDevice.ButtonRing.DebounceThresholdMs;
                var touchPanelPollingRate = MajInstances.Setting.Misc.InputDevice.TouchPanel.PollingRateMs;
                var touchPanelDebounceThresholdMs = MajInstances.Setting.Misc.InputDevice.TouchPanel.DebounceThresholdMs;
                Dictionary<string, dynamic>? btnConnProperties = null;
                Dictionary<string, dynamic>? touchPanelConnProperties = null;

                if (btnPollingRate != 0)
                {
                    btnConnProperties = new()
                    {
                        { "PollingRateMs", btnPollingRate },
                        { "DebounceTimeMs", btnDebounceThresholdMs }
                    };
                }
                if (touchPanelPollingRate != 0)
                {
                    touchPanelConnProperties = new()
                    {
                        { "PollingRateMs", touchPanelPollingRate },
                        { "DebounceTimeMs", touchPanelDebounceThresholdMs }
                    };
                }

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
                Debug.LogException(e);
            }
        }
        void StartInternalIOListener()
        {
            UniTask.Void(async () =>
            {
                var executionQueue = GameManager.ExecutionQueue;
                while (!_cancelSource.IsCancellationRequested)
                {
                    try
                    {
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
                        Debug.LogException(e);
                    }
                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
                }
            });
        }
        void StartExternalIOListener()
        {
            UniTask.Void(async () =>
            {
                var executionQueue = GameManager.ExecutionQueue;
                while (!_cancelSource.IsCancellationRequested)
                {
                    try
                    {
                        while (executionQueue.TryDequeue(out var eventAction))
                            eventAction();
                        UpdateSensorState();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
                }
            });
        }
        void ExternalIOEventHandler(IOEventType eventType,DeviceClassification deviceType,string msg)
        {
            var executionQueue = IOManager.ExecutionQueue;
            var logContent = $"From external IOManager:\nEventType: {eventType}\nDeviceType: {deviceType}\nMsg: {msg.Trim()}";
            switch (eventType)
            {
                case IOEventType.Attach:
                case IOEventType.Debug:
                    executionQueue.Enqueue(() => Debug.Log(logContent));
                    break;
                case IOEventType.ConnectionError:
                case IOEventType.SerialDeviceReadError:
                case IOEventType.HidDeviceReadError:
                case IOEventType.ReconnectionError:
                case IOEventType.InvalidDevicePropertyError:
                    executionQueue.Enqueue(() => Debug.LogError(logContent));
                    break;
                case IOEventType.Detach:
                    executionQueue.Enqueue(() => Debug.LogWarning(logContent));
                    break;
            }
        }
        void OnApplicationQuit()
        {
            _cancelSource.Cancel();
            RawInput.Stop();
            if (_recvTask != null && !_recvTask.IsCompleted)
                _recvTask.Wait();
        }
        public void BindAnyArea(EventHandler<InputEventArgs> checker) => OnAnyAreaTrigger += checker;
        public void BindArea(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var sensor = _sensors.Find(x => x.Type == sType);
            var button = _buttons.Find(x => x.Type == sType);
            if (sensor == null || button is null)
                throw new Exception($"{sType} Sensor or Button not found.");

            sensor.AddSubscriber(checker);
            button.AddSubscriber(checker);
        }
        public void UnbindAnyArea(EventHandler<InputEventArgs> checker) => OnAnyAreaTrigger -= checker;
        public void UnbindArea(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var sensor = _sensors.Find(x => x.Type == sType);
            var button = _buttons.Find(x => x.Type == sType);
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
            if (target > SensorType.A8)
                throw new ArgumentOutOfRangeException("Button index cannot greater than A8");
            var button = _buttons.Find(x => x.Type == target);

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
        public Button? GetButton(SensorType type) => _buttons.Find(x => x.Type == type);
        public Sensor GetSensor(SensorType target) => _sensors[(int)target];
        public Sensor[] GetSensors() => _sensors.ToArray();
        public Sensor[] GetSensors(SensorGroup group) => _sensors.Where(x => x.Group == group).ToArray();
        public void ClearAllSubscriber()
        {
            foreach(var sensor in _sensors)
                sensor.ClearSubscriber();
            foreach(var button in _buttons)
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