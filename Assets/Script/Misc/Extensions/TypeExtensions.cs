using System;
#nullable enable
namespace MajdataPlay.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsIntType(this Type source)
        {
            return source == typeof(int) || source == typeof(long) ||
                   source == typeof(short) || source == typeof(byte) ||
                   source == typeof(uint) || source == typeof(ulong) ||
                   source == typeof(ushort) || source == typeof(sbyte);
        }
        public static bool IsFloatType(this Type source)
        {
            return source == typeof(float) || source == typeof(double) ||
                   source == typeof(decimal);
        }
    }
}