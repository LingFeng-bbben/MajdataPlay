using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace MajdataPlay.Platform.Android.IO
{
    public static class AndroidKeyboard
    {
        public static ReadOnlySpan<bool> KeyStates
        {
            get
            {
                return _keyStates;
            }
        }
        static bool _isInited = false;
        static OnDispatchKeyEventCallbackProxy _onDispatchKeyEventCallbackProxy;
        readonly static bool[] _keyStates = new bool[1024];

        internal static void Init()
        {
            if(_isInited)
            {
                return;
            }
            _onDispatchKeyEventCallbackProxy = new OnDispatchKeyEventCallbackProxy();
            AndroidRuntime.MajdataPlayActivityClass.CallStatic("registerDispatchKeyEventCallback", _onDispatchKeyEventCallbackProxy);
            _isInited = true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPressed(KeyCode keyCode)
        {
            return _keyStates[(int)keyCode];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool IsPreesedUnsafe(KeyCode keyCode)
        {
            fixed(bool* ptr = _keyStates)
            {
                return ptr[(int)keyCode];
            }
        }
        unsafe class OnDispatchKeyEventCallbackProxy : AndroidJavaProxy
        {
            public OnDispatchKeyEventCallbackProxy() : base("net.majdata.majdataplay.CSharpOnDispatchKeyEventCallback") { }

            public void OnDispatchKeyEvent(int action, int keyCode)
            {
                fixed(bool* ptr = AndroidKeyboard._keyStates)
                {
                    ptr[keyCode] = action != 1;
                }
            }
        }
    }
}
