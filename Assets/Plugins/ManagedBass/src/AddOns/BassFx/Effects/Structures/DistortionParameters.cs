using System.Runtime.InteropServices;

namespace ManagedBass.Fx
{
    /// <summary>
    /// Parameters for Distortion Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class DistortionParameters : IEffectParameter
    {
        /// <summary>
        /// Distortion Drive (0...5).
        /// </summary>
        public float fDrive;

        /// <summary>
        /// Dry (unaffected) signal mix (-5...+5).
        /// </summary>
        public float fDryMix = 5f;

        /// <summary>
        /// Wet (affected) signal mix (-5...+5).
        /// </summary>
        public float fWetMix = 0.1f;

        /// <summary>
        /// Feedback (-1...+1).
        /// </summary>
        public float fFeedback;

        /// <summary>
        /// Distortion volume (0...+2).
        /// </summary>
        public float fVolume = 0.3f;

        /// <summary>
        /// A <see cref="FXChannelFlags" /> flag to define on which channels to apply the effect. Default: <see cref="FXChannelFlags.All"/>
        /// </summary>
        public FXChannelFlags lChannel = FXChannelFlags.All;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.Distortion;
    }
}