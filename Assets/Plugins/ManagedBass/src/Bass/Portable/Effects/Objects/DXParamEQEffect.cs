using System;

namespace ManagedBass.DirectX8
{
    /// <summary>
    /// DX8 ParamEQ Effect.
    /// </summary>
    [Obsolete("Use DXParamEQ instead.")]
    public sealed class DXParamEQEffect : Effect<DXParamEQParameters>
    {
        /// <summary>
        /// Center frequency, in hertz, in the range from 80 to 16000. This value cannot exceed one-third of the frequency of the channel. Default 100 Hz.
        /// </summary>
        public double Center
        {
            get => Parameters.fCenter;
            set
            {
                Parameters.fCenter = (float) value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Bandwidth, in semitones, in the range from 1 to 36. Default 18 semitones.
        /// </summary>
        public double Bandwidth
        {
            get => Parameters.fBandwidth;
            set
            {
                Parameters.fBandwidth = (float) value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gain, in the range from -15 to 15. Default 0 dB.
        /// </summary>
        public double Gain
        {
            get => Parameters.fGain;
            set
            {
                Parameters.fGain = (float) value;

                OnPropertyChanged();
            }
        }
    }
}