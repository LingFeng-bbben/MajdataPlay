namespace ManagedBass.DirectX8
{
    /// <summary>
    /// DX8 Reverb Effect.
    /// </summary>
    public sealed class DXReverbEffect : Effect<DXReverbParameters>
    {
        /// <summary>
        /// Input gain of signal, in decibels (dB), in the range from -96 through 0. The default value is 0 dB.
        /// </summary>
        public double InGain
        {
            get => Parameters.fInGain;
            set
            {
                Parameters.fInGain = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Reverb mix, in dB, in the range from -96 through 0. The default value is 0 dB.
        /// </summary>
        public double ReverbMix
        {
            get => Parameters.fReverbMix;
            set
            {
                Parameters.fReverbMix = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Reverb time, in milliseconds, in the range from 0.001 through 3000. The default value is 1000.
        /// </summary>
        public double ReverbTime
        {
            get => Parameters.fReverbTime;
            set
            {
                Parameters.fReverbTime = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// In the range from 0.001 through 0.999. The default value is 0.001.
        /// </summary>
        public double HighFreqRTRatio
        {
            get => Parameters.fHighFreqRTRatio;
            set
            {
                Parameters.fHighFreqRTRatio = (float)value;

                OnPropertyChanged();
            }
        }
    }
}