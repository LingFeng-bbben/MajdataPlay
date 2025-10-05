using System.Runtime.InteropServices;

namespace ManagedBass.Fx
{
    /// <summary>
    /// Parameters for Compressor Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CompressorParameters : IEffectParameter
    {
        /// <summary>
        /// Output gain in dB of signal after compression, in the range from -60 to 60. Default = 5.
        /// </summary>
        public float fGain = 5f;

        /// <summary>
        /// Point in dB at which compression begins, in decibels, in the range from -60 to 0. Default = -15.
        /// </summary>
        public float fThreshold = -15f;

        /// <summary>
        /// Compression ratio, in the range from 1 to 100. Default = 3.
        /// </summary>
        public float fRatio = 3f;

        /// <summary>
        /// Time in ms before compression reaches its full value, in the range from 0.01 to 500. Default = 20.
        /// </summary>
        public float fAttack = 20f;

        /// <summary>
        /// Time (speed) in ms at which compression is stopped after Input drops below <see cref="fThreshold"/>, in the range from 50 to 3000. Default = 200.
        /// </summary>
        public float fRelease = 200f;

        /// <summary>
        /// A <see cref="FXChannelFlags" /> flag to define on which channels to apply the effect. Default: <see cref="FXChannelFlags.All"/>
        /// </summary>
        public FXChannelFlags lChannel = FXChannelFlags.All;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.Compressor;
    }
}