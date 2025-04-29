using UnityEngine;
using System;
using System.Linq;
using MajdataPlay.Extensions;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using MychIO;
using DeviceType = MajdataPlay.IO.ButtonRingDeviceType;
using MychIO.Device;
using System.Collections.Generic;
using MychIO.Event;
using System.Runtime.CompilerServices;
using MajdataPlay.Collections;
using System.Collections.Concurrent;
using System.Security.Policy;
using HidSharp.Platform.Windows;
using System.Threading;
//using Microsoft.Win32;
//using System.Windows.Forms;
//using Application = UnityEngine.Application;
//using System.Security.Policy;
#nullable enable
namespace MajdataPlay.IO
{
    internal static unsafe partial class InputManager
    {
        public static bool IsTouchPanelConnected
        {
            get
            {
                return TouchPanel.IsConnected;
            }
        }
        public static bool IsButtonRingConnected
        {
            get
            {
                return ButtonRing.IsConnected;
            }
        }
        public static float FingerRadius
        {
            get
            {
                return MajEnv.UserSettings.IO.InputDevice.TouchPanel.TouchSimulationRadius;
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

        readonly static IOThreadSynchronization _ioThreadSync = new IOThreadSynchronization();

        static IOManager? _ioManager = null;

        static IReadOnlyDictionary<int, int> _instanceID2SensorIndexMappingTable = new Dictionary<int, int>();
        static InputManager()
        {
            _isSensorRendererEnabled = MajEnv.UserSettings.Debug.DisplaySensor;
            _btnDebounceThresholdMs = TimeSpan.FromMilliseconds(MajInstances.Settings.IO.InputDevice.ButtonRing.DebounceThresholdMs);
            _btnPollingRateMs = TimeSpan.FromMilliseconds(MajInstances.Settings.IO.InputDevice.ButtonRing.PollingRateMs);
            _sensorDebounceThresholdMs = TimeSpan.FromMilliseconds(MajInstances.Settings.IO.InputDevice.TouchPanel.DebounceThresholdMs);
            _sensorPollingRateMs = TimeSpan.FromMilliseconds(MajInstances.Settings.IO.InputDevice.TouchPanel.PollingRateMs);
            _isBtnDebounceEnabled = MajInstances.Settings.IO.InputDevice.ButtonRing.Debounce;
            _isSensorDebounceEnabled = MajInstances.Settings.IO.InputDevice.TouchPanel.Debounce;
            for (var i = 0; i < 33; i++)
            {
                if (i.InRange(0, 7))
                {
                    _btnLastTriggerTimes[i] = TimeSpan.Zero;
                }
                _sensorLastTriggerTimes[i] = TimeSpan.Zero;
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
            var buttons = _buttons.Span;
            var sensors = _sensors.Span;
            var executionQueue = MajEnv.ExecutionQueue;
            try
            {
                ButtonRing.OnPreUpdate();
                TouchPanel.OnPreUpdate();
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
            TimeSpan lastTriggerTime;
            TimeSpan debounceTime;
            if (isBtn)
            {
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

            //ButtonRing.OnButtonRingStateChanged(i, majState);
            _buttonRingInputBuffer.Enqueue(new()
            {
                Index = i,
                State = majState,
                Timestamp = MajTimeline.UnscaledTime
            });
        }
        class IOThreadSynchronization
        {
            public ReadOnlySpan<byte> Buffer
            {
                get
                {
                    return Memory.Span;
                }
            }
            public ReadOnlyMemory<byte> Memory { get; set; } = ReadOnlyMemory<byte>.Empty;

            readonly EventWaitHandle _eventWaitHandle = new(false, EventResetMode.AutoReset);

            public bool WaitOne()
            {
                return _eventWaitHandle.WaitOne();
            }
            public void Notify()
            {
                _eventWaitHandle.Set();
            }
        }
    }
}