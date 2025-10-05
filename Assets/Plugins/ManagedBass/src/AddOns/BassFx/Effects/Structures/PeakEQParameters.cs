using System.Runtime.InteropServices;

namespace ManagedBass.Fx
{
    /// <summary>
    /// Parameters for PeakEQ Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class PeakEQParameters : IEffectParameter
    {
        /// <summary>
        /// Number of bands (0...n), more bands means more memory and cpu usage. Default = 0.
        /// </summary>
        public int lBand;

        /// <summary>
        /// Bandwidth in octaves (0.1...4...n), <see cref="fQ"/> is not in use (<see cref="fBandwidth"/> has priority over <see cref="fQ"/>). Default = 1 (0=not in use).
        /// <para>In most cases users should use the minimum of 0.5 octave.</para>
        /// <para>The bandwidth in octaves (between -3 dB frequencies for BPF and notch or between midpoint (dBgain/2) gain frequencies for peaking EQ).</para>
        /// </summary>
        public float fBandwidth = 1f;

        /// <summary>
        /// EE kinda definition of Q (0.1...1...n), if bandwidth is not in use. Default = 0.0 (0=not in use).
        /// </summary>
        public float fQ;

        /// <summary>
        /// Center frequency in Hz (1Hz...nHz). Default = 1000 (max. is 1/2 of the samplerate).
        /// </summary>
        /// <remarks>Use 'oldcenter * freq / oldfreq' to update the <see cref="fCenter"/> after changing the samplerate.</remarks>
        public float fCenter = 1000f;

        /// <summary>
        /// Gain in dB (-15...0...+15). Default 0dB.
        /// </summary>
        public float fGain;

        /// <summary>
        /// A <see cref="FXChannelFlags" /> flag to define on which channels to apply the effect. Default: <see cref="FXChannelFlags.All"/>
        /// </summary>
        public FXChannelFlags lChannel = FXChannelFlags.All;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.PeakEQ;
    }
}