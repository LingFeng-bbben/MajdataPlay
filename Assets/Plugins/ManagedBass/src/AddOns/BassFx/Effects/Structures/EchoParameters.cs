using System.Runtime.InteropServices;

namespace ManagedBass.Fx
{
    /// <summary>
    /// Parameters for Echo Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class EchoParameters : IEffectParameter
    {
        /// <summary>
        /// Dry (unaffected) signal mix (-2...+2). 
        /// </summary>
        public float fDryMix;

        /// <summary>
        /// Wet (affected) signal mix (-2...+2). 
        /// </summary>
        public float fWetMix;

        /// <summary>
        /// Feedback (-1...+1).
        /// </summary>
        public float fFeedback;

        /// <summary>
        /// Delay in seconds (0+...6).
        /// </summary>
        public float fDelay;

        /// <summary>
        /// Echo adjoining channels to each other? Default is disabled.
        /// </summary>
        /// <remarks>Only allowed with even number of channels!
        /// <para>If enabled and a stream has an even number of channels then, each even channels will be echoed to each other.</para>
        /// </remarks>
        public int bStereo;

        /// <summary>
        /// A <see cref="FXChannelFlags" /> flag to define on which channels to apply the effect. Default: <see cref="FXChannelFlags.All"/>
        /// </summary>
        public FXChannelFlags lChannel = FXChannelFlags.All;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.Echo;
    }
}