using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MajdataPlay.Platform.iOS.PInvoke;
using UnityEngine;

namespace MajdataPlay.Platform.iOS
{
    public static class IOSNativeSettings
    {
        public static bool Inited { get; internal set; }

        // Root
        public static string AppVersion { get; private set; }

        // Network
        public static bool Online { get; private set; }

        // Debug
        public static string DebugLogLevel { get; private set; }
        public static bool DebugNoCache { get; private set; }
        public static bool DebugNoteFolding { get; private set; }

        // Majnet
        public static bool MajnetEnabled { get; private set; }
        public static string MajnetApi { get; private set; }
        public static string MajnetUsername { get; private set; }
        public static string MajnetPassword { get; private set; }
        public static bool MajnetAutoLogin { get; private set; }

        // Custom
        public static bool CustomEnabled { get; private set; }

        public static string CustomName { get; private set; }
        public static string CustomApi { get; private set; }
        public static string CustomUsername { get; private set; }
        public static string CustomPassword { get; private set; }
        public static bool CustomAutoLogin { get; private set; }

        internal static void Init()
        {
            Debug.Log($"[{nameof(IOSNativeSettings)}] Init");
            // Root
            AppVersion = GetString("app_version", "0.1.50");

            // Network
            Online = GetBool("enabled_online", false);

            // Debug
            DebugLogLevel = GetString("debug_log_level", "Info");
            DebugNoCache = GetBool("debug_no_cache", false);
            DebugNoteFolding = GetBool("debug_note_folding", false);

            // Majnet
            MajnetEnabled = GetBool("majnet_enabled", false);
            MajnetApi = GetString("majnet_api", "https://majdata.net/api/api3/");
            MajnetUsername = GetString("majnet_username", "");
            MajnetPassword = GetString("majnet_password", "");
            MajnetAutoLogin = GetBool("majnet_auto_login", false);

            // Custom
            CustomEnabled = GetBool("custom_enabled", false);
            CustomName = GetString("custom_name", "");
            CustomApi = GetString("custom_api", "");
            CustomUsername = GetString("custom_username", "");
            CustomPassword = GetString("custom_password", "");
            CustomAutoLogin = GetBool("custom_auto_login", false);

            Inited = true;
            Debug.Log($"[{nameof(IOSNativeSettings)}] Init finished");
        }

        public static void Reload()
        {
            Inited = false;
            Init();
        }

        public static bool GetBool(string key, bool defaultValue)
        {
#if !UNITY_EDITOR
            return NativeSettingsPInvoke.GetBoolSetting(key, defaultValue);
#else
            return defaultValue;
#endif
        }

        public static int GetInt(string key, int defaultValue)
        {
#if !UNITY_EDITOR
            return NativeSettingsPInvoke.GetIntSetting(key, defaultValue);
#else
            return defaultValue;
#endif
        }

        public static string GetString(string key, string defaultValue)
        {
#if !UNITY_EDITOR
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = NativeSettingsPInvoke.GetStringSetting(key, defaultValue ?? "");
                if (ptr == IntPtr.Zero) return defaultValue ?? "";
                return Marshal.PtrToStringUTF8(ptr) ?? "";
            }
            finally
            {
                if (ptr != IntPtr.Zero) NativeSettingsPInvoke.FreeCString(ptr);
            }
#else
            return defaultValue;
#endif
        }
    }
}
