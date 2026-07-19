using MajdataPlay.Platform.Android.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Platform.Android
{
    public delegate void OnActivityResultCallback(object? sender, int requestCode, int resultCode, AndroidJavaObject? intent);
    public static class AndroidRuntime
    {
        public static event EventHandler<AndroidJavaObject?>? OnNewIntent;
        public static event OnActivityResultCallback? OnActivityResult;

        public static AndroidJavaClass MajdataPlayActivityClass { get; private set; } = null!;
        public static AndroidJavaObject CurrentActivity { get; private set; } = null!;

        

        readonly static OnNewIntentCallbackProxy _onNewIntentCallbackProxy = new();
        readonly static OnActivityResultCallbackProxy _onActivityResultCallbackProxy = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
#if UNITY_EDITOR
            return;
#endif
            MajdataPlayActivityClass = new AndroidJavaClass("net.majdata.majdataplay.MajdataPlayActivity");
            UnityEngine.Debug.Log("[Android]Get current activity");
            CurrentActivity = MajdataPlayActivityClass.GetStatic<AndroidJavaObject>("currentActivity");
            UnityEngine.Debug.Log("[Android]Setting onNewIntent callback proxy");
            MajdataPlayActivityClass.CallStatic("registerOnNewIntentCallback", _onNewIntentCallbackProxy);
            UnityEngine.Debug.Log("[Android]Setting onActivityResult callback proxy");
            MajdataPlayActivityClass.CallStatic("registerOnActivityResultCallback", _onActivityResultCallbackProxy);

            AndroidKeyboard.Init();
        }
        static void Android_OnNewIntent(AndroidJavaObject intent)
        {
            if (OnNewIntent is not null)
            {
                OnNewIntent(null, intent);
            }
        }
        static void Android_OnActivityResult(int requestCode, int resultCode, AndroidJavaObject? intent)
        {
            if (OnActivityResult is not null)
            {
                OnActivityResult(null, requestCode, resultCode, intent);
            }
        }
        class OnNewIntentCallbackProxy : AndroidJavaProxy
        {
            public OnNewIntentCallbackProxy() : base("net.majdata.majdataplay.CSharpOnNewIntentCallback") { }

            public void OnNewIntent(AndroidJavaObject intent)
            {
                Android_OnNewIntent(intent);
            }
        }
        class OnActivityResultCallbackProxy : AndroidJavaProxy
        {
            public OnActivityResultCallbackProxy() : base("net.majdata.majdataplay.CSharpOnActivityResultCallback") { }

            public void OnActivityResult(int requestCode, int resultCode, AndroidJavaObject? intent)
            {
                Android_OnActivityResult(requestCode, resultCode, intent);
            }
        }
    }
}
