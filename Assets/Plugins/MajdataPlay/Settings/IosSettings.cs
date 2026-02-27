using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MajdataPlay.Settings
{
    public static class IosSettings
    {
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern bool _GetBoolSetting(string key, bool defaultValue);
    [DllImport("__Internal")] private static extern int _GetIntSetting(string key, int defaultValue);
    [DllImport("__Internal")] private static extern IntPtr _GetStringSetting(string key, string defaultValue);
#endif

        public static bool GetBool(string key, bool defaultValue)
        {
#if UNITY_IOS && !UNITY_EDITOR
        return _GetBoolSetting(key, defaultValue);
#else
            return defaultValue;
#endif
        }

        public static int GetInt(string key, int defaultValue)
        {
#if UNITY_IOS && !UNITY_EDITOR
        return _GetIntSetting(key, defaultValue);
#else
            return defaultValue;
#endif
        }

        public static string GetString(string key, string defaultValue)
        {
#if UNITY_IOS && !UNITY_EDITOR
        var ptr = _GetStringSetting(key, defaultValue);
        return Marshal.PtrToStringUTF8(ptr);
#else
            return defaultValue;
#endif
        }
    }
}