using HidSharp;
using HidSharp.Platform.Windows;
using MajdataPlay.Collections;
using MajdataPlay.Numerics;
using MajdataPlay.Settings;
using MajdataPlay.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
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
                return MajEnv.Settings.IO.InputDevice.TouchPanel.TouchSimulationRadius;
            }
        }
        public static ReadOnlySpan<SwitchStatus> ButtonStatusInThisFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _btnStatusInThisFrame;
        }
        public static ReadOnlySpan<SwitchStatus> ButtonStatusInPreviousFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _btnStatusInPreviousFrame;
        }
        public static ReadOnlySpan<SwitchStatus> SensorStatusInThisFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sensorStatusInThisFrame;
        }
        public static ReadOnlySpan<SwitchStatus> SensorStatusInPreviousFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sensorStatusInPreviousFrame;
        }
#if UNITY_ANDROID
        public static ReadOnlySpan<int> SensorClickedCountInThisFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sensorClickedCountInThisFrame;
        }
#endif
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
            new Button(KeyCode.B1,ButtonZone.A1),
            new Button(KeyCode.B2,ButtonZone.A2),
            new Button(KeyCode.B3,ButtonZone.A3),
            new Button(KeyCode.B4,ButtonZone.A4),
            new Button(KeyCode.B5,ButtonZone.A5),
            new Button(KeyCode.B6,ButtonZone.A6),
            new Button(KeyCode.B7,ButtonZone.A7),
            new Button(KeyCode.B8,ButtonZone.A8),
            new Button(KeyCode.Test,ButtonZone.Test),
            new Button(KeyCode.SelectP1,ButtonZone.P1),
            new Button(KeyCode.Service,ButtonZone.Service),
            new Button(KeyCode.SelectP2,ButtonZone.P2),
        };
        readonly static TimeSpan[] _btnLastTriggerTimes = new TimeSpan[12];
        readonly static SwitchStatus[] _btnStatusInPreviousFrame = new SwitchStatus[12];
        readonly static SwitchStatus[] _btnStatusInThisFrame = new SwitchStatus[12];

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
        readonly static SwitchStatus[] _sensorStatusInPreviousFrame = new SwitchStatus[33];
        readonly static SwitchStatus[] _sensorStatusInThisFrame = new SwitchStatus[33];
#if UNITY_ANDROID
        readonly static int[] _sensorClickedCountInThisFrame = new int[33];
