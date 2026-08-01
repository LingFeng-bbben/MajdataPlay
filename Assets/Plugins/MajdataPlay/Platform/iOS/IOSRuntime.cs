using AOT;
using MajdataPlay.Diagnostics;
using MajdataPlay.Platform.iOS.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MajdataPlay.Platform.iOS.PInvoke.AppStatusPInvoke;
#nullable enable
namespace MajdataPlay.Platform.iOS
{
    public static class IOSRuntime
    {
        public static bool IsAppInForeground
        {
            get
            {
                return AppStatusPInvoke.IsAppInForeground();
            }
        }
        public static bool IsAppFocused
        {
            get
            {
                return AppStatusPInvoke.IsAppFocused();
            }
        }

        public static event EventHandler<bool>? OnAppFocusChanged;
        public static event EventHandler<bool>? OnAppForegroundChanged;

        readonly static GCHandle _onFocusChangedCallbackHandle;
        readonly static GCHandle _onForegroundChangedCallbackHandle;
        readonly static AppStatusChangeDelegate _onFocusChanged;
        readonly static AppStatusChangeDelegate _onForegroundChanged;

        static IOSRuntime()
        {
            _onFocusChanged = NativeFocusCallback;
            _onForegroundChanged = NativeForegroundCallback;
            _onFocusChangedCallbackHandle = GCHandle.Alloc(_onFocusChanged, GCHandleType.Pinned);
            _onForegroundChangedCallbackHandle = GCHandle.Alloc(_onForegroundChanged, GCHandleType.Pinned);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
#if UNITY_IOS || UNITY_EDITOR_OSX
            MajDebug.LogInfo($"[{nameof(IOSRuntime)}] Init");
            try
            {
                IOSNativeSettings.Init();
                MajDebug.LogInfo($"[{nameof(IOSRuntime)}] Register app status callbacks");
                AppStatusPInvoke.RegisterAppStatusCallbacks(NativeForegroundCallback, NativeFocusCallback);
                MajDebug.LogInfo($"[{nameof(IOSRuntime)}] Init finished");
            }
            catch (Exception ex)
            {
                MajDebug.LogError($"[{nameof(IOSRuntime)}] Init failed: {ex}");
            }
#endif
        }

        #region Callback
        [MonoPInvokeCallback(typeof(AppStatusChangeDelegate))]
        static void NativeForegroundCallback(bool isForeground)
        {
            if(OnAppForegroundChanged is not null)
            {
                OnAppForegroundChanged(null, isForeground);
            }
        }

        [MonoPInvokeCallback(typeof(AppStatusChangeDelegate))]
        static void NativeFocusCallback(bool isFocused)
        {
            if(OnAppFocusChanged is not null)
            {
                OnAppFocusChanged(null, isFocused);
            }
        }
        #endregion
    }
}
