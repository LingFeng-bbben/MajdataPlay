using System.Runtime.InteropServices;

namespace ManagedBass.DirectX8
{
    /// <summary>
    /// Parameters for DX8 Distortion Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class DXDistortionParameters : IEffectParameter
    {
        /// <summary>
        /// Amount of signal change after distortion, in the range from -60 through 0. The default value is 0 dB.
        /// </summary>
        public float fGain;

        /// <summary>
        /// Percentage of distortion intensity, in the range in the range from 0 through 100. The default value is 50 percent.
        /// </summary>
        public float fEdge;

        /// <summary>
        /// Center frequency of harmonic content addition, in the range from 100 through 8000. The default value is 4000 Hz.
        /// </summary>
        public float fPostEQCenterFrequency;

        /// <summary>
        /// Width of frequency band that determines range of harmonic content addition, in the range from 100 through 8000. The default value is 4000 Hz.
        /// </summary>
        public float fPostEQBandwidth;

        /// <summary>
        /// Filter cutoff for high-frequency harmonics attenuation, in the range from 100 through 8000. The default value is 4000 Hz.
        /// </summary>
        public float fPreLowpassCutoff;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.DXDistortion;
    }
}