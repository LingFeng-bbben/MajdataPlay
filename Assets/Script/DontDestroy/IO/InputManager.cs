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
using Microsoft.Win32;
using System.Windows.Forms;
using Application = UnityEngine.Application;
using System.Security.Policy;
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
                sensors[index] = child.GetComponent<Sensor>();
                sensors[index].Type = (SensorType)index;
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
            switch(MajInstances.Setting.Misc.InputDevice)
            {
                case DeviceType.Keyboard:
                    CheckEnvironment(false);
                    StartInputDevicesListener(); 
                    break;
                case DeviceType.IO4:
                    CheckEnvironment();
                    StartExternalIOManager();
                    break;
                case DeviceType.HID:
                    CheckEnvironment();
                    StartExternalIOManager(true);
                    break;
            }

        }
        void StartInputDevicesListener()
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
            UniTask.Void(async () =>
            {
                while (!_cancelSource.IsCancellationRequested)
                {
                    if (useDummy)
                        UpdateMousePosition();
                    else
                        UpdateSensorState();
                    UpdateButtonState();
                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
                }
            });
        }
        void StartExternalIOManager(bool useHID = false)
        {
            _ioManager = new();
            var executionQueue = IOManager.ExecutionQueue;
            var buttonRingCallbacks = new Dictionary<ButtonRingZone, Action<ButtonRingZone, InputState>>();
            var touchPanelCallbacks = new Dictionary<TouchPanelZone, Action<TouchPanelZone, InputState>>();
            var eventCallbacks = new Dictionary<IOEventType, ControllerEventDelegate>
            {
                {
                    IOEventType.Attach,
                    (eventType, deviceType, message) =>
                    {
                        executionQueue.Enqueue(() =>
                        {
                            Debug.Log($"From external IOManager:\nEventType: {eventType}\nDeviceType: {deviceType}\nMsg: {message.Trim()}");
                        });
                    }
                },
                {
                    IOEventType.ConnectionError,
                    (eventType, deviceType, message) =>
                    {
                        executionQueue.Enqueue(() =>
                        {
                            Debug.LogError($"From external IOManager:\nEventType: {eventType}\nDeviceType: {deviceType}\nMsg: {message.Trim()}");
                        });
                    }
                },
                {
                    IOEventType.Debug,
                    (eventType, deviceType, message) =>
                    {
                        executionQueue.Enqueue(() =>
                        {
                            Debug.Log($"From external IOManager:\nEventType: {eventType}\nDeviceType: {deviceType}\nMsg: {message.Trim()}");
                        });
                    }
                },
                {
                    IOEventType.Detach,
                    (eventType, deviceType, message) =>
                    {
                        executionQueue.Enqueue(() =>
                        {
                            Debug.LogWarning($"From external IOManager:\nEventType: {eventType}\nDeviceType: {deviceType}\nMsg: {message.Trim()}");
                        });
                    }
                },
                {
                    IOEventType.SerialDeviceReadError,
                    (eventType, deviceType, message) =>
                    {
                        executionQueue.Enqueue(() =>
                        {
                            Debug.LogError($"From external IOManager:\nEventType: {eventType}\nDeviceType: {deviceType}\nMsg: {message.Trim()}");
                        });
                    }
                }

            };

            foreach (ButtonRingZone zone in Enum.GetValues(typeof(ButtonRingZone)))
                buttonRingCallbacks[zone] = (zone, state) => executionQueue.Enqueue(() => OnKeyStateChanged(zone, state));

            foreach (TouchPanelZone zone in Enum.GetValues(typeof(TouchPanelZone)))
                touchPanelCallbacks[zone] = (zone, state) => _COMReport[(int)zone] = state is InputState.On;

            
            _ioManager.Destroy();
            _ioManager.SubscribeToEvents(eventCallbacks);

            try
            {
                _ioManager.AddTouchPanel(AdxTouchPanel.GetDeviceName(),
                                         inputSubscriptions: touchPanelCallbacks);
                if(useHID)
                {
                    _ioManager.AddButtonRing(AdxHIDButtonRing.GetDeviceName(),
                                         inputSubscriptions: buttonRingCallbacks);
                }
                else
                {
                    _ioManager.AddButtonRing(AdxIO4ButtonRing.GetDeviceName(),
                                         inputSubscriptions: buttonRingCallbacks);
                }
                _ioManager.AddLedDevice(AdxLedDevice.GetDeviceName());
                
                UniTask.Void(async () =>
                {
                    while (!_cancelSource.IsCancellationRequested)
                    {
                        while (executionQueue.TryDequeue(out var eventAction))
                            eventAction();
                        UpdateSensorState();
                        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
                    }
                });
            }
            catch (Exception e)
            {
                Debug.LogException(e);
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
            var sensor = sensors.Find(x => x.Type == sType);
            var button = _buttons.Find(x => x.Type == sType);
            if (sensor == null || button is null)
                throw new Exception($"{sType} Sensor or Button not found.");

            sensor.OnStatusChanged += checker;
            button.OnStatusChanged += checker;
        }
        public void UnbindAnyArea(EventHandler<InputEventArgs> checker) => OnAnyAreaTrigger -= checker;
        public void UnbindArea(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var sensor = sensors.Find(x => x.Type == sType);
            var button = _buttons.Find(x => x.Type == sType);
            if (sensor == null || button is null)
                throw new Exception($"{sType} Sensor or Button not found.");

            sensor.OnStatusChanged -= checker;
            button.OnStatusChanged -= checker;
        }
        public bool CheckAreaStatus(SensorType sType, SensorStatus targetStatus)
        {
            return CheckSensorStatus(sType,targetStatus) || CheckButtonStatus(sType, targetStatus);
        }
        public bool CheckSensorStatus(SensorType target, SensorStatus targetStatus)
        {
            var sensor = sensors[(int)target];
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
        public Sensor GetSensor(SensorType target) => sensors[(int)target];
        public Sensor[] GetSensors() => sensors.ToArray();
        public Sensor[] GetSensors(SensorGroup group) => sensors.Where(x => x.Group == group).ToArray();
        public void ClearAllSubscriber()
        {
            foreach(var sensor in sensors)
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