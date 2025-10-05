using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ManagedBass
{
    /// <summary>
    /// Contains Helper and Extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Clips a value between a Minimum and a Maximum.
        /// </summary>
        public static T Clip<T>(this T Item, T MinValue, T MaxValue)
            where T : IComparable<T>
        {
            if (Item.CompareTo(MaxValue) > 0)
                return MaxValue;

            return Item.CompareTo(MinValue) < 0 ? MinValue : Item;
        }
        
        /// <summary>
        /// Converts <see cref="Resolution"/> to <see cref="BassFlags"/>
        /// </summary>
        public static BassFlags ToBassFlag(this Resolution Resolution)
        {
            switch (Resolution)
            {
                case Resolution.Byte:
                    return BassFlags.Byte;
                case Resolution.Float:
                    return BassFlags.Float;
                default:
                    return BassFlags.Default;
            }
        }
        
        /// <summary>
        /// Returns the <param name="N">n'th (max 15)</param> pair of Speaker Assignment Flags
        /// </summary>
        public static BassFlags SpeakerN(int N) => (BassFlags)(N << 24);

        static bool? _floatable;

        /// <summary>
        /// Check whether Floating point streams are supported in the Current Environment.
        /// </summary>
        public static bool SupportsFloatingPoint
        {
            get
            {
                if (_floatable.HasValue) 
                    return _floatable.Value;

                // try creating a floating-point stream
                var hStream = Bass.CreateStream(44100, 1, BassFlags.Float, StreamProcedureType.Dummy);

                _floatable = hStream != 0;

                // floating-point channels are supported! (free the test stream)
                if (_floatable.Value) 
                    Bass.StreamFree(hStream);

                return _floatable.Value;
            }
        }

        /// <summary>
        /// Gets a <see cref="Version"/> object for a version number returned by BASS.
        /// </summary>
        public static Version GetVersion(int Num)
        {
            return new Version(Num >> 24 & 0xff,
                               Num >> 16 & 0xff,
                               Num >> 8 & 0xff,
                               Num & 0xff);
        }
        
        /// <summary>
        /// Returns a string representation for given number of channels.
        /// </summary>
        public static string ChannelCountToString(int Channels)
        {
            switch (Channels)
            {
                case 1:
                    return "Mono";
                case 2:
                    return "Stereo";
                case 3:
                    return "2.1";
                case 4:
                    return "Quad";
                case 5:
                    return "4.1";
                case 6:
                    return "5.1";
                case 7:
                    return "6.1";
                default:
                    if (Channels < 1)
                        throw new ArgumentException("Channels must be greater than Zero.");
                    return Channels + " Channels";
            }
        }

        /// <summary>
        /// Extract an array of strings from a pointer to ANSI null-terminated string ending with a double null.
        /// </summary>
        public static string[] ExtractMultiStringAnsi(IntPtr Ptr)
        {
            var l = new List<string>();

            while (true)
            {
                var str = Ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(Ptr);

                if (string.IsNullOrEmpty(str))
                    break;
                
                l.Add(str);

                // char '\0'
                Ptr += str.Length + 1;
            }

            return l.ToArray();
        }

        /// <summary>
        /// Extract an array of strings from a pointer to UTF-8 null-terminated string ending with a double null.
        /// </summary>
        public static string[] ExtractMultiStringUtf8(IntPtr Ptr)
        {
            var l = new List<string>();

            while (true)
            {
                var str = PtrToStringUtf8(Ptr, out int size);

                if (string.IsNullOrEmpty(str))
                    break;
 
                l.Add(str);

                Ptr += size + 1;
            }

            return l.ToArray();
        }

        static unsafe string PtrToStringUtf8(IntPtr Ptr, out int Size)
        {
            Size = 0;

            var bytes = (byte*)Ptr.ToPointer();

            if (Ptr == IntPtr.Zero || bytes[0] == 0)
                return null;
            
            while (bytes[Size] != 0)
                ++Size;
            
            var buffer = new byte[Size];
            Marshal.Copy(Ptr, buffer, 0, Size);
            
            return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Returns a Unicode string from a pointer to a Utf-8 string.
        /// </summary>
        public static string PtrToStringUtf8(IntPtr Ptr)
        {
            return PtrToStringUtf8(Ptr, out int size);
        }

        /// <summary>
        /// Returns a <see cref="StreamProcedure"/> which can be used to Play Silence on a Device (Useful during Wasapi Loopback Capture).
        /// </summary>
        public static StreamProcedure SilenceStreamProcedure { get; } = (Handle, Buffer, Length, User) =>
        {
            for (var i = 0; i < Length; ++i)
                Marshal.WriteByte(Buffer, i, 0);

            return Length;
        };

        /// <summary>
        /// Returns an instance of <see cref="FileProcedures"/> wrapped around a <see cref="Stream"/>.
        /// </summary>
        /// <param name="InputStream">The <see cref="Stream"/> to use with BASS.</param>
        public static FileProcedures StreamFileProcedures(Stream InputStream)
        {
            return new StreamFileProcedures(InputStream);
        }

        /// <summary>
        /// Applies the Effect on a <see cref="MediaPlayer"/>.
        /// </summary>
        /// <param name="Effect">The Effect to Apply.</param>
        /// <param name="Player">The <see cref="MediaPlayer"/> to apply the Effect on.</param>
        /// <param name="Priority">Priority of the Effect in DSP chain.</param>
        public static void ApplyOn<T>(this Effect<T> Effect, MediaPlayer Player, int Priority = 0)
            where T : class, IEffectParameter, new()
        {
            Effect.ApplyOn(Player.Handle, Priority);

            Player.MediaLoaded += NewHandle =>
            {
                Effect.Dispose();

                Effect.ApplyOn(NewHandle, Priority);
            };
        }
    }
}
