using System;

namespace UnityEditor.TestTools.TestRunner.GUI.Controls
{
    /// <summary>
    /// Provides methods for dealing with common bit operations.
    /// </summary>
    internal static class BitUtility
    {
        /// <summary>
        /// Evaluates the cardinality of an integer, treating the value as a bit set.
        /// Optimization based on http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel.
        /// </summary>
        /// <param name="integer">The input integer value.</param>
        /// <returns>The number of bits set in the provided input integer value.</returns>
        internal static int GetCardinality(int integer)
        {
            unchecked
            {
                integer = integer - ((integer >> 1) & 0x55555555);
                integer = (integer & 0x33333333) + ((integer >> 2) & 0x33333333);
                integer = (((integer + (integer >> 4)) & 0xF0F0F0F) * 0x1010101) >> 24;
            }

            return integer;
        }
    }
}
