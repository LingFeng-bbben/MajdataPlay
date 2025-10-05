using System.Runtime.InteropServices;

namespace ManagedBass.DirectX8
{
    /// <summary>
    /// Parameters for DX8 Echo Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class DXEchoParameters : IEffectParameter
    {
        /// <summary>
        /// Ratio of wet (processed) signal to dry (unprocessed) signal. Must be in the range from 0 (default) through 100 (all wet).
        /// </summary>
        public float fWetDryMix;

        /// <summary>
        /// Percentage of output fed back into input, in the range from 0 through 100. The default value is 0.
        /// </summary>
        public float fFeedback;

        /// <summary>
        /// Delay for left channel, in milliseconds, in the range from 1 through 2000. The default value is 333 ms.
        /// </summary>
        public float fLeftDelay;

        /// <summary>
        /// Delay for right channel, in milliseconds, in the range from 1 through 2000. The default value is 333 ms.
        /// </summary>
        public float fRightDelay;

        /// <summary>
        /// Value that specifies whether to swap left and right delays with each successive echo. The default value is <see langword="false" />, meaning no swap.
        /// </summary>
        public bool lPanDelay;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.DXEcho;
    }
}