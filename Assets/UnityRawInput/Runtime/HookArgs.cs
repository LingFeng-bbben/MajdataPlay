using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityRawInput
{
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardArgs
    {
        public uint Code;
        public uint ScanCode;
        public KeyboardFlags Flags;
        public uint Time;
        public UIntPtr ExtraInfo;

        public uint TrueScanCode => Flags.HasFlag(KeyboardFlags.Extended) ? ScanCode + 0x100 : ScanCode;
        public uint Advanced => TrueScanCode << 8;
        public uint Hybrid => Code + Advanced;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator KeyboardArgs (IntPtr ptr)
            => Marshal.PtrToStructure<KeyboardArgs>(ptr);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator RawKey (KeyboardArgs args)
        {
            if (Enum.IsDefined(typeof(RawKey), args.Code))
                return (RawKey)args.Code;
            if (Enum.IsDefined(typeof(RawKey), args.Advanced))
                return (RawKey)args.Advanced;
            if (Enum.IsDefined(typeof(RawKey), args.Hybrid))
                return (RawKey)args.Hybrid;
            return (RawKey)args.Code;
        }
    }

    [Flags]
    public enum KeyboardFlags : uint
    {
        Extended = 0x01,
        LowerInjected = 0x02,
        Injected = 0x10,
        AltDown = 0x20,
        Up = 0x80
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseArgs
    {
        public Vector2Int Point;
        public uint MouseData;
        public MouseFlags Flags;
        public uint Time;
        public UIntPtr ExtraInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator MouseArgs (IntPtr ptr)
            => Marshal.PtrToStructure<MouseArgs>(ptr);
    }

    [Flags]
    public enum MouseFlags : uint
    {
        Injected = 0x01,
        LowerInjected = 0x02
    }
}
