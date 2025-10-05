using System.Runtime.InteropServices;

namespace ManagedBass.Fx
{
    /// <summary>
    /// Parameters for AutoWah Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class AutoWahParameters : IEffectParameter
    {
        /// <summary>
        /// Dry (unaffected) signal mix (-2...+2). Default = 0.5.
        /// </summary>
        public float fDryMix = 0.5f;

        /// <summary>
        /// Wet (affected) signal mix (-2...+2). Default = 1.5.
        /// </summary>
        public float fWetMix = 1.5f;

        /// <summary>
        /// Feedback (-1...+1). Default = 0.5.
        /// </summary>
        public float fFeedback = 0.5f;

        /// <summary>
        /// Rate of sweep in cycles per second (0&lt;...&lt;10). Default = 2.
        /// </summary>
        public float fRate = 2;

        /// <summary>
        /// Sweep range in octaves (0&lt;...&lt;10). Default = 4.3.
        /// </summary>
        public float fRange = 4.3f;

        /// <summary>
        /// Base frequency of sweep range (0&lt;...1000). Default = 50.
        /// </summary>
        public float fFreq = 50;

        /// <summary>
        /// A <see cref="FXChannelFlags" /> flag to define on which channels to apply the effect. Default: <see cref="FXChannelFlags.All"/>
        /// </summary>
        public FXChannelFlags lChannel = FXChannelFlags.All;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.AutoWah;
    }
}