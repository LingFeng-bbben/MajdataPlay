using System.Runtime.InteropServices;

namespace ManagedBass.Fx
{
    /// <summary>
    /// Parameters for Rotate Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class RotateParameters : IEffectParameter
    {
        /// <summary>
        /// Rotation rate/speed in Hz (A negative rate can be used for reverse direction).
        /// </summary>
        public float fRate;

        /// <summary>
        /// A <see cref="FXChannelFlags" /> flag to define on which channels to apply the effect. Default: <see cref="FXChannelFlags.All"/>
        /// </summary>
        public FXChannelFlags lChannel = FXChannelFlags.All;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.Rotate;
    }
}