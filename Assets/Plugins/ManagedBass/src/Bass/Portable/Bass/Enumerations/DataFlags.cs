using System;

namespace ManagedBass
{
    /// <summary>
    /// Flags to be used with <see cref="Bass.ChannelGetData(int,IntPtr,int)" />.
    /// </summary>
    [Flags]
    public enum DataFlags
    {
        /// <summary>
        /// Query how much data is buffered
        /// </summary>
        Available,

        /// <summary>
        /// FFT flag: FFT for each channel, else all combined
        /// </summary>
        FFTIndividual = 0x10,

        /// <summary>
        /// FFT flag: no Hanning window
        /// </summary>
        FFTNoWindow = 0x20,

        /// <summary>
        /// FFT flag: pre-remove DC bias
        /// </summary>
        FFTRemoveDC = 0x40,

        /// <summary>
        /// FFT flag: return complex data
        /// </summary>
        FFTComplex = 0x80,
        
        /// <summary>
        /// flag: return 8.24 fixed-point data (Android)
        /// </summary>
        Fixed = 0x20000000,

        /// <summary>
        /// flag: return floating-point sample data
        /// </summary>
        Float = 0x40000000,

        /// <summary>
        /// 256 sample FFT (returns 128 floating-point values)
        /// </summary>
        FFT256 = unchecked((int)0x80000000),

        /// <summary>
        /// 512 sample FFT (returns 256 floating-point values)
        /// </summary>
        FFT512 = unchecked((int)0x80000001),

        /// <summary>
        /// 1024 sample FFT (returns 512 floating-point values)
        /// </summary>
        FFT1024 = unchecked((int)0x80000002),

        /// <summary>
        /// 2048 sample FFT (returns 1024 floating-point values)
        /// </summary>
        FFT2048 = unchecked((int)0x80000003),

        /// <summary>
        /// 4096 sample FFT (returns 2048 floating-point values)
        /// </summary>
        FFT4096 = unchecked((int)0x80000004),

        /// <summary>
        /// 8192 sample FFT (returns 4096 floating-point values)
        /// </summary>
        FFT8192 = unchecked((int)0x80000005),

        /// <summary>
        /// 16384 sample FFT (returns 8192 floating-point values)
        /// </summary>
        FFT16384 = unchecked((int)0x80000006),

        /// <summary>
        /// 32768 FFT
        /// </summary>
        FFT32768 = unchecked((int)0x80000007)
    }
}
