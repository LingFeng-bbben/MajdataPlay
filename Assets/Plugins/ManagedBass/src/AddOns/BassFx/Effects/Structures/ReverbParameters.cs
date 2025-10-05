using System.Runtime.InteropServices;

namespace ManagedBass.Fx
{
    /// <summary>
    /// Parameters for Reverb Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ReverbParameters : IEffectParameter
    {
        /// <summary>
        /// Dry (unaffected) signal mix (0.0...1.0, def. 0).
        /// </summary>
        public float fDryMix;

        /// <summary>
        /// Wet (affected) signal mix (0.0...3.0, def. 1.0).
        /// </summary>
        public float fWetMix = 1f;

        /// <summary>
        /// Room size (0.0...1.0, def. 0.5).
        /// </summary>
        public float fRoomSize = 0.5f;

        /// <summary>
        /// Damping factor (0.0...1.0, def. 0.5).
        /// </summary>
        public float fDamp = 0.5f;

        /// <summary>
        /// Stereo width (0.0...1.0, def. 1.0).
        /// </summary>
        /// <remarks>It should at least be 4 for moderate scaling ratios. A value of 32 is recommended for best quality (better quality = higher CPU usage).</remarks>
        public float fWidth = 1f;

        /// <summary>
        /// Mode: 0 = no freeze or 1 = freeze, def. 0 (no freeze).
        /// </summary>
        public int lMode;

        /// <summary>
        /// A <see cref="FXChannelFlags" /> flag to define on which channels to apply the effect. Default: <see cref="FXChannelFlags.All"/>
        /// </summary>
        public FXChannelFlags lChannel = FXChannelFlags.All;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.Freeverb;
    }
}