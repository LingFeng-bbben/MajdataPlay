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
            var ledOptions = MajEnv.UserSettings.Misc.OutputDevice.Led;
            var buttonRingOptions = MajEnv.UserSettings.Misc.InputDevice.ButtonRing;
            var touchPanelOptions = MajEnv.UserSettings.Misc.InputDevice.TouchPanel;
            var includeHidDevice = ledOptions.Type == DeviceType.HID ||
                                   (buttonRingOptions.Type == DeviceType.HID || buttonRingOptions.Type == DeviceType.IO4) ||
                                   touchPanelOptions.Type == DeviceType.HID;
            if (!includeHidDevice)
                return;
            _hidDevices = DeviceList.Local.GetHidDevices();
            MajDebug.Log($"All available HID devices:\n{string.Join('\n', _hidDevices)}");
            DeviceList.Local.Changed += OnDeviceListChanged;
        }
        public static bool TryGetDevice(DeviceFilter filter, [NotNullWhen(true)] out HidDevice? device)
        {
            lock (_hidDevices)
            {
                try
                {
                    var index = filter.Index;
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
                        device = _cacheList[index.Clamp(0, _cacheList.Count - 1)];
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
                device = null;
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
