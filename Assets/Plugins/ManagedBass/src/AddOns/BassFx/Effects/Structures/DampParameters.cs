using System.Runtime.InteropServices;

namespace ManagedBass.Fx
{
    /// <summary>
    /// Parameters for Damp Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class DampParameters : IEffectParameter
    {
        /// <summary>
        /// Target volume level (0&lt;...1, linear).
        /// </summary>
        public float fTarget = 1;

        /// <summary>
        /// Quiet volume level (0...1, linear). 
        /// </summary>
        public float fQuiet;

        /// <summary>
        /// Amplification adjustment rate (0...1, linear).
        /// </summary>
        public float fRate;

        /// <summary>
        /// Amplification level (0...1...n, linear). 
        /// </summary>
        public float fGain;

        /// <summary>
        /// Delay in seconds before increasing level (0...n, linear).
        /// </summary>
        public float fDelay;

        /// <summary>
        /// A <see cref="FXChannelFlags" /> flag to define on which channels to apply the effect. Default: <see cref="FXChannelFlags.All"/>
        /// </summary>
        public FXChannelFlags lChannel = FXChannelFlags.All;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.Damp;
    }
}