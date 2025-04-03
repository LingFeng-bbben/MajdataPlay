using MajdataPlay.Utils;
using MychIO.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.IO
{
    internal static class ButtonRing
    {
        readonly static bool[] _states = new bool[13];

        public static bool IsKeyDown(KeyCode keyCode)
        {
            return _states[(int)keyCode];
        }
        public static bool IsKeyUp(KeyCode keyCode)
        {
            return !IsKeyDown(keyCode);
        }
        internal static void OnButtonDown(ButtonRingZone zone)
        {
            _states[GetIndexByButtonRingZone(zone)] = true;
        }
        internal static void OnButtonUp(ButtonRingZone zone)
        {
            _states[GetIndexByButtonRingZone(zone)] = false;
        }
        internal static void OnButtonDown(KeyCode zone)
        {
            if (zone == KeyCode.Unset)
                return;
            _states[(int)zone] = true;
        }
        internal static void OnButtonUp(KeyCode zone)
        {
            if (zone == KeyCode.Unset)
                return;
            _states[(int)zone] = false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GetIndexByButtonRingZone(ButtonRingZone btnZone)
        {
            return btnZone switch
            {
                ButtonRingZone.BA1 => 0,
                ButtonRingZone.BA2 => 1,
                ButtonRingZone.BA3 => 2,
                ButtonRingZone.BA4 => 3,
                ButtonRingZone.BA5 => 4,
                ButtonRingZone.BA6 => 5,
                ButtonRingZone.BA7 => 6,
                ButtonRingZone.BA8 => 7,
                ButtonRingZone.ArrowUp => 9,
                ButtonRingZone.ArrowDown => 11,
                ButtonRingZone.Select => 8,
                ButtonRingZone.InsertCoin => 10,
                _ => throw new ArgumentOutOfRangeException("Does your 8-key game have 9 keys?")
            };
        }
    }
}
