using System.Runtime.InteropServices;

namespace ManagedBass.Fx
{
    /// <summary>
    /// Parameters for Chorus Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class ChorusParameters : IEffectParameter
    {
        /// <summary>
        /// Dry (unaffected) signal mix (-2...+2). Default = 0.9
        /// </summary>
        public float fDryMix = 0.9f;

        /// <summary>
        /// Wet (affected) signal mix (-2...+2). Default = 0.35.
        /// </summary>
        public float fWetMix = 0.35f;

        /// <summary>
        /// Feedback (-1...+1). Default = 0.5.
        /// </summary>
        public float fFeedback = 0.5f;

        /// <summary>
        /// Minimum delay in ms (0&lt;...6000). Default = 1.
        /// </summary>
        public float fMinSweep = 1;

        /// <summary>
        /// Maximum delay in ms (0&lt;...6000). Default = 400.
        /// </summary>
        public float fMaxSweep = 400;

        /// <summary>
        /// Rate in ms/s (0&lt;...1000). Default = 200.
        /// </summary>
        public float fRate = 200;

        /// <summary>
        /// A <see cref="FXChannelFlags" /> flag to define on which channels to apply the effect. Default: <see cref="FXChannelFlags.All"/>
        /// </summary>
        public FXChannelFlags lChannel = FXChannelFlags.All;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.Chorus;
    }
}