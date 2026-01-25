using System;
using System.Threading;
using System.Threading.Tasks;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using MajdataPlay.IO.PdxTouch;
using MajdataPlay.Utils;

#nullable enable
namespace MajdataPlay.IO
{
    internal static partial class InputManager
    {
        internal static class PdxTouchPanel
        {
            private const int VID = 0x3356;
            private const int PID = 0x3003;
            private const int CONFIGURATION = 1;
            private const int INTERFACE_NUMBER = 1;
            private const int PACKET_SIZE = 64;
            private const byte REPORT_ID = 2;
            private const int MAX_TOUCH_POINTS = 10;
            private const int TOUCH_POINT_SIZE = 6;
            private const int RECONNECT_INTERVAL = 1000;

            // PDX 坐标范围
            private const float MIN_X = 18432f;
            private const float MIN_Y = 0f;
            private const float MAX_X = 0f;
            private const float MAX_Y = 32767f;
            private const bool FLIP = true;

            private static PdxTouchDevice? _device;
            private static Task _deviceTask = Task.CompletedTask;

            public static void Start()
            {
                var pdxOptions = MajEnv.Settings.IO.InputDevice.PdxTouch;
                if (!pdxOptions.Enable)
                {
                    MajDebug.LogInfo("PdxTouchPanel: Disabled in settings");
                    return;
                }

                var touchRadius = pdxOptions.TouchRadius;

                // 1P 设备
                _device = new PdxTouchDevice(0, null, touchRadius);
                _deviceTask = Task.Factory.StartNew(() => _device.Start(), TaskCreationOptions.LongRunning);
                MajDebug.LogInfo($"PdxTouchPanel: Starting 1P device");
            }

            private class PdxTouchDevice
            {
                private readonly int _playerNo;
                private readonly string? _locationPath;
                private readonly TouchSensorMapper _mapper;
                private UsbDevice? _usbDevice;
                private volatile bool _isConnected;

                // 手指状态追踪
                private readonly FingerPoint[] _fingerPoints = new FingerPoint[256];
                private ulong _frameAccumulator;
                private readonly object _touchLock = new();

                public bool IsConnected => _isConnected;

                public PdxTouchDevice(int playerNo, string? locationPath, int touchRadius)
                {
                    _playerNo = playerNo;
                    _locationPath = locationPath;
                    _mapper = new TouchSensorMapper(MIN_X, MIN_Y, MAX_X, MAX_Y, touchRadius, FLIP);

                    for (int i = 0; i < _fingerPoints.Length; i++)
                    {
                        _fingerPoints[i] = new FingerPoint();
                    }
                }

                public void Start()
                {
                    var currentThread = Thread.CurrentThread;
                    var token = MajEnv.GlobalCT;
                    var isReconnecting = false;

                    currentThread.Name = $"IO/PDX {_playerNo + 1}P Thread";
                    currentThread.IsBackground = true;
                    currentThread.Priority = MajEnv.THREAD_PRIORITY_IO;

                DEVICE_START:
                    try
                    {
                        if (token.IsCancellationRequested)
                            return;

                        if (!TryOpenDevice())
                        {
                            MajDebug.LogWarning($"PdxTouchPanel: Cannot open {_playerNo + 1}P device" +
                                (_locationPath != null ? $" at {_locationPath}" : ""));
                            if (!isReconnecting)
                            {
                                return;
                            }
                            Thread.Sleep(RECONNECT_INTERVAL);
                            goto DEVICE_START;
                        }

                        _isConnected = true;
                        isReconnecting = true;
                        MajDebug.LogInfo($"PdxTouchPanel: {_playerNo + 1}P device connected");

                        ReadLoop(token);
                    }
                    catch (OperationCanceledException)
                    {
                        // 正常退出
                    }
                    catch (Exception e)
                    {
                        MajDebug.LogError($"PdxTouchPanel: {_playerNo + 1}P error: {e}");
                    }
                    finally
                    {
                        _isConnected = false;
                        CloseDevice();
                    }

                    if (!token.IsCancellationRequested && isReconnecting)
                    {
                        MajDebug.LogInfo($"PdxTouchPanel: {_playerNo + 1}P reconnecting...");
                        Thread.Sleep(RECONNECT_INTERVAL);
                        goto DEVICE_START;
                    }
                }

