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

#nullable enable
namespace MajdataPlay.IO
{
    internal static class HidManager
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
        static HidManager()
        {
            var manufacturer = MajEnv.UserSettings.IO.Manufacturer;
            var buttonRingOptions = MajEnv.UserSettings.IO.InputDevice.ButtonRing;

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
        static void OnDeviceListChanged(object? sender,EventArgs e)
        {
            lock(_hidDevices)
            {
                _hidDevices = DeviceList.Local.GetHidDevices();
            }
        }
    }
}