#endif

        static bool _useDummy = false;
        readonly static bool _isBtnDebounceEnabled = false;
        readonly static bool _isSensorDebounceEnabled = false;
        readonly static bool _isSensorRendererEnabled = false;

        readonly static IOThreadSynchronization _ioThreadSync = new IOThreadSynchronization();

        static IReadOnlyDictionary<int, int> _instanceID2SensorIndexMappingTable = new Dictionary<int, int>();

        static SerialPortConnInfo _touchPanelSerialConnInfo = default;
        static SerialPortConnInfo _ledDeviceSerialConnInfo = default;
        static HidConnInfo _ledDeviceHidConnInfo = default;
        static HidConnInfo _buttonRingHidConnInfo = default;

        static int _playerIndex = 1;
        static DeviceManufacturerOption _deviceManufacturer = DeviceManufacturerOption.General;
        static ButtonRingDeviceOption _buttonRingDevice = ButtonRingDeviceOption.Keyboard;
        static InputManager()
        {
            _isSensorRendererEnabled = MajEnv.Settings.Debug.DisplaySensor;
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
            var len = _cachedPositions.Length;
            for (var i = 0; i < len; i++)
            {
                _cachedPositions[i] = new ulong?[len];
            }
            //for (var i = 0; i < 8; i++)
            //{
            //    _touchRecords.Add((SensorArea)i, new(10));
            //}
            MajEnv.OnApplicationQuit += OnApplicationQuit;
        }
        internal static void Init(IReadOnlyDictionary<int, int> instanceID2SensorIndexMappingTable)
        {
            Input.multiTouchEnabled = true;
            EnhancedTouchSupport.Enable();
            _instanceID2SensorIndexMappingTable = instanceID2SensorIndexMappingTable;
            _lastScreenHeight = Screen.height;
            _lastScreenWidth = Screen.width;
#if UNITY_STANDALONE
            IODeviceDetect();
            ButtonRing.Init();
            TouchPanel.Init();
            LedDevice.Init();
#endif
        }
        //static ScreenInfo GenerateScreenInfo(int width, int height)
        //{
        //    var mainCamera = Majdata<IMainCameraProvider>.Instance!.MainCamera;

        //    var positionMappingTable = new ulong[width][];
        //    var screenToWorldPoints = new Point[width][];
        //    for (var x = 0; x < width; x++)
        //    {
        //        positionMappingTable[x] = new ulong[height];
        //        screenToWorldPoints[x] = new Point[height];
        //        for (var y = 0; y < height; y++)
        //        {
        //            ref var value = ref positionMappingTable[x][y];
        //            ref var worldPoint = ref screenToWorldPoints[x][y];

        //            var pos = new Vector3(x, y);
        //            Vector3 cubeRay = mainCamera.ScreenToWorldPoint(pos);
        //            var rayToCenter = cubeRay - new Vector3(0, 0, -10);
        //            worldPoint = new()
        //            {
        //                X = rayToCenter.x,
        //                Y = rayToCenter.y,
        //                Z = rayToCenter.z
        //            };
        //            var radToCenter = (rayToCenter).magnitude;

        //            if (radToCenter > 9.28)
        //            {
        //                value |= 1UL << (9 + 34);
        //                continue;
        //            }
        //            else if (radToCenter > 5.4f)
        //            {
        //                // out of the screen area to the button area
        //                var degree = -Mathf.Atan2(rayToCenter.y, rayToCenter.x) * Mathf.Rad2Deg + 180;
        //                var btnPos = (int)(degree / 45f);
        //                switch (btnPos)
        //                {
        //                    case 0:
        //                        value |= 1UL << (6 + 34);
        //                        continue;
        //                    case 1:
        //                        value |= 1UL << (7 + 34);
        //                        continue;
        //                    default:
        //                        value |= 1UL << (btnPos - 2 + 34);
        //                        continue;
        //                }
        //            }
        //            for (int i = 0; i < 9; i++)
        //            {
        //                var rad = FingerRadius;
        //                var circular = new Vector3(rad * Mathf.Sin(45f * i), rad * Mathf.Cos(45f * i));
        //                if (i == 8) circular = Vector3.zero;
        //                var ray = new Ray(cubeRay + circular, Vector3.forward);
        //                var ishit = Physics.Raycast(ray, out var hitInfom);
        //                if (ishit)
        //                {
        //                    var id = hitInfom.colliderInstanceID;
        //                    if (_instanceID2SensorIndexMappingTable.TryGetValue(id, out var index))
        //                    {
        //                        value |= 1UL << index;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return new ScreenInfo()
        //    {
        //        Width = width,
        //        Height = height,
        //        ScreenToWorldPoints = screenToWorldPoints,
        //        TouchPanelMappingTable = positionMappingTable,
        //        FingerRadius = FingerRadius
        //    };
        //}
        static void IODeviceDetect()
        {
            const int YUAN_HID_1P_PID = 22352;
            const int YUAN_HID_1P_VID = 11836;
            const int YUAN_HID_2P_PID = 22352;
            const int YUAN_HID_2P_VID = 11852;
            const int DAO_HID_PID = 4644;
            const int DAO_HID_VID = 3727;
            const int GENERAL_HID_1P_PID = 33;
            const int GENERAL_HID_1P_VID = 3235;
            const int GENERAL_HID_2P_PID = 33;
            const int GENERAL_HID_2P_VID = 3235;

            var hidDevices = HidManager.Devices;
#if ENABLE_IL2CPP
            var serialPorts = "NotSupported";
#else
            var serialPorts = SerialPort.GetPortNames();
#endif

            var ioSettings = MajEnv.Settings.IO;
            var playerIndex = ioSettings.InputDevice.Player;
            var buttonRingSettings = ioSettings.InputDevice.ButtonRing;
            var touchPanelSettings = ioSettings.InputDevice.TouchPanel;
            var ledDeviceSettings = ioSettings.OutputDevice.Led;
            var userManufacturer = ioSettings.Manufacturer;
            var userButtonRingType = buttonRingSettings.Type;

            var manufacturer = DeviceManufacturerOption.General;
            var buttonRingType = userButtonRingType ?? ButtonRingDeviceOption.Keyboard;

            MajDebug.LogInfo($"All available HID devices:\n{string.Join('\n', hidDevices)}");
            MajDebug.LogInfo($"All available serial ports:\n{string.Join('\n', serialPorts)}");

            try
            {
                if (userButtonRingType is not null && buttonRingType == ButtonRingDeviceOption.Keyboard)
                {
                    manufacturer = DeviceManufacturerOption.General;

                    _deviceManufacturer = manufacturer;
                    _buttonRingDevice = buttonRingType;
                    _touchPanelSerialConnInfo = new()
                    {
                        Port = touchPanelSettings.SerialPortOptions.Port ?? (playerIndex == 1 ? 3 : 4),
                        BaudRate = touchPanelSettings.SerialPortOptions.BaudRate ?? 9600,
                    };
                    _ledDeviceSerialConnInfo = new()
                    {
                        Port = ledDeviceSettings.SerialPortOptions.Port ?? (playerIndex == 1 ? 21 : 22),
                        BaudRate = ledDeviceSettings.SerialPortOptions.BaudRate ?? 115200,
                    };
                    return;
                }

                if (userManufacturer is not null)
                {
                    MajDebug.LogInfo("User has set the IO manufacturer and button ring type, use the user-set values");
                    manufacturer = (DeviceManufacturerOption)userManufacturer;
                    switch (manufacturer)
                    {
                        case DeviceManufacturerOption.General:
                            buttonRingType = ButtonRingDeviceOption.HID;
                            _touchPanelSerialConnInfo = new()
                            {
                                Port = touchPanelSettings.SerialPortOptions.Port ?? (playerIndex == 1 ? 3 : 4),
                                BaudRate = touchPanelSettings.SerialPortOptions.BaudRate ?? 9600,
                            };
                            _buttonRingHidConnInfo = new()
                            {
                                DeviceName = buttonRingSettings.HidOptions.DeviceName ?? string.Empty,
                                ProductId = buttonRingSettings.HidOptions.ProductId ?? (playerIndex == 1 ? GENERAL_HID_1P_PID : GENERAL_HID_2P_PID),
                                VendorId = buttonRingSettings.HidOptions.VendorId ?? (playerIndex == 1 ? GENERAL_HID_1P_VID : GENERAL_HID_2P_VID),
                                Exclusice = buttonRingSettings.HidOptions.Exclusice,
                                OpenPriority = buttonRingSettings.HidOptions.OpenPriority
                            };
                            _ledDeviceSerialConnInfo = new()
                            {
                                Port = ledDeviceSettings.SerialPortOptions.Port ?? (playerIndex == 1 ? 21 : 22),
                                BaudRate = ledDeviceSettings.SerialPortOptions.BaudRate ?? 115200,
                            };
                            break;
                        case DeviceManufacturerOption.Yuan:
                            buttonRingType = ButtonRingDeviceOption.HID;
                            _touchPanelSerialConnInfo = new()
                            {
                                Port = touchPanelSettings.SerialPortOptions.Port ?? (playerIndex == 1 ? 3 : 4),
                                BaudRate = touchPanelSettings.SerialPortOptions.BaudRate ?? 9600,
                            };
                            _buttonRingHidConnInfo = new()
                            {
                                DeviceName = buttonRingSettings.HidOptions.DeviceName ?? string.Empty,
                                ProductId = buttonRingSettings.HidOptions.ProductId ?? (playerIndex == 1 ? YUAN_HID_1P_PID : YUAN_HID_2P_PID),
                                VendorId = buttonRingSettings.HidOptions.VendorId ?? (playerIndex == 1 ? YUAN_HID_1P_VID : YUAN_HID_2P_VID),
                                Exclusice = buttonRingSettings.HidOptions.Exclusice,
                                OpenPriority = buttonRingSettings.HidOptions.OpenPriority
                            };
                            _ledDeviceSerialConnInfo = new()
                            {
                                Port = ledDeviceSettings.SerialPortOptions.Port ?? (playerIndex == 1 ? 21 : 22),
                                BaudRate = ledDeviceSettings.SerialPortOptions.BaudRate ?? 115200,
                            };
                            break;
                        case DeviceManufacturerOption.Dao:
                            buttonRingType = ButtonRingDeviceOption.HID;
                            _buttonRingHidConnInfo = new()
                            {
                                DeviceName = buttonRingSettings.HidOptions.DeviceName ?? string.Empty,
                                ProductId = buttonRingSettings.HidOptions.ProductId ?? DAO_HID_PID,
                                VendorId = buttonRingSettings.HidOptions.VendorId ?? DAO_HID_VID,
                                Exclusice = buttonRingSettings.HidOptions.Exclusice,
                                OpenPriority = buttonRingSettings.HidOptions.OpenPriority
                            };
                            _ledDeviceHidConnInfo = new()
                            {
                                DeviceName = ledDeviceSettings.HidOptions.DeviceName ?? string.Empty,
                                ProductId = ledDeviceSettings.HidOptions.ProductId ?? DAO_HID_PID,
                                VendorId = ledDeviceSettings.HidOptions.VendorId ?? DAO_HID_VID,
                                Exclusice = ledDeviceSettings.HidOptions.Exclusice,
                                OpenPriority = ledDeviceSettings.HidOptions.OpenPriority
                            };
                            break;
                    }
                }
                else
                {
                    var yuanDefaultHidPID = YUAN_HID_1P_PID;
                    var yuanDefaultHidVID = YUAN_HID_1P_VID;
                    var daoDefaultHidPID = DAO_HID_PID;
                    var daoDefaultHidVID = DAO_HID_VID;
                    var generalDefaultHidPID = GENERAL_HID_1P_PID;
                    var generalDefaultHidVID = GENERAL_HID_1P_VID;

                    var touchPanelDefaultSerialPort = 3;
                    var ledDeviceDefaultSerialPort = 21;

                    if(playerIndex != 1)
                    {
                        yuanDefaultHidPID = YUAN_HID_2P_PID;
                        yuanDefaultHidVID = YUAN_HID_2P_VID;
                        generalDefaultHidPID = GENERAL_HID_2P_PID;
                        generalDefaultHidVID = GENERAL_HID_2P_VID;
                        touchPanelDefaultSerialPort = 4;
                        ledDeviceDefaultSerialPort = 22;
                    }


                    MajDebug.LogInfo("User has not set the IO manufacturer and button ring type, will be detected automatically");
                    var filteredHidDevices = hidDevices.Where(x =>
                    {
                        var isYuan = x.ProductID == yuanDefaultHidPID && x.VendorID == yuanDefaultHidVID;
                        var isDao = x.ProductID == daoDefaultHidPID && x.VendorID == daoDefaultHidVID;
                        var isGeneral = x.ProductID == generalDefaultHidPID && x.VendorID == generalDefaultHidVID;
                        var result = isYuan || isDao || isGeneral;

                        return result;
                    });
                    if (filteredHidDevices.Count() != 0)
                    {
                        if (hidDevices.Any(x => x.ProductID == yuanDefaultHidPID && x.VendorID == yuanDefaultHidVID))
                        {
                            MajDebug.LogInfo("Manufacturer detect result: Yuan");
                            manufacturer = DeviceManufacturerOption.Yuan;
                            buttonRingType = ButtonRingDeviceOption.HID;
                            _buttonRingHidConnInfo = new()
                            {
                                DeviceName = string.Empty,
                                ProductId = yuanDefaultHidPID,
                                VendorId = yuanDefaultHidVID,
                                Exclusice = false,
                                OpenPriority = OpenPriority.VeryHigh
                            };
                            _touchPanelSerialConnInfo = new()
                            {
                                Port = touchPanelDefaultSerialPort,
                                BaudRate = 9600,
                            };
                            _ledDeviceSerialConnInfo = new()
                            {
                                Port = ledDeviceDefaultSerialPort,
                                BaudRate = 115200,
                            };
                        }
                        else if (hidDevices.Any(x => x.ProductID == daoDefaultHidPID && x.VendorID == daoDefaultHidVID))
                        {
                            MajDebug.LogInfo("Manufacturer detect result: Dao");
                            manufacturer = DeviceManufacturerOption.Dao;
                            buttonRingType = ButtonRingDeviceOption.HID;
                            _buttonRingHidConnInfo = new()
                            {
                                DeviceName = string.Empty,
                                ProductId = daoDefaultHidPID,
                                VendorId = daoDefaultHidVID,
                                Exclusice = false,
                                OpenPriority = OpenPriority.VeryHigh
                            };
                            _ledDeviceHidConnInfo = new()
                            {
                                DeviceName = string.Empty,
                                ProductId = daoDefaultHidPID,
                                VendorId = daoDefaultHidVID,
                                Exclusice = false,
                                OpenPriority = OpenPriority.VeryHigh
                            };
                        }
                        else if (hidDevices.Any(x => x.ProductID == generalDefaultHidPID && x.VendorID == generalDefaultHidVID))
                        {
                            MajDebug.LogInfo("Manufacturer detect result: General");
                            manufacturer = DeviceManufacturerOption.General;
                            buttonRingType = ButtonRingDeviceOption.HID;
                            _touchPanelSerialConnInfo = new()
                            {
                                Port = touchPanelDefaultSerialPort,
                                BaudRate = 9600,
                            };
                            _buttonRingHidConnInfo = new()
                            {
                                DeviceName = string.Empty,
                                ProductId = generalDefaultHidPID,
                                VendorId = generalDefaultHidVID,
                                Exclusice = false,
                                OpenPriority = OpenPriority.VeryHigh
                            };
                            _ledDeviceSerialConnInfo = new()
                            {
                                Port = ledDeviceDefaultSerialPort,
                                BaudRate = 115200,
                            };
                        }
                        else
                        {
                            throw new ArgumentException("?");
                        }
                    }
                    else
                    {
                        MajDebug.LogWarning("No HID device detected, fallback to keyboard");
                        manufacturer = DeviceManufacturerOption.General;
                        buttonRingType = ButtonRingDeviceOption.Keyboard;
                        _touchPanelSerialConnInfo = new()
                        {
                            Port = touchPanelDefaultSerialPort,
                            BaudRate = 9600,
                        };
                        _ledDeviceSerialConnInfo = new()
                        {
                            Port = ledDeviceDefaultSerialPort,
                            BaudRate = 115200,
                        };
                    }
                }
            }
            catch(Exception e)
            {
                MajDebug.LogException(e);
            }
            finally
            {
                MajDebug.LogInfo($"Player: {(playerIndex == 1 ? "1P" : "2P")}");
                MajDebug.LogInfo($"IO manufacturer: {manufacturer}");
                MajDebug.LogInfo($"Button ring device type: {buttonRingType}");
                _deviceManufacturer = manufacturer;
                _buttonRingDevice = buttonRingType;
                _playerIndex = playerIndex;
            }
        }
        internal static void OnFixedUpdate()
        {
            //_updateIOListener();
        }
        internal static void OnPreUpdate()
        {
            var buttons = _buttons.Span;
            var sensors = _sensors.Span;
            
            try
            {
#if UNITY_STANDALONE
                ButtonRing.OnPreUpdate();
                TouchPanel.OnPreUpdate();
#elif UNITY_ANDROID
                Array.Fill(_sensorClickedCountInThisFrame, 0);
#endif
                var height = Screen.height;
                var width = Screen.width;

                if(height != _lastScreenHeight || width != _lastScreenWidth)
                {
                    _lastScreenWidth = width;
                    _lastScreenHeight = height;
                    _version++;
                }

                UpdateMousePosition();
                UpdateSensorState();
                UpdateButtonState();
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
        public static void BindArea(EventHandler<InputEventArgs> checker, ButtonZone sType)
        {
            var sensor = GetSensor(sType.ToSensorArea());
            var button = GetButton(sType);
            if (sensor == null || button is null)
            {
                throw new Exception($"{sType} Sensor or Button not found.");
            }

            sensor.AddSubscriber(checker);
            button.AddSubscriber(checker);
        }
        public static void UnbindAnyArea(EventHandler<InputEventArgs> checker) => OnAnyAreaTrigger -= checker;
        public static void UnbindArea(EventHandler<InputEventArgs> checker, ButtonZone sType)
        {
            var sensor = GetSensor(sType.ToSensorArea());
            var button = GetButton(sType);
            if (sensor is null || button is null)
            {
                throw new Exception($"{sType} Sensor or Button not found.");
            }

            sensor.RemoveSubscriber(checker);
            button.RemoveSubscriber(checker);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckAreaStatus(ButtonZone sType, SwitchStatus targetStatus)
        {
            return CheckSensorStatus(sType.ToSensorArea(),targetStatus) || CheckButtonStatus(sType, targetStatus);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckSensorStatus(SensorArea target, SwitchStatus targetStatus)
        {
            ThrowIfSensorIndexOutOfRange(target);

            var sensor = _sensors.Span[(int)target];
            if (sensor is null)
                throw new Exception($"{target} Sensor or Button not found.");

            return sensor.State == targetStatus;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckButtonStatus(ButtonZone target, SwitchStatus targetStatus)
        {
            ThrowIfButtonIndexOutOfRange(target);
            var button = GetButton(target);

            if (button is null)
                throw new Exception($"{target} Button not found.");

            return button.State == targetStatus;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckButtonStatusInThisFrame(ButtonZone target, SwitchStatus targetStatus)
        {
            ThrowIfButtonIndexOutOfRange(target);
            var index = GetButtonIndex(target);

            return _btnStatusInThisFrame[index] == targetStatus;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckButtonStatusInPreviousFrame(ButtonZone target, SwitchStatus targetStatus)
        {
            ThrowIfButtonIndexOutOfRange(target);
            var index = GetButtonIndex(target);

            return _btnStatusInPreviousFrame[index] == targetStatus;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchStatus GetButtonStatusInThisFrame(ButtonZone target)
        {
            ThrowIfButtonIndexOutOfRange(target);
            var index = GetButtonIndex(target);

            return _btnStatusInThisFrame[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchStatus GetButtonStatusInPreviousFrame(ButtonZone target)
        {
            ThrowIfButtonIndexOutOfRange(target);
            var index = GetButtonIndex(target);

            return _btnStatusInPreviousFrame[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsButtonClickedInThisFrame(ButtonZone target)
        {
            ThrowIfButtonIndexOutOfRange(target);
            var index = GetButtonIndex(target);

            return _btnStatusInPreviousFrame[index] == SwitchStatus.Off &&
                   _btnStatusInThisFrame[index] == SwitchStatus.On;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsButtonClickedInThisFrame_OR(params ButtonZone[] targets)
        {
            return IsButtonClickedInThisFrame_OR(targets.AsSpan());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsButtonClickedInThisFrame_OR(ReadOnlySpan<ButtonZone> targets)
        {
            foreach (var target in targets)
            {
                ThrowIfButtonIndexOutOfRange(target);
                var index = GetButtonIndex(target);

                var result = _btnStatusInPreviousFrame[index] == SwitchStatus.Off &&
                             _btnStatusInThisFrame[index] == SwitchStatus.On;
                if (result)
                {
                    return true;
                }
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsButtonClickedInThisFrame_AND(params ButtonZone[] targets)
        {
            return IsButtonClickedInThisFrame_AND(targets.AsSpan());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsButtonClickedInThisFrame_AND(ReadOnlySpan<ButtonZone> targets)
        {
            foreach (var target in targets)
            {
                ThrowIfButtonIndexOutOfRange(target);
                var index = GetButtonIndex(target);

                var result = _btnStatusInPreviousFrame[index] == SwitchStatus.Off &&
                             _btnStatusInThisFrame[index] == SwitchStatus.On;
                if (!result)
                {
                    return false;
                }
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckSensorStatusInThisFrame(SensorArea target, SwitchStatus targetStatus)
        {
            ThrowIfSensorIndexOutOfRange(target);
            var index = (int)target;

            return _sensorStatusInThisFrame[index] == targetStatus;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckSensorStatusInPreviousFrame(SensorArea target, SwitchStatus targetStatus)
        {
            ThrowIfSensorIndexOutOfRange(target);
            var index = (int)target;

            return _sensorStatusInPreviousFrame[index] == targetStatus;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchStatus GetSensorStatusInThisFrame(SensorArea target)
        {
            ThrowIfSensorIndexOutOfRange(target);
            var index = (int)target;

            return _sensorStatusInThisFrame[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchStatus GetSensorStatusInPreviousFrame(SensorArea target)
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

            return _sensorStatusInPreviousFrame[index] == SwitchStatus.Off &&
                   _sensorStatusInThisFrame[index] == SwitchStatus.On;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSensorClickedInThisFrame_OR(params SensorArea[] targets)
        {
            return IsSensorClickedInThisFrame_OR(targets.AsSpan());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSensorClickedInThisFrame_OR(ReadOnlySpan<SensorArea> targets)
        {
            foreach (var target in targets)
            {
                ThrowIfSensorIndexOutOfRange(target);
                var index = (int)target;

                var result = _sensorStatusInPreviousFrame[index] == SwitchStatus.Off &&
                             _sensorStatusInThisFrame[index] == SwitchStatus.On;
                if (result)
                {
                    return true;
                }
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSensorClickedInThisFrame_AND(params SensorArea[] targets)
        {
            return IsSensorClickedInThisFrame_AND(targets.AsSpan());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSensorClickedInThisFrame_AND(ReadOnlySpan<SensorArea> targets)
        {
            foreach (var target in targets)
            {
                ThrowIfSensorIndexOutOfRange(target);
                var index = (int)target;

                var result = _sensorStatusInPreviousFrame[index] == SwitchStatus.Off &&
                             _sensorStatusInThisFrame[index] == SwitchStatus.On;
                if (!result)
                {
                    return false;
                }
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Button? GetButton(ButtonZone zone)
        {
            var buttons = _buttons.Span;
            ThrowIfButtonIndexOutOfRange(zone);

            return buttons[(int)zone];
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
        static bool JitterDetect(SensorArea zone, TimeSpan now)
        {
            var index = (int)zone;
            TimeSpan lastTriggerTime = _sensorLastTriggerTimes[index];
            TimeSpan debounceTime = _sensorDebounceThresholdMs;

            var diff = now - lastTriggerTime;
            if (diff < debounceTime)
            {
                MajDebug.LogInfo($"[Debounce] Received sensor response\nZone: {zone}\nInterval: {diff.Milliseconds}ms");
                return true;
            }
            return false;
        }
        /// <summary>
        /// Used to check whether the device activation is caused by abnormal jitter
        /// </summary>
        /// <param name="zone"></param>
        /// <returns>
        /// If the trigger interval is lower than the debounce threshold, returns <see cref="bool">true</see>, otherwise <see cref="bool">false</see>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool JitterDetect(ButtonZone zone, TimeSpan now)
        {
            var index = (int)zone;
            TimeSpan lastTriggerTime = _btnLastTriggerTimes[index];
            TimeSpan debounceTime = _btnDebounceThresholdMs;

            var diff = now - lastTriggerTime;
            if (diff < debounceTime)
            {
                MajDebug.LogInfo($"[Debounce] Received button response\nZone: {zone}\nInterval: {diff.Milliseconds}ms");
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ThrowIfButtonIndexOutOfRange(ButtonZone target)
        {
            if (target > ButtonZone.P2 || target < ButtonZone.A1)
            {
                throw new ArgumentOutOfRangeException("Button index cannot greater than A8");
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ThrowIfSensorIndexOutOfRange(SensorArea area)
        {
            if (area < SensorArea.A1 || area > SensorArea.E8)
                throw new ArgumentOutOfRangeException();
        }
        static int GetButtonIndex(ButtonZone area)
        {
            return (int)area;
        }
        class IOThreadSynchronization
        {
            public ReadOnlySpan<byte> ReadBuffer
            {
                get
                {
                    return ReadBufferMemory.Span;
                }
            }
            public Span<byte> WriteBuffer
            {
                get
                {
                    return WriteBufferMemory.Span;
                }
            }
            public Memory<byte> WriteBufferMemory { get; set; } = Memory<byte>.Empty;
            public ReadOnlyMemory<byte> ReadBufferMemory { get; set; } = ReadOnlyMemory<byte>.Empty;

            readonly EventWaitHandle _eventWaitHandle = new(false, EventResetMode.AutoReset);

            public bool WaitNotify()
            {
                return _eventWaitHandle.WaitOne();
            }
            public void Notify()
            {
                _eventWaitHandle.Set();
            }
        }
        readonly struct HidConnInfo
        {
            public string DeviceName { get; init; }
            public int ProductId { get; init; }
            public int VendorId { get; init; }
            public bool Exclusice { get; init; }
            public OpenPriority OpenPriority { get; init; }
        }
        readonly struct SerialPortConnInfo
        {
            public int Port { get; init; }
            public int BaudRate { get; init; }
        }
    }
}