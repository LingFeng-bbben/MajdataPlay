namespace ManagedBass.Fx
{
    /// <summary>
    /// BassFx Chorus Effect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// True vintage chorus works the same way as flanging. 
    /// It mixes a varying delayed signal with the original to produce a large number of harmonically related notches in the frequency response. 
    /// Chorus uses a longer delay than flanging, so there is a perception of "spaciousness", although the delay is too short to hear as a distinct slap-back echo. 
    /// There is also little or no feedback, so the effect is more subtle.
    /// </para>
    /// <para>
    /// The <see cref="DryMix"/> is the volume of Input signal and the <see cref="WetMix"/> is the volume of delayed signal. 
    /// The <see cref="Feedback"/> sets feedback of chorus. 
    /// The <see cref="Rate"/>, <see cref="MinSweep"/> and <see cref="MaxSweep"/> control how fast and far the frequency notches move. 
    /// The <see cref="Rate"/> is the rate of delay change in millisecs per sec, <see cref="MaxSweep"/>-<see cref="MinSweep"/> is the range or width of sweep in ms.
    /// </para>
    /// </remarks>
    public sealed class ChorusEffect : Effect<ChorusParameters>
    {
        #region Presets
        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void Flanger()
        {
            Parameters.fDryMix = 1;
            Parameters.fWetMix = 0.35f;
            Parameters.fFeedback = 0.5f;
            Parameters.fMinSweep = 1;
            Parameters.fMaxSweep = 5;
            Parameters.fRate = 1;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void Exaggerated()
        {
            Parameters.fDryMix = 0.7f;
            Parameters.fWetMix = 0.25f;
            Parameters.fFeedback = 0.5f;
            Parameters.fMinSweep = 1;
            Parameters.fMaxSweep = 200;
            Parameters.fRate = 50;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void MotorCycle()
        {
            Parameters.fDryMix = 0.9f;
            Parameters.fWetMix = 0.45f;
            Parameters.fFeedback = 0.5f;
            Parameters.fMinSweep = 1;
            Parameters.fMaxSweep = 100;
            Parameters.fRate = 25;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void Devil()
        {
            Parameters.fDryMix = 0.9f;
            Parameters.fWetMix = 0.35f;
            Parameters.fFeedback = 0.5f;
            Parameters.fMinSweep = 1;
            Parameters.fMaxSweep = 50;
            Parameters.fRate = 200;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void ManyVoices()
        {
            Parameters.fDryMix = 0.9f;
            Parameters.fWetMix = 0.35f;
            Parameters.fFeedback = 0.5f;
            Parameters.fMinSweep = 1;
            Parameters.fMaxSweep = 400;
            Parameters.fRate = 200;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void BackChipmunk()
        {
            Parameters.fDryMix = 0.9f;
            Parameters.fWetMix = -0.2f;
            Parameters.fFeedback = 0.5f;
            Parameters.fMinSweep = 1;
            Parameters.fMaxSweep = 400;
            Parameters.fRate = 400;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void Water()
        {
            Parameters.fDryMix = 0.9f;
            Parameters.fWetMix = -0.4f;
            Parameters.fFeedback = 0.5f;
            Parameters.fMinSweep = 1;
            Parameters.fMaxSweep = 2;
            Parameters.fRate = 1;

            OnPreset();
        }

        /// <summary>
        /// Set up a Preset.
        /// </summary>
        public void Airplane()
        {
            Parameters.fDryMix = 0.3f;
            Parameters.fWetMix = 0.4f;
            Parameters.fFeedback = 0.5f;
            Parameters.fMinSweep = 1;
            Parameters.fMaxSweep = 10;
            Parameters.fRate = 5;

            OnPreset();
        }
        #endregion

        /// <summary>
        /// Dry (unaffected) signal mix (-2...+2). Default = 0.9
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
        /// Feedback (-1...+1). Default = 0.5.
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
        /// Maximum delay in ms (0&lt;...6000). Default = 400.
        /// </summary>
        public double MaxSweep
        {
            get => Parameters.fMaxSweep;
            set
            {
                Parameters.fMaxSweep = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Minimum delay in ms (0&lt;...6000). Default = 1.
        /// </summary>
        public double MinSweep
        {
            get => Parameters.fMinSweep;
            set
            {
                Parameters.fMinSweep = (float)value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Rate in ms/s (0&lt;...1000). Default = 200.
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
        /// Wet (affected) signal mix (-2...+2). Default = 0.35.
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