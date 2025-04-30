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
#nullable enable
namespace MajdataPlay.IO
{
    internal static class HidManager
    {
        static IEnumerable<HidDevice> _hidDevices = Array.Empty<HidDevice>();
        readonly static List<HidDevice> _cacheList = new(); 
        static HidManager()
        {
            var manufacturer = MajEnv.UserSettings.IO.Manufacturer;
            var buttonRingOptions = MajEnv.UserSettings.IO.InputDevice.ButtonRing;
            var includeHidDevice = buttonRingOptions.Type == ButtonRingDeviceType.HID || 
                                   manufacturer == DeviceManufacturer.Dao;
            if (!includeHidDevice)
                return;
            _hidDevices = DeviceList.Local.GetHidDevices();
            MajDebug.Log($"All available HID devices:\n{string.Join('\n', _hidDevices)}");
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
                        devices = _cacheList.ToArray();
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
        static void OnDeviceListChanged(object? sender,EventArgs e)
        {
            lock(_hidDevices)
            {
                _hidDevices = DeviceList.Local.GetHidDevices();
            }
        }
    }
}
