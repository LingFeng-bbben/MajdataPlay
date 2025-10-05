namespace ManagedBass.Fx
{
    /// <summary>
    /// BassFx Pitch shift Effect.
    /// </summary>
    /// <remarks>
    /// This effect uses FFT for its pitch shifting while maintaining duration.
    /// </remarks>
    public sealed class PitchShiftEffect : Effect<PitchShiftParameters>
    {
        /// <summary>
        /// A factor value which is between 0.5 (one octave down) and 2 (one octave up) (1 won't change the pitch, default).
        /// </summary>
        public double PitchShift
        {
            get => Parameters.fPitchShift;
            set
            {
                Parameters.fPitchShift = (float)value;

                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Semitones (0 won't change the pitch). Default = 0.
        /// </summary>
        public double Semitones
        {
            get => Parameters.fSemitones;
            set
            {
                Parameters.fSemitones = (float)value;

                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Defines the FFT frame size used for the processing. Typical values are 1024, 2048 (default) and 4096, max is 8192.
        /// </summary>
        /// <remarks>It may be any value up to 8192 but it MUST be a power of 2.</remarks>
        public long FFTFrameSize
        {
            get => Parameters.lFFTsize;
            set
            {
                Parameters.lFFTsize = value;

                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Is the STFT oversampling factor which also determines the overlap between adjacent STFT frames. Default = 8.
        /// </summary>
        /// <remarks>It should at least be 4 for moderate scaling ratios. A value of 32 is recommended for best quality (better quality = higher CPU usage).</remarks>
        public long OversamplingFactor
        {
            get => Parameters.lOsamp;
            set
            {
                Parameters.lOsamp = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A <see cref="FXChannelFlags" /> flag to define on which channels to apply the effect. Default: <see cref="FXChannelFlags.All"/>
        /// </summary>
        public FXChannelFlags Channels
        {
            get => Parameters.lChannel;
            set
            {
                Parameters.lChannel = value;

                OnPropertyChanged();
            }
        }
    }
}