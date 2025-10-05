using System.Runtime.InteropServices;

namespace ManagedBass.DirectX8
{
    /// <summary>
    /// Parameters for DX8 Gargle Effect (Windows only).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class DXGargleParameters : IEffectParameter
    {
        /// <summary>
        /// Rate of modulation, in Hertz. Must be in the range from 1 through 1000. Default 500 Hz.
        /// </summary>
        public int dwRateHz = 500;

        /// <summary>
        /// Shape of the modulation wave. Default = <see cref="DXWaveform.Sine"/>.
        /// </summary>
        public DXWaveform dwWaveShape = DXWaveform.Sine;

        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        public EffectType FXType => EffectType.DXGargle;
    }
}