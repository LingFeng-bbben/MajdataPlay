namespace ManagedBass.DirectX8
{
    /// <summary>
    /// DX8 Distortion Effect.
    /// </summary>
    public sealed class DXDistortionEffect : Effect<DXDistortionParameters>
    {
        /// <summary>
        /// Amount of signal change after distortion, in the range from -60 through 0. The default value is 0 dB.
        /// </summary>
        public double Gain
        {
            get => Parameters.fGain;
            set
            {
                Parameters.fGain = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Percentage of distortion intensity, in the range in the range from 0 through 100. The default value is 50 percent.
        /// </summary>
        public double Edge
        {
            get => Parameters.fEdge;
            set
            {
                Parameters.fEdge = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Center frequency of harmonic content addition, in the range from 100 through 8000. The default value is 4000 Hz.
        /// </summary>
        public double PostEQCenterFrequency
        {
            get => Parameters.fPostEQCenterFrequency;
            set
            {
                Parameters.fPostEQCenterFrequency = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Width of frequency band that determines range of harmonic content addition, in the range from 100 through 8000. The default value is 4000 Hz.
        /// </summary>
        public double PostEQBandwidth
        {
            get => Parameters.fPostEQBandwidth;
            set
            {
                Parameters.fPostEQBandwidth = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Filter cutoff for high-frequency harmonics attenuation, in the range from 100 through 8000. The default value is 4000 Hz.
        /// </summary>
        public double PreLowpassCutoff
        {
            get => Parameters.fPreLowpassCutoff;
            set
            {
                Parameters.fPreLowpassCutoff = (float)value;

                OnPropertyChanged();
            }
        }
    }
}