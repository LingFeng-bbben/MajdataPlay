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
        [DllImport("__Internal")] private static extern void _FreeCString(IntPtr p);
#endif

        public static class Cache
        {
            public static bool Inited { get; internal set; }

            // Root
            public static string AppVersion;
            public static bool JsonIgnore;

            // Network
            public static bool Online;

            // Debug
            public static string DebugLogLevel;
            public static bool DebugNoCache;
            public static bool DebugNoteFolding;

            // Majnet
            public static bool MajnetEnabled;
            public static string MajnetApi;
            public static string MajnetUsername;
            public static string MajnetPassword;

            // Custom
            public static bool CustomEnabled;

            public static string CustomName;
            public static string CustomApi;
            public static string CustomUsername;
            public static string CustomPassword;
        }

        public static void Init()
        {
            if (Cache.Inited) return;

            // Root
            Cache.AppVersion = GetString("app_version", "0.1.48");
            Cache.JsonIgnore = GetBool("json_ignore", false);

            // Network
            Cache.Online = GetBool("enabled_online", false);

            // Debug
            Cache.DebugLogLevel = GetString("debug_log_level", "Info");
            Cache.DebugNoCache = GetBool("debug_no_cache", false);
            Cache.DebugNoteFolding = GetBool("debug_note_folding", false);

            // Majnet
            Cache.MajnetEnabled = GetBool("majnet_enabled", false);
            Cache.MajnetApi = GetString("majnet_api", "https://majdata.net/api/api3/");
            Cache.MajnetUsername = GetString("majnet_username", "");
            Cache.MajnetPassword = GetString("majnet_password", "");

            // Custom
            Cache.CustomEnabled = GetBool("custom_enabled", false);
            Cache.CustomName = GetString("custom_name", "");
            Cache.CustomApi = GetString("custom_api", "");
            Cache.CustomUsername = GetString("custom_username", "");
            Cache.CustomPassword = GetString("custom_password", "");

            Cache.Inited = true;
        }

        public static void Reload()
        {
            Cache.Inited = false;
            Init();
        }

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
            return _GetIntSetting(key, intDefaultValue: defaultValue);
#else
            return defaultValue;
#endif
        }

        public static string GetString(string key, string defaultValue)
        {
#if UNITY_IOS && !UNITY_EDITOR
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = _GetStringSetting(key, defaultValue ?? "");
                if (ptr == IntPtr.Zero) return defaultValue ?? "";
                return Marshal.PtrToStringUTF8(ptr) ?? "";
            }
            finally
            {
                if (ptr != IntPtr.Zero) _FreeCString(ptr);
            }
#else
            return defaultValue;
#endif
        }
    }
}