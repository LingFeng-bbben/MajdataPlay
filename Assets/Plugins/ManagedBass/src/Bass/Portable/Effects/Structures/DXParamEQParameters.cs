using System.Runtime.InteropServices;

namespace ManagedBass.DirectX8
{
    /// <summary>
    /// Parameters for DX8 ParamEQ Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class DXParamEQParameters : IEffectParameter
    {
        /// <summary>
        /// Center frequency, in hertz, in the range from 80 to 16000. This value cannot exceed one-third of the frequency of the channel. Default 100 Hz.
        /// </summary>
        public float fCenter;

        /// <summary>
        /// Bandwidth, in semitones, in the range from 1 to 36. Default 18 semitones.
        /// </summary>
        public float fBandwidth;

        /// <summary>
        /// Gain, in the range from -15 to 15. Default 0 dB.
        /// </summary>
        public float fGain;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.DXParamEQ;
    }
}