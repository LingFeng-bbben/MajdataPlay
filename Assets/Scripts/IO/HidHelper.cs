#if UNITY_STANDALONE
using HidSharp;
using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MajdataPlay.Settings;
using MajdataPlay.Diagnostics;

#nullable enable
namespace MajdataPlay.IO
{

    internal static class HidHelper
    {
        public static IEnumerable<HidDevice> Devices
        {
            get
            {
                return _hidDevices;
            }
        }
        static IEnumerable<HidDevice> _hidDevices = Array.Empty<HidDevice>();
        readonly static List<HidDevice> _cacheList = new(); 
        static HidHelper()
        {
            var manufacturer = MajEnv.Settings.IO.Manufacturer;
            var buttonRingOptions = MajEnv.Settings.IO.InputDevice.ButtonRing;

            _hidDevices = DeviceList.Local.GetHidDevices();
            DeviceList.Local.Changed += OnDeviceListChanged;
        }
        public static bool TryGetDevices(DeviceFilter filter, [NotNullWhen(true)] out IEnumerable<HidDevice> devices)
        {
            lock (_hidDevices)
            {
                try
                {
                    var pid = filter.ProductId;
                    var vid = filter.VendorId;
                    var deviceName = filter.DeviceName;

                    foreach(var d in _hidDevices)
                    {
                        if(pid == d.ProductID && vid == d.VendorID)
                        {
                            var isMatch = false;
                            if (!string.IsNullOrEmpty(deviceName))
                            {
                                if ($"{d.GetManufacturer()} {d.GetProductName()}" == deviceName)
                                {
                                    isMatch = true;
                                }
                            }
                            else
                            {
                                isMatch = true;
                            }
                            if(isMatch)
                            {
                                _cacheList.Add(d);
                            }
                        }
                    }
                    if(_cacheList.Count != 0)
                    {
                        devices = _cacheList.ToArray()
                                            .OrderBy(x => x.GetInterfaceIndex());
                        return true;
                    }
                }
                catch (Exception e)
                {
                    MajDebug.LogException(e);
                }
                finally
                {
                    _cacheList.Clear();
                }
                devices = Array.Empty<HidDevice>();
                return false;
            }
        }
        public static bool TryGetAndOpenDevice(
            string tag,
            DeviceFilter filter,
            OpenConfiguration hidConfig,
            [NotNullWhen(true)] out HidDevice? hidDevice,
            [NotNullWhen(true)] out HidStream? hidStream,
            bool isReconnecting)
        {
            hidDevice = default;
            hidStream = default;
            if (!HidHelper.TryGetDevices(filter, out var devices))
            {
                if (isReconnecting)
                {
                    MajDebug.LogError(tag, $"HID device was lost, waiting for device to reconnect");
                    return false;
                }
                else
                {
                    MajDebug.LogWarning(tag, "HID device not found");
                    return false;
                }
            }
            foreach (var d in devices)
            {
                MajDebug.LogInfo(tag, $"Trying to open HID device...\nDevice: {d}");
                if (d.TryOpen(hidConfig, out hidStream))
                {
                    hidDevice = d;
                    break;
                }
                else
                {
                    MajDebug.LogError(tag, $"cannot open HID devices:\nDevice: {d}");
                }
            }
            if (hidStream is null || hidDevice is null)
            {
                MajDebug.LogError(tag, $"No HID devices available");
                hidDevice = default;
                hidStream = default;
                if (isReconnecting)
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        static void OnDeviceListChanged(object? sender,EventArgs e)
        {
            lock(_hidDevices)
            {
                _hidDevices = DeviceList.Local.GetHidDevices();
            }
        }
    }
}
#endif
