using System.Runtime.InteropServices;

namespace ManagedBass.Fx
{
    /// <summary>
    /// Parameters for Phaser Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class PhaserParameters : IEffectParameter
    {
        /// <summary>
        /// Dry (unaffected) signal mix (-2...+2). Default = 0.
        /// </summary>
        public float fDryMix = 0.999f;

        /// <summary>
        /// Wet (affected) signal mix (-2...+2). Default = 0.
        /// </summary>
        public float fWetMix = 0.999f;

        /// <summary>
        /// Feedback (-1...+1). Default = 0.
        /// </summary>
        public float fFeedback;

        /// <summary>
        /// Rate of sweep in cycles per second (0&lt;...&lt;10). Default = 0.
        /// </summary>
        public float fRate = 1;

        /// <summary>
        /// Sweep range inoctaves (0&lt;...&lt;10). Default = 0.
        /// </summary>
        public float fRange = 4.3f;

        /// <summary>
        /// Base frequency of sweep range (0&lt;...1000). Default = 0.
        /// </summary>
        public float fFreq = 50;

        /// <summary>
        /// A <see cref="FXChannelFlags" /> flag to define on which channels to apply the effect. Default: <see cref="FXChannelFlags.All"/>
        /// </summary>
        public FXChannelFlags lChannel = FXChannelFlags.All;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.Phaser;
    }
}