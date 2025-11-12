using System;
using MajdataPlay.Utils;
//using Microsoft.Win32;
//using System.Windows.Forms;
//using Application = UnityEngine.Application;
//using System.Security.Policy;
#nullable enable
namespace MajdataPlay.IO
{
    internal static unsafe partial class InputManager
    {
        static class KeyboardHelper
        {
            public static bool IsKeyDown(KeyCode keyCode)
            {
#if UNITY_STANDALONE_WIN
                var result = Win32API.GetAsyncKeyState((int)ToWinKeyCode(keyCode));
                return (result & 0x8000) != 0;
#elif UNITY_STANDALONE
                return Input.GetKey(ToUnityKeyCode(keyCode));
#else
                return false;
#endif
            }
            public static bool IsKeyUp(KeyCode keyCode)
            {
                return !IsKeyDown(keyCode);
            }
            static Win32API.RawKey ToWinKeyCode(KeyCode keyCode)
            {
                return keyCode switch
                {
                    KeyCode.B1 => Win32API.RawKey.W,
                    KeyCode.B2 => Win32API.RawKey.E,
                    KeyCode.B3 => Win32API.RawKey.D,
                    KeyCode.B4 => Win32API.RawKey.C,
                    KeyCode.B5 => Win32API.RawKey.X,
                    KeyCode.B6 => Win32API.RawKey.Z,
                    KeyCode.B7 => Win32API.RawKey.A,
                    KeyCode.B8 => Win32API.RawKey.Q,
                    KeyCode.Test => Win32API.RawKey.Numpad9,
                    KeyCode.SelectP1 => Win32API.RawKey.Multiply,
                    KeyCode.Service => Win32API.RawKey.Numpad7,
                    KeyCode.SelectP2 => Win32API.RawKey.Numpad3,
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
}