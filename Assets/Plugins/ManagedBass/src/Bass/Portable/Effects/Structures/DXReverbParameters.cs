using System.Runtime.InteropServices;

namespace ManagedBass.DirectX8
{
    /// <summary>
    /// Parameters for DX8 Reverb Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class DXReverbParameters : IEffectParameter
    {
        /// <summary>
        /// Input gain of signal, in decibels (dB), in the range from -96 through 0. The default value is 0 dB.
        /// </summary>
        public float fInGain;

        /// <summary>
        /// Reverb mix, in dB, in the range from -96 through 0. The default value is 0 dB.
        /// </summary>
        public float fReverbMix;

        /// <summary>
        /// Reverb time, in milliseconds, in the range from 0.001 through 3000. The default value is 1000.
        /// </summary>
        public float fReverbTime;

        /// <summary>
        /// In the range from 0.001 through 0.999. The default value is 0.001.
        /// </summary>
        public float fHighFreqRTRatio;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.DXReverb;
    }
}