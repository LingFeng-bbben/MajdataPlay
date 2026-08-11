using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA
using NativeLong = System.Int32;
using NativeULong = System.UInt32;
#else
using NativeLong = System.Int64;
using NativeULong = System.UInt64;
#endif

namespace MajdataPlay.UnsafeKit
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct CLong : IEquatable<CLong>
    {
        public readonly NativeLong Value;

        public CLong(NativeLong value)
        {
            Value = value;
        }

        public static implicit operator CLong(byte value) => new CLong((NativeLong)value);
        public static implicit operator CLong(sbyte value) => new CLong((NativeLong)value);
        public static implicit operator CLong(Int16 value) => new CLong((NativeLong)value);
        public static implicit operator CLong(Int32 value) => new CLong((NativeLong)value);
        public static implicit operator CLong(Int64 value) => new CLong((NativeLong)value);

        public static implicit operator byte(CLong clong) => (byte)clong.Value;
        public static implicit operator sbyte(CLong clong) => (sbyte)clong.Value;
        public static implicit operator Int16(CLong clong) => (Int16)clong.Value;
        public static implicit operator Int32(CLong clong) => (Int32)clong.Value;
        public static implicit operator Int64(CLong clong) => (Int64)clong.Value;

        public static explicit operator CULong(CLong clong) => (NativeULong)clong.Value;

        public bool Equals(CLong other) => Value == other.Value;
        public override bool Equals(object obj)
        {
            if (obj is CLong other)
            {
                return Value == other.Value;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(CLong a, CLong b) => a.Value == b.Value;
        public static bool operator !=(CLong a, CLong b) => a.Value != b.Value;
    }
}
