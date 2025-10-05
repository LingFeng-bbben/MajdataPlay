using System.Runtime.InteropServices;

namespace ManagedBass.DirectX8
{
    /// <summary>
    /// Parameters for DX8 Compressor Effect (Windows only).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class DXCompressorParameters : IEffectParameter
    {
        /// <summary>
        /// Output gain of signal in dB after compression, in the range from -60 to 60. The default value is 0 dB.
        /// </summary>
        public float fGain;

        /// <summary>
        /// Time in ms before compression reaches its full value, in the range from 0.01 to 500. The default value is 10 ms.
        /// </summary>
        public float fAttack = 10;

        /// <summary>
        /// Time (speed) in ms at which compression is stopped after input drops below <see cref="fThreshold"/>, in the range from 50 to 3000. The default value is 200 ms.
        /// </summary>
        public float fRelease = 200;

        /// <summary>
        /// Point at which compression begins, in dB, in the range from -60 to 0. The default value is -20 dB.
        /// </summary>
        public float fThreshold = -20;

        /// <summary>
        /// Compression ratio, in the range from 1 to 100. The default value is 3, which means 3:1 compression.
        /// </summary>
        public float fRatio = 3;

        /// <summary>
        /// Time in ms after <see cref="fThreshold"/> is reached before attack phase is started, in milliseconds, in the range from 0 to 4. The default value is 4 ms.
        /// </summary>
        public float fPredelay = 4;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.DXCompressor;
    }
}