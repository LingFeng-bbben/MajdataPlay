using System;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.GUI.Controls
{
    /// <summary>
    /// Provides methods for dealing with common enumerator operations.
    /// </summary>
    internal static class FlagEnumUtility
    {
        /// <summary>
        /// Checks for the presence of a flag in a flag enum value.
        /// </summary>
        /// <param name="value">The value to check for the presence of the flag.</param>
        /// <param name="flag">The flag whose presence is to be checked.</param>
        /// <typeparam name="T">The flag enum type.</typeparam>
        /// <returns></returns>
        internal static bool HasFlag<T>(T value, T flag) where T : Enum
        {
            ValidateUnderlyingType<T>();

            var intValue = (int)(object)value;
            var intFlag = (int)(object)flag;
            return (intValue & intFlag) == intFlag;
        }

        /// <summary>
        /// Sets a flag in a flag enum value.
        /// </summary>
        /// <param name="value">The value where the flag should be set.</param>
        /// <param name="flag">The flag to be set.</param>
        /// <typeparam name="T">The flag enum type.</typeparam>
        /// <returns>The input value with the flag set.</returns>
        internal static T SetFlag<T>(T value, T flag) where T : Enum
        {
            ValidateUnderlyingType<T>();

            var intValue = (int)(object)value;
            var intFlag = (int)(object)flag;
            var result = intValue | intFlag;
            return (T)Enum.ToObject(typeof(T), result);
        }

        /// <summary>
        /// Removes a flag in a flag enum value.
        /// </summary>
        /// <param name="value">The value where the flag should be removed.</param>
        /// <param name="flag">The flag to be removed.</param>
        /// <typeparam name="T">The flag enum type.</typeparam>
        /// <returns>The input value with the flag removed.</returns>
        internal static T RemoveFlag<T>(T value, T flag) where T : Enum
        {
            ValidateUnderlyingType<T>();

            var intValue = (int)(object)value;
            var intFlag = (int)(object)flag;
            var result = intValue & ~intFlag;
            return (T)Enum.ToObject(typeof(T), result);
        }

        /// <summary>
        /// Validates that the underlying type of an enum is integer.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <exception cref="ArgumentException">Thrown if the underlying type of the enum type parameter is not integer.</exception>
        private static void ValidateUnderlyingType<T>() where T : Enum
        {
            if (Enum.GetUnderlyingType(typeof(T)) != typeof(int))
            {
                throw new ArgumentException("Argument underlying type must be integer.");
            }
        }
    }
}
