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
    public readonly struct CULong : IEquatable<CULong>
    {
        public readonly NativeULong Value;

        public CULong(NativeULong value)
        {
            Value = value;
        }

        public static implicit operator CULong(byte value) => new CULong((NativeULong)value);
        public static implicit operator CULong(UInt16 value) => new CULong((NativeULong)value);
        public static implicit operator CULong(UInt32 value) => new CULong((NativeULong)value);
        public static implicit operator CULong(UInt64 value) => new CULong((NativeULong)value);

        public static implicit operator byte(CULong CULong) => (byte)CULong.Value;
        public static implicit operator UInt16(CULong CULong) => (UInt16)CULong.Value;
        public static implicit operator UInt32(CULong CULong) => (UInt32)CULong.Value;
        public static implicit operator UInt64(CULong CULong) => (UInt64)CULong.Value;

        public static explicit operator CLong(CULong culong) => (NativeLong)culong.Value;

        public bool Equals(CULong other) => Value == other.Value;
        public override bool Equals(object obj)
        {
            if (obj is CULong other)
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

        public static bool operator ==(CULong a, CULong b) => a.Value == b.Value;
        public static bool operator !=(CULong a, CULong b) => a.Value != b.Value;
    }
}
