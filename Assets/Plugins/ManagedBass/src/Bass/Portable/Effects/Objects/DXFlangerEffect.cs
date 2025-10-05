namespace ManagedBass.DirectX8
{
    /// <summary>
    /// DX8 Flanger Effect.
    /// </summary>
    public sealed class DXFlangerEffect : Effect<DXFlangerParameters>
    {
        /// <summary>
        /// Ratio of wet (processed) signal to dry (unprocessed) signal. Must be in the range from 0 (default) through 100 (all wet).
        /// </summary>
        public double WetDryMix
        {
            get => Parameters.fWetDryMix;
            set
            {
                Parameters.fWetDryMix = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Percentage by which the delay time is modulated by the low-frequency oscillator (LFO), in hundredths of a percentage point. Must be in the range from 0 through 100. The default value is 25.
        /// </summary>
        public double Depth
        {
            get => Parameters.fDepth;
            set
            {
                Parameters.fDepth = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Percentage of output signal to feed back into the effect's input, in the range from -99 to 99. The default value is 0.
        /// </summary>
        public double Feedback
        {
            get => Parameters.fFeedback;
            set
            {
                Parameters.fFeedback = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Frequency of the LFO, in the range from 0 to 10. The default value is 0.
        /// </summary>
        public double Frequency
        {
            get => Parameters.fFrequency;
            set
            {
                Parameters.fFrequency = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Waveform of the LFO. Default = <see cref="DXWaveform.Sine"/>.
        /// </summary>
        public DXWaveform Waveform
        {
            get => Parameters.lWaveform;
            set
            {
                Parameters.lWaveform = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Number of milliseconds the input is delayed before it is played back, in the range from 0 to 4. The default value is 0 ms.
        /// </summary>
        public double Delay
        {
            get => Parameters.fDelay;
            set
            {
                Parameters.fDelay = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Phase differential between left and right LFOs. Default = <see cref="DXPhase.Zero"/>.
        /// </summary>
        public DXPhase Phase
        {
            get => Parameters.lPhase;
            set
            {
                Parameters.lPhase = value;

                OnPropertyChanged();
            }
        }
    }
}