using MajdataPlay.Utils;
using System;
using MajdataPlay.Types;
using UnityEngine;
using UnityEngine.XR;

namespace MajdataPlay.IO
{
    internal static class Keyboard
    {
        public static bool IsKeyDown(KeyCode keyCode)
        {
#if UNITY_STANDALONE_WIN
            var result = Win32API.GetAsyncKeyState((int)ToWinKeyCode(keyCode));
            return (result & 0x8000) != 0;
#else
            return Input.GetKey(ToUnityKeyCode(keyCode));
#endif
        }
        public static bool IsKeyUp(KeyCode keyCode)
        {
            return !IsKeyDown(keyCode);
        }
        static Win32RawKey ToWinKeyCode(KeyCode keyCode)
        {
            return keyCode switch
            {
                KeyCode.B1 => MajInstances.Setting.WindowsKeyCodes.B1,
                KeyCode.B2 => MajInstances.Setting.WindowsKeyCodes.B2,
                KeyCode.B3 => MajInstances.Setting.WindowsKeyCodes.B3,
                KeyCode.B4 => MajInstances.Setting.WindowsKeyCodes.B4,
                KeyCode.B5 => MajInstances.Setting.WindowsKeyCodes.B5,
                KeyCode.B6 => MajInstances.Setting.WindowsKeyCodes.B6,
                KeyCode.B7 => MajInstances.Setting.WindowsKeyCodes.B7,
                KeyCode.B8 => MajInstances.Setting.WindowsKeyCodes.B8,
                KeyCode.Test => MajInstances.Setting.WindowsKeyCodes.Test,
                KeyCode.Service => MajInstances.Setting.WindowsKeyCodes.Service,
                KeyCode.SelectP1 => MajInstances.Setting.WindowsKeyCodes.SelectP1,
                KeyCode.SelectP2 => MajInstances.Setting.WindowsKeyCodes.SelectP2,
                _ => throw new ArgumentOutOfRangeException(nameof(keyCode)),
            };
        }
        static UnityEngine.KeyCode ToUnityKeyCode(KeyCode keyCode)
        {
            return keyCode switch
            {
                KeyCode.B1 => UnityEngine.KeyCode.W,
                KeyCode.B2 => UnityEngine.KeyCode.E,
                KeyCode.B3 => UnityEngine.KeyCode.D,
                KeyCode.B4 => UnityEngine.KeyCode.C,
                KeyCode.B5 => UnityEngine.KeyCode.X,
                KeyCode.B6 => UnityEngine.KeyCode.Z,
                KeyCode.B7 => UnityEngine.KeyCode.A,
                KeyCode.B8 => UnityEngine.KeyCode.Q,
                KeyCode.Test => UnityEngine.KeyCode.Keypad9,
                KeyCode.SelectP1 => UnityEngine.KeyCode.KeypadMultiply,
                KeyCode.Service => UnityEngine.KeyCode.Keypad7,
                KeyCode.SelectP2 => UnityEngine.KeyCode.Keypad3,
                _ => throw new ArgumentOutOfRangeException(nameof(keyCode)),
            };
        }
    }
}
