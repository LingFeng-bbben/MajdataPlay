using System.Runtime.InteropServices;

namespace ManagedBass.DirectX8
{
    /// <summary>
    /// Parameters for DX8 Chorus Effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class DXChorusParameters : IEffectParameter
    {
        /// <summary>
        /// Ratio of wet (processed) signal to dry (unprocessed) signal. Must be in the range from 0 (default) through 100 (all wet).
        /// </summary>
        public float fWetDryMix = 50f;

        /// <summary>
        /// Percentage by which the delay time is modulated by the low-frequency oscillator, in hundredths of a percentage point. Must be in the range from 0 through 100. The default value is 25.
        /// </summary>
        public float fDepth = 25f;

        /// <summary>
        /// Percentage of output signal to feed back into the effect's input, in the range from -99 to 99. The default value is 0.
        /// </summary>
        public float fFeedback;

        /// <summary>
        /// Frequency of the LFO, in the range from 0 to 10. The default value is 0.
        /// </summary>
        public float fFrequency;

        /// <summary>
        /// Waveform of the LFO. Default = <see cref="DXWaveform.Sine"/>.
        /// </summary>
        public DXWaveform lWaveform = DXWaveform.Sine;

        /// <summary>
        /// Number of milliseconds the input is delayed before it is played back, in the range from 0 to 20. The default value is 0 ms.
        /// </summary>
        public float fDelay;

        /// <summary>
        /// Phase differential between left and right LFOs. Default is <see cref="DXPhase.Zero"/>.
        /// </summary>
        public DXPhase lPhase = DXPhase.Zero;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.DXChorus;
    }
}