using System.Runtime.InteropServices;

namespace ManagedBass.Fx
{
    /// <summary>
    /// Parameters for BQF Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class BQFParameters : IEffectParameter
    {
        /// <summary>
        /// BQF Filter Type.
        /// </summary>
        public BQFType lFilter = BQFType.AllPass;

        /// <summary>
        /// Cut-off frequency (Center in PEAKINGEQ and Shelving filters) in Hz (1...info.freq/2). Default = 200Hz.
        /// </summary>
        public float fCenter = 200f;

        /// <summary>
        /// Gain in dB (-15...0...+15). Default 0dB (used only for PEAKINGEQ and Shelving filters).
        /// </summary>
        public float fGain;

        /// <summary>
        /// Bandwidth in octaves (0.1...4...n), Q is not in use (<see cref="fBandwidth"/> has priority over <see cref="fQ"/>). Default = 1 (0=not in use).
        /// The bandwidth in octaves (between -3 dB frequencies for <see cref="BQFType.BandPass"/> and <see cref="BQFType.Notch"/> or between midpoint (dBgain/2) gain frequencies for PEAKINGEQ).
        /// </summary>
        public float fBandwidth = 1f;

        /// <summary>
        /// EE kinda definition of Q (0.1...1...n), if <see cref="fBandwidth"/> is not in use. Default = 0.0 (0=not in use).
        /// </summary>
        public float fQ;

        /// <summary>
        /// A shelf slope parameter (linear, used only with Shelving filters) (0...1...n). Default = 0.0.
        /// When 1, the shelf slope is as steep as you can get it and remain monotonically increasing or decreasing gain with frequency.
        /// </summary>
        public float fS;

        /// <summary>
        /// A <see cref="FXChannelFlags" /> flag to define on which channels to apply the effect. Default: <see cref="FXChannelFlags.All"/>
        /// </summary>
        public FXChannelFlags lChannel = FXChannelFlags.All;
        
        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.BQF;
    }
}