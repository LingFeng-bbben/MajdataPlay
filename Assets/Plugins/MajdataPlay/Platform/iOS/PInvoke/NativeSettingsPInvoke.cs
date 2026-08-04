using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MajdataPlay.Platform.iOS
{
    internal static class NativeSettingsPInvoke
    {
        [DllImport("__Internal", EntryPoint = "_GetBoolSetting")]
        public static extern bool GetBoolSetting(string key, bool defaultValue);
        [DllImport("__Internal", EntryPoint = "_GetIntSetting")]
        public static extern int GetIntSetting(string key, int defaultValue);
        [DllImport("__Internal", EntryPoint = "_GetStringSetting")]
        public static extern IntPtr GetStringSetting(string key, string defaultValue);
        [DllImport("__Internal", EntryPoint = "_FreeCString")]
        public static extern void FreeCString(IntPtr p);
    }
}
