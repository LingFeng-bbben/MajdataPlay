namespace ManagedBass.Fx
{
    /// <summary>
    /// BassFx Distortion Effect.
    /// </summary>
    public sealed class DistortionEffect : Effect<DistortionParameters>
    {
        #region Presets
        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void Hard()
        {
            Parameters.fDrive = 1;
            Parameters.fDryMix = 0;
            Parameters.fWetMix = 1;
            Parameters.fFeedback = 0;
            Parameters.fVolume = 1;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void VeryHard()
        {
            Parameters.fDrive = 5;
            Parameters.fDryMix = 0;
            Parameters.fWetMix = 1;
            Parameters.fFeedback = 0.1f;
            Parameters.fVolume = 1;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void Medium()
        {
            Parameters.fDrive = 0.2f;
            Parameters.fDryMix = 1;
            Parameters.fWetMix = 1;
            Parameters.fFeedback = 0.1f;
            Parameters.fVolume = 1;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void Soft()
        {
            Parameters.fDrive = 0;
            Parameters.fDryMix = -2.95f;
            Parameters.fWetMix = -0.05f;
            Parameters.fFeedback = -0.18f;
            Parameters.fVolume = 0.25f;

            OnPreset();
        }
        #endregion

        /// <summary>
        /// Distortion Drive (0...5).
        /// </summary>
        public double Drive
        {
            get => Parameters.fDrive;
            set
            {
                Parameters.fDrive = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Dry (unaffected) signal mix (-5...+5).
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
        /// Feedback (-1...+1).
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
        /// Distortion volume (0...+2).
        /// </summary>
        public double Volume
        {
            get => Parameters.fVolume;
            set
            {
                Parameters.fVolume = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Wet (affected) signal mix (-5...+5).
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