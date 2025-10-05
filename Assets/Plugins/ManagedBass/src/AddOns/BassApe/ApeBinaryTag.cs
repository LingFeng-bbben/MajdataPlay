using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ManagedBass.Ape
{
    /// <summary>
    /// APE binary tag structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ApeBinaryTag
    {
        IntPtr key, data;

        int length;

        /// <summary>
        /// The binary tag data.
        /// </summary>
        public byte[] Data
        {
            get
            {
                if (data == IntPtr.Zero || length == 0)
                    return null;

                var arr = new byte[length];
                Marshal.Copy(data, arr, 0, length);
                return arr;
            }
        }

        /// <summary>
        /// The name of the tag.
        /// </summary>
        public string Key => key == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(key);

        /// <summary>
        /// The size of data in bytes.
        /// </summary>
        public int Length => length;

        /// <summary>
        /// Read a Tag.
        /// </summary>
        /// <param name="Channel">The Chanel to read the tag from.</param>
        /// <param name="Index">Index of tag.</param>
        public static ApeBinaryTag Read(int Channel, int Index)
        {
            return BassMarshal.PtrToStructure<ApeBinaryTag>(Bass.ChannelGetTags(Channel, TagType.ApeBinary + Index));
        }

        /// <summary>
        /// Read all APE binary tags
        /// </summary>
        /// <param name="Channel">The Chanel to read the tag from.</param>
        public static IEnumerable<ApeBinaryTag> ReadAll(int Channel)
        {
            ApeBinaryTag tag;

            for (var i = 0; (tag = Read(Channel, i)) != null; ++i)
                yield return tag;
        }

        /// <summary>
        /// Returns the Key of the binary tag.
        /// </summary>
        public override string ToString() => Key;
    }
}