                private bool TryOpenDevice()
                {
                    try
                    {
                        // 使用 VID/PID 查找设备
                        // 注意：当前版本的 LibUsbDotNet 不支持路径匹配
                        // 如果需要区分多个相同 VID/PID 的设备，需要使用序列号
                        var finder = new UsbDeviceFinder(VID, PID);

                        // 如果指定了路径，尝试用它作为序列号匹配
                        if (!string.IsNullOrWhiteSpace(_locationPath))
                        {
                            finder = new UsbDeviceFinder(VID, PID, _locationPath);
                        }

                        _usbDevice = UsbDevice.OpenUsbDevice(finder);
                        if (_usbDevice == null)
                        {
                            return false;
                        }

                        // 配置设备
                        if (_usbDevice is IUsbDevice wholeDevice)
                        {
                            wholeDevice.SetConfiguration(CONFIGURATION);
                            wholeDevice.ClaimInterface(INTERFACE_NUMBER);
                        }

                        return true;
                    }
                    catch (Exception e)
                    {
                        MajDebug.LogError($"PdxTouchPanel: Failed to open device: {e}");
                        return false;
                    }
                }

                private void CloseDevice()
                {
                    try
                    {
                        if (_usbDevice != null)
                        {
                            if (_usbDevice is IUsbDevice wholeDevice)
                            {
                                wholeDevice.ReleaseInterface(INTERFACE_NUMBER);
                            }
                            _usbDevice.Close();
                            _usbDevice = null;
                        }
                    }
                    catch (Exception e)
                    {
                        MajDebug.LogError($"PdxTouchPanel: Error closing device: {e}");
                    }
                }

                private void ReadLoop(CancellationToken token)
                {
                    byte[] buffer = new byte[PACKET_SIZE];
                    var reader = _usbDevice!.OpenEndpointReader(ReadEndpointID.Ep02);

                    try
                    {
                        while (!token.IsCancellationRequested && _usbDevice != null)
                        {
                            var ec = reader.Read(buffer, 100, out int bytesRead);

                            if (ec != ErrorCode.None)
                            {
                                if (ec == ErrorCode.IoTimedOut)
                                    continue;

                                MajDebug.LogWarning($"PdxTouchPanel: {_playerNo + 1}P read error: {ec}");
                                break;
                            }

                            if (bytesRead > 0)
                            {
                                OnTouchData(buffer);
                            }
                        }
                    }
                    finally
                    {
                        reader?.Dispose();
                    }
                }

                private void OnTouchData(byte[] data)
                {
                    byte reportId = data[0];
                    if (reportId != REPORT_ID)
                        return;

                    // 解析触摸点
                    for (int i = 0; i < MAX_TOUCH_POINTS; i++)
                    {
                        var index = i * TOUCH_POINT_SIZE + 1;
                        if (data[index] == 0)
                            continue;

                        bool isPressed = (data[index] & 0x01) == 1;
                        var fingerId = data[index + 1];
                        ushort x = BitConverter.ToUInt16(data, index + 2);
                        ushort y = BitConverter.ToUInt16(data, index + 4);

                        HandleFinger(x, y, fingerId, isPressed);
                    }

                    // 将累积的触摸状态写入共享数组
                    WriteToSharedState();
                }

                private void HandleFinger(ushort x, ushort y, int fingerId, bool isPressed)
                {
                    // 安全检查
                    if (fingerId < 0 || fingerId >= 256)
                        return;

                    lock (_touchLock)
                    {
                        ref var point = ref _fingerPoints[fingerId];

                        if (isPressed)
                        {
                            ulong touchMask = _mapper.ParseTouchPoint(x, y);
                            point.IsActive = true;
                            point.Mask = touchMask;
                            _frameAccumulator |= touchMask;
                        }
                        else
                        {
                            if (point.IsActive)
                            {
                                point.IsActive = false;
                                point.Mask = 0;
                            }
                        }
                    }
                }

                private void WriteToSharedState()
                {
                    ulong currentMask;
                    lock (_touchLock)
                    {
                        // 累积所有活跃手指的掩码
                        currentMask = _frameAccumulator;
                        for (int i = 0; i < _fingerPoints.Length; i++)
                        {
                            if (_fingerPoints[i].IsActive)
                            {
                                currentMask |= _fingerPoints[i].Mask;
                            }
                        }
                        _frameAccumulator = 0;
                    }

                    // 使用队列传递数据到主线程
                    var now = MajTimeline.UnscaledTime;
                    for (int i = 0; i < 34; i++)
                    {
                        bool state = (currentMask & (1UL << i)) != 0;
                        var switchState = state ? SwitchStatus.On : SwitchStatus.Off;

                        _touchPanelInputBuffer.Enqueue(new InputDeviceReport
                        {
                            Index = i,
                            State = switchState,
                            Timestamp = now
                        });
                    }
                }

                private struct FingerPoint
                {
                    public bool IsActive;
                    public ulong Mask;
                }
            }
        }
    }
}