using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MajdataPlay.Utils.Win32API;

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
            return false;
#endif
        }
        public static bool IsKeyUp(KeyCode keyCode)
        {
#if UNITY_STANDALONE_WIN
            return !IsKeyDown(keyCode);
#else
            return false;
#endif
        }
        static RawKey ToWinKeyCode(KeyCode keyCode)
        {
            return keyCode switch
            {
                KeyCode.B1 => RawKey.W,
                KeyCode.B2 => RawKey.E,
                KeyCode.B3 => RawKey.D,
                KeyCode.B4 => RawKey.C,
                KeyCode.B5 => RawKey.X,
                KeyCode.B6 => RawKey.Z,
                KeyCode.B7 => RawKey.A,
                KeyCode.B8 => RawKey.Q,
                KeyCode.Test => RawKey.Numpad9,
                KeyCode.SelectP1 => RawKey.Multiply,
                KeyCode.Service => RawKey.Numpad7,
                KeyCode.SelectP2 => RawKey.Numpad3,
                _ => throw new ArgumentOutOfRangeException(nameof(keyCode)),
            };
        }
    }
}
