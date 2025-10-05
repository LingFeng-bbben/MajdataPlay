namespace ManagedBass.Fx
{
    /// <summary>
    /// BassFx Phaser Effect.
    /// </summary>
    public sealed class PhaserEffect : Effect<PhaserParameters>
    {
        #region Presets
        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void PhaseShift()
        {
            Parameters.fDryMix = 0.999f;
            Parameters.fWetMix = 0.999f;
            Parameters.fFeedback = 0;
            Parameters.fRate = 1;
            Parameters.fRange = 4;
            Parameters.fFreq = 100;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void SlowInvertPhaseShiftWithFeedback()
        {
            Parameters.fDryMix = 0.999f;
            Parameters.fWetMix = -0.999f;
            Parameters.fFeedback = -0.6f;
            Parameters.fRate = 0.2f;
            Parameters.fRange = 6;
            Parameters.fFreq = 100;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void BasicPhase()
        {
            Parameters.fDryMix = 0.999f;
            Parameters.fWetMix = 0.999f;
            Parameters.fFeedback = 0;
            Parameters.fRate = 1;
            Parameters.fRange = 4.3f;
            Parameters.fFreq = 50;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void PhaseWithFeedback()
        {
            Parameters.fDryMix = 0.999f;
            Parameters.fWetMix = 0.999f;
            Parameters.fFeedback = 0.6f;
            Parameters.fRate = 1;
            Parameters.fRange = 4;
            Parameters.fFreq = 40;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void Medium()
        {
            Parameters.fDryMix = 0.999f;
            Parameters.fWetMix = 0.999f;
            Parameters.fFeedback = 0;
            Parameters.fRate = 1;
            Parameters.fRange = 7;
            Parameters.fFreq = 100;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void Fast()
        {
            Parameters.fDryMix = 0.999f;
            Parameters.fWetMix = 0.999f;
            Parameters.fFeedback = 0;
            Parameters.fRate = 1;
            Parameters.fRange = 7;
            Parameters.fFreq = 400;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void InvertWithInvertFeedback()
        {
            Parameters.fDryMix = 0.999f;
            Parameters.fWetMix = -0.999f;
            Parameters.fFeedback = -0.2f;
            Parameters.fRate = 1;
            Parameters.fRange = 7;
            Parameters.fFreq = 200;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void TremoloWah()
        {
            Parameters.fDryMix = 0.999f;
            Parameters.fWetMix = 0.999f;
            Parameters.fFeedback = 0.6f;
            Parameters.fRate = 1;
            Parameters.fRange = 4;
            Parameters.fFreq = 60;

            OnPreset();
        }
        #endregion

        /// <summary>
        /// Dry (unaffected) signal mix (-2...+2). Default = 0.
        /// </summary>
        public double DryMix
        {
            get => Parameters.fDryMix;
            set
            {
                Parameters.fDryMix = (float)value;

                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Feedback (-1...+1). Default = 0.
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
        /// Base frequency of sweep range (0&lt;...1000). Default = 0.
        /// </summary>
        public double Frequency
        {
            get => Parameters.fFreq;
            set
            {
                Parameters.fFreq = (float)value;

                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Sweep range inoctaves (0&lt;...&lt;10). Default = 0.
        /// </summary>
        public double Range
        {
            get => Parameters.fRange;
            set
            {
                Parameters.fRange = (float)value;

                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Rate of sweep in cycles per second (0&lt;...&lt;10). Default = 0.
        /// </summary>
        public double Rate
        {
            get => Parameters.fRate;
            set
            {
                Parameters.fRate = (float)value;

                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Wet (affected) signal mix (-2...+2). Default = 0.
        /// </summary>
        public double WetMix
        {
            get => Parameters.fWetMix;
            set
            {
                Parameters.fWetMix = (float)value;

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