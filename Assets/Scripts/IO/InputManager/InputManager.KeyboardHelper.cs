using System;
#if UNITY_STANDALONE
using MajdataPlay.Platform.Win32;
#endif
using UnityEngine.InputSystem;
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
                var keyboard = Keyboard.current;
                if(keyboard is null)
                {
                    return false;
                }
                var unityKeyCode = ToUnityKeyCode(keyCode);
                return keyboard[unityKeyCode].isPressed;
#else
                return false;
#endif
            }
            public static bool IsKeyUp(KeyCode keyCode)
            {
                return !IsKeyDown(keyCode);
            }
#if UNITY_STANDALONE
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
#endif
            static UnityEngine.InputSystem.Key ToUnityKeyCode(KeyCode keyCode)
            {
                return keyCode switch
                {
                    KeyCode.B1 => UnityEngine.InputSystem.Key.W,
                    KeyCode.B2 => UnityEngine.InputSystem.Key.E,
                    KeyCode.B3 => UnityEngine.InputSystem.Key.D,
                    KeyCode.B4 => UnityEngine.InputSystem.Key.C,
                    KeyCode.B5 => UnityEngine.InputSystem.Key.X,
                    KeyCode.B6 => UnityEngine.InputSystem.Key.Z,
                    KeyCode.B7 => UnityEngine.InputSystem.Key.A,
                    KeyCode.B8 => UnityEngine.InputSystem.Key.Q,
                    KeyCode.Test => UnityEngine.InputSystem.Key.Numpad9,
                    KeyCode.SelectP1 => UnityEngine.InputSystem.Key.NumpadMultiply,
                    KeyCode.Service => UnityEngine.InputSystem.Key.Numpad7,
                    KeyCode.SelectP2 => UnityEngine.InputSystem.Key.Numpad3,
                    _ => throw new ArgumentOutOfRangeException(nameof(keyCode)),
                };
            }
        }
    }
